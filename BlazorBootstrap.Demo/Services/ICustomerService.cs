namespace BlazorBootstrap.Demo;

public interface ICustomerService
{
    public Task<IEnumerable<Customer2>> GetCustomers(FilterItem filter, CancellationToken cancellationToken);
}