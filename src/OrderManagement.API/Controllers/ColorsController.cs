using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Commands;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ColorsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ColorsController> _logger;

    public ColorsController(IMediator mediator, ILogger<ColorsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ColorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ColorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllColorsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ColorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ColorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetColorByIdQuery { Id = id };
        var color = await _mediator.Send(query, cancellationToken);

        if (color == null)
            return NotFound($"Cor com ID {id} não encontrada");

        return Ok(color);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ColorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ColorDto>> Create([FromBody] CreateColorDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateColorCommand { Color = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ColorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ColorDto>> Update(int id, [FromBody] UpdateColorDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateColorCommand { Id = id, Color = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteColorCommand { Id = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

