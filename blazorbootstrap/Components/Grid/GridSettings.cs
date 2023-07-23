namespace BlazorBootstrap;

public class GridSettings
{
    /// <summary>
    /// Page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Size of the page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Current filters.
    /// </summary>
    public IEnumerable<FilterItem>? Filters { get; set; }
}
