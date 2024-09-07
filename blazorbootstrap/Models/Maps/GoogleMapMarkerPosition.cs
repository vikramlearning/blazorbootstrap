namespace BlazorBootstrap;

public record class GoogleMapMarkerPosition
{
    #region Constructors

    public GoogleMapMarkerPosition(double latitude, double longitude)
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
