namespace BlazorBootstrap.Demo.Models
{
    public class Department
    {
        public string Name { get; set; }
        public List<Employee1> Employees { get; set; }

        public Department(string name, List<Employee1> employees)
        {
            Name = name;
            Employees = employees;
        }
    }
}
