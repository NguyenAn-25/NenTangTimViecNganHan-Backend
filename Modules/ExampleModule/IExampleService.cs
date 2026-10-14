using DotNetSkeleton.Modules.ExampleModule.Entities;

namespace DotNetSkeleton.Modules.ExampleModule;

public interface IExampleService
{
    Task<IEnumerable<Example>> GetAllAsync();
    Task<Example?> GetByIdAsync(int id);
    Task<Example> CreateAsync(string name);
}