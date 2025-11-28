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
public class ProductPricesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductPricesController> _logger;

    public ProductPricesController(IMediator mediator, ILogger<ProductPricesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os preços de produtos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductPriceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductPriceDto>>> GetAll([FromQuery] int? productId, [FromQuery] int? priceTableId, CancellationToken cancellationToken)
    {
        var query = new GetAllProductPricesQuery { ProductId = productId, PriceTableId = priceTableId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém um preço de produto por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetProductPriceByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Preço do produto com ID {id} não encontrado");

        return Ok(result);
    }

    /// <summary>
    /// Obtém o preço de um produto em uma tabela de preços específica
    /// </summary>
    [HttpGet("product/{productId}/pricetable/{priceTableId}")]
    [ProducesResponseType(typeof(ProductPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceDto>> GetByProductAndPriceTable(int productId, int priceTableId, CancellationToken cancellationToken)
    {
        var query = new GetAllProductPricesQuery { ProductId = productId, PriceTableId = priceTableId };
        var results = await _mediator.Send(query, cancellationToken);
        
        var result = results.FirstOrDefault();
        if (result == null)
            return NotFound($"Preço não encontrado para o produto {productId} na tabela de preços {priceTableId}");

        return Ok(result);
    }

    /// <summary>
    /// Cria um novo preço de produto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductPriceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductPriceDto>> Create([FromBody] CreateProductPriceDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateProductPriceCommand { ProductPrice = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um preço de produto
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductPriceDto>> Update(int id, [FromBody] UpdateProductPriceDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateProductPriceCommand { Id = id, ProductPrice = dto };
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Exclui um preço de produto
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteProductPriceCommand { Id = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

