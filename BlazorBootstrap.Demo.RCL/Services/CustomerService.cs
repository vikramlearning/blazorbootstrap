namespace BlazorBootstrap.Demo.RCL.Services;

public class CustomerService : ICustomerService
{
    private readonly HttpClient _httpClient;

    public CustomerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Customer2>> GetCustomersAsync(FilterItem filter, CancellationToken cancellationToken = default)
    {
        var customers = await _httpClient.GetFromJsonAsync<Customer2[]>("sample-data/customer/customer.json", cancellationToken);
        if (customers is null)
            return Enumerable.Empty<Customer2>();

        var parameterExpression = Expression.Parameter(typeof(Customer2)); // second param optional
        var lambda = ExpressionExtensions.GetExpressionDelegate<Customer2>(parameterExpression, filter);
        return customers.Where(lambda.Compile()).OrderBy(customer => customer.CustomerName);
    }

    public async Task<Tuple<IEnumerable<Customer2>, int>> GetCustomersAsync(IEnumerable<FilterItem> filters, int pageNumber, int pageSize, string sortKey, SortDirection sortDirection, CancellationToken cancellationToken = default)
    {
        var customers = await _httpClient.GetFromJsonAsync<IEnumerable<Customer2>>("sample-data/customer/customer.json", cancellationToken);
        if (customers is null)
            return new(Enumerable.Empty<Customer2>(), 0);

        // apply filters
        if (filters is not null && filters.Any())
        {
            var parameterExpression = Expression.Parameter(typeof(Customer2)); // second param optional
            Expression<Func<Customer2, bool>> lambda = null;

            foreach (var filter in filters)
            {
                if (lambda is null)
                    lambda = ExpressionExtensions.GetExpressionDelegate<Customer2>(parameterExpression, filter);
                else
                    lambda = lambda.And(ExpressionExtensions.GetExpressionDelegate<Customer2>(parameterExpression, filter));
            }
            customers = customers.Where(lambda.Compile());
        }

        // apply sorting then paging
        if (sortKey == "CustomerId")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(customers.OrderBy(e => e.CustomerId).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(customers.OrderByDescending(e => e.CustomerId).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
        }
        else if (sortKey == "CustomerName")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(customers.OrderBy(e => e.CustomerName).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(customers.OrderByDescending(e => e.CustomerName).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
        }
        else if (sortKey == "Phone")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(customers.OrderBy(e => e.Phone).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(customers.OrderByDescending(e => e.Phone).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
        }
        else if (sortKey == "Email")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(customers.OrderBy(e => e.Email).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(customers.OrderByDescending(e => e.Email).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
        }
        else if (sortKey == "Address")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(customers.OrderBy(e => e.Address).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(customers.OrderByDescending(e => e.Address).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
        }
        else if (sortKey == "PostalZip")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(customers.OrderBy(e => e.PostalZip).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(customers.OrderByDescending(e => e.PostalZip).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
        }
        else if (sortKey == "Country")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(customers.OrderBy(e => e.Country).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(customers.OrderByDescending(e => e.Country).Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
        }
        else if (string.IsNullOrEmpty(sortKey))
        {
            return new(customers.Skip((pageNumber - 1) * pageSize).Take(pageSize), customers.Count());
        }

        return new(customers, customers.Count());
    }
}
