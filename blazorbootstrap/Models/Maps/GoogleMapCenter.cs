namespace BlazorBootstrap;

public record class GoogleMapCenter
{
    #region Constructors

    public GoogleMapCenter(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    #endregion

    #region Properties, Indexers

    [JsonPropertyName("lat")] 
    public double Latitude { get; }

    [JsonPropertyName("lng")] 
    public double Longitude { get; }

    #endregion
}
