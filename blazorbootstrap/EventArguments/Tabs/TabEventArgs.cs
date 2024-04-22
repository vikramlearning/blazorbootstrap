namespace BlazorBootstrap;

public class TabEventArgs
{
    #region Constructors

    public TabEventArgs(string name, string title)
    {
        Name = name;
        Title = title;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the <see cref="Tab" /> name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="Tab" /> title.
    /// </summary>
    public string Title { get; }

    #endregion
}
