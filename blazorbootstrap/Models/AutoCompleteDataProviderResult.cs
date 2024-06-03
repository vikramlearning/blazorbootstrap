namespace BlazorBootstrap;

public class AutoCompleteDataProviderResult<TItem>
{
    #region Properties, Indexers

    /// <summary>
    /// The provided items by the request.
    /// </summary>
    public IReadOnlyCollection<TItem> Data { get; init; } = default!;

    /// <summary>
    /// The total item count in the source (for pagination and infinite scroll).
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    public int? TotalCount { get; init; }

    #endregion
}
