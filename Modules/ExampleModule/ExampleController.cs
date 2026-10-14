using DotNetSkeleton.Modules.ExampleModule.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DotNetSkeleton.Modules.ExampleModule;

[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    private readonly IExampleService _exampleService;

    // Inject IExampleService qua Constructor
    public ExampleController(IExampleService exampleService)
    {
        _exampleService = exampleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _exampleService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _exampleService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = $"Không tìm thấy Example có Id = {id}" });
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExampleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "Name không được để trống" });
        }

        var result = await _exampleService.CreateAsync(dto.Name);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}