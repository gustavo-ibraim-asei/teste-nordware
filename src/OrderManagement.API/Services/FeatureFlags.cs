namespace OrderManagement.API.Services;

public class FeatureFlags : IFeatureFlags
{
    private readonly Dictionary<string, bool> _features = new()
    {
        { "new-dashboard", false },
        { "real-time-updates", true },
        { "advanced-analytics", false },
        { "multitenancy", true }
    };

    public bool IsEnabled(string featureName)
    {
        return _features.TryGetValue(featureName, out bool enabled) && enabled;
    }

    public void SetFeature(string featureName, bool enabled)
    {
        _features[featureName] = enabled;
    }
}





