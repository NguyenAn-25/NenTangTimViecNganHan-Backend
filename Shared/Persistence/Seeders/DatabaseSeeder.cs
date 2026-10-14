namespace DotNetSkeleton.Shared.Persistence.Seeders;

using System.Diagnostics;
using DotNetSkeleton.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly IEnumerable<ISeeder> _seeders;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext context,
        IEnumerable<ISeeder> seeders,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _seeders = seeders;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        await _context.Database.MigrateAsync();

        _logger.LogInformation("🌱 ===== Database seeding started =====");

        var stopwatch = Stopwatch.StartNew();
        var totalSeeded = 0;

        foreach (var seeder in _seeders.OrderBy(s => s.Order))
        {
            var name = seeder.GetType().Name;

            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("⏳ Running {Seeder}...", name);

                var count = await seeder.SeedAsync(_context);
                totalSeeded += count;

                if (count == 0)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("⏭️ {Seeder}: data already exists, skipped", name);
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("✅ {Seeder}: seeded {Count} record(s)", name, count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ {Seeder} failed", name);
                throw;
            }
        }

        stopwatch.Stop();

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(
                "🎉 ===== Seeding finished in {Elapsed} ms, {Total} record(s) added =====",
                stopwatch.ElapsedMilliseconds, totalSeeded);
    }
}