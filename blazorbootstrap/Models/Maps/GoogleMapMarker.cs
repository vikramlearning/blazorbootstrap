namespace BlazorBootstrap;

public class GoogleMapMarker
{
    #region Properties, Indexers

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public string? Content { get; set; }

    public PinElement? PinElement { get; set; }

    public GoogleMapMarkerPosition? Position { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public string? Title { get; set; }

    /// <summary>
    /// Variable for disabling info windows on the maps
    /// </summary>
    public bool DisableInfoWindow { get; set; } = false;

    #endregion
}
