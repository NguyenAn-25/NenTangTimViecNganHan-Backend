# Các thành phần Shared

Trang này giải thích từng thành phần hiện có trong `Shared/` và cách module sử dụng chúng.

## `Shared/Api`

### `ApiResponse<T>`

`ApiResponse<T>` là envelope JSON chung:

```json
{
  "success": true,
  "message": null,
  "data": {},
  "errors": null,
  "code": null,
  "traceId": "..."
}
```

`Succeed(data, message, traceId)` tạo response thành công. `Fail(message, code, traceId, errors)` tạo response lỗi. HTTP status vẫn là nguồn xác định status: envelope không biến lỗi 404 thành 200. Xem [Quy ước API](api-conventions.md) để biết contract đầy đủ.

## `Shared/Controllers`

### `ApiControllerBase`

Controller của module kế thừa `ApiControllerBase` để dùng helper đóng gói `ApiResponse<T>`:

| Helper | HTTP status | Dùng khi |
|---|---:|---|
| `OkResponse(data, message?)` | 200 | Đọc hoặc cập nhật thành công |
| `CreatedAtActionResponse(actionName, routeValues, data, message?)` | 201 | Tạo tài nguyên và có route đọc lại |
| `AcceptedResponse(message)` | 202 | Tiếp nhận xử lý bất đồng bộ, chỉ cần thông báo |
| `AcceptedResponse(data, message?)` | 202 | Tiếp nhận xử lý và trả thông tin |
| `AcceptedAtActionResponse(...)` | 202 | Tiếp nhận xử lý, trả Location và data |
| `NoContentResponse()` | 204 | Thành công nhưng không trả body |
| `BadRequestResponse(...)`, `UnauthorizedResponse(...)`, `ForbiddenResponse(...)`, `NotFoundResponse(...)`, `ConflictResponse(...)` | 4xx | Trả lỗi trực tiếp từ controller |

Mỗi helper lấy `HttpContext.TraceIdentifier` để response có thể đối chiếu với request/log.

## `Shared/Exceptions`

### `AppException`

`AppException` mô tả lỗi nghiệp vụ dự kiến bằng HTTP status, `code`, message và dictionary lỗi tùy chọn. Có thể dùng factory:

- `AppException.Validation(errors, message?)`
- `AppException.BadRequest(message, code?)`
- `AppException.Unauthorized(message?)`
- `AppException.Forbidden(message?)`
- `AppException.NotFound(message?)`
- `AppException.Conflict(message, code?)`
- `AppException.ServiceUnavailable(message, code?)`

Status code constructor bị giới hạn từ 400 đến 599. Ví dụ:

```csharp
throw AppException.NotFound("Không tìm thấy đơn hàng.");
throw AppException.Conflict("Mã đơn đã tồn tại.", "order_code_exists");
```

Service ném lỗi nghiệp vụ; service không tự tạo `IActionResult` và không phụ thuộc ASP.NET controller.

### `GlobalExceptionHandler`

`GlobalExceptionHandler` được đăng ký qua `AddSharedApi()` và chạy bởi `UseExceptionHandler()`. Nó:

- đổi `AppException` thành status/code/message từ exception;
- đổi `FluentValidation.ValidationException` và `DataAnnotations.ValidationException` thành 400;
- ghi exception không dự kiến vào log, trả 500 với message tổng quát và `traceId`;
- không gửi stack trace hoặc chi tiết exception nội bộ về client.

## `Shared/Validation`

### Predicate chung

`CommonValidationRules` là nơi chứa logic kiểm tra thuần để annotation và FluentValidation dùng chung:

| Hàm | Luật |
|---|---|
| `IsE164Phone` | Dấu `+`, theo sau bởi 8–15 chữ số; chữ số đầu trong dãy từ 1 đến 9 |
| `IsSixDigitOtp` | Đúng 6 chữ số |
| `IsPasswordStrengthValid` | 8–128 ký tự, có chữ in hoa, chữ số và ký tự đặc biệt |
| `IsRequiredText` | Không null/rỗng/khoảng trắng |
| `HasLengthBetween` | Độ dài nằm trong khoảng truyền vào |

### Data Annotations

`E164PhoneAttribute`, `SixDigitOtpAttribute`, `PasswordStrengthAttribute` tái sử dụng predicate chung trên request DTO. Ghép `[Required]` khi trường bắt buộc; attribute luật định dạng cho phép trường trống để lỗi Required không bị lặp.

### FluentValidation

`FluentValidationRuleBuilderExtensions` cung cấp `MustBeE164Phone`, `MustBeSixDigitOtp`, `MustHaveStrongPassword`, `MustBeRequiredText`, `MustHaveLengthBetween`. Dùng `.WithMessage(...)` để thay message chung theo tên field/ngữ cảnh module.

### Kết quả validation

`ApiValidationResultFactory` thay default result factory của SharpGrip FluentValidation AutoValidation để FluentValidation trả cùng envelope với Data Annotations. `SharedApiExtensions.AddSharedApi()` cấu hình `InvalidModelStateResponseFactory` cho model binding/annotation errors.

## `Shared/Configurations`

- `DatabaseConfiguration.AddDatabase`: đăng ký `AppDbContext` và provider `UseNpgsql` bằng `ConnectionStrings:Default`.
- `SwaggerConfiguration.AddSwaggerDocs`: đăng ký API explorer và Swashbuckle.
- `SwaggerConfiguration.UseSwaggerDocs`: bật Swagger UI chỉ khi môi trường là Development.
- `SeederConfiguration.AddSeeders`: đăng ký `ISeeder` và `DatabaseSeeder` hiện dùng.
- `SeederConfiguration.SeedDatabaseAsync`: tạo DI scope rồi chạy database seeder.

## `Shared/Extensions`

- `SharedApiExtensions.AddSharedApi`: đăng ký exception handler, Problem Details support, FluentValidation auto-validation và response factory cho lỗi model state.
- `DependencyInjection.AddApplicationServices`: hiện đăng ký `IExampleService` và scan FluentValidation validator trong assembly chứa `CreateExampleDtoValidator`. Tên hàm rộng hơn nội dung hiện tại; khi thêm module, đăng ký service/module mới rõ ràng theo hướng dẫn [Phát triển module](module-development.md).

## `Shared/Persistence`

- `AppDbContext`: điểm cấu hình EF Core context và DbSet của các module.
- `AppDbContextFactory`: tạo context khi chạy EF CLI mà không khởi động web app/seeder. Nó đọc appsettings, User Secrets và environment variables.
- `Migrations/`: migration và model snapshot.
- `Seeders/ISeeder`: contract seeder với thứ tự chạy và hàm `SeedAsync`.
- `Seeders/DatabaseSeeder`: chạy migration trước, sau đó chạy các seeder theo `Order`, ghi log và dừng nếu seeder lỗi.
- `Seeders/AuthPermissionCatalogSeeder`: nạp catalog permission của AuthModule.
- `Seeders/AuthDevelopmentAdminSeeder`: tạo một admin local tùy chỉnh khi có đủ secrets; chỉ chạy trong Development, không tạo session/OTP và không ghi đè user có sẵn.
- `Seeders/AuthDevelopmentAdminAccountsSeeder`: tạo hai admin fixture với password Development `123456`; chỉ chạy trong Development và không nâng quyền user có sẵn.
- `Seeders/AuthDevelopmentSampleDataSeeder`: tạo 24 user mẫu với password Development `123456`, 44 OTP và 40 session lịch sử đã hết hạn; chỉ chạy trong Development.
- `Seeders/ExampleDataSeeder`: seeder mẫu đang có trong source nhưng hiện không đăng ký trong `AddSeeders()`.

Cách tạo/áp dụng migration và sự khác nhau giữa startup Development/Production nằm trong [Cơ sở dữ liệu và migration](persistence-and-migrations.md).

## `Shared/Middlewares`

Folder hiện chỉ có placeholder `.gitkeep`; chưa có middleware tùy biến nằm trong folder này. Exception handling dùng ASP.NET Core `IExceptionHandler` qua middleware `UseExceptionHandler()` và class nằm trong `Shared/Exceptions`.
