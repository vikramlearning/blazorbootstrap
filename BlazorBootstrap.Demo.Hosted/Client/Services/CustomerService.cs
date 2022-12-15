namespace BlazorBootstrap.Demo.Hosted.Client;

public class CustomerService : ICustomerService
{
    private readonly HttpClient _httpClient;

    public CustomerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Customer>> GetCustomers(FilterItem filter)
    {
        var customers = await _httpClient.GetFromJsonAsync<Customer[]>("sample-data/autocomplete/customer.json");

        var parameterExpression = Expression.Parameter(typeof(Customer)); // second param optional
        var lambda = ExpressionExtensions.GetExpressionDelegate<Customer>(parameterExpression, filter);
        return customers.Where(lambda.Compile()).OrderBy(customer => customer.CustomerName);
    }
}