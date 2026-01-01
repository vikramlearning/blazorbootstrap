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
    [AddedVersion("1.0.0")]
    [Description("Gets the <b>Tab</b> name.")]
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="Tab" /> title.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Gets the <b>Tab</b> title.")]
    public string Title { get; }

    #endregion
}
