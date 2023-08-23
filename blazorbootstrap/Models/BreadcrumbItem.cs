namespace BlazorBootstrap;

public class BreadcrumbItem
{
    #region Properties, Indexers

    public string? Href { get; set; }

    /// <summary>
    /// Represents the current page.
    /// </summary>
    public bool IsCurrentPage { get; set; }

    public string? Text { get; set; }

    #endregion
}