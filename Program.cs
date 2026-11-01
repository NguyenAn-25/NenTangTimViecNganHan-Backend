using Microsoft.EntityFrameworkCore;
using Npgsql;
using DotNetSkeleton.Shared.Configurations;
using DotNetSkeleton.Shared.Extensions;
using DotNetSkeleton.Shared.Api;
using DotNetSkeleton.Modules.AuthModule.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddAuthModule(builder.Configuration, builder.Environment);
builder.Services
    .AddSharedApi()
    .AddSwaggerDocs()
    .AddDatabase(builder.Configuration);

builder.Services.AddSeeders();

builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseExceptionHandler();

//config swagger
app.UseSwaggerDocs();

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//config seeder
if (app.Environment.IsDevelopment())
{
    await app.SeedDatabaseAsync();
}

app.MapGet("/", (HttpContext context) =>
{
    return Results.Ok(ApiResponse<string>.Succeed("Chào, tôi là dotnet", traceId: context.TraceIdentifier));
});

app.MapGet("/test-db", async (HttpContext context, IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("Default");
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync(context.RequestAborted);
    return Results.Ok(ApiResponse<string>.Succeed("Kết nối Postgres thành công!", traceId: context.TraceIdentifier));
});

app.Run();


// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast = Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
