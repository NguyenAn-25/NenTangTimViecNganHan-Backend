using DotNetSkeleton.Modules.ExampleModule.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Shared.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    public DbSet<Example> Examples { get; set; }
}