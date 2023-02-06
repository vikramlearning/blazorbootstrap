namespace BlazorBootstrap.Demo.Server;

public record class Employee1
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Designation { get; set; }
    public DateOnly DOJ { get; set; }
    public bool IsActive { get; set; }
}
