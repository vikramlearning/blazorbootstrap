namespace BlazorBootstrap;

public class GridSettings
{
    #region Properties, Indexers

    /// <summary>
    /// Current filters.
    /// </summary>
    public IReadOnlyCollection<FilterItem>? Filters { get; set; }

    /// <summary>
    /// Page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Size of the page.
    /// </summary>
    public int PageSize { get; set; }

    #endregion
}
