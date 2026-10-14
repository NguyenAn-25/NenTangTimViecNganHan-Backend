using Microsoft.EntityFrameworkCore;
using Npgsql;
using DotNetSkeleton.Shared.Configurations;
using DotNetSkeleton.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services
    .AddSwaggerDocs()
    .AddDatabase(builder.Configuration);

builder.Services.AddSeeders();

builder.Services.AddApplicationServices();

var app = builder.Build();

//config swagger
app.UseSwaggerDocs();

app.UseHttpsRedirection();

app.MapControllers();

//config seeder
if (app.Environment.IsDevelopment())
{
    await app.SeedDatabaseAsync();
}

app.MapGet("/", async (IConfiguration config) =>
{
    return Results.Ok("Chào, tôi là dotnet");
});

app.MapGet("/test-db", async (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("Default");
    try
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        return Results.Ok("Kết nối Postgres thành công!");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Lỗi: {ex.GetType().Name} - {ex.Message}");
    }
});

app.Run();


// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast = Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
