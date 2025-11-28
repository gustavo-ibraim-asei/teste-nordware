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
public class PriceTablesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PriceTablesController> _logger;

    public PriceTablesController(IMediator mediator, ILogger<PriceTablesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as tabelas de preços
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PriceTableDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PriceTableDto>>> GetAll([FromQuery] bool? onlyActive, CancellationToken cancellationToken)
    {
        var query = new GetAllPriceTablesQuery { OnlyActive = onlyActive };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém uma tabela de preços por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PriceTableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceTableDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetPriceTableByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Tabela de preços com ID {id} não encontrada");

        return Ok(result);
    }

    /// <summary>
    /// Cria uma nova tabela de preços
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PriceTableDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriceTableDto>> Create([FromBody] CreatePriceTableDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreatePriceTableCommand { PriceTable = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza uma tabela de preços
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PriceTableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriceTableDto>> Update(int id, [FromBody] UpdatePriceTableDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdatePriceTableCommand { Id = id, PriceTable = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Exclui uma tabela de preços
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeletePriceTableCommand { Id = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

