namespace BlazorBootstrap.Demo;

public interface ICustomerService
{
    public Task<IEnumerable<Customer>> GetCustomers(FilterItem filter, CancellationToken cancellationToken);
}