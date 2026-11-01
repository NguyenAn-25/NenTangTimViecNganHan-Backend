# Tài liệu AuthModule

Các trang trong folder này mô tả riêng nghiệp vụ xác thực, quản lý user và permission trong AuthModule. Kiến trúc, API conventions, DI, PostgreSQL và migration chung của ứng dụng nằm ở [tài liệu toàn hệ thống](../README.md).

| Tài liệu | Nội dung |
|---|---|
| [Luồng xác thực](authentication.md) | Đăng ký, OTP, login, JWT, refresh, logout và reset mật khẩu |
| [User và permission](users-and-permissions.md) | API quản trị user, function/action, quyền truy cập và bootstrap admin |
| [Dữ liệu và migration](data-model-and-migrations.md) | Sáu entity/bảng thuộc AuthModule và quan hệ |
| [Cấu hình AuthModule](configuration.md) | JWT, Stringee, OTP local và biến môi trường |

## Phạm vi dữ liệu

Tài liệu AuthModule chỉ mô tả sáu entity được yêu cầu: `user`, `auth_sessions`, `auth_otps`, `permissions`, `permission_details` và `functions`. Không bao gồm các entity nghiệp vụ khác.
