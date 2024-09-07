namespace BlazorBootstrap;

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
