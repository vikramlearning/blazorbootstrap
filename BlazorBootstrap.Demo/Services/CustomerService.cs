namespace BlazorBootstrap.Demo;

public class CustomerService : ICustomerService
{
    private readonly HttpClient _httpClient;

    public CustomerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Customer2>> GetCustomers(FilterItem filter, CancellationToken cancellationToken)
    {
        var customers = await _httpClient.GetFromJsonAsync<Customer2[]>("sample-data/customer/customer.json");
        if (customers is null)
            return Enumerable.Empty<Customer2>();

        var parameterExpression = Expression.Parameter(typeof(Customer2)); // second param optional
        var lambda = ExpressionExtensions.GetExpressionDelegate<Customer2>(parameterExpression, filter);
        return customers.Where(lambda.Compile()).OrderBy(customer => customer.CustomerName);
    }
}