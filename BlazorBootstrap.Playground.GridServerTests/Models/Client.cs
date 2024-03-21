namespace BlazorBootstrap.Playground.GridServerTests.Models;

public sealed record Client {
    public Client(
        Guid Id,
        string Email,
        string Name,
        ClientType Type,
        ClientStatus Status,
        DateTime RegisteredAt,
        bool IsPowerUser = false,
        double Karma = 0.0,
        int Rating = 0
    ) {
        this.Id = Id;
        this.Email = Email;
        this.Name = Name;
        this.Type = Type;
        this.Status = Status;
        this.RegisteredAt = RegisteredAt;
        this.IsPowerUser = IsPowerUser;
        this.Karma = Karma;
        this.Rating = Rating;
    }
    public Client() {
        Id = Guid.Empty;
        Email = string.Empty;
        Name = string.Empty;
        Type = ClientType.Guest;
        Status = ClientStatus.Disabled;
        RegisteredAt = DateTime.MinValue;
        IsPowerUser = false;
        Karma = 0.0;
        Rating = 0;
    }
    
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string Name { get; init; }
    public ClientType Type { get; init; }
    public ClientStatus Status { get; init; }
    public DateTime RegisteredAt { get; init; }
    public bool IsPowerUser { get; init; }
    public double Karma { get; init; }
    public int Rating { get; init; }
};