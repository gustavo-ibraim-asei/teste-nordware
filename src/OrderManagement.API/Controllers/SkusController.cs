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
public class SkusController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SkusController> _logger;

    public SkusController(IMediator mediator, ILogger<SkusController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SkuDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SkuDto>>> GetAll([FromQuery] int? productId, CancellationToken cancellationToken)
    {
        var query = new GetAllSkusQuery { ProductId = productId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SkuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkuDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetSkuByIdQuery { Id = id };
        var sku = await _mediator.Send(query, cancellationToken);

        if (sku == null)
            return NotFound($"SKU com ID {id} não encontrado");

        return Ok(sku);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SkuDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkuDto>> Create([FromBody] CreateSkuDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateSkuCommand { Sku = dto };
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SkuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkuDto>> Update(int id, [FromBody] UpdateSkuDto dto, CancellationToken cancellationToken)
    {
        var command = new UpdateSkuCommand { Id = id, Sku = dto };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteSkuCommand { Id = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retorna SKUs que possuem estoque disponível
    /// </summary>
    [HttpGet("with-stock")]
    [ProducesResponseType(typeof(List<SkuWithStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SkuWithStockDto>>> GetSkusWithStock(CancellationToken cancellationToken)
    {
        var query = new GetSkusWithStockQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

