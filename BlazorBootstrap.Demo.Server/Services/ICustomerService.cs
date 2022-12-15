namespace BlazorBootstrap.Demo.Server;

public interface ICustomerService
{
    public Task<IEnumerable<Customer>> GetCustomers(FilterItem filter);
}