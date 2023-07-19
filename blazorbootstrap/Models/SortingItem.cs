namespace BlazorBootstrap;

/// <summary>
/// Item describes one sorting criteria.
/// </summary>
public sealed class SortingItem<TItem>
{
    /// <summary>
    /// Sorting as string value. Can be used to pass value between application layers (ie. WebAPI call parameter).
    /// </summary>
    public string SortString { get; }

    /// <summary>
    /// Selector function of sorting key. To be used for automatic in-memory sorting.
    /// </summary>
    public Expression<Func<TItem, IComparable>> SortKeySelector { get; }

    /// <summary>
    /// Sort direction of SortString/SortKeySelector.
    /// </summary>
    public SortDirection SortDirection { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public SortingItem(string sortString, Expression<Func<TItem, IComparable>> sortKeySelector, SortDirection sortDirection)
    {
        SortString = sortString;
        SortKeySelector = sortKeySelector;
        SortDirection = sortDirection;
    }
}
