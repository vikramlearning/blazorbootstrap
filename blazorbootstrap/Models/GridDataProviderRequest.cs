﻿namespace BlazorBootstrap;

public class GridDataProviderRequest<TItem>
{
    #region Methods

    public GridDataProviderResult<TItem> ApplyTo(IEnumerable<TItem> data)
    {
        if (data == null)
            return new GridDataProviderResult<TItem> { Data = null, TotalCount = null };

        var resultData = data;

        // apply filter
        if (Filters != null && Filters.Any())
            try
            {
                var parameterExpression = Expression.Parameter(typeof(TItem)); // second param optional
                Expression<Func<TItem, bool>>? lambda = null;

                foreach (var filter in Filters)
                    if (lambda is null)
                        lambda = ExpressionExtensions.GetExpressionDelegate<TItem>(parameterExpression, filter);
                    else
                        lambda = lambda.And(ExpressionExtensions.GetExpressionDelegate<TItem>(parameterExpression, filter)!);

                if (lambda is not null)
                    resultData = resultData.Where(lambda.Compile());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        // apply sorting
        if (Sorting != null && Sorting.Any())
        {
            IOrderedEnumerable<TItem> orderedData = null!;
            var index = 1;

            foreach (var sortItem in Sorting)
            {
                if (index == 1)
                {
                    orderedData = sortItem.SortDirection == SortDirection.Ascending
                                      ? resultData.OrderBy(sortItem.SortKeySelector.Compile())
                                      : resultData.OrderByDescending(sortItem.SortKeySelector.Compile());
                }
                else
                {
                    if (orderedData != null)
                        orderedData = sortItem.SortDirection == SortDirection.Ascending
                                          ? orderedData.ThenBy(sortItem.SortKeySelector.Compile())
                                          : orderedData.ThenByDescending(sortItem.SortKeySelector.Compile());
                }

                index++;
            }

            resultData = orderedData;
        }

        // apply paging
        var skip = 0;
        var take = data.Count();
        var totalCount = resultData!.Count(); // before paging

        if (PageNumber > 0 && PageSize > 0)
        {
            skip = (PageNumber - 1) * PageSize;
            take = PageSize;
            resultData = resultData!.Skip(skip).Take(take);
        }

        return new GridDataProviderResult<TItem> { Data = resultData, TotalCount = totalCount };
    }

    #endregion

    #region Properties, Indexers

    public CancellationToken CancellationToken { get; init; } = default;

    /// <summary>
    /// Current filters.
    /// </summary>
    public IEnumerable<FilterItem> Filters { get; init; } = default!;

    /// <summary>
    /// Page number.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Size of the page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Current sorting.
    /// </summary>
    public IEnumerable<SortingItem<TItem>> Sorting { get; init; } = default!;

    #endregion
}
