namespace BlazorBootstrap;

public class AccordionEventArgs
{
    #region Constructors

    public AccordionEventArgs(string name, string title)
    {
        Name = name;
        Title = title;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the <see cref="AccordionItem" /> name.
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(null)]
    [Description("Gets the <b>AccordionItem</b> name.")]
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="AccordionItem" /> title.
    /// </summary>
    [AddedVersion("1.7.0")]
    [DefaultValue(null)]
    [Description("Gets the <b>AccordionItem</b> title.")]
    public string Title { get; }

    #endregion
}
