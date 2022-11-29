namespace BlazorBootstrap.Demo;

public record class Employee2
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Designation { get; init; }
    public decimal Salary { get; init; }
    public bool IsActive { get; init; }
}
