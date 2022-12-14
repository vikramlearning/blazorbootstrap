namespace BlazorBootstrap.Demo.Hosted.Client;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Designation { get; set; }
    public decimal Salary { get; set; }
    public DateTime DOJ { get; set; }
    public bool IsActive { get; set; }
}