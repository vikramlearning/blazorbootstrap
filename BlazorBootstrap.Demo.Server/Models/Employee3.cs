namespace BlazorBootstrap.Demo.Server;

public record class Employee3
{
    public int Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; set; }
    public string Company { get; set; }
    public string Designation { get; init; }
    public DateOnly DOJ { get; init; }
    public decimal Salary { get; init; }
    public bool IsActive { get; init; }
}
