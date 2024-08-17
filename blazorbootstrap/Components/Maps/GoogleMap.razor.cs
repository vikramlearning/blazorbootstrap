namespace BlazorBootstrap;

public partial class GoogleMap : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<GoogleMap>? objRef;

    private string? GoogleMapsJsFileUrl => $"https://maps.googleapis.com/maps/api/js?key={ApiKey}&libraries=maps,marker";

    #endregion

    #region Methods

    private void OnScriptLoad()
    {
        Console.WriteLine($"OnScriptLoad called...");
        Task.Run(async () => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.googlemaps.initialize", Id, Zoom, Center, Markers));
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            try
            {
                //if (IsRenderComplete)
                //    await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.alert.dispose", Id);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the google maps API key.
    /// </summary>
    [Parameter]
    public string? ApiKey { get; set; }

    [Parameter]
    public GoogleMapCenter Center { get; set; } = default!;

    /// <summary>
    /// Gets or sets the <see cref="GoogleMap" /> height.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public double? Height { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Height" /> units.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Unit.Px" />.
    /// </remarks>
    [Parameter]
    public Unit HeightUnit { get; set; } = Unit.Px;

    /// <summary>
    /// Gets or sets the <see cref="GoogleMap" /> width.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public double? Width { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Width" /> units.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Unit.Percentage" />.
    /// </remarks>
    [Parameter]
    public Unit WidthUnit { get; set; } = Unit.Percentage;

    /// <summary>
    /// Gets or sets the <see cref="GoogleMap" /> zoom level.
    /// </summary>
    /// <remarks>
    /// Default value is 14.
    /// </remarks>
    [Parameter]
    public int Zoom { get; set; } = 14;

    protected override string? StyleNames
        => BuildStyleNames(
            Style,
            ($"width:{Width!.Value.ToString(CultureInfo.InvariantCulture)}{WidthUnit.ToCssString()}", Width is not null && Width.Value > 0),
            ($"height:{Height!.Value.ToString(CultureInfo.InvariantCulture)}{HeightUnit.ToCssString()}", Height is not null && Height.Value > 0));

    [Parameter]
    public IEnumerable<GoogleMapMarker>? Markers { get; set; }

    #endregion
}

public class GoogleMapCenter
{
    public GoogleMapCenter(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    [JsonPropertyName("lat")]
    public double Latitude { get; }

    [JsonPropertyName("lng")]
    public double Longitude { get; }
}

public class GoogleMapMarker
{

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Content { get; set; }

    public PinElement? PinElement { get; set; }

    public GoogleMapMarkerPosition? Position { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }
}

public class GoogleMapMarkerPosition
{
    public GoogleMapMarkerPosition(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    [JsonPropertyName("lat")]
    public double Latitude { get; }

    [JsonPropertyName("lng")]
    public double Longitude { get; }
}


public class PinElement
{
    public double Scale { get; set; } = 1.0;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Background { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BorderColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GlyphColor { get; set; }
}
