using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap.Demo.Server;

public class CustomerService : ICustomerService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;

    public CustomerService(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager= navigationManager;
        _httpClient.BaseAddress = new Uri(navigationManager.BaseUri);
    }

    public async Task<IEnumerable<Customer>> GetCustomers(FilterItem filter)
    {
        try
        {
            var customers = await _httpClient.GetFromJsonAsync<Customer[]>("sample-data/autocomplete/customer.json");

            var parameterExpression = Expression.Parameter(typeof(Customer)); // second param optional
            var lambda = ExpressionExtensions.GetExpressionDelegate<Customer>(parameterExpression, filter);
            return customers.Where(lambda.Compile()).OrderBy(customer => customer.CustomerName);
        }
        catch
        {
            return new List<Customer>();
        }
    }
}