namespace BlazorBootstrap.Components;

public class GridDataProviderRequest<TItem>
{
    /// <summary>
    /// Current sorting.
    /// </summary>
    public IReadOnlyList<SortingItem<TItem>> Sorting { get; init; }

    public GridDataProviderResult<TItem> ApplyTo(IEnumerable<TItem> data)
    {
        if(data == null)
            return new GridDataProviderResult<TItem> { Data = null, TotalCount = null };

        IEnumerable<TItem> resultData = data;

        // sorting
        if(Sorting != null && Sorting.Any())
        {
            IOrderedEnumerable<TItem> orderedData = (Sorting[0].SortDirection == SortDirection.Ascending)
                       ? resultData.OrderBy(Sorting[0].SortKeySelector.Compile())
                       : resultData.OrderByDescending(Sorting[0].SortKeySelector.Compile());

            for (int i = 1; i < Sorting.Count; i++)
            {
                orderedData = (Sorting[i].SortDirection == SortDirection.Ascending)
                    ? orderedData.ThenBy(Sorting[i].SortKeySelector.Compile())
                    : orderedData.ThenByDescending(Sorting[i].SortKeySelector.Compile());
            }
            resultData = orderedData;
        }

        return new GridDataProviderResult<TItem>
        {
            Data = resultData.ToList(),
            TotalCount = data.Count()
        };
    }
}
