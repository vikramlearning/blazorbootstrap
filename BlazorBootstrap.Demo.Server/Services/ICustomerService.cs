namespace BlazorBootstrap.Demo.Server;

public interface ICustomerService
{
    public Task<IEnumerable<Customer2>> GetCustomersAsync(FilterItem filter, CancellationToken cancellationToken = default);

    public Task<Tuple<IEnumerable<Customer2>, int>> GetCustomersAsync(IEnumerable<FilterItem> filters, int pageNumber, int pageSize, string sortKey, SortDirection sortDirection, CancellationToken cancellationToken = default);
}