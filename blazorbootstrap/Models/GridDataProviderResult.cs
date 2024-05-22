namespace BlazorBootstrap;

public class GridDataProviderResult<TItem>
{
    #region Properties, Indexers

    /// <summary>
    /// The provided items by the request.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    public IEnumerable<TItem>? Data { get; init; }

    /// <summary>
    /// The total item count in the source (for pagination and infinite scroll).
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    public int? TotalCount { get; init; }

    #endregion
}
