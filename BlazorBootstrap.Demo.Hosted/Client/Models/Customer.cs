namespace BlazorBootstrap.Demo.Hosted.Client;

public record Customer(int CustomerId, string CustomerName);

public record Customer2
(
    int CustomerId,
    string CustomerName,
    string Phone,
    string Email,
    string Address,
    string PostalZip,
    string Country
);