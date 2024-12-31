namespace Locations.Service.Osrm.Settings;

public class OsrmApiSettings
{
    private const string DefaultBaseUrl = "http://router.project-osrm.org";
    private const int DefaultTimeoutInSeconds = 30;
    public string BaseUrl { get; set; } = DefaultBaseUrl;
    public int TimeoutInSeconds { get; set; } = DefaultTimeoutInSeconds;
}