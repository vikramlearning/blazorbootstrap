using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Transactions: a searchable, sortable, paged grid with account, category, and period filters and in/out totals.
/// <c>?account=</c> preselects an account.
/// </summary>
public partial class Transactions : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string accountFilter = string.Empty;

    private string categoryFilter = string.Empty;

    private Grid<FintechTransaction> grid = default!;

    private FintechPeriod period = FintechPeriod.Last30Days;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        if (State.FindAccount(Account) is { } account)
            accountFilter = account.Id;

        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private IEnumerable<FintechTransaction> Filtered()
    {
        var since = FintechSeedData.Today.Date.AddDays(-(int)period + 1);
        var term = search.Trim();

        return State.Transactions.Where(item =>
            item.Date >= since
            && (accountFilter.Length == 0 || item.AccountId == accountFilter)
            && (categoryFilter.Length == 0 || item.CategoryId == categoryFilter)
            && (term.Length == 0 || item.Description.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task OnAccountChangedAsync(ChangeEventArgs args)
    {
        accountFilter = args.Value?.ToString() ?? string.Empty;
        await RefreshAsync();
    }

    private async Task OnCategoryChangedAsync(ChangeEventArgs args)
    {
        categoryFilter = args.Value?.ToString() ?? string.Empty;
        await RefreshAsync();
    }

    private async Task OnPeriodChangedAsync(FintechPeriod value)
    {
        if (period == value)
            return;

        period = value;
        await RefreshAsync();
    }

    private async Task OnSearchChangedAsync(string value)
    {
        searchBoxValue = value;
        await OnSearchValueAsync(value);
    }

    private Task OnSearchInputAsync(ChangeEventArgs args) => OnSearchValueAsync(args.Value?.ToString() ?? string.Empty);

    private async Task OnSearchValueAsync(string value)
    {
        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await grid.RefreshDataAsync();
        StateHasChanged();
    });

    private async Task RefreshAsync()
    {
        await grid.ResetPageNumber();
        StateHasChanged();
    }

    private Task<GridDataProviderResult<FintechTransaction>> TransactionsDataProvider(GridDataProviderRequest<FintechTransaction> request) =>
        Task.FromResult(request.ApplyTo(Filtered().ToList()));

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the account to preselect, from <c>?account=</c>; an unknown value shows all accounts.</summary>
    [SupplyParameterFromQuery(Name = "account")] public string? Account { get; set; }

    private string MatchText => $"{Filtered().Count()} found";

    private decimal MoneyIn => Filtered().Where(item => item.Amount > 0).Sum(item => item.Amount);

    private decimal MoneyOut => -Filtered().Where(item => item.Amount < 0).Sum(item => item.Amount);

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
