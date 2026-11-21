using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's customers: everyone who registered or placed an order, in a <see cref="Grid{TItem}" /> (name, email,
/// account type, country, orders, total spent, first and last order) with paging, sorting, the column chooser, and CSV
/// export, searched by name or email and filtered by account type and country; its view is saved for the visit and can be
/// reset. A customer has no page of their own: the row's stretched button (or opening the page with <c>?email=</c>, as
/// the header's search does) shows their orders beside the list, each linking to the order screen.
/// </summary>
public partial class AdminCustomers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string Guest = "guest";

    private const string Registered = "registered";

    private readonly Dictionary<string, ElementReference> viewButtons = new(StringComparer.OrdinalIgnoreCase);

    private string accountFilter = string.Empty;

    /// <summary>The <c>?email=</c> value last applied, so a re-render doesn't undo a selection made on the page.</summary>
    private string? appliedQuery;

    private ElementReference? countRegion;

    private string countryFilter = string.Empty;

    private ElementReference detailHeading;

    private Grid<CustomerRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? lastStatus;

    private ElementReference listHeading;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The element to focus once the next render has put it on the page.</summary>
    private Func<ElementReference?>? pendingFocus;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private string? selectedEmail;

    private string? statusMessage;

    private ElementReference statusRegion;

    private int statusRepeats;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid; moves focus after a render that needs it.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (pendingFocus is { } target)
        {
            pendingFocus = null;
            await EcommerceDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "customers", DefaultPageSize, "last", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        var query = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim();

        if (string.Equals(query, appliedQuery, StringComparison.OrdinalIgnoreCase))
            return;

        appliedQuery = query;

        if (query is null)
            return;

        selectedEmail = query.Length > 120 ? query[..120] : query;
        pendingFocus = () => detailHeading;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(string message)
    {
        statusRepeats = message == lastStatus ? statusRepeats + 1 : 1;
        lastStatus = message;
        statusMessage = statusRepeats == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusRepeats})");
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = accountFilter = countryFilter = string.Empty;
        await RefreshAsync();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>Closes the detail panel; focus returns to the customer's row button, or to the list's heading when that row isn't shown.</summary>
    private void CloseDetail()
    {
        var email = selectedEmail;
        selectedEmail = null;

        if (email is not null && viewButtons.TryGetValue(email, out var button))
            pendingFocus = () => button;
        else
            pendingFocus = () => listHeading;
    }

    private bool IsSelected(string email) => string.Equals(email, selectedEmail, StringComparison.OrdinalIgnoreCase);

    private IEnumerable<EcommerceCustomerSummary> Matching()
    {
        IEnumerable<EcommerceCustomerSummary> customers = State.Customers();
        var term = search.Trim();

        if (term.Length > 0)
            customers = customers.Where(customer => customer.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || customer.Email.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (accountFilter == Registered)
            customers = customers.Where(customer => customer.AccountId is not null);
        else if (accountFilter == Guest)
            customers = customers.Where(customer => customer.AccountId is null);

        if (countryFilter.Length > 0)
            customers = customers.Where(customer => customer.Country == countryFilter);

        return customers;
    }

    private async Task OnAccountChanged(ChangeEventArgs args)
    {
        accountFilter = args.Value?.ToString() is { } value && value is Registered or Guest ? value : string.Empty;
        await RefreshAsync();
    }

    private async Task OnCountryChanged(ChangeEventArgs args)
    {
        countryFilter = args.Value?.ToString() is { } value && EcommerceSeedData.Countries.Contains(value) ? value : string.Empty;
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
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of customers the grid asked for: the most recent last order first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<CustomerRow>> ProvideCustomersAsync(GridDataProviderRequest<CustomerRow> request) =>
        view.ApplyAsync(request, Matching().Select(customer => new CustomerRow(
            customer.Email,
            customer.Name,
            customer.AccountId is null ? "Guest" : "Registered",
            customer.AccountId is not null,
            customer.Country ?? "Not known yet",
            customer.OrderCount,
            customer.Spent,
            customer.FirstOrderOn,
            customer.LastOrderOn)).ToList());

    /// <summary>A filter changed: back to the first page of the filtered customers.</summary>
    private async Task RefreshAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();

        StateHasChanged();
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        Announce("The view is reset: the most recent customers first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    private string RowClass(CustomerRow row) => IsSelected(row.Email) ? "ea-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a customer's orders and moves focus to the panel's heading, which a screen reader then reads.</summary>
    private async Task Select(string email)
    {
        selectedEmail = email;
        pendingFocus = () => detailHeading;

        // The row's tint follows the selection.
        await RefreshGridAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the email address of the customer to show, from <c>?email=</c>, used by the header's search and other admin screens.</summary>
    [SupplyParameterFromQuery(Name = "email")] public string? Email { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || accountFilter.Length > 0 || countryFilter.Length > 0;

    private string LeadText
    {
        get
        {
            var customers = State.Customers();
            var registered = customers.Count(customer => customer.AccountId is not null);
            var repeat = customers.Count(customer => customer.OrderCount > 1);

            return $"{EcommerceDisplay.Plural(customers.Count, "customer")} · {EcommerceDisplay.Count(registered)} registered · {EcommerceDisplay.Count(customers.Count - registered)} guest checkout · "
                + $"{EcommerceDisplay.Count(repeat)} ordered more than once";
        }
    }

    private string MatchText
    {
        get
        {
            var total = State.Customers().Count;
            var count = Matching().Count();

            return count == total ? $"Showing all {EcommerceDisplay.Plural(total, "customer")}." : $"{EcommerceDisplay.Count(count)} of {EcommerceDisplay.Plural(total, "customer")} match.";
        }
    }

    private EcommerceCustomerSummary? SelectedCustomer => selectedEmail is null
        ? null
        : State.Customers().FirstOrDefault(customer => string.Equals(customer.Email, selectedEmail, StringComparison.OrdinalIgnoreCase));

    private IReadOnlyList<EcommerceOrder> SelectedOrders => selectedEmail is null
        ? Array.Empty<EcommerceOrder>()
        : State.Orders
            .Where(order => string.Equals(order.ShipTo.Email, selectedEmail, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(order => order.PlacedOn)
            .ToList();

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One customer as the grid shows and exports it: every exported column is a property.</summary>
    private sealed record CustomerRow(
        string Email,
        string Name,
        string Account,
        bool IsRegistered,
        string Country,
        int Orders,
        decimal Spent,
        DateTime? FirstOrderOn,
        DateTime? LastOrderOn)
    {
        /// <summary>Gets the first order's date as it is exported, like "2026-02-14", or empty.</summary>
        public string FirstOrderText => FirstOrderOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

        /// <summary>Gets the last order's time as it is exported, like "2026-11-09 08:12", or empty.</summary>
        public string LastOrderText => LastOrderOn?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    #endregion
}
