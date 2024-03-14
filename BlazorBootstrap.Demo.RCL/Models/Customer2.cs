using System.Text.Json.Serialization;

namespace BlazorBootstrap.Demo.RCL.Models;

public record Customer2 {
    public Customer2(
        int CustomerId,
        string? CustomerName,
        string? Phone,
        string? Email,
        string? Address,
        string? PostalZip,
        string? Country,
        CustomerType CustomerType = CustomerType.Two) {
        this.CustomerId = CustomerId;
        this.CustomerName = CustomerName;
        this.Phone = Phone;
        this.Email = Email;
        this.Address = Address;
        this.PostalZip = PostalZip;
        this.Country = Country;
        this.CustomerType = CustomerType;
    }

    public int CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? PostalZip { get; init; }
    public string? Country { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerType CustomerType { get; init; }

    public void Deconstruct(
        out int CustomerId,
        out string CustomerName,
        out string Phone,
        out string Email,
        out string Address,
        out string PostalZip,
        out string Country,
        out CustomerType CustomerType
    ) {
        CustomerId = this.CustomerId;
        CustomerName = this.CustomerName;
        Phone = this.Phone;
        Email = this.Email;
        Address = this.Address;
        PostalZip = this.PostalZip;
        Country = this.Country;
        CustomerType = this.CustomerType;
    }
}
