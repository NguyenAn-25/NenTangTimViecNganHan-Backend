namespace DotNetSkeleton.Shared.Persistence.Seeders;
public interface ISeeder
{
    // Số nhỏ chạy trước, dùng khi có khoá ngoại (vd: Category phải seed trước Product)
    int Order { get; }

    Task<int> SeedAsync(AppDbContext context);
}