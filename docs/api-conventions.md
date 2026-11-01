# Quy ước API dùng chung

Các module controller kế thừa `ApiControllerBase`, các service dùng `AppException` cho lỗi nghiệp vụ, còn request DTO dùng Data Annotations hoặc FluentValidation. Hạ tầng chung được đăng ký tại `Program.cs` bằng `AddSharedApi()` và `UseExceptionHandler()`.

## Response envelope

Các response JSON từ Auth, Users và Example thống nhất theo dạng:

```json
{
  "success": true,
  "message": null,
  "data": {
    "example": "dữ liệu response DTO"
  },
  "errors": null,
  "code": null,
  "traceId": "00-..."
}
```

- `success`: kết quả xử lý.
- `message`: thông báo tùy chọn.
- `data`: DTO hoặc collection trả về; có thể `null` khi chỉ có thông báo.
- `errors`: lỗi gắn theo tên field, thường dùng cho validation.
- `code`: mã lỗi ổn định cho client xử lý.
- `traceId`: mã request để tìm lỗi tương ứng trong log.

HTTP status vẫn thể hiện kết quả: ví dụ 200, 201, 202, 204, 400, 401, 403, 404, 409, 429 và 500. `204 No Content` giữ đúng ngữ nghĩa HTTP nên không có body/envelope.

Ví dụ lỗi validation:

```json
{
  "success": false,
  "message": "Dữ liệu đầu vào không hợp lệ.",
  "data": null,
  "errors": {
    "Phone": [
      "Số điện thoại phải theo định dạng E.164, ví dụ +84901234567."
    ]
  },
  "code": "validation_error",
  "traceId": "00-..."
}
```

Các API Auth/Users/Example trước đây trả DTO thô hoặc object ẩn danh; hiện tại response JSON được bọc trong `data`. Client hiện có cần đọc các trường theo cấu trúc envelope mới.

## Helper trong controller

`ApiControllerBase` ở `Shared/Controllers` có sẵn helper:

- `OkResponse(data, message?)` — trả 200.
- `CreatedAtActionResponse(actionName, routeValues, data, message?)` — trả 201 kèm Location.
- `AcceptedResponse(message)` — trả 202 với thông báo; có overload nhận `data`.
- `AcceptedAtActionResponse(actionName, routeValues, data, message?)` — trả 202 kèm Location và data.
- `NoContentResponse()` — trả 204.
- `BadRequestResponse(message, code?)`, `UnauthorizedResponse(message?)`, `ForbiddenResponse(message?)`, `NotFoundResponse(message?)`, `ConflictResponse(message, code?)` — trả lỗi envelope với HTTP status phù hợp.

Ví dụ controller mới:

```csharp
[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductsService productsService) : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductResponseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await productsService.GetByIdAsync(id, cancellationToken);
        return OkResponse(product);
    }
}
```

Response DTO vẫn là kiểu trả về của endpoint; helper chỉ đóng gói DTO vào envelope. Dùng `CreatedAtActionResponse` cho thao tác tạo có route đọc tài nguyên, và giữ `NoContentResponse` cho thao tác không cần response body.

## Exception dùng ở mọi module

Dùng `AppException` trong `Shared/Exceptions` cho lỗi nghiệp vụ dự kiến. Global handler tự đổi exception thành HTTP status và envelope:

```csharp
throw AppException.NotFound("Không tìm thấy sản phẩm.");
throw AppException.Conflict("Mã sản phẩm đã tồn tại.", "product_code_exists");
throw AppException.BadRequest("Trạng thái không hợp lệ.");
```

Các helper có sẵn: `Validation`, `BadRequest`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`, `ServiceUnavailable`. Nếu cần mã lỗi riêng, truyền tham số `code`; `Validation` nhận thêm dictionary lỗi theo field.

`GlobalExceptionHandler` xử lý `AppException`, lỗi validation phát sinh trong service và lỗi ngoài dự kiến. Lỗi không dự kiến trả 500 với thông báo tổng quát, không gửi exception nội bộ ra client; exception cùng `traceId` được ghi vào log. Với request auth, JWT Bearer events cũng trả envelope cho 401 và 403.

Không bắt exception rồi tự tạo anonymous response trong từng controller. Hãy ném `AppException` khi đó là lỗi nghiệp vụ có dự kiến; exception bất ngờ để global handler xử lý.

## Validation dùng chung

Các predicate nằm ở `Shared/Validation/CommonValidationRules`:

- `IsE164Phone`: số điện thoại `+` theo sau bởi 8–15 chữ số.
- `IsSixDigitOtp`: OTP đúng 6 chữ số.
- `IsPasswordStrengthValid`: password dài 8–128 ký tự, có chữ in hoa, chữ số và ký tự đặc biệt.
- `IsRequiredText` và `HasLengthBetween`: kiểm tra text dùng chung.

### Dùng với Data Annotations

Các custom attribute tái sử dụng trong request DTO:

```csharp
using System.ComponentModel.DataAnnotations;
using DotNetSkeleton.Shared.Validation.Attributes;

public sealed class ExampleRequestDto
{
    [Required]
    [E164Phone]
    public string Phone { get; init; } = string.Empty;

    [Required]
    [SixDigitOtp]
    public string Code { get; init; } = string.Empty;

    [Required]
    [PasswordStrength]
    public string Password { get; init; } = string.Empty;
}
```

`[ApiController]` tự validate DTO. Khi có lỗi, `AddSharedApi()` gom lỗi theo field và trả envelope `400 validation_error`. Ghép `[Required]` với custom attribute khi field bắt buộc; custom attribute cho phép giá trị null/rỗng để tránh báo hai lỗi cho cùng trường hợp thiếu dữ liệu.

### Dùng với FluentValidation

Các rule extensions ở `Shared/Validation/FluentValidationRuleBuilderExtensions` gọi cùng predicate:

```csharp
RuleFor(request => request.Phone)
    .MustBeE164Phone();

RuleFor(request => request.Code)
    .MustBeSixDigitOtp();

RuleFor(request => request.Name)
    .MustBeRequiredText()
    .MustHaveLengthBetween(3, 80);
```

Có thể thêm `.WithMessage("...")` sau mỗi rule để tùy biến thông báo theo nghiệp vụ. Ví dụ `CreateExampleDtoValidator` đang dùng các rule text dùng chung.

## Đăng ký hạ tầng

Trong `Program.cs`:

```csharp
builder.Services.AddSharedApi();

var app = builder.Build();
app.UseExceptionHandler();
```

`AddSharedApi()` đăng ký global exception handler, Problem Details services, response factory cho ModelState và FluentValidation auto-validation với response factory dùng chung. `AddApplicationServices()` scan validator; các module có thể thêm service/validator riêng mà không tự cài lại response lỗi. Rate-limit rejection của Auth cũng trả cùng envelope.

## Thêm validator cho module mới

1. Tạo request DTO và đánh dấu `[ApiController]` trên controller.
2. Dùng annotation chung cho luật đơn giản, hoặc FluentValidation khi cần rule liên quan nhiều trường/nghiệp vụ.
3. Đăng ký/scan validator theo cơ chế DI hiện có.
4. Service ném `AppException` cho các lỗi dự kiến; controller gọi helper từ `ApiControllerBase`.
5. Khai báo `ProducesResponseType<ApiResponse<T>>` để OpenAPI mô tả đúng response envelope.
