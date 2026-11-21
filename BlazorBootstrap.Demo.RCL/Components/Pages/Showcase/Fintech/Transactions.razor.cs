using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Transactions: in/out totals and a <see cref="Grid{TItem}" /> of the transactions in the chosen period (30 days, 90 days,
/// or 12 months) on the site's clock, filtered by account and category and searched by description or note, newest first,
/// with paging, sorting, the column chooser, and CSV export. Each row opens the transaction's page (a stretched link) and
/// says "Pending", "Scheduled", or "Disputed" in text. The grid's view is saved for the visit and can be reset; data screens
/// show loading, empty, and error states. <c>?account=</c> preselects an account.
/// </summary>
public partial class Transactions : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private string accountFilter = string.Empty;

    private string categoryFilter = string.Empty;

    private bool focusSearch;

    private Grid<TransactionRow>? grid;

    private int gridVersion;

    private int pageSize = DefaultPageSize;

    private FintechPeriod period = FintechPeriod.Last30Days;

    private string search = string.Empty;

    private ElementReference searchInput;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private string? statusMessage;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        // "Clear filters" leaves the page once used, so focus goes to the search box rather than being lost.
        if (focusSearch)
        {
            focusSearch = false;
            await FintechDisplay.FocusAsync(searchInput);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "transactions", DefaultPageSize, "date", SortDirection.Descending);

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

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string TransactionPath(int transactionId) => string.Create(CultureInfo.InvariantCulture, $"{FintechLayout.Base}/transactions/{transactionId}");

    private async Task ClearFiltersAsync()
    {
        accountFilter = categoryFilter = search = searchBoxValue = string.Empty;
        focusSearch = true;
        await FiltersChangedAsync();
    }

    /// <summary>Formats a date like "Nov 24", with the year for an earlier year.</summary>
    private string DayText(DateTime date) => date.Year == State.Today.Year ? FintechDisplay.Day(date) : date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    private IEnumerable<FintechTransaction> Filtered()
    {
        var since = State.Today.ToDateTime(TimeOnly.MinValue).AddDays(-(int)period + 1);
        var term = search.Trim();

        return State.Transactions.Where(item =>
            item.Date >= since
            && (accountFilter.Length == 0 || item.AccountId == accountFilter)
            && (categoryFilter.Length == 0 || item.CategoryId == categoryFilter)
            && (term.Length == 0 || item.Description.Contains(term, StringComparison.OrdinalIgnoreCase) || (item.Note?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)));
    }

    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private async Task OnAccountChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        accountFilter = State.FindAccount(value) is null ? string.Empty : value;
        await FiltersChangedAsync();
    }

    private async Task OnCategoryChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        categoryFilter = State.Categories.Any(category => category.Id == value) ? value : string.Empty;
        await FiltersChangedAsync();
    }

    private async Task OnPeriodChangedAsync(FintechPeriod value)
    {
        if (period == value || !Enum.IsDefined(value))
            return;

        period = value;
        await FiltersChangedAsync();
    }

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private Task<GridDataProviderResult<TransactionRow>> ProvideRowsAsync(GridDataProviderRequest<TransactionRow> request) => view.ApplyAsync(request, Rows);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private async Task ResetViewAsync()
    {
        const string message = "The view is reset: newest first, 25 transactions a page, and every column shown.";

        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        statusMessage = statusMessage == message ? message + " (2)" : message;
    }

    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the account to preselect, from <c>?account=</c>; an unknown value shows all accounts.</summary>
    [SupplyParameterFromQuery(Name = "account")] public string? Account { get; set; }

    /// <summary>Gets the grid's accessible name: what the rows are, with the filters applied.</summary>
    private string CaptionText
    {
        get
        {
            var parts = new List<string> { period == FintechPeriod.Last12Months ? "Transactions in the last 12 months" : $"Transactions in the last {(int)period} days" };
            parts.Add(accountFilter.Length == 0 ? "all accounts" : State.FindAccount(accountFilter)?.Name ?? "all accounts");

            if (categoryFilter.Length > 0)
                parts.Add(State.FindCategory(categoryFilter).Name);

            return string.Join(", ", parts);
        }
    }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool HasFilters => accountFilter.Length > 0 || categoryFilter.Length > 0 || search.Trim().Length > 0;

    private string MatchText => Filtered().Count() switch
    {
        0 => "None found",
        1 => "1 found",
        var count => $"{Count(count)} found",
    };

    private decimal MoneyIn => Filtered().Where(item => item.Amount > 0).Sum(item => item.Amount);

    private decimal MoneyOut => -Filtered().Where(item => item.Amount < 0).Sum(item => item.Amount);

    /// <summary>Gets the filtered transactions as grid rows, newest first.</summary>
    private IReadOnlyList<TransactionRow> Rows => Filtered()
        .Select(item => new TransactionRow(item.Id, item.Date, item.Description, State.FindCategory(item.CategoryId).Name, State.FindAccount(item.AccountId)?.Name ?? item.AccountId,
            item.Amount, item.DisputeId is not null ? "Disputed" : item.IsPending ? DateOnly.FromDateTime(item.Date) > State.Today ? "Scheduled" : "Pending" : string.Empty,
            item.DisputeId, item.Note))
        .ToList();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One transaction as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record TransactionRow(int Id, DateTime Date, string Description, string Category, string Account, decimal Amount, string Status, int? DisputeId, string? Note);

    #endregion
}
