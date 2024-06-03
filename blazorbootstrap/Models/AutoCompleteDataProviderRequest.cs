namespace BlazorBootstrap;

public class AutoCompleteDataProviderRequest<TItem>
{
    #region Methods

    public AutoCompleteDataProviderResult<TItem> ApplyTo(IReadOnlyCollection<TItem> data)
    {
        if (data is null)
            return new AutoCompleteDataProviderResult<TItem> { Data = null, TotalCount = null };

        var resultData = data;

        // apply filter
        if (Filter is not null)
            try
            {
                var parameterExpression = Expression.Parameter(typeof(TItem)); // second param optional
                var lambda = ExpressionExtensions.GetExpressionDelegate<TItem>(parameterExpression, Filter);

                if (lambda is not null)
                    resultData = resultData.Where(lambda.Compile()).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        var totalCount = resultData.Count;

        return new AutoCompleteDataProviderResult<TItem> { Data = resultData.ToList(), TotalCount = totalCount };
    }

    #endregion

    #region Properties, Indexers

    public CancellationToken CancellationToken { get; init; } = default;
    public FilterItem? Filter { get; init; } = default!;

    #endregion
}
