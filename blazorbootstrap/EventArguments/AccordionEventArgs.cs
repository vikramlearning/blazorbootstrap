namespace BlazorBootstrap;

public class AccordionEventArgs
{
    public AccordionEventArgs(string name, string title)
    {
        Name = name;
        Title = title;
    }

    /// <summary>
    /// Gets the <see cref="AccordionItem"/> name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="AccordionItem"/> title.
    /// </summary>
    public string Title { get; }
}
