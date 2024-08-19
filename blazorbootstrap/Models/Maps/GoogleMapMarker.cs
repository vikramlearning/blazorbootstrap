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

    #endregion
}
