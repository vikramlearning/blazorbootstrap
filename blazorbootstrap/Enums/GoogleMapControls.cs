namespace BlazorBootstrap;

public enum GoogleMapControls
{
    /// <summary>
    /// Enables both zoom and street view controls.
    /// </summary>
    Full,
    /// <summary>
    /// Removes the zoom controls, but keeps the street view control.
    /// </summary>
    NoZoom,
    /// <summary>
    /// Removes street view controls, but keeps the zoom control.
    /// </summary>
    NoStreetView,
    /// <summary>
    /// Removes all controls entirely, including zoom and street view.
    /// </summary>
    NoZoomAndStreetView,
}