namespace BlazorBootstrap;

public class RibbonItemEventArgs : EventArgs
{
    #region Constructors

    public RibbonItemEventArgs(string name)
    {
        Name = name;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the <see cref="RibbonItem" /> name.
    /// </summary>
    public string? Name { get; }

    #endregion
}
