using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetSkeleton.Modules.AuthModule.Configuration;
using DotNetSkeleton.Modules.AuthModule.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DotNetSkeleton.Modules.AuthModule.Services;

/// <summary>Production SMS adapter that sends OTP messages through Stringee's third-party REST API.</summary>
public sealed class StringeeOtpSmsSender : IOtpSmsSender
{
    // Stringee's external SMS endpoint; credentials are never sent in the request body.
    private static readonly Uri SmsEndpoint = new("https://api.stringee.com/v1/sms");
    private readonly StringeeOptions _options;
    private readonly HttpClient _httpClient;

    public StringeeOtpSmsSender(IOptions<StringeeOptions> options, HttpClient httpClient)
    {
        _options = options.Value;
        _httpClient = httpClient;
    }

    public async Task SendAsync(string phone, string code, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            sms = new[]
            {
                new
                {
                    from = _options.BrandName,
                    to = NormalizePhone(phone),
                    text = $"Mã xác thực của bạn. Không chia sẻ với bất kỳ ai. Mã: {code}"
                }
            }
        };

        // Stringee authenticates this third-party request with a short-lived HS256 JWT header.
        using var request = new HttpRequestMessage(HttpMethod.Post, SmsEndpoint);
        request.Headers.Add("X-STRINGEE-AUTH", CreateAuthToken());
        request.Headers.Accept.ParseAdd("application/json");
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Stringee SMS request failed with HTTP status {(int)response.StatusCode}.",
                inner: null,
                statusCode: response.StatusCode);
        }

        StringeeSmsResponse? result;
        try
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            result = JsonSerializer.Deserialize<StringeeSmsResponse>(
                responseBody,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException exception)
        {
            throw new HttpRequestException("Stringee returned an invalid SMS response.", exception);
        }

        var smsResult = result?.Results is { Count: 1 } results ? results[0] : null;
        if (result?.SmsSent != 1 || smsResult?.Code != 0)
        {
            var resultCode = smsResult?.Code?.ToString() ?? "missing";
            var providerMessage = string.IsNullOrWhiteSpace(smsResult?.Message)
                ? "no provider message"
                : smsResult.Message;
            throw new HttpRequestException(
                $"Stringee rejected the OTP SMS (smsSent={result?.SmsSent.ToString() ?? "missing"}, " +
                $"resultCode={resultCode}, message={providerMessage}).");
        }
    }

    private string CreateAuthToken()
    {
        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            typ = "JWT",
            alg = "HS256",
            cty = "stringee-api;v=1"
        }));
        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            jti = $"{_options.ApiKeySid}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid():N}",
            iss = _options.ApiKeySid,
            exp = DateTimeOffset.UtcNow.AddMinutes(60).ToUnixTimeSeconds(),
            rest_api = true
        }));
        var unsignedToken = $"{header}.{payload}";
        var signature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_options.ApiKeySecret),
            Encoding.UTF8.GetBytes(unsignedToken));

        return $"{unsignedToken}.{Base64UrlEncode(signature)}";
    }

    private static string NormalizePhone(string phone) =>
        phone.StartsWith('+') ? phone[1..] : phone;

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private sealed record StringeeSmsResponse(
        [property: JsonPropertyName("smsSent")] int SmsSent,
        [property: JsonPropertyName("result")] List<StringeeSmsResult>? Results);

    private sealed record StringeeSmsResult(
        [property: JsonPropertyName("r")] int? Code,
        [property: JsonPropertyName("msg")] string? Message);
}
