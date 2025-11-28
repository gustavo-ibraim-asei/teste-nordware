namespace OrderManagement.API.Services;

public interface IFeatureFlags
{
    bool IsEnabled(string featureName);
    void SetFeature(string featureName, bool enabled);
}


