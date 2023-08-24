namespace BlazorBootstrap;

public class AutoCompleteDataProviderRequest<TItem>
{
    #region Methods

    public AutoCompleteDataProviderResult<TItem> ApplyTo(IEnumerable<TItem> data)
    {
        if (data == null)
            return new AutoCompleteDataProviderResult<TItem> { Data = null, TotalCount = null };

        var resultData = data;

        // apply filter
        if (Filter != null)
            try
            {
                var parameterExpression = Expression.Parameter(typeof(TItem)); // second param optional
                var lambda = ExpressionExtensions.GetExpressionDelegate<TItem>(parameterExpression, Filter);

                resultData = resultData.Where(lambda.Compile());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        var totalCount = resultData.Count();

        return new AutoCompleteDataProviderResult<TItem> { Data = resultData.ToList(), TotalCount = totalCount };
    }

    #endregion

    #region Properties, Indexers

    public CancellationToken CancellationToken { get; init; } = default;
    public FilterItem Filter { get; init; }

    #endregion
}
