using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's customers: everyone who registered or placed an order, with their account type, order count, total
/// spent, and last order, searchable by name or email, sortable by spend or last order, and paged. Choosing a customer
/// (or opening the page with <c>?email=</c>) shows their orders beside the list, each linking to the order screen.
/// </summary>
public partial class AdminCustomers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int PageSize = 10;

    private readonly Dictionary<string, Button> viewButtons = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The <c>?email=</c> value last applied, so a re-render doesn't undo a selection made on the page.</summary>
    private string? appliedQuery;

    private ElementReference detailHeading;

    private ElementReference listHeading;

    /// <summary>The element to focus once the next render has put it on the page.</summary>
    private Func<ElementReference?>? pendingFocus;

    private int page = 1;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private string? selectedEmail;

    private CustomerSort sort = CustomerSort.LastOrderNewest;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (pendingFocus is { } target)
        {
            pendingFocus = null;
            await EcommerceDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

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

    private static string OrderCountText(int count) => count == 1 ? "1 order" : $"{count} orders";

    /// <summary>Closes the detail panel; focus returns to the customer's "View orders" button, or to the list's heading when that row isn't shown.</summary>
    private void CloseDetail()
    {
        var email = selectedEmail;
        selectedEmail = null;

        if (email is not null
            && PageItems.Any(customer => string.Equals(customer.Email, email, StringComparison.OrdinalIgnoreCase))
            && viewButtons.TryGetValue(email, out var button))
            pendingFocus = () => button.Element;
        else
            pendingFocus = () => listHeading;
    }

    private bool IsSelected(EcommerceCustomerSummary customer) => string.Equals(customer.Email, selectedEmail, StringComparison.OrdinalIgnoreCase);

    private void OnPageChanged(int value) => page = value;

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;
        page = 1;
    }

    /// <summary>
    /// Filters on every keystroke without changing the box's bound value. On Blazor Server a render answering an earlier
    /// keystroke would otherwise write that older text back into the box and drop what was typed since.
    /// </summary>
    private void OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        page = 1;
    }

    private void OnSortChanged(ChangeEventArgs args)
    {
        if (Enum.TryParse<CustomerSort>(args.Value?.ToString(), out var value) && Enum.IsDefined(value))
        {
            sort = value;
            page = 1;
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Shows a customer's orders and moves focus to the panel's heading, which a screen reader then reads.</summary>
    private void Select(EcommerceCustomerSummary customer)
    {
        selectedEmail = customer.Email;
        pendingFocus = () => detailHeading;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the email address of the customer to show, from <c>?email=</c>, used by links from other admin screens.</summary>
    [SupplyParameterFromQuery(Name = "email")] public string? Email { get; set; }

    private string LeadText
    {
        get
        {
            var customers = State.Customers();
            var registered = customers.Count(customer => customer.AccountId is not null);

            return $"{customers.Count} {(customers.Count == 1 ? "customer" : "customers")} · {registered} registered · {customers.Count - registered} guest checkout";
        }
    }

    private string MatchText
    {
        get
        {
            var total = State.Customers().Count;
            var count = Matches.Count;

            if (count == total)
                return TotalPages > 1 ? $"Showing all {total} customers · page {CurrentPage} of {TotalPages}" : $"Showing all {total} customers";

            var text = count == 1 ? $"1 of {total} customers matches" : $"{count} of {total} customers match";

            return TotalPages > 1 ? $"{text} · page {CurrentPage} of {TotalPages}" : text;
        }
    }

    private IReadOnlyList<EcommerceCustomerSummary> Matches
    {
        get
        {
            IEnumerable<EcommerceCustomerSummary> customers = State.Customers();
            var term = search.Trim();

            if (term.Length > 0)
                customers = customers.Where(customer =>
                    customer.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || customer.Email.Contains(term, StringComparison.OrdinalIgnoreCase));

            // Customers who haven't ordered yet sort last either way.
            return (sort switch
            {
                CustomerSort.LastOrderOldest => customers.OrderBy(customer => customer.LastOrderOn is null).ThenBy(customer => customer.LastOrderOn),
                CustomerSort.SpentHighest => customers.OrderByDescending(customer => customer.Spent).ThenByDescending(customer => customer.LastOrderOn),
                CustomerSort.SpentLowest => customers.OrderBy(customer => customer.OrderCount == 0).ThenBy(customer => customer.Spent),
                _ => customers.OrderBy(customer => customer.LastOrderOn is null).ThenByDescending(customer => customer.LastOrderOn),
            }).ThenBy(customer => customer.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }

    private int CurrentPage => Math.Clamp(page, 1, TotalPages);

    private IReadOnlyList<EcommerceCustomerSummary> PageItems => Matches.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

    private EcommerceCustomerSummary? SelectedCustomer => selectedEmail is null
        ? null
        : State.Customers().FirstOrDefault(customer => string.Equals(customer.Email, selectedEmail, StringComparison.OrdinalIgnoreCase));

    private IReadOnlyList<EcommerceOrder> SelectedOrders => selectedEmail is null
        ? Array.Empty<EcommerceOrder>()
        : State.Orders
            .Where(order => string.Equals(order.ShipTo.Email, selectedEmail, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(order => order.PlacedOn)
            .ToList();

    private string SortText => sort switch
    {
        CustomerSort.LastOrderOldest => "sorted by last order, oldest first",
        CustomerSort.SpentHighest => "sorted by total spent, highest first",
        CustomerSort.SpentLowest => "sorted by total spent, lowest first",
        _ => "sorted by last order, newest first",
    };

    [Inject] private EcommerceState State { get; set; } = default!;

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(Matches.Count / (double)PageSize));

    #endregion

    #region Nested Types

    private enum CustomerSort
    {
        LastOrderNewest,
        LastOrderOldest,
        SpentHighest,
        SpentLowest,
    }

    #endregion
}
