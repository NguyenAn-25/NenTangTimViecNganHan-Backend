namespace DotNetSkeleton.Shared.Persistence.Seeders;

using DotNetSkeleton.Modules.ExampleModule.Entities;
using Microsoft.EntityFrameworkCore;

public class ExampleSeeder : ISeeder
{
    public int Order => 1;

    public async Task<int> SeedAsync(AppDbContext context)
    {
        var names = new[] { "Example 1", "Example 2", "Example 3", "Example 4" };

        // Lấy các Name đã tồn tại trong DB
        var existingNames = await context.Examples
            .Where(e => names.Contains(e.Name))
            .Select(e => e.Name)
            .ToListAsync();

        // Chỉ thêm những cái chưa có
        var newExamples = names
            .Except(existingNames)
            .Select(n => new Example { Name = n })
            .ToList();

        // đã có đủ dữ liệu
        if (newExamples.Count == 0)
            return 0;

        await context.Examples.AddRangeAsync(newExamples);
        await context.SaveChangesAsync();

        return newExamples.Count;
    }
}