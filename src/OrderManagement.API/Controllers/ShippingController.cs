using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Services;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShippingController : ControllerBase
{
    private readonly IShippingCalculationService _shippingService;
    private readonly IMapper _mapper;
    private readonly ILogger<ShippingController> _logger;

    public ShippingController(IShippingCalculationService shippingService, IMapper mapper, ILogger<ShippingController> logger)
    {
        _shippingService = shippingService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Calcula opções de frete disponíveis
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(ShippingCalculationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShippingCalculationResponseDto>> CalculateShipping([FromBody] ShippingCalculationRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculando frete para CEP: {ZipCode}, valor do pedido: {OrderTotal}", request.ZipCode, request.OrderTotal);

        try
        {
            List<Domain.ValueObjects.ShippingOption> shippingOptions = await _shippingService.CalculateShippingOptionsAsync(request.ZipCode, request.OrderTotal, request.TotalWeight, cancellationToken);

            ShippingCalculationResponseDto response = new ShippingCalculationResponseDto
            {
                ZipCode = request.ZipCode,
                OrderTotal = request.OrderTotal,
                Options = _mapper.Map<List<ShippingOptionDto>>(shippingOptions)
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Calcula frete para um pedido específico
    /// </summary>
    [HttpGet("calculate/{orderId}")]
    [ProducesResponseType(typeof(ShippingCalculationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShippingCalculationResponseDto>> CalculateShippingForOrder(int orderId, CancellationToken cancellationToken)
    {
        // Este endpoint seria implementado para calcular frete baseado em um pedido existente
        // Por enquanto, retorna not implemented
        return StatusCode(501, new { message = "Not implemented yet" });
    }
}

