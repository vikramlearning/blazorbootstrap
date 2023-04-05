namespace BlazorBootstrap;

public class AccordionEventArgs
{
    public AccordionEventArgs(string name, string title)
    {
        Name = name;
        Title = title;
    }

    public string Name { get; }
    public string Title { get; }
}
