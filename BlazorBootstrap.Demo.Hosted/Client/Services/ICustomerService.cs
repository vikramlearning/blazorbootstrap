namespace BlazorBootstrap.Demo.Hosted.Client;

public interface ICustomerService
{
    public Task<IEnumerable<Customer>> GetCustomers(FilterItem filter, CancellationToken cancellationToken);
}