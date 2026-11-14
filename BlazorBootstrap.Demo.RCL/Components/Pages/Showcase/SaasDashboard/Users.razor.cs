using BlazorBootstrap.Demo.RCL.Models.Showcase.SaasDashboard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.SaasDashboard;

/// <summary>
/// Users screen: a searchable, sortable, paged grid of accounts with add, edit, and suspend actions.
/// </summary>
public partial class Users : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, DropdownToggleButton> actionToggles = new();

    private Button addButton = default!;

    private SaasAccountEditor editor = default!;

    private Grid<SaasAccount> grid = default!;

    private string planFilter = string.Empty;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private string statusFilter = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && New == 1)
        {
            Navigation.NavigateTo($"{SaasDashboardLayout.Base}/users", replace: true);
            await editor.AddAsync(() => addButton.Element);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private Task<GridDataProviderResult<SaasAccount>> AccountsDataProvider(GridDataProviderRequest<SaasAccount> request)
    {
        return Task.FromResult(request.ApplyTo(FilteredAccounts().ToList()));
    }

    private ElementReference? ActionToggleFor(int accountId) =>
        actionToggles.TryGetValue(accountId, out var toggle) ? toggle.Element : null;

    private Task AddAsync() => editor.AddAsync(() => addButton.Element);

    /// <summary>The menu item that opens the dialog is hidden by the time it closes, so focus returns to the row's menu button.</summary>
    private Task EditAsync(SaasAccount account) => editor.EditAsync(account, () => ActionToggleFor(account.Id));

    private IEnumerable<SaasAccount> FilteredAccounts()
    {
        IEnumerable<SaasAccount> accounts = State.Accounts;
        var term = search.Trim();

        if (term.Length > 0)
            accounts = accounts.Where(account =>
                account.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || account.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                || account.Company.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (Enum.TryParse<SaasAccountStatus>(statusFilter, out var status))
            accounts = accounts.Where(account => account.Status == status);

        if (planFilter.Length > 0)
            accounts = accounts.Where(account => account.PlanId == planFilter);

        return accounts;
    }

    private async Task OnPlanChanged(ChangeEventArgs args)
    {
        planFilter = args.Value?.ToString() ?? string.Empty;
        await RefreshAsync();
    }

    private async Task OnSavedAsync(SaasAccountSavedEventArgs args)
    {
        if (args.IsNew)
        {
            search = string.Empty;
            searchBoxValue = string.Empty;
            statusFilter = string.Empty;
            planFilter = string.Empty;
        }

        await RefreshAsync();
    }

    private async Task OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    /// <summary>
    /// Filters on every keystroke without changing the box's bound value. On Blazor Server a render answering an earlier
    /// keystroke would otherwise write that older text back into the box and drop what was typed since.
    /// </summary>
    private async Task OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

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

    private async Task OnStatusChanged(ChangeEventArgs args)
    {
        statusFilter = args.Value?.ToString() ?? string.Empty;
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        await grid.ResetPageNumber();
        StateHasChanged();
    }

    /// <summary>Suspends or reactivates an account; the chosen menu item disappears, so focus returns to the row's menu button.</summary>
    private async Task SetStatusAsync(SaasAccount account, SaasAccountStatus status)
    {
        State.UpdateAccount(account with { Status = status });
        await SaasDisplay.FocusAsync(ActionToggleFor(account.Id));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string MatchText => FilteredAccounts().Count() is var matches && matches == State.Accounts.Count
        ? $"Showing all {matches} accounts"
        : $"{matches} of {State.Accounts.Count} accounts match";

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets or sets the query flag that opens the add-user dialog (<c>?new=1</c>), used by the overview's "Add user" button.</summary>
    [SupplyParameterFromQuery(Name = "new")] public int? New { get; set; }

    [Inject] private SaasDashboardState State { get; set; } = default!;

    #endregion
}
