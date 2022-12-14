namespace BlazorBootstrap.Demo.Hosted.Client;

public record class Employee1
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Designation { get; init; }
    public DateOnly DOJ { get; init; }
    public bool IsActive { get; init; }
}
