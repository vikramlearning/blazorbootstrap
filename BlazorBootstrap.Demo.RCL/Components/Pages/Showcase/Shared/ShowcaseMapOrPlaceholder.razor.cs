using BlazorBootstrap.Demo.RCL.Models.Showcase;
using System.Globalization;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// Shows a <see cref="GoogleMap" /> when the host has a Google Maps API key configured,
/// otherwise a styled static placeholder with the same locations.
/// </summary>
public partial class ShowcaseMapOrPlaceholder : ComponentBase
{
    #region Fields and Constants

    private const string ApiKeyPlaceholder = "<<ApiKey>>";

    private string? apiKey;
    private string locationsKey = string.Empty;
    private List<GoogleMapMarker> markers = new();

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        apiKey = Configuration["GoogleMap:ApiKey"];
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        // Numbered like the placeholder's pins and list, so "pin 2" means the same place with or without a key.
        markers = Locations
            .Select((location, index) => new GoogleMapMarker
            {
                PinElement = new PinElement { GlyphText = (index + 1).ToString(CultureInfo.InvariantCulture) },
                Position = new GoogleMapMarkerPosition(location.Latitude, location.Longitude),
                Title = location.Name
            })
            .ToList();
        locationsKey = string.Join("|", Locations.Select(location => string.Create(CultureInfo.InvariantCulture, $"{location.Name}:{location.Latitude}:{location.Longitude}")));
    }

    #endregion

    #region Private Methods

    private string PinStyle(ShowcaseLocation location)
    {
        if (Locations.Count == 0)
            return string.Empty;

        var minLat = Locations.Min(l => l.Latitude);
        var maxLat = Locations.Max(l => l.Latitude);
        var minLng = Locations.Min(l => l.Longitude);
        var maxLng = Locations.Max(l => l.Longitude);

        var x = maxLng - minLng < 0.000001 ? 0.5 : (location.Longitude - minLng) / (maxLng - minLng);
        var y = maxLat - minLat < 0.000001 ? 0.5 : 1 - (location.Latitude - minLat) / (maxLat - minLat);

        // Keep pins away from the edges so labels stay visible.
        var left = 12 + x * 76;
        var top = 14 + y * 68;

        return string.Create(CultureInfo.InvariantCulture, $"left: {left:0.##}%; top: {top:0.##}%;");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the accessible description of the placeholder map.
    /// </summary>
    [Parameter] public string AriaLabel { get; set; } = "Map preview showing the listed locations";

    /// <summary>
    /// Gets or sets the map center. Defaults to the first location.
    /// </summary>
    [Parameter] public GoogleMapCenter? Center { get; set; }

    [Inject] private IConfiguration Configuration { get; set; } = default!;

    private GoogleMapCenter DefaultCenter => Locations.Count == 0
        ? new GoogleMapCenter(0, 0)
        : new GoogleMapCenter(Locations.Average(l => l.Latitude), Locations.Average(l => l.Longitude));

    /// <summary>
    /// Gets whether a real Google Maps API key is configured.
    /// </summary>
    private bool HasApiKey => !string.IsNullOrWhiteSpace(apiKey) && apiKey != ApiKeyPlaceholder;

    /// <summary>
    /// Gets or sets the map height in pixels.
    /// </summary>
    [Parameter] public int Height { get; set; } = 360;

    /// <summary>
    /// Gets or sets the locations to show.
    /// </summary>
    [Parameter, EditorRequired] public IReadOnlyList<ShowcaseLocation> Locations { get; set; } = Array.Empty<ShowcaseLocation>();

    /// <summary>
    /// Gets or sets whether the numbered location list is rendered below the map.
    /// </summary>
    [Parameter] public bool ShowList { get; set; } = true;

    /// <summary>
    /// Gets or sets the map zoom level.
    /// </summary>
    [Parameter] public int Zoom { get; set; } = 12;

    #endregion
}
