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

    /// <summary>
    /// Makes it possible to change the Cluster Algorithms, by changing type or the zoom level under options.
    /// </summary>
    public class GoogleMapClusterAlgorithm
    {
        public string Type { get; set; } = GoogleMapAlgorithmTypes.SuperClusterAlgorithm.ToString();
        public GoogleMapClusterAlgorithmOptions Options { get; set; } = new();
    }
    
    /// <summary>
    /// Currently only has MaxZoom, but can be expanded with more options in the future.
    /// </summary>
    public class GoogleMapClusterAlgorithmOptions
    {
        public int? MaxZoom { get; set; } = 16;
    }
    
    /// <summary>
    /// Cluster renderer for Google Maps. Be aware that the properties only applies if you have a custom SVG icon.
    /// </summary>
    /// <remarks>
    /// ShowMarkerCount is set to true by default.
    /// </remarks>
    public class GoogleMapClusterRenderer
    {
        // Properties for the cluster count
        public string? TextColor { get; set; }
        public string? TextFontSize { get; set; }
        public bool ShowMarkerCount { get; set; } = true;
        // Properties for the cluster icon
        public string? SvgIcon { get; set; }
        
    }
    
    /// <summary>
    /// Defines the event arguments when the user clicks on a cluster.
    /// </summary>
    public class GoogleMapClusterClickEvent
    {
        public GoogleMapMarkerPosition Position { get; set; } = default!;
        public IEnumerable<GoogleMapMarker> Markers { get; set; } = default!;
    }
}