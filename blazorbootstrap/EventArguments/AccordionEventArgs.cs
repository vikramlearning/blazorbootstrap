namespace BlazorBootstrap;

/// <summary>
/// Events invoked by an <see cref="AccordionItem"/>.
/// </summary>
public class AccordionEventArgs
{
    #region Constructors

/// <summary>
/// Default constructor
/// </summary> 
    public AccordionEventArgs(string? name, string title)
    {
        Name = name;
        Title = title;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the <see cref="AccordionItem" /> name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the <see cref="AccordionItem" /> title.
    /// </summary>
    public string Title { get; }

    #endregion
}
