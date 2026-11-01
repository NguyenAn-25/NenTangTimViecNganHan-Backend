# Phát triển module

Hướng dẫn này mô tả cách thêm một tính năng mới mà vẫn dùng được hạ tầng toàn hệ thống.

## Cấu trúc khuyến nghị

```text
Modules/InventoryModule/
├── Controllers/
├── DTOs/
│   ├── Requests/
│   └── Responses/
├── Entities/
├── Services/
│   └── Interfaces/
└── Validators/
```

Đặt entity, DTO, controller và service trong module sở hữu nghiệp vụ đó. Interface service để trong `Services/Interfaces/`. Tên DTO/entity theo convention của repository, ví dụ `CreateInventoryRequestDto`, `InventoryResponseDto`, `InventoryEntity`.

## 1. Tạo request và response DTO

Request DTO là đầu vào API; response DTO là dữ liệu public. Không dùng entity EF làm contract của endpoint mới.

```csharp
public sealed class CreateInventoryRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;
}

public sealed record InventoryResponseDto(Guid PublicId, string Name);
```

Dùng Data Annotations cho luật đơn giản. Với luật có nhiều điều kiện, tạo FluentValidation validator. Có thể dùng custom attributes và rule builder từ `Shared/Validation`; xem [Quy ước API](api-conventions.md).

## 2. Tạo service interface và implementation

Khai báo interface trong `Services/Interfaces/`, triển khai quy tắc nghiệp vụ trong service. Service nhận `AppDbContext`/dependency qua constructor, không nhận `ControllerBase` hay `HttpContext`.

Lỗi nghiệp vụ dự kiến ném `AppException`, ví dụ `AppException.NotFound(...)` hoặc `AppException.Conflict(...)`. Global exception handler sẽ tạo response HTTP; không tự bọc lỗi trong mỗi service.

## 3. Tạo controller

Controller kế thừa `ApiControllerBase`, gắn route/HTTP attributes và dùng helper trả response:

```csharp
[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(IInventoryService service) : ApiControllerBase
{
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType<ApiResponse<InventoryResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InventoryResponseDto>>> Get(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(publicId, cancellationToken);
        return OkResponse(item);
    }
}
```

Dùng `OkResponse`, `CreatedAtActionResponse`, `AcceptedResponse` hoặc `NoContentResponse` phù hợp status HTTP. Tránh trả `Ok(new { ... })` rải rác; xem [Quy ước API](api-conventions.md).

## 4. Validate request

Đăng ký Data Annotations trên request DTO hoặc tạo `AbstractValidator<T>`. `AddSharedApi()` bật FluentValidation AutoValidation và custom response factory; ModelState cũng có response factory chung. Validator phải được đăng ký trong DI để request tự validate trước khi vào action.

Luật dùng nhiều nơi nên đặt vào `Shared/Validation/CommonValidationRules` và expose dưới dạng attribute hoặc FluentValidation extension. Quy tắc đặc thù một module giữ trong module đó.

## 5. Đăng ký module trong DI

Tạo extension riêng nếu module có nhiều registration, ví dụ `AddInventoryModule(this IServiceCollection services)`, nơi đăng ký service/validator/controller cần thiết. Gọi extension từ `Program.cs`.

Trong project hiện tại, AuthModule có `AddAuthModule(...)`. `AddApplicationServices()` đang phục vụ ExampleModule và scan validator trong assembly chứa `CreateExampleDtoValidator`; khi thêm module, mở rộng registration có chủ đích thay vì đặt nghiệp vụ module vào `Shared`.

## 6. Đăng ký persistence

Nếu module cần lưu dữ liệu:

1. Tạo entity trong `Modules/InventoryModule/Entities/`.
2. Thêm DbSet/cấu hình entity vào `AppDbContext` theo quy ước EF hiện tại.
3. Tạo migration bằng local `dotnet-ef`.
4. Review migration và snapshot.
5. Tạo `ISeeder` chỉ khi module cần dữ liệu nền; đăng ký qua `AddSeeders()`.

Chi tiết command và môi trường deploy nằm trong [Cơ sở dữ liệu và migration](persistence-and-migrations.md). Không mở rộng dữ liệu AuthModule ngoài các entity đã được giới hạn nếu thay đổi không thuộc yêu cầu đó.

## 7. Bảo vệ endpoint

Endpoint công khai không cần authorize. Với endpoint cần authentication, dùng `[Authorize]`; với quyền theo function/action, gắn policy theo convention AuthModule và đảm bảo catalog permission chứa quyền tương ứng. AuthModule hiện là module cung cấp chính sách permission động; module mới không nên tự tạo một hệ thống claim khác nếu có thể dùng policy chung.

## 8. Checklist review module

- Controller chỉ xử lý HTTP và gọi service.
- Request/response DTO nằm trong folder riêng.
- Validation tự chạy trước khi gọi action.
- Success/error đều theo response convention.
- Service dùng `AppException` cho lỗi dự kiến.
- Mọi DI registrations của module được gọi từ composition root.
- Schema thay đổi đi qua migration; production không trông chờ app startup tự migrate.
