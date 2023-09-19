namespace BlazorBootstrap.Demo.RCL.Services;

public class EmployeeService : IEmployeeService
{
    public Tuple<IEnumerable<Employee>, int> GetEmployees(
        IEnumerable<FilterItem> filters,
        int pageNumber,
        int pageSize,
        string sortKey,
        SortDirection sortDirection)
    {
        IEnumerable<Employee> employees = new List<Employee>
        {
            new Employee { Id = 101, FirstName = "Eathan", LastName = "Ellis", Designation = "Associate Architect", Salary = 19000, DOJ = new DateTime(1998, 11, 17), IsActive = true },
            new Employee { Id = 103, FirstName = "Cohan", LastName = "Wheatley", Designation = "Senior DevOps Engineer", Salary = 19000, DOJ = new DateTime(1985, 1, 5), IsActive = true },
            new Employee { Id = 116, FirstName = "Charis", LastName = "Guerra", Designation = "Data Scientist", Salary = 12000, DOJ = new DateTime(1995, 4, 17), IsActive = true },
            new Employee { Id = 110, FirstName = "Julian", LastName = "Walmsley", Designation = "Senior AI Engineer", Salary = 16500.50M, DOJ = new DateTime(1985, 6, 8), IsActive = false },
            new Employee { Id = 115, FirstName = "Emer", LastName = "Strickland", Designation = "AI Engineer", Salary = 7700, DOJ = new DateTime(1991, 8, 23), IsActive = true },
            new Employee { Id = 102, FirstName = "Kornelia", LastName = "Lord", Designation = "Developer", Salary = 8000, DOJ = new DateTime(1977, 1, 12), IsActive = true },
            new Employee { Id = 112, FirstName = "Loretta", LastName = "Koch", Designation = "Administrator", Salary = 8000, DOJ = new DateTime(1977, 1, 12), IsActive = true },
            new Employee { Id = 105, FirstName = "Ivy", LastName = "Lloyd", Designation = "Solution Architect", Salary = 24000, DOJ = new DateTime(1989, 10, 2), IsActive = true },
            new Employee { Id = 109, FirstName = "Isha", LastName = "Davison", Designation = "App Maker", Salary = 8000, DOJ = new DateTime(1994, 5, 12), IsActive = true },
            new Employee { Id = 111, FirstName = "Glenda", LastName = "Potter", Designation = "Data Engineer", Salary = 12000, DOJ = new DateTime(1991, 1, 1), IsActive = true },
            new Employee { Id = 106, FirstName = "Chance", LastName = "Bowler", Designation = "Auditor", Salary = 8000, DOJ = new DateTime(1996, 7, 1), IsActive = true },
            new Employee { Id = 114, FirstName = "Ralphy", LastName = "Estrada", Designation = "QA", Salary = 8000, DOJ = new DateTime(1994, 1, 12), IsActive = true },
            new Employee { Id = 108, FirstName = "Zayne", LastName = "Simmons", Designation = "Data Analyst", Salary = 12000, DOJ = new DateTime(1994, 5, 12), IsActive = true },
            new Employee { Id = 118, FirstName = "Kristopher", LastName = "Lawrence", Designation = "QA", Salary = 8000, DOJ = new DateTime(1995, 4, 17), IsActive = true },
            new Employee { Id = 107, FirstName = "Roisin", LastName = "Farmer", Designation = "Solutions Architect", Salary = 24000, DOJ = new DateTime(1989, 10, 2), IsActive = true },
            new Employee { Id = 113, FirstName = "Merlin", LastName = "Correa", Designation = "Content Developer", Salary = 8000, DOJ = new DateTime(1994, 5, 12), IsActive = true },
            new Employee { Id = 117, FirstName = "Sharna", LastName = "Macleod", Designation = "Data Analyst", Salary = 12000, DOJ = new DateTime(1995, 4, 17), IsActive = true },
        };

        // apply filters
        if (filters is not null && filters.Any())
        {
            var parameterExpression = Expression.Parameter(typeof(Employee)); // second param optional
            Expression<Func<Employee, bool>> lambda = null;

            foreach (var filter in filters)
            {
                if (lambda is null)
                    lambda = ExpressionExtensions.GetExpressionDelegate<Employee>(parameterExpression, filter);
                else
                    lambda = lambda.And(ExpressionExtensions.GetExpressionDelegate<Employee>(parameterExpression, filter));
            }
            employees = employees.Where(lambda.Compile());
        }

        // apply sorting then paging
        if (sortKey == "Id")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
        }
        else if (sortKey == "FirstName")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.FirstName).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.FirstName).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
        }
        else if (sortKey == "LastName")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.LastName).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.LastName).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
        }
        else if (sortKey == "Designation")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.Designation).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.Designation).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
        }
        else if (sortKey == "Salary")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.Salary).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.Salary).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
        }
        else if (sortKey == "DOJ")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.DOJ).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.DOJ).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
        }
        else if (sortKey == "IsActive")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.IsActive).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.IsActive).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
        }
        else if (string.IsNullOrEmpty(sortKey))
        {
            return new(employees.Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count());
        }

        return new(employees, employees.Count());
    }
}
