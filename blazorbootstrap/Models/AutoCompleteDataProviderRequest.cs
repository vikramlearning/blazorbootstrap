namespace BlazorBootstrap;

public class AutoCompleteDataProviderRequest<TItem>
{
    public FilterItem Filter { get; init; }

    public AutoCompleteDataProviderResult<TItem> ApplyTo(IEnumerable<TItem> data)
    {
        if (data == null)
            return new AutoCompleteDataProviderResult<TItem> { Data = null, TotalCount = null };

        IEnumerable<TItem> resultData = data;

        // apply filter
        if (Filter != null)
        {
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
        }

        var totalCount = resultData.Count();

        return new AutoCompleteDataProviderResult<TItem>
        {
            Data = resultData.ToList(),
            TotalCount = totalCount
        };
    }
}
