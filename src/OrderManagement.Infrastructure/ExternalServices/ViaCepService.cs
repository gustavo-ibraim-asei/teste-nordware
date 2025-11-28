using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace OrderManagement.Infrastructure.ExternalServices;

public class ViaCepService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepService> _logger;

    public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ViaCepResponse?> GetAddressByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        try
        {
            string cleanZipCode = zipCode.Replace("-", "");
            // Polly policies are applied at HttpClient level in Program.cs
            ViaCepResponse? response = await _httpClient.GetFromJsonAsync<ViaCepResponse>(
                $"https://viacep.com.br/ws/{cleanZipCode}/json/",
                cancellationToken);

            if (response?.Erro == true)
            {
                _logger.LogWarning("CEP não encontrado: {ZipCode}", zipCode);
                return null;
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar endereço do ViaCEP para CEP: {ZipCode}", zipCode);
            return null;
        }
    }
}

public class ViaCepResponse
{
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Localidade { get; set; }
    public string? Uf { get; set; }
    public bool Erro { get; set; }
}

