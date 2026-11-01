using DotNetSkeleton.Modules.ExampleModule.DTOs.Requests;
using DotNetSkeleton.Modules.ExampleModule.Entities;
using DotNetSkeleton.Modules.ExampleModule;
using DotNetSkeleton.Modules.ExampleModule.Exceptions;
using DotNetSkeleton.Shared.Api;
using DotNetSkeleton.Shared.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DotNetSkeleton.Modules.ExampleModule.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExampleController(IExampleService exampleService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IEnumerable<Example>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Example>>>> GetAll() =>
        OkResponse(await exampleService.GetAllAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<Example>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Example>>> GetById(int id)
    {
        var result = await exampleService.GetByIdAsync(id);
        if (result == null)
            throw new ExampleNotFoundByIdException(id);

        return OkResponse(result);
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<Example>>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<Example>>> Create([FromBody] CreateExampleDto dto)
    {
        var result = await exampleService.CreateAsync(dto.Name);
        return CreatedAtActionResponse(nameof(GetById), new { id = result.Id }, result);
    }
}
