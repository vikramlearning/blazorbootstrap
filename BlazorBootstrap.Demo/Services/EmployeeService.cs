namespace BlazorBootstrap.Demo;

public class EmployeeService : IEmployeeService
{
    public Tuple<IEnumerable<Employee>, int> GetEmployees(int pageNumber, int pageSize, string sortKey, SortDirection sortDirection)
    {
        var employees = new List<Employee>
        {
            new Employee { Id = 101, FirstName = "Eathan", LastName = "Ellis", Designation = "Associate Architect" },
            new Employee { Id = 103, FirstName = "Cohan", LastName = "Wheatley", Designation = "Senior DevOps Engineer" },
            new Employee { Id = 104, FirstName = "Jody", LastName = "Frost", Designation = "Senior Data Engineer" },
            new Employee { Id = 116, FirstName = "Charis", LastName = "Guerra", Designation = "Data Scientist" },
            new Employee { Id = 110, FirstName = "Julian", LastName = "Walmsley", Designation = "Senior AI Engineer" },
            new Employee { Id = 115, FirstName = "Emer", LastName = "Strickland", Designation = "AI Engineer" },
            new Employee { Id = 102, FirstName = "Kornelia", LastName = "Lord", Designation = "Developer" },
            new Employee { Id = 112, FirstName = "Loretta", LastName = "Koch", Designation = "Administrator" },
            new Employee { Id = 105, FirstName = "Ivy", LastName = "Lloyd", Designation = "Solution Architect" },
            new Employee { Id = 109, FirstName = "Isha", LastName = "Davison", Designation = "App Maker" },
            new Employee { Id = 111, FirstName = "Glenda", LastName = "Potter", Designation = "Data Engineer" },
            new Employee { Id = 106, FirstName = "Chance", LastName = "Bowler", Designation = "Auditor" },
            new Employee { Id = 114, FirstName = "Ralphy", LastName = "Estrada", Designation = "" },
            new Employee { Id = 108, FirstName = "Zayne", LastName = "Simmons", Designation = "Data Analyst" },
            new Employee { Id = 118, FirstName = "Kristopher", LastName = "Lawrence", Designation = "" },
            new Employee { Id = 107, FirstName = "Roisin", LastName = "Farmer", Designation = "Solutions Architect" },
            new Employee { Id = 113, FirstName = "Merlin", LastName = "Correa", Designation = "" },
            new Employee { Id = 117, FirstName = "Sharna", LastName = "Macleod", Designation = "Data Analyst" },
        };

        // apply sorting then paging
        if (sortKey == "Id")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count);
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count);
        }
        else if (sortKey == "FirstName")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.FirstName).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count);
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.FirstName).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count);
        }
        else if (sortKey == "LastName")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.LastName).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count);
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.LastName).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count);
        }
        else if (sortKey == "Designation")
        {
            if (sortDirection == SortDirection.Ascending)
                return new(employees.OrderBy(e => e.Designation).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count);
            else if (sortDirection == SortDirection.Descending)
                return new(employees.OrderByDescending(e => e.Designation).Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count);
        }
        else if (string.IsNullOrEmpty(sortKey))
        {
            return new(employees.Skip((pageNumber - 1) * pageSize).Take(pageSize), employees.Count);
        }

        return new(employees, employees.Count);
    }
}