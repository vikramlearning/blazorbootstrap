namespace BlazorBootstrap;

public class BreadcrumbItem
{
    public string Text { get; set; }

    public string Href { get; set; }

    /// <summary>
    /// Represents the current page.
    /// </summary>
    public bool IsCurrentPage { get; set; }
}
