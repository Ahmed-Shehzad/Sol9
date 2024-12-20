namespace Locations.Service.Nominatim.Settings;

public class NominatimApiSettings
{
    private const string DefaultBaseUrl = "https://nominatim.openstreetmap.org";
    private const int DefaultTimeoutInSeconds = 30;
    public string BaseUrl { get; set; } = DefaultBaseUrl;
    public int TimeoutInSeconds { get; set; } = DefaultTimeoutInSeconds;
}