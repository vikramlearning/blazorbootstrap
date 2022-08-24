using System.Linq.Expressions;

namespace BlazorBootstrap;

public class GridDataProviderRequest<TItem>
{
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
    public IReadOnlyList<SortingItem<TItem>> Sorting { get; init; }

    /// <summary>
    /// Current filters.
    /// </summary>
    public IReadOnlyList<FilterItem> Filters { get; init; }

    public GridDataProviderResult<TItem> ApplyTo(IEnumerable<TItem> data)
    {
        if (data == null)
            return new GridDataProviderResult<TItem> { Data = null, TotalCount = null };

        IEnumerable<TItem> resultData;
        IEnumerable<TItem> filteredData = resultData = data;

        // apply filter
        if (Filters != null && Filters.Any())
        {
            try
            {
                var parameterExpression = Expression.Parameter(typeof(TItem)); // second param optional
                Expression<Func<TItem, bool>> lambda = null;

                foreach (var filter in Filters)
                {
                    if (lambda is null)
                        lambda = ExpressionExtensions.GetExpressionDelegate<TItem>(parameterExpression, filter);
                    else
                        lambda = lambda.And(ExpressionExtensions.GetExpressionDelegate<TItem>(parameterExpression, filter));
                }

                filteredData = resultData.Where(lambda.Compile());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // apply sorting
        if (Sorting != null && Sorting.Any())
        {
            IOrderedEnumerable<TItem> orderedData = (Sorting[0].SortDirection == SortDirection.Ascending)
                       ? (filteredData ?? resultData).OrderBy(Sorting[0].SortKeySelector.Compile())
                       : (filteredData ?? resultData).OrderByDescending(Sorting[0].SortKeySelector.Compile());

            for (int i = 1; i < Sorting.Count; i++)
            {
                orderedData = (Sorting[i].SortDirection == SortDirection.Ascending)
                    ? orderedData.ThenBy(Sorting[i].SortKeySelector.Compile())
                    : orderedData.ThenByDescending(Sorting[i].SortKeySelector.Compile());
            }
            resultData = orderedData;
        }

        // apply paging
        var skip = 0;
        var take = data.Count();

        if (PageNumber > 0 && PageSize > 0)
        {
            skip = (PageNumber - 1) * PageSize;
            take = PageSize;
        }
        resultData = resultData.Skip(skip).Take(take);

        return new GridDataProviderResult<TItem>
        {
            Data = resultData.ToList(),
            TotalCount = data.Count()
        };
    }
}