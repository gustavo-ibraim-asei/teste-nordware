using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderManagement.Infrastructure.ExternalServices;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CepController : ControllerBase
{
    private readonly ViaCepService _viaCepService;
    private readonly ILogger<CepController> _logger;

    public CepController(ViaCepService viaCepService, ILogger<CepController> logger)
    {
        _viaCepService = viaCepService;
        _logger = logger;
    }

    /// <summary>
    /// Consulta endereço por CEP usando ViaCEP
    /// </summary>
    [HttpGet("{zipCode}")]
    [ProducesResponseType(typeof(ViaCepResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ViaCepResponse>> GetAddressByZipCode(string zipCode, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Consultando CEP: {ZipCode}", zipCode);

            ViaCepResponse? address = await _viaCepService.GetAddressByZipCodeAsync(zipCode, cancellationToken);

            if (address == null)
            {
                _logger.LogWarning("CEP não encontrado: {ZipCode}", zipCode);
                return NotFound(new { message = $"CEP {zipCode} não encontrado" });
            }

            return Ok(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar CEP {ZipCode}", zipCode);
            return BadRequest(new { message = $"Erro ao consultar CEP: {ex.Message}" });
        }
    }
}



