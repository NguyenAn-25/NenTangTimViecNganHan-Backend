# Tài liệu hệ thống

Bộ tài liệu ở thư mục này mô tả **toàn bộ kiến trúc và các thành phần dùng chung của DotNetSkeleton**. Phần nghiệp vụ xác thực, quản lý user và permission được tách riêng trong [AuthModule](auth-module/README.md).

## Tài liệu toàn hệ thống

| Tài liệu | Mô tả |
|---|---|
| [Kiến trúc hệ thống](system-architecture.md) | Từ khởi động ứng dụng đến request pipeline, module và lớp dùng chung |
| [Các thành phần Shared](shared-components.md) | Vai trò từng thư mục/class dưới `Shared/` |
| [Quy ước API](api-conventions.md) | Response envelope, controller helpers, exception và validation |
| [Phát triển module](module-development.md) | Tạo module mới, DTO, service, validator, DI và persistence |
| [Cơ sở dữ liệu và migration](persistence-and-migrations.md) | `AppDbContext`, EF Core, seeder, môi trường Development/Production |
| [Cấu hình và chạy hệ thống](system-configuration.md) | .NET, PostgreSQL, cấu hình ứng dụng, chạy local và vận hành cơ bản |

## Tài liệu AuthModule

- [Mục lục AuthModule](auth-module/README.md)
- [Luồng xác thực](auth-module/authentication.md)
- [Quản lý user và permission](auth-module/users-and-permissions.md)
- [Mô hình dữ liệu AuthModule](auth-module/data-model-and-migrations.md)
- [Cấu hình AuthModule](auth-module/configuration.md)

## Bắt đầu nhanh

1. Cài .NET SDK 10 và PostgreSQL.
2. Tạo database PostgreSQL, cấu hình `ConnectionStrings:Default`.
3. Cấu hình `Jwt:SigningKey` và các secret cần cho môi trường đang chạy.
4. Chạy `dotnet run --launch-profile http` để khởi động Development.
5. Mở Swagger tại `http://localhost:5213/swagger`.

Trong Development, ứng dụng tự áp dụng EF migrations và chạy các seeder đã đăng ký. Production không tự áp dụng migration hoặc chạy seeder; xem [Cơ sở dữ liệu và migration](persistence-and-migrations.md).

## Phạm vi

Tài liệu tổng hệ thống giải thích vai trò của các thành phần, không thay thế tài liệu nghiệp vụ của từng module. Schema chi tiết trong AuthModule chỉ mô tả sáu entity được giới hạn: `user`, `auth_sessions`, `auth_otps`, `permissions`, `permission_details` và `functions`.
