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
public class StockOfficesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StockOfficesController> _logger;

    public StockOfficesController(IMediator mediator, ILogger<StockOfficesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as filiais de estoque
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<StockOfficeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StockOfficeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllStockOfficesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém uma filial por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StockOfficeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockOfficeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetStockOfficeByIdQuery { Id = id };
        var stockOffice = await _mediator.Send(query, cancellationToken);

        if (stockOffice == null)
            return NotFound($"Filial com ID {id} não encontrada");

        return Ok(stockOffice);
    }

    /// <summary>
    /// Cria uma nova filial
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StockOfficeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StockOfficeDto>> Create([FromBody] CreateStockOfficeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateStockOfficeCommand { StockOffice = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza uma filial
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(StockOfficeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StockOfficeDto>> Update(int id, [FromBody] UpdateStockOfficeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateStockOfficeCommand { Id = id, StockOffice = dto };
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

    /// <summary>
    /// Deleta uma filial
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteStockOfficeCommand { Id = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

