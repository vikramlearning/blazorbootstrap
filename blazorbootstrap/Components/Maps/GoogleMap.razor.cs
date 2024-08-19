namespace BlazorBootstrap;

public partial class GoogleMap : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<GoogleMap>? objRef;

    private string? GoogleMapsJsFileUrl => $"https://maps.googleapis.com/maps/api/js?key={ApiKey}&libraries=maps,marker";

    #endregion

    #region Methods

    //public ValueTask SetCenter() { return ValueTask.CompletedTask; }

    //public ValueTask SetZoom() { return ValueTask.CompletedTask; }

    public ValueTask AddMarkerAsync(GoogleMapMarker marker)
    {
        JSRuntime.InvokeVoidAsync("window.blazorBootstrap.googlemaps.addMarker", Id, marker, objRef);
        return ValueTask.CompletedTask;
    }

    public ValueTask UpdateMarkersAsync(IEnumerable<GoogleMapMarker> markers)
    {
        JSRuntime.InvokeVoidAsync("window.blazorBootstrap.googlemaps.updateMarkers", Id, markers, objRef);
        return ValueTask.CompletedTask;
    }

    public ValueTask RefreshAsync() 
    {
        JSRuntime.InvokeVoidAsync("window.blazorBootstrap.googlemaps.initialize", Id, Zoom, Center, Markers, Clickable, objRef);
        return ValueTask.CompletedTask; 
    }

    private void OnScriptLoad()
    {
        Console.WriteLine($"OnScriptLoad called...");
        Task.Run(async () => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.googlemaps.initialize", Id, Zoom, Center, Markers, Clickable, objRef));
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

    [JSInvokable]
    public async Task OnMarkerClickJS(GoogleMapMarker marker)
    {
        if (OnMarkerClick.HasDelegate)
            await OnMarkerClick.InvokeAsync(marker);
    }

    [Parameter]
    public EventCallback<GoogleMapMarker> OnMarkerClick { get; set; }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the google maps API key.
    /// </summary>
    [Parameter]
    public string? ApiKey { get; set; }

    [Parameter]
    public bool Clickable { get; set; }

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

public record class GoogleMapCenter
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

public record class GoogleMapMarkerPosition
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Background { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BorderColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Glyph { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GlyphColor { get; set; }

    public double Scale { get; set; } = 1.0;

    public bool UseIconFonts { get; set; }
}

public class GoogleMapMarkerEventArgs : EventArgs
{
    #region Constructors

    public GoogleMapMarkerEventArgs(GoogleMapMarker marker)
    {
        Marker = marker;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the elementId.
    /// </summary>
    public GoogleMapMarker Marker { get; }

    #endregion
}
