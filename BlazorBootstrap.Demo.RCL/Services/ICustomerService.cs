namespace BlazorBootstrap.Demo.RCL.Services;

public interface ICustomerService
{
    public Task<IReadOnlyCollection<Customer2>> GetCustomersAsync(FilterItem filter, CancellationToken cancellationToken = default);
    public Task<Tuple<IReadOnlyCollection<Customer2>, int>> GetCustomersAsync(IEnumerable<FilterItem> filters, int pageNumber, int pageSize, string sortKey, SortDirection sortDirection, CancellationToken cancellationToken = default);
}
