using System.Linq.Expressions;
using BlazorBootstrap.Playground.GridServerTests.Models;
using Bogus;

namespace BlazorBootstrap.Playground.GridServerTests.Services;

public interface ClientService {
    public Task<Tuple<IEnumerable<Client>, int>> GetClients(
        IEnumerable<FilterItem> filters,
        int pageNumber,
        int pageSize,
        string sortKey,
        SortDirection sortDirection
    );
}

public class FakeClientService : ClientService {
    private readonly IEnumerable<Client> clients;

    public FakeClientService() {
        var clientFaker = new Faker<Client>()
                .StrictMode(true)
                .RuleFor(e => e.Id, f => f.Random.Guid())
                .RuleFor(e => e.Name, f => f.Name.FullName())
                .RuleFor(
                    e => e.Email,
                    (f, u) =>
                        u.Name.Split(' ') is [{ } firstName, { } lastName, ..]
                            ? f.Internet.Email(firstName, lastName)
                            : f.Internet.Email(u.Name)
                )
                .RuleFor(e => e.Type, f => f.PickRandom<ClientType>())
                .RuleFor(e => e.Status, f => f.PickRandom<ClientStatus>())
                .RuleFor(e => e.RegisteredAt, f => f.Date.Past())
                .RuleFor(e => e.IsPowerUser, f => f.Random.Bool())
                .RuleFor(e => e.Karma, f => f.Random.Double(0.0, 100.0))
                .RuleFor(e => e.Rating, f => f.Random.Int(0, 100))
            ;
        clients = clientFaker.GenerateBetween(100, 1000);
    }

    public Task<Tuple<IEnumerable<Client>, int>> GetClients(
        IEnumerable<FilterItem> filters,
        int pageNumber,
        int pageSize,
        string sortKey,
        SortDirection sortDirection
    ) {
        IEnumerable<Client> result = clients;

        // apply filters
        if (filters is not null && filters.Any()) {
            var parameterExpression = Expression.Parameter(typeof(Client)); // second param optional
            Expression<Func<Client, bool>> lambda = null;

            foreach (var filter in filters) {
                if (lambda is null)
                    lambda = ExpressionExtensions.GetExpressionDelegate<Client>(parameterExpression, filter);
                else
                    lambda = lambda.And(
                        ExpressionExtensions.GetExpressionDelegate<Client>(parameterExpression, filter)
                    );
            }

            result = clients.Where(lambda.Compile());
        }

        result = (sortKey, sortDirection) switch {
            ("Id", SortDirection.Ascending) => result.OrderBy(e => e.Id),
            ("Id", SortDirection.Descending) => result.OrderByDescending(e => e.Id),
            ("Email", SortDirection.Ascending) => result.OrderBy(e => e.Email),
            ("Email", SortDirection.Descending) => result.OrderByDescending(e => e.Email),
            ("Name", SortDirection.Ascending) => result.OrderBy(e => e.Name),
            ("Name", SortDirection.Descending) => result.OrderByDescending(e => e.Name),
            ("Type", SortDirection.Ascending) => result.OrderBy(e => e.Type),
            ("Type", SortDirection.Descending) => result.OrderByDescending(e => e.Type),
            ("Status", SortDirection.Ascending) => result.OrderBy(e => e.Status),
            ("Status", SortDirection.Descending) => result.OrderByDescending(e => e.Status),
            ("RegisteredAt", SortDirection.Ascending) => result.OrderBy(e => e.RegisteredAt),
            ("RegisteredAt", SortDirection.Descending) => result.OrderByDescending(e => e.RegisteredAt),
            ("IsPowerUser", SortDirection.Ascending) => result.OrderBy(e => e.IsPowerUser),
            ("IsPowerUser", SortDirection.Descending) => result.OrderByDescending(e => e.IsPowerUser),
            ("Karma", SortDirection.Ascending) => result.OrderBy(e => e.Karma),
            ("Karma", SortDirection.Descending) => result.OrderByDescending(e => e.Karma),
            ("Rating", SortDirection.Ascending) => result.OrderBy(e => e.Rating),
            ("Rating", SortDirection.Descending) => result.OrderByDescending(e => e.Rating),
            _ => result
        };

        return Task.FromResult<Tuple<IEnumerable<Client>, int>>(
            pageNumber > 0 && pageSize > 0
            ? new(
                result.Skip((pageNumber - 1) * pageSize).Take(pageSize),
                result.Count()
            )
            : new(result, result.Count())
            );
    }
}