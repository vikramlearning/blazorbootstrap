namespace BlazorBootstrap.Demo.RCL.Models;

public record Employee4
{
    public int? Id { get; set; }
    public string? Name { get; set; } = "";
    public string? Designation { get; set; } = "";
    public DateOnly? DOJ { get; set; }
    public bool IsActive { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Three;
}
