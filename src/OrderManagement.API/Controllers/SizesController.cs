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
public class SizesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SizesController> _logger;

    public SizesController(IMediator mediator, ILogger<SizesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SizeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SizeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllSizesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SizeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SizeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetSizeByIdQuery { Id = id };
        var size = await _mediator.Send(query, cancellationToken);

        if (size == null)
            return NotFound($"Tamanho com ID {id} não encontrado");

        return Ok(size);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SizeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SizeDto>> Create([FromBody] CreateSizeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateSizeCommand { Size = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SizeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SizeDto>> Update(int id, [FromBody] UpdateSizeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateSizeCommand { Id = id, Size = dto };
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
            var command = new DeleteSizeCommand { Id = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

