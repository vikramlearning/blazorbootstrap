using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Transactions: in/out totals and a plain table (caption and header scopes) of the transactions in the chosen period on
/// the site's clock, filtered by account and category and searched by description, newest first, shown 25 at a time.
/// Each row links to the transaction's page and says "Pending", "Scheduled", or "Disputed" in text. Below the medium
/// breakpoint the category and account move under the description, so the table fits a phone. <c>?account=</c>
/// preselects an account.
/// </summary>
public partial class Transactions : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many more rows "Show more" adds.</summary>
    private const int PageSize = 25;

    private string accountFilter = string.Empty;

    private string categoryFilter = string.Empty;

    /// <summary>The index of the first row the last "Show more" added, until focus has moved to it; -1 otherwise.</summary>
    private int firstNewIndex = -1;

    private ElementReference firstNewLink;

    private bool focusFirstNew;

    private bool focusSearch;

    private FintechPeriod period = FintechPeriod.Last30Days;

    private string search = string.Empty;

    private TextInput? searchInput;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private int shownCount = PageSize;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusFirstNew)
        {
            focusFirstNew = false;
            await FintechDisplay.FocusAsync(firstNewLink);
        }

        // "Clear filters" leaves the page once used, so focus goes to the search box rather than being lost.
        if (focusSearch)
        {
            focusSearch = false;
            await FintechDisplay.FocusAsync(searchInput?.Element);
        }
    }

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

    private static string TransactionPath(int transactionId) => string.Create(CultureInfo.InvariantCulture, $"{FintechLayout.Base}/transactions/{transactionId}");

    private void ClearFilters()
    {
        accountFilter = string.Empty;
        categoryFilter = string.Empty;
        search = string.Empty;
        searchBoxValue = string.Empty;
        focusSearch = true;
        ResetPaging();
    }

    private IEnumerable<FintechTransaction> Filtered()
    {
        var since = State.Today.ToDateTime(TimeOnly.MinValue).AddDays(-(int)period + 1);
        var term = search.Trim();

        return State.Transactions.Where(item =>
            item.Date >= since
            && (accountFilter.Length == 0 || item.AccountId == accountFilter)
            && (categoryFilter.Length == 0 || item.CategoryId == categoryFilter)
            && (term.Length == 0 || item.Description.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    private void OnAccountChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        accountFilter = State.FindAccount(value) is null ? string.Empty : value;
        ResetPaging();
    }

    private void OnCategoryChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        categoryFilter = State.Categories.Any(category => category.Id == value) ? value : string.Empty;
        ResetPaging();
    }

    private void OnPeriodChanged(FintechPeriod value)
    {
        if (period == value || !Enum.IsDefined(value))
            return;

        period = value;
        ResetPaging();
    }

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value ?? string.Empty;
        OnSearchValue(searchBoxValue);
    }

    private void OnSearchInput(ChangeEventArgs args) => OnSearchValue(args.Value?.ToString() ?? string.Empty);

    private void OnSearchValue(string value)
    {
        if (search == value)
            return;

        search = value;
        ResetPaging();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>A changed filter starts the list from the top again.</summary>
    private void ResetPaging()
    {
        shownCount = PageSize;
        firstNewIndex = -1;
    }

    private void ShowMore()
    {
        firstNewIndex = shownCount;
        shownCount += PageSize;
        focusFirstNew = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the account to preselect, from <c>?account=</c>; an unknown value shows all accounts.</summary>
    [SupplyParameterFromQuery(Name = "account")] public string? Account { get; set; }

    /// <summary>Gets the table caption: what the rows are, with the filters applied.</summary>
    private string CaptionText
    {
        get
        {
            var parts = new List<string> { $"Transactions in the last {(int)period} days" };
            parts.Add(accountFilter.Length == 0 ? "all accounts" : State.FindAccount(accountFilter)?.Name ?? "all accounts");

            if (categoryFilter.Length > 0)
                parts.Add(State.FindCategory(categoryFilter).Name);

            if (search.Trim().Length > 0)
                parts.Add($"matching \"{search.Trim()}\"");

            return string.Join(", ", parts) + ", newest first";
        }
    }

    private int FilteredCount => Filtered().Count();

    private bool HasFilters => accountFilter.Length > 0 || categoryFilter.Length > 0 || search.Trim().Length > 0;

    private string MatchText => FilteredCount switch
    {
        0 => "None found",
        1 => "1 found",
        var count => string.Create(CultureInfo.InvariantCulture, $"{count} found"),
    };

    private decimal MoneyIn => Filtered().Where(item => item.Amount > 0).Sum(item => item.Amount);

    private decimal MoneyOut => -Filtered().Where(item => item.Amount < 0).Sum(item => item.Amount);

    private IReadOnlyList<FintechTransaction> Shown => Filtered().Take(shownCount).ToList();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
