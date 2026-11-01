# Cấu hình và chạy hệ thống

Tài liệu này nói về môi trường chạy chung và PostgreSQL. Secret riêng của xác thực/SMS nằm trong [Cấu hình AuthModule](auth-module/configuration.md).

## Yêu cầu

- .NET SDK 10.x; project target `net10.0`.
- PostgreSQL.
- Terminal tại thư mục có `DotNetSkeleton.csproj`.

```bash
dotnet --version
dotnet --list-sdks
```

## Cấu hình .NET

ASP.NET Core nạp cấu hình từ các nguồn mặc định: `appsettings.json`, `appsettings.{Environment}.json`, User Secrets trong Development, environment variables và arguments dòng lệnh. Cấu hình cấp sau ghi đè cấu hình cấp trước.

| File | Vai trò |
|---|---|
| `appsettings.json` | Log level, `AllowedHosts`, issuer/audience và thời lượng token mặc định |
| `appsettings.Development.json` | Connection string và signing key mặc định chỉ dành cho local, cùng log level Development |
| `appsettings.Production.json` | Tùy chọn cấu hình Production; hiện chủ yếu dùng cấu hình mặc định |
| `Properties/launchSettings.json` | Profile local `http`/`https`, URL và môi trường Development |

Không đặt credential Production vào JSON được commit. Project đã có `UserSecretsId`; User Secrets hoặc biến môi trường sẽ ghi đè giá trị local trong appsettings.

## PostgreSQL local

Khởi động PostgreSQL bằng Docker Compose:

```bash
docker compose up -d
```

Compose tạo database `mphqa_database` trong container cùng tên và lưu dữ liệu ở volume `mphqa_postgres_data`. `appsettings.Development.json` kết nối đến `localhost:5432` bằng user `postgres`, password `postgres`. Nếu máy local dùng thông tin khác, ghi đè bằng User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=mphqa_database;Username=postgres;Password=your_password"
```

Tên cấu hình chính xác là `ConnectionStrings:Default`. `AddDatabase()` dùng cấu hình này để gọi `UseNpgsql` và dừng khởi động với thông báo rõ ràng nếu chưa được cấu hình. Signing key Development chỉ dành cho local; Production phải dùng key riêng từ secret manager hoặc biến môi trường. Nếu đổi tên connection string key, phải cập nhật nơi đăng ký database và factory cho EF CLI.

## Khôi phục, build và chạy

```bash
dotnet restore
dotnet tool restore
dotnet build
dotnet run --launch-profile http
```

Profile `http` dùng `http://localhost:5213`; profile `https` dùng `https://localhost:7215` và `http://localhost:5213`. Cả hai profile hiện đặt `ASPNETCORE_ENVIRONMENT=Development`.

Mở Swagger:

```text
http://localhost:5213/swagger
```

Swagger chỉ được bật trong Development. Nếu dùng HTTPS local và chứng chỉ chưa trust:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Trong Development startup sẽ áp dụng migrations và chạy các seeder đã đăng ký. Xem [Cơ sở dữ liệu và migration](persistence-and-migrations.md).

## Biến môi trường cho triển khai

Nested keys dùng `__`:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__Default="Host=db-host;Port=5432;Database=dotnet_skeleton;Username=app_user;Password=..."
```

Đặt thông tin xác thực và key riêng qua secret manager của nền tảng. AuthModule cần `Jwt:SigningKey`; Production còn yêu cầu Stringee đầy đủ. Xem [Cấu hình AuthModule](auth-module/configuration.md).

## Endpoint tiện ích

| Route | Mục đích |
|---|---|
| `GET /` | Kiểm tra tiến trình API đang trả lời |
| `GET /test-db` | Mở kết nối PostgreSQL để kiểm tra cấu hình |

Hai endpoint trả về `ApiResponse<T>` như các controller; lỗi ngoài dự kiến đi qua exception handler chung.

## Logging và kiểm tra lỗi

Log level mặc định là `Information`; log từ ASP.NET Core giảm xuống `Warning`. Development cũng hạ log EF Core/SQL command. Khi API trả lỗi, dùng `traceId` trong response để tìm log tương ứng.

Nếu database không kết nối được, kiểm tra PostgreSQL đang chạy, database đã tạo và connection string đang được nạp từ đúng môi trường/secret store.
