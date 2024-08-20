namespace BlazorBootstrap;

public partial class GoogleMap : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private DotNetObjectReference<GoogleMap>? objRef;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Adds a marker to the GoogleMap.
    /// </summary>
    /// <param name="marker">The marker to add to the map.</param>
    /// <returns>A completed task.</returns>
    public ValueTask AddMarkerAsync(GoogleMapMarker marker)
    {
        JSRuntime.InvokeVoidAsync("window.blazorBootstrap.googlemaps.addMarker", Id, marker, objRef);

        return ValueTask.CompletedTask;
    }

    [JSInvokable]
    public async Task OnMarkerClickJS(GoogleMapMarker marker)
    {
        if (OnMarkerClick.HasDelegate)
            await OnMarkerClick.InvokeAsync(marker);
    }

    /// <summary>
    /// Refreshes the Google Maps component.
    /// </summary>
    /// <returns>A completed task.</returns>
    public ValueTask RefreshAsync()
    {
        JSRuntime.InvokeVoidAsync("window.blazorBootstrap.googlemaps.initialize", Id, Zoom, Center, Markers, Clickable, objRef);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Updates the markers on the Google Map.
    /// </summary>
    /// <returns>A completed task.</returns>
    public ValueTask UpdateMarkersAsync(IEnumerable<GoogleMapMarker> markers)
    {
        JSRuntime.InvokeVoidAsync("window.blazorBootstrap.googlemaps.updateMarkers", Id, markers, objRef);

        return ValueTask.CompletedTask;
    }

    private void OnScriptLoad()
    {
        Task.Run(async () => await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.googlemaps.initialize", Id, Zoom, Center, Markers, Clickable, objRef));
    }

    #endregion

    #region Properties, Indexers

    protected override string? StyleNames =>
        BuildStyleNames(
            Style,
            ($"width:{Width!.Value.ToString(CultureInfo.InvariantCulture)}{WidthUnit.ToCssString()}", Width is not null && Width.Value > 0),
            ($"height:{Height!.Value.ToString(CultureInfo.InvariantCulture)}{HeightUnit.ToCssString()}", Height is not null && Height.Value > 0)
        );

    /// <summary>
    /// Gets or sets the Google Maps API key.
    /// </summary>
    [Parameter]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the center parameter.
    /// </summary>
    [Parameter] 
    public GoogleMapCenter Center { get; set; } = default!;

    /// <summary>
    /// Makes the marker clickable if set to <see langword="true" />.
    /// </summary>
    [Parameter] 
    public bool Clickable { get; set; }

    private string? GoogleMapsJsFileUrl => $"https://maps.googleapis.com/maps/api/js?key={ApiKey}&libraries=maps,marker";

    /// <summary>
    /// Gets or sets the height of the <see cref="GoogleMap" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public double? Height { get; set; }

    /// <summary>
    /// Gets or sets the units for the <see cref="Height" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Unit.Px" />.
    /// </remarks>
    [Parameter]
    public Unit HeightUnit { get; set; } = Unit.Px;

    /// <summary>
    /// Gets or sets the markers.
    /// </summary>
    [Parameter] 
    public IEnumerable<GoogleMapMarker>? Markers { get; set; }

    /// <summary>
    /// Event fired when a user clicks on a marker.
    /// This event fires only when <see cref="Clickable" /> is set to <see langword="true" />.
    /// </summary>
    [Parameter] 
    public EventCallback<GoogleMapMarker> OnMarkerClick { get; set; }

    /// <summary>
    /// Gets or sets the width of the <see cref="GoogleMap" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public double? Width { get; set; }

    /// <summary>
    /// Gets or sets the units for the <see cref="Width" />.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Unit.Percentage" />.
    /// </remarks>
    [Parameter]
    public Unit WidthUnit { get; set; } = Unit.Percentage;

    /// <summary>
    /// Gets or sets the zoom level of the <see cref="GoogleMap" />.
    /// </summary>
    /// <remarks>
    /// Default value is 14.
    /// </remarks>
    [Parameter]
    public int Zoom { get; set; } = 14;

    #endregion
}
