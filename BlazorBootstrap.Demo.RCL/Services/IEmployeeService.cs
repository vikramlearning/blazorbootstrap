namespace BlazorBootstrap.Demo.RCL.Services;

public interface IEmployeeService
{
    public Tuple<IReadOnlyCollection<Employee>, int> GetEmployees(IReadOnlyCollection<FilterItem> filters, int pageNumber, int pageSize, string sortKey, SortDirection sortDirection);
}
