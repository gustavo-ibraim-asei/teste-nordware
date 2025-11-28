using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderManagement.API.Services;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlags _featureFlags;
    private readonly ILogger<FeatureFlagsController> _logger;

    public FeatureFlagsController(IFeatureFlags featureFlags, ILogger<FeatureFlagsController> logger)
    {
        _featureFlags = featureFlags;
        _logger = logger;
    }

    [HttpGet("{featureName}")]
    public IActionResult GetFeature(string featureName)
    {
        return Ok(new { feature = featureName, enabled = _featureFlags.IsEnabled(featureName) });
    }

    [HttpPost("{featureName}")]
    public IActionResult SetFeature(string featureName, [FromBody] bool enabled)
    {
        _featureFlags.SetFeature(featureName, enabled);
        _logger.LogInformation("Feature flag {FeatureName} definida para {Enabled}", featureName, enabled);
        return Ok(new { feature = featureName, enabled });
    }
}

