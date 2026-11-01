# Quản lý người dùng và phân quyền

## API quản lý user

Các endpoint nằm dưới `/api/v1/users`. Chúng yêu cầu access token hợp lệ và permission tương ứng.

Response JSON dùng envelope chung và đặt DTO bên trong `data`; tạo tài khoản trả HTTP `201 Created` cùng Location. Xem [Quy ước API](../api-conventions.md) để biết cấu trúc response.

| Method và route | Permission | Tác dụng |
|---|---|---|
| `GET /api/v1/users?page=1&pageSize=20&search=...` | `users:read` | Lọc toàn bộ user, trả summary |
| `GET /api/v1/users/{id}` | `users:read` | Xem chi tiết user theo ID |
| `POST /api/v1/users` | `users:create` | Tạo tài khoản ADMIN bằng phone, password và permission |
| `PUT /api/v1/users/{id}` | `users:update` | Đổi permission profile của ADMIN |
| `PUT /api/v1/users/{id}/password` | `users:update` | Đổi password của user và thu hồi session hiện có |
| `PATCH /api/v1/users/{id}/status` | `users:update` | Bật hoặc tắt user |

Các endpoint là công cụ quản trị, yêu cầu access token hợp lệ cùng permission tương ứng. Danh sách và xem chi tiết bao gồm cả tài khoản `USER` đăng ký công khai lẫn tài khoản `ADMIN`. Tạo mới qua API quản trị luôn tạo `ADMIN`. Danh sách mặc định bắt đầu ở trang 1, 20 dòng/trang; `pageSize` tối đa 100. API trả `PagedUsersResponseDto` gồm summary `items`, `page`, `pageSize` và `totalCount`.

### Tiêu chí lọc danh sách

Có thể kết hợp các query parameter sau:

| Parameter | Ý nghĩa |
|---|---|
| `search` | Tìm chuỗi con trong số điện thoại |
| `role` | Lọc theo role (`USER` hoặc `ADMIN`) |
| `isActive` | Lọc trạng thái bật/tắt (`true`/`false`) |
| `isVerified` | Lọc trạng thái xác minh (`true`/`false`) |
| `permissionId` | Lọc user được gán permission profile này |
| `createdFrom` | Lọc từ thời điểm tạo trở đi, ISO 8601, inclusive |
| `createdTo` | Lọc đến thời điểm tạo, ISO 8601, inclusive |
| `page`, `pageSize` | Phân trang; mặc định 1/20, tối đa 100 dòng/trang |

Ví dụ: `GET /api/v1/users?role=USER&isActive=true&isVerified=true&search=1202&page=1&pageSize=20`.

### Tạo user

Admin tạo tài khoản quản trị trực tiếp bằng số điện thoại, mật khẩu ban đầu và permission profile đang hoạt động. Role luôn là `ADMIN`, nên request không nhận `role`. Số điện thoại phải theo E.164 và chưa được tài khoản nào sử dụng (kể cả tài khoản `USER` đăng ký công khai). Mật khẩu dài 8–128 ký tự, có chữ in hoa, chữ số và ký tự đặc biệt. Tài khoản được tạo active và verified; luồng tạo bởi admin không dùng OTP.

```json
{
  "phone": "+84901234567",
  "password": "StrongPass@123",
  "permissionId": 2
}
```

Số điện thoại được kiểm tra trước khi tạo và unique constraint của database cũng được xử lý để tránh trùng khi có hai request đồng thời. Tạo thành công trả `201 Created` cùng detail DTO và `Location` trỏ đến tài khoản vừa tạo.

### Cập nhật user

`PUT /api/v1/users/{id}` chỉ thay đổi permission profile. `{id}` là ID nội bộ dạng số. Chỉ có thể chọn profile đang active:

```json
{
  "permissionId": 2
}
```

Phone và role không được cập nhật qua endpoint này. Chỉ tài khoản role `ADMIN` có permission profile; gửi yêu cầu đổi permission cho tài khoản `USER` sẽ nhận lỗi 400. Khi permission thay đổi, các session đang hoạt động bị revoke để người dùng đăng nhập lại và nhận claim mới.

### Đổi password

`PUT /api/v1/users/{id}/password` nhận password mới theo cùng quy tắc mạnh như đăng ký. Hệ thống lưu password hash, không trả password trong response và thu hồi mọi session hiện có. Thành công trả `204 No Content`.

```json
{
  "newPassword": "NewStrongPass@456"
}
```

### Bật hoặc tắt tài khoản

`PATCH /api/v1/users/{id}/status` nhận:

```json
{
  "isActive": false
}
```

Tắt user sẽ revoke các session chưa revoke. Chỉ bật lại tài khoản đã verified; tài khoản chờ xác minh phải hoàn tất đăng ký trước. Bật lại user không tự khôi phục session cũ; người dùng đăng nhập lại để tạo session mới.

## Mô hình permission

Ba khái niệm được ghép lại như sau:

- `functions` khai báo nhóm chức năng mà API thực sự bảo vệ, tên tiếng Việt/tiếng Anh và các `actions` được hỗ trợ.
- `user.role` là role của tài khoản, chỉ nhận `USER` hoặc `ADMIN`.
- `permissions` là các permission profile chứa tập function/action; chỉ user `ADMIN` mới được gắn profile qua `user.permission_id`.
- `permission_details` là danh sách action được cấp cho từng permission profile.

`USER` phải có `permission_id = NULL`; `ADMIN` phải tham chiếu một permission profile đang hoạt động. Database có check constraint và service cũng kiểm tra quy tắc này. Một permission profile có thể gắn cho nhiều admin; chỉnh `permission_details` sẽ cập nhật quyền của tất cả admin đang dùng profile đó. Profile mặc định là `ADMIN_DEFAULT`; có thể tạo thêm profile với các grant khác nhau.

Policy được viết theo dạng:

```csharp
[Authorize(Policy = "permission:users:read")]
```

Policy provider tách `users` thành function và `read` thành action. JWT chứa role claim `ADMIN`/`USER`; claim `permission` chỉ được thêm cho user `ADMIN` có permission profile đang hoạt động. User `USER` không nhận claim phân quyền động. Authorization handler so khớp claim, ví dụ `users:read`.

Claim được chụp vào JWT lúc tạo session. Nếu thay đổi grant trực tiếp trong PostgreSQL, hãy revoke các session của admin để buộc đăng nhập lại; nếu không, quyền cũ có thể còn trong access token đến khi token hết hạn. Hiện project chưa có API quản lý `permissions`/`permission_details`, nên việc chỉnh quyền cần làm qua thao tác quản trị database có kiểm soát.

## Catalog mặc định

Seeder và migration khai báo đúng function/action đang được dùng bởi các endpoint quản lý user:

| Function | Actions |
|---|---|
| `users` | `read`, `create`, `update` |

Profile `ADMIN_DEFAULT` khởi tạo với ba grant `users:read`, `users:create`, `users:update` để dùng được các API hiện có. Seeder chỉ thêm bộ grant ban đầu khi tạo profile này lần đầu; những lần khởi động sau không tự khôi phục grant đã bị gỡ. Có thể tạo permission profile khác rồi cấp action phù hợp qua `permission_details`.

### Tạo tài khoản quản trị ban đầu

Migration và `AuthPermissionCatalogSeeder` tạo profile `ADMIN_DEFAULT`. Trong Development, `AuthDevelopmentAdminAccountsSeeder` tự tạo hai tài khoản ADMIN giả lập để thử local: `+12025550190` và `+12025550191`, cùng mật khẩu `123456` như user mẫu. Seeder không nâng quyền tài khoản có sẵn; nếu fixture admin đã được tạo với mật khẩu cũ `Admin@123456`, seeder sẽ cập nhật về `123456`, còn số điện thoại thuộc cấu hình tài khoản khác thì báo lỗi. Có thể tạo thêm một admin với số điện thoại/mật khẩu riêng bằng `Seed:AdminPhone` và `Seed:AdminPassword` trong User Secrets. Tất cả tài khoản được seed đều active, có profile `ADMIN_DEFAULT` và bỏ qua xác minh OTP; chỉ dùng thông tin fixture trong local Development, không dùng chúng ở Production.

Trong Development, `AuthDevelopmentSampleDataSeeder` tạo 24 user mẫu: 16 active, 4 chờ xác minh và 4 đã xác minh nhưng bị tắt. Tất cả có password `123456`; 16 tài khoản active có thể dùng để thử đăng nhập. Tám tài khoản còn lại không đăng nhập được vì `is_active = false`. Seeder thêm 44 OTP và 40 session lịch sử; tất cả đều hết hạn, session không có refresh token dùng được. Số điện thoại giả lập nằm trong dải `+12025550100`–`+12025550123`.

Nếu thực hiện bootstrap trực tiếp bằng PostgreSQL, chỉ người vận hành có quyền truy cập DB mới nên chạy câu lệnh tương đương sau, thay UUID bằng `public_id` của tài khoản đã xác minh:

```sql
BEGIN;
UPDATE "user"
SET role = 'ADMIN',
    permission_id = (SELECT id FROM permissions WHERE name = 'ADMIN_DEFAULT' AND status = 'ACTIVE')
WHERE public_id = '00000000-0000-0000-0000-000000000000'
  AND is_active = true
  AND is_verified = true;
UPDATE auth_sessions
SET revoked_at = now()
WHERE user_id = (
    SELECT id FROM "user"
    WHERE public_id = '00000000-0000-0000-0000-000000000000'
)
  AND revoked_at IS NULL;
COMMIT;
```

Sau khi đổi role trực tiếp trong database, cần revoke session như trong ví dụ rồi người dùng đăng nhập lại để JWT mới nhận permission claim tương ứng. Nếu bỏ qua bước revoke, JWT cũ vẫn giữ claim đã được phát trước đó.

## Response và quyền riêng tư

Summary khi xem danh sách gồm `id`, `publicId`, phone, `isActive`, `isVerified`, `createdAt`, role và object `permission` (`id`, `name`); permission là `null` với role `USER`. Detail thêm `verifiedAt`, `lastActiveAt` và object permission đầy đủ hơn (`id`, `name`, `description`, `status`). Permission được trả chung trong object, không tách `permissionId` và tên profile ở cấp user. Các trường password/hash không được trả trong DTO. Role và permission status được trả bằng nhãn tiếng Việt; trong PostgreSQL lần lượt lưu `USER`/`ADMIN` và `ACTIVE`/`INACTIVE`.
