namespace BlazorBootstrap
{
    /// <summary>
    /// Represents the options for clustering markers on a Google Map.
    /// To enable clustering, set <see cref="ClusteringEnabled"/> to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// Default value for <see cref="ClusteringEnabled"/> is <c>false</c>.
    /// </remarks>
    public class GoogleMapClusterOptions
    {
        public GoogleMapClusterRenderer? Renderer { get; set; }
        public GoogleMapClusterAlgorithm? Algorithm { get; set; }
        public bool ClusteringEnabled { get; set; } = false; 
        public bool EnableClusterClick { get; set; }
    }
    
    public class GoogleMapClusterAlgorithm
    {
        public string Type { get; set; } = "SuperClusterAlgorithm";
        public Dictionary<string, object> Options { get; set; } = new();
    }
    
    /// <summary>
    /// Cluster renderer for Google Maps. Be aware that the properties only applies if you have a custom SVG icon.
    /// </summary>
    public class GoogleMapClusterRenderer
    {
        // Properties for the cluster count
        public string? TextColor { get; set; }
        public string? TextFontSize { get; set; }
        public bool ShowMarkerCount { get; set; } = true;
        // Properties for the cluster icon
        public string? SvgIcon { get; set; }
        
    }
    
    public class GoogleMapClusterClickEvent
    {
        public GoogleMapMarkerPosition Position { get; set; } = default!;
        public IEnumerable<GoogleMapMarker> Markers { get; set; } = default!;
    }
}