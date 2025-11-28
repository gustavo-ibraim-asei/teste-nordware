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
public class StocksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StocksController> _logger;

    public StocksController(IMediator mediator, ILogger<StocksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<StockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StockDto>>> GetAll([FromQuery] int? skuId, [FromQuery] int? stockOfficeId, CancellationToken cancellationToken)
    {
        var query = new GetAllStocksQuery { SkuId = skuId, StockOfficeId = stockOfficeId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetStockByIdQuery { Id = id };
        var stock = await _mediator.Send(query, cancellationToken);

        if (stock == null)
            return NotFound($"Estoque com ID {id} não encontrado");

        return Ok(stock);
    }

    [HttpPost]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StockDto>> Create([FromBody] CreateStockDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateStockCommand { Stock = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StockDto>> Update(int id, [FromBody] UpdateStockDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateStockCommand { Id = id, Stock = dto };
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
            var command = new DeleteStockCommand { Id = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reserve")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StockDto>> ReserveStock(int id, [FromBody] ReserveStockDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ReserveStockCommand { StockId = id, Reserve = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/decrease")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StockDto>> DecreaseStock(int id, [FromBody] DecreaseStockDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DecreaseStockCommand { StockId = id, Decrease = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

