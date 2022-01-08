namespace BlazorBootstrap.Demo;

public interface IEmployeeService
{
    public Tuple<IEnumerable<Employee>, int> GetEmployees(int pageNumber, int pageSize, string sortKey, SortDirection sortDirection);
}

