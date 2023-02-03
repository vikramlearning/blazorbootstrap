namespace BlazorBootstrap.Demo.Server;

public class Department
{
    public string Name { get; set; }
    public IEnumerable<Employee1> Employees { get; set; }

    public Department(string name, IEnumerable<Employee1> employees)
    {
        Name = name;
        Employees = employees;
    }
}