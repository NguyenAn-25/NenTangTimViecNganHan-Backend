# DotNetSkeleton

Template ASP.NET Core Web API được tổ chức theo mô hình module/feature, phù hợp để phát triển các ứng dụng backend có khả năng mở rộng.

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
```

Phiên bản SDK phải tương thích với `net10.0` trong file `DotNetSkeleton.csproj`.

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

## 3. Cấu hình database bằng User Secrets

Tạo một database PostgreSQL, ví dụ:

```sql
CREATE DATABASE dotnet_skeleton;
```

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

## 7. Cấu trúc project

```text
DotNetSkeleton/
├── `Program.cs`
├── `appsettings.json`
├── `DotNetSkeleton.csproj`
├── Modules/
│   ├── AuthModule/
│   └── ExampleModule/
│       ├── `ExampleController.cs`
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
- `Seeders`: Dữ liệu mẫu được tạo khi chạy Development.
- `Validators`: Kiểm tra dữ liệu đầu vào.
- `DependencyInjection.cs`: Đăng ký service và validator.

## 8. Tạo module mới

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

## 9. Migration

Cài đặt Entity Framework CLI nếu chưa có:

```bash
dotnet tool install --global dotnet-ef
```

Kiểm tra:

```bash
dotnet ef --version
```

Sau khi thêm hoặc thay đổi entity:

```bash
dotnet ef migrations add AddProduct
```

Cập nhật database:

```bash
dotnet ef database update
```

Xem danh sách migration:

```bash
dotnet ef migrations list
```

Xóa migration cuối cùng nếu migration chưa được áp dụng:

```bash
dotnet ef migrations remove
```

## 10. Seeder

Khi chạy ở môi trường `Development`, ứng dụng sẽ:

1. Tự động chạy các migration chưa được áp dụng.
2. Tìm các class implement `ISeeder`.
3. Chạy seeder theo thứ tự `Order`.
4. Bỏ qua dữ liệu đã tồn tại.

Tạo seeder mới bằng cách implement `ISeeder`:

```csharp
public class ProductSeeder : ISeeder
{
    public int Order => 2;

    public async Task<int> SeedAsync(AppDbContext context)
    {
        // Thêm dữ liệu mẫu tại đây
        return 0;
    }
}
```

## 11. Quy trình phát triển module

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

## 12. Build và publish

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