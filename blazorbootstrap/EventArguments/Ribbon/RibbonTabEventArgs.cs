namespace BlazorBootstrap;

public class RibbonTabEventArgs
{
    #region Constructors

    public RibbonTabEventArgs(string name, string title)
    {
        Name = name;
        Title = title;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the <see cref="RibbonTab" /> name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="RibbonTab" /> title.
    /// </summary>
    public string Title { get; }

    #endregion
}
