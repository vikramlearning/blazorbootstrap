namespace BlazorBootstrap;

/// <summary>
/// Provides resize data for the <see cref="SplitView" /> component.
/// </summary>
public class SplitViewResizeEventArgs : EventArgs
{
    #region Constructors

    public SplitViewResizeEventArgs(double primaryPaneSize, double secondaryPaneSize, SplitViewOrientation orientation)
    {
        PrimaryPaneSize = primaryPaneSize;
        SecondaryPaneSize = secondaryPaneSize;
        Orientation = orientation;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the current orientation.
    /// </summary>
    public SplitViewOrientation Orientation { get; }

    /// <summary>
    /// Gets the primary pane size as a percentage.
    /// </summary>
    public double PrimaryPaneSize { get; }

    /// <summary>
    /// Gets the secondary pane size as a percentage.
    /// </summary>
    public double SecondaryPaneSize { get; }

    #endregion
}