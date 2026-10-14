namespace DotNetSkeleton.Modules.ExampleModule;

using DotNetSkeleton.Modules.ExampleModule.Entities;
using DotNetSkeleton.Shared.Persistence;
using Microsoft.EntityFrameworkCore;


public class ExampleService : IExampleService
{
    private readonly AppDbContext _context;

    // Inject AppDbContext qua Constructor
    public ExampleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Example>> GetAllAsync()
    {
        return await _context.Examples.ToListAsync();
    }

    public async Task<Example?> GetByIdAsync(int id)
    {
        return await _context.Examples.FindAsync(id);
    }

    public async Task<Example> CreateAsync(string name)
    {
        var example = new Example
        {
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        _context.Examples.Add(example);
        await _context.SaveChangesAsync();

        return example;
    }
}