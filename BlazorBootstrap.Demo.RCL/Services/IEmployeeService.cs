namespace BlazorBootstrap.Demo.RCL.Services;

public interface IEmployeeService
{
    public Tuple<IEnumerable<Employee>, int> GetEmployees(IEnumerable<FilterItem> filters, int pageNumber, int pageSize, string sortKey, SortDirection sortDirection);
}
