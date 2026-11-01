# DotNetSkeleton

Template ASP.NET Core Web API được tổ chức theo mô hình module/feature, phù hợp để phát triển các ứng dụng backend có khả năng mở rộng.

Tài liệu chi tiết bằng tiếng Việt: [mục lục tài liệu trong docs](docs/README.md).

## Công nghệ sử dụng

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Swagger/OpenAPI
- FluentValidation
- Dependency Injection
- EF Core Migration và Database Seeder

## 1. Yêu cầu môi trường

Cần cài đặt:

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL
- Visual Studio 2022 hoặc Visual Studio Code

Kiểm tra .NET:

```bash
dotnet --version
dotnet --list-sdks
```

SDK đang được chọn cần là phiên bản 10.x để build target `net10.0` trong file `DotNetSkeleton.csproj`.

## 2. Tải và cài đặt project

Clone repository:

```bash
git clone <repository-url>
cd DotNetSkeleton
```

Khôi phục thư viện:

```bash
dotnet restore
```

Build project:

```bash
dotnet build
```

## 3. Cấu hình database local

Khởi động PostgreSQL bằng Docker Compose:

```bash
docker compose up -d
```

Compose tạo database `mphqa_database` và lưu dữ liệu trong volume `mphqa_postgres_data`. Trong Development, `appsettings.Development.json` kết nối `localhost:5432` với database này, user `postgres`, password `postgres`. Nếu thông tin đăng nhập local của bạn khác, ghi đè bằng User Secrets:

Khởi tạo User Secrets:

```bash
dotnet user-secrets init
```

Lưu connection string:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=dotnet_skeleton;Username=postgres;Password=your_password"
```

Kiểm tra cấu hình:

```bash
dotnet user-secrets list
```

Xóa connection string:

```bash
dotnet user-secrets remove "ConnectionStrings:Default"
```

Không commit password hoặc connection string thật vào Git.

## 4. Chạy project

Chạy project ở môi trường Development:

```bash
dotnet run --environment Development
```

Hoặc chạy theo profile:

```bash
dotnet run --launch-profile http
```

```bash
dotnet run --launch-profile https
```

Các địa chỉ mặc định:

```text
HTTP:  http://localhost:5213
HTTPS: https://localhost:7215
```

Nếu HTTPS báo lỗi chứng chỉ:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

## 5. Swagger

Mở Swagger tại:

```text
http://localhost:5213/swagger
```

hoặc:

```text
https://localhost:7215/swagger
```

Swagger hiện chỉ được bật trong môi trường `Development`.

## 6. API mẫu

Kiểm tra ứng dụng:

```http
GET /
```

Kiểm tra kết nối PostgreSQL:

```http
GET /test-db
```

Example module:

```http
GET /api/Example
GET /api/Example/{id}
POST /api/Example
```

Request tạo Example:

```json
{
  "name": "Example mới"
}
```

Trường `name` phải có từ 3 đến 20 ký tự.

## 7. Auth cơ bản

Auth dùng số điện thoại theo định dạng E.164 (ví dụ `+84901234567`), password được hash bằng ASP.NET Core `PasswordHasher`, access token là JWT và refresh token ngẫu nhiên được xoay vòng sau mỗi lần refresh. OTP và refresh token chỉ được lưu dưới dạng hash trong PostgreSQL.

OTP được lưu dạng hash. Trong Development, SMS đang tạm tắt; OTP dùng mã `012345` và được ghi trong application log, kể cả khi có cấu hình Stringee. Ngoài Development, OTP là chuỗi số ngẫu nhiên 6 chữ số và được gửi qua SMS. Mật khẩu đăng ký/đặt lại phải dài 8–128 ký tự, có chữ in hoa, chữ số và ký tự đặc biệt; `123456` chỉ là mật khẩu của dữ liệu seed Development.

| Method | Endpoint | Mục đích |
|---|---|---|
| `POST` | `/api/v1/auth/register` | Tạo tài khoản chưa kích hoạt và gửi OTP |
| `POST` | `/api/v1/auth/otp/verify/{publicId}` | Xác minh OTP của challenge |
| `POST` | `/api/v1/auth/otp/resend/{publicId}` | Gửi lại OTP của challenge |
| `POST` | `/api/v1/auth/register/complete` | Hoàn tất đăng ký sau OTP đã xác minh, đặt password và cấp token |
| `POST` | `/api/v1/auth/login` | Đăng nhập bằng phone/password |
| `POST` | `/api/v1/auth/refresh` | Xoay refresh token và cấp access token mới |
| `POST` | `/api/v1/auth/logout` | Thu hồi session hiện tại; cần Bearer token |
| `POST` | `/api/v1/auth/password/forgot` | Yêu cầu OTP đặt lại mật khẩu |
| `POST` | `/api/v1/auth/password/reset` | Đặt password mới sau khi OTP đã xác minh |
| `GET` | `/api/v1/auth/me` | Lấy hồ sơ của chính mình; cần Bearer token |

Quản lý user cần permission tương ứng; role `admin` được seed với các quyền `users:read`, `users:create` và `users:update`:

| Method | Endpoint | Permission | Mục đích |
|---|---|---|---|
| `GET` | `/api/v1/users?page=1&pageSize=20&search=...` | `users:read` | Tìm kiếm theo phone và phân trang |
| `GET` | `/api/v1/users/{id}` | `users:read` | Xem thông tin một user theo ID |
| `POST` | `/api/v1/users` | `users:create` | Tạo tài khoản ADMIN bằng phone, password và permission |
| `PUT` | `/api/v1/users/{id}` | `users:update` | Đổi permission profile |
| `PUT` | `/api/v1/users/{id}/password` | `users:update` | Đổi password và thu hồi session |
| `PATCH` | `/api/v1/users/{id}/status` | `users:update` | Bật/tắt user |

API quản lý do admin thao tác và liệt kê toàn bộ tài khoản `USER` lẫn `ADMIN`; danh sách trả summary gọn, còn xem theo `id` trả detail. Các luồng quản trị dùng ID nội bộ dạng số; `publicId` vẫn có trong response để dùng ở các luồng công khai. Hỗ trợ lọc theo phone (`search`), role, trạng thái active/verified, permission profile và khoảng ngày tạo. Khi tạo, admin gửi phone chưa được sử dụng, password mạnh và `permissionId`; hệ thống tạo tài khoản `ADMIN` active/verified trực tiếp, không gửi OTP. Tạo thành công trả `201 Created`. Khi cập nhật, chỉ đổi permission (chỉ áp dụng cho tài khoản `ADMIN`); đổi permission hoặc password sẽ thu hồi session hiện có. Permission trong response được nhóm thành object `permission`; summary có `id`, `name`, detail bổ sung `description`, `status`. `verifiedAt` ghi thời điểm xác minh tài khoản; thời điểm xác minh từng OTP challenge vẫn nằm trên bản ghi OTP.

Ví dụ đăng ký:

```json
{
  "phone": "+84901234567"
}
```

`/auth/register` và `/auth/password/forgot` trả challenge có `publicId`. Gửi mã OTP đến `/auth/otp/verify/{publicId}`. Sau khi xác minh thành công, đăng ký hoàn tất qua `/auth/register/complete` với `publicId` và password; đặt lại mật khẩu qua `/auth/password/reset` với `publicId` và `newPassword`. Đăng ký bằng số đã được xác minh trả `409 Conflict` để người dùng đăng nhập hoặc dùng chức năng quên mật khẩu. Mật khẩu dài 8–128 ký tự, có chữ in hoa, chữ số và ký tự đặc biệt.

Thêm khóa JWT vào User Secrets trước khi chạy. Khóa cần tối thiểu 32 ký tự ngẫu nhiên:

```bash
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)"
```

Trong Development, ứng dụng hiện không gọi Stringee; lấy mã OTP trong application log để thử local. Stringee được giữ sẵn cho môi trường ngoài Development. Cấu hình provider bằng User Secrets khi cần tích hợp:

```bash
dotnet user-secrets set "Stringee:ApiKeySid" "your-api-key-sid"
dotnet user-secrets set "Stringee:ApiKeySecret" "your-api-key-secret"
dotnet user-secrets set "Stringee:BrandName" "YOUR_BRANDNAME"
```

Trong Production, phải cấu hình đủ ba giá trị Stringee bằng secret manager hoặc biến môi trường (`Stringee__ApiKeySid`, `Stringee__ApiKeySecret`, `Stringee__BrandName`). Brandname cần được đăng ký/kích hoạt cho SMS OTP trên tài khoản Stringee. Không commit thông tin xác thực.

Migration `20261026170000_AddAuthenticationEntities` tạo các bảng auth/permission. Trong Development, `AuthPermissionCatalogSeeder` tạo profile `ADMIN_DEFAULT`; tài khoản đăng ký công khai luôn có role `USER` và không nhận permission profile. Hai tài khoản ADMIN local được seed tự động như mô tả ở mục Seeder bên dưới. Chạy migration thủ công bằng `dotnet tool run dotnet-ef -- database update` nếu không khởi động app trong Development. Không cấp role ADMIN từ request đăng ký công khai; Production cần quy trình bootstrap admin được kiểm soát.

## 8. Cấu trúc project

```text
DotNetSkeleton/
├── `Program.cs`
├── `appsettings.json`
├── `DotNetSkeleton.csproj`
├── Modules/
│   ├── AuthModule/
│   │   ├── Controllers/
│   │   ├── Authorization/
│   │   ├── Configuration/
│   │   ├── DTOs/
│   │   │   ├── Requests/
│   │   │   └── Responses/
│   │   ├── Entities/
│   │   └── Services/
│   │       ├── `AuthService.cs`
│   │       ├── `UserService.cs`
│   │       └── Interfaces/
│   └── ExampleModule/
│       ├── Controllers/
│       │   └── `ExampleController.cs`
│       ├── `ExampleService.cs`
│       ├── `IExampleService.cs`
│       ├── DTOs/
│       ├── Entities/
│       └── Validators/
├── Shared/
│   ├── Configurations/
│   ├── Extensions/
│   ├── Middlewares/
│   └── Persistence/
│       ├── `AppDbContext.cs`
│       ├── Migrations/
│       └── Seeders/
└── Properties/
    └── `launchSettings.json`
```

### Các thành phần chính

- `Program.cs`: Cấu hình và khởi chạy ứng dụng.
- `Modules`: Chứa các module theo domain hoặc feature.
- `Shared`: Chứa các thành phần dùng chung.
- `AppDbContext`: Cấu hình Entity Framework Core.
- `Migrations`: Các migration của database.
- `Seeders`: Permission catalog được khởi tạo trong Development; migration cũng tạo catalog mặc định cho môi trường khác.
- `Validators`: Kiểm tra dữ liệu đầu vào.
- `DependencyInjection.cs`: Đăng ký service và validator.

## 9. Tạo module mới

Ví dụ tạo module `ProductModule`.

### Bước 1: Tạo cấu trúc thư mục

```text
Modules/ProductModule/
├── ProductController.cs
├── ProductService.cs
├── IProductService.cs
├── DTOs/
│   ├── Requests/
│   └── Responses/
├── Entities/
└── Validators/
```

### Bước 2: Tạo entity

```csharp
namespace DotNetSkeleton.Modules.ProductModule.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Bước 3: Đăng ký entity trong `AppDbContext`

```csharp
public DbSet<Product> Products { get; set; }
```

### Bước 4: Tạo DTO và validator

```csharp
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

```csharp
using FluentValidation;

public class CreateProductDtoValidator
    : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);
    }
}
```

Validator sẽ được tự động scan bởi cấu hình hiện tại.

### Bước 5: Tạo service

```csharp
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
}
```

Service chịu trách nhiệm xử lý nghiệp vụ và làm việc với `AppDbContext`.

### Bước 6: Đăng ký service

Mở `DependencyInjection.cs`:

```csharp
services.AddScoped<IProductService, ProductService>();
```

### Bước 7: Tạo controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllAsync();
        return Ok(result);
    }
}
```

Sau khi chạy project, API sẽ xuất hiện trong Swagger.

## 10. Migration

Cài đặt công cụ Entity Framework CLI của repo:

```bash
dotnet tool restore
```

Kiểm tra:

```bash
dotnet tool run dotnet-ef -- --version
```

Sau khi thêm hoặc thay đổi entity:

```bash
dotnet tool run dotnet-ef -- migrations add AddProduct
```

Cập nhật database:

```bash
dotnet tool run dotnet-ef -- database update
```

Xem danh sách migration:

```bash
dotnet tool run dotnet-ef -- migrations list
```

Xóa migration cuối cùng nếu migration chưa được áp dụng:

```bash
dotnet tool run dotnet-ef -- migrations remove
```

## 11. Seeder

Khi chạy ở môi trường `Development`, ứng dụng sẽ:

1. Tự động chạy các migration chưa được áp dụng.
2. Chạy `AuthPermissionCatalogSeeder` để đồng bộ function `users` và profile quyền `ADMIN_DEFAULT`. Grant ban đầu chỉ được tạo một lần.
3. Chạy `AuthDevelopmentAdminAccountsSeeder` để tạo hai tài khoản ADMIN giả lập, chỉ dùng local.
4. Chạy `AuthDevelopmentAdminSeeder` nếu có cấu hình thêm `Seed:AdminPhone` và `Seed:AdminPassword` trong User Secrets.
5. Chạy `AuthDevelopmentSampleDataSeeder` để thêm 24 user thường giả lập, 44 OTP và 40 session lịch sử.

Hai tài khoản ADMIN local dùng số `+12025550190`, `+12025550191`, mật khẩu `123456`; đăng nhập bằng số điện thoại và mật khẩu này. Đây là thông tin cố định chỉ dành cho môi trường Development, không dùng cho Production. Có thể cấu hình thêm một admin riêng qua User Secrets; seeder này cũng chỉ chạy trong Development và bỏ qua xác minh OTP. Các user thường mẫu dùng số điện thoại giả lập `+12025550100`–`+12025550123` và mật khẩu `123456`; 16 user active có thể đăng nhập, còn tài khoản chờ xác minh/đã tắt thì không. OTP/session mẫu đều hết hạn. Production không chạy các seeder dữ liệu này.

Tạo seeder mới bằng cách implement `ISeeder`:

```csharp
public class ProductSeeder : ISeeder
{
    public int Order => 4;

    public async Task<int> SeedAsync(AppDbContext context)
    {
        // Thêm dữ liệu mẫu tại đây
        return 0;
    }
}
```

## 12. Quy trình phát triển module

Khi tạo chức năng mới:

1. Tạo thư mục module.
2. Tạo entity.
3. Thêm `DbSet` vào `AppDbContext`.
4. Tạo DTO request/response.
5. Tạo validator.
6. Tạo service và interface.
7. Đăng ký service vào Dependency Injection.
8. Tạo controller.
9. Tạo migration.
10. Cập nhật database.
11. Chạy project và kiểm tra bằng Swagger.

## 13. Build và publish

Build Release:

```bash
dotnet build --configuration Release
```

Publish:

```bash
dotnet publish --configuration Release --output ./publish
```

Chạy bản publish:

```bash
dotnet ./publish/DotNetSkeleton.dll
```
