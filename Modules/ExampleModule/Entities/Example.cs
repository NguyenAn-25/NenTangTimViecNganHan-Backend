namespace DotNetSkeleton.Modules.ExampleModule.Entities;

public class Example
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}