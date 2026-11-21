using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's Orders module, in two views. The list is a <see cref="Grid{TItem}" /> of every order (number linking to
/// the order, placed, customer, email, items, total, status, delivery, coupon, and refunded) with paging, sorting, the
/// column chooser, and CSV export, searched by number, name, or email and filtered by status (preselected by
/// <c>?status=</c>); its view is saved for the visit and can be reset. Selected orders move on together from a bulk bar:
/// mark placed ones packed, print the carrier's labels and ship packed ones, or mark shipped ones delivered
/// (<see cref="EcommerceState.BulkAdvanceOrders" />). The fulfilment board shows the orders to pack, to ship, and in
/// transit, each with its one next step; shipping one asks for a tracking number in a <see cref="Modal" />.
/// </summary>
public partial class AdminOrders : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private static readonly (EcommerceOrderStatus Status, string Title, string EmptyText)[] boardColumns =
    {
        (EcommerceOrderStatus.Placed, "To pack", "Nothing to pack. New orders land here."),
        (EcommerceOrderStatus.Packed, "To ship", "Nothing waiting for the carrier."),
        (EcommerceOrderStatus.Shipped, "In transit", "No orders on the way."),
    };

    private readonly Dictionary<int, Button> shipButtons = new();

    private string? appliedStatusQuery;

    private ElementReference? countRegion;

    private string? errorMessage;

    private bool focusStatus;

    private Grid<OrderRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string? lastAnnouncement;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private int repeatCount;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The orders selected in the grid (on its current page).</summary>
    private List<OrderRow> selected = new();

    private EditContext shipContext = default!;

    private bool shipped;

    private ShipForm shipForm = new();

    private Modal shipModal = default!;

    private EcommerceOrder? shipping;

    private bool showBoard;

    private string statusFilter = string.Empty;

    private string? statusMessage;

    private ElementReference statusRegion;

    private TextInput? trackingInput;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusStatus)
        {
            focusStatus = false;
            await EcommerceDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "orders", DefaultPageSize, "placed", SortDirection.Descending);
        shipContext = new EditContext(shipForm);
        State.Changed += OnStateChanged;
    }

    /// <summary>Applies <c>?status=</c> when it changes (also when the page is already open and only the query changes).</summary>
    protected override async Task OnParametersSetAsync()
    {
        if (StatusQuery == appliedStatusQuery)
            return;

        appliedStatusQuery = StatusQuery;
        statusFilter = ParseStatus(StatusQuery)?.ToString() ?? string.Empty;

        if (statusFilter.Length > 0)
            showBoard = false;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static EcommerceOrderStatus? ParseStatus(string? value) =>
        Enum.TryParse<EcommerceOrderStatus>(value, ignoreCase: true, out var status) && Enum.IsDefined(status) && !int.TryParse(value, out _)
            ? status
            : null;

    /// <summary>
    /// Moves an order one step on from the board. The card leaves its column with its button, so focus moves to the status
    /// message that announces the move.
    /// </summary>
    private async Task AdvanceAsync(EcommerceOrder order, EcommerceOrderStatus expected)
    {
        errorMessage = null;

        if (State.FindOrder(order.Id)?.Status != expected || !State.AdvanceOrder(order.Id))
        {
            errorMessage = $"Order #{order.Id} couldn't be moved: its status changed. The board now shows where it is.";
            await EcommerceDisplay.FocusAsync(statusRegion);

            return;
        }

        Announce(expected == EcommerceOrderStatus.Placed ? $"Order #{order.Id} marked packed." : $"Order #{order.Id} marked delivered.");
        await EcommerceDisplay.FocusAsync(statusRegion);
    }

    /// <summary>Sets the status message; a repeat of the same message gets a counter, so screen readers announce it again.</summary>
    private void Announce(string message)
    {
        repeatCount = message == lastAnnouncement ? repeatCount + 1 : 1;
        lastAnnouncement = message;
        statusMessage = repeatCount > 1 ? $"{message} ({repeatCount})" : message;
    }

    /// <summary>
    /// Moves the selected orders in one status on together with <see cref="EcommerceState.BulkAdvanceOrders" />, then refreshes
    /// the grid and clears the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync(EcommerceOrderStatus from)
    {
        errorMessage = null;

        var ids = selected.Where(row => row.StatusValue == from).Select(row => row.Id).ToList();
        var outcome = State.BulkAdvanceOrders(ids, from);
        var done = from switch
        {
            EcommerceOrderStatus.Placed => "packed",
            EcommerceOrderStatus.Packed => "shipped with the carrier's labels",
            _ => "delivered",
        };
        var others = selected.Count - ids.Count;

        switch (outcome.Result)
        {
            case EcommerceBulkResult.Ok:
                Announce($"Marked {EcommerceDisplay.Plural(outcome.Changed, "order")} {done}."
                    + (outcome.Unchanged > 0 ? $" {EcommerceDisplay.Plural(outcome.Unchanged, "order was", "orders were")} no longer in that status." : string.Empty)
                    + (others > 0 ? $" {EcommerceDisplay.Plural(others, "other selected order")} stayed as {(others == 1 ? "it was" : "they were")}." : string.Empty));
                break;
            case EcommerceBulkResult.NoChange:
                Announce("No orders changed: the selected orders had already moved on.");
                break;
            case EcommerceBulkResult.NotAllowed:
                errorMessage = "Only the store team can move orders on. Nothing changed.";
                break;
            default:
                errorMessage = "Those orders couldn't be moved on. Nothing changed.";
                break;
        }

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<OrderRow>();
        focusStatus = true;
        StateHasChanged();
    }

    private List<EcommerceOrder> BoardOrders(EcommerceOrderStatus status) => State.Orders
        .Where(order => order.Status == status)
        .OrderBy(order => order.PlacedOn)
        .ThenBy(order => order.Id)
        .ToList();

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = string.Empty;
        await RefreshAsync();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<OrderRow>();
        Announce("Selection cleared.");
        focusStatus = true;
    }

    private async Task ConfirmShipAsync()
    {
        if (shipping is null)
            return;

        var id = shipping.Id;
        var tracking = shipForm.TrackingNumber;

        errorMessage = null;

        if (State.FindOrder(id)?.Status == EcommerceOrderStatus.Packed && State.AdvanceOrder(id, tracking))
        {
            shipped = true;
            Announce($"Order #{id} shipped with tracking number {tracking}.");
        }
        else
        {
            errorMessage = $"Order #{id} couldn't be shipped: it is no longer waiting for the carrier.";
        }

        await shipModal.HideAsync();
    }

    private int CountOf(EcommerceOrderStatus status) => State.Orders.Count(order => order.Status == status);

    private IEnumerable<EcommerceOrder> FilteredOrders()
    {
        IEnumerable<EcommerceOrder> orders = State.Orders;
        var term = search.Trim().TrimStart('#');

        if (term.Length > 0)
            orders = orders.Where(order =>
                order.Id.ToString(CultureInfo.InvariantCulture).Contains(term, StringComparison.Ordinal)
                || order.ShipTo.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || order.ShipTo.Email.Contains(term, StringComparison.OrdinalIgnoreCase));

        if (ParseStatus(statusFilter) is { } status)
            orders = orders.Where(order => order.Status == status);

        return orders;
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
    /// Filters on every keystroke without changing the box's bound value, so a render answering an earlier keystroke never
    /// writes older text back into the box.
    /// </summary>
    private async Task OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    private void OnSelectionChanged(HashSet<OrderRow> items) => selected = items.ToList();

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. After shipping, the card's "Ship…" button is gone,
    /// so focus goes to the status message; otherwise back to the button that opened the dialog.
    /// </summary>
    private async Task OnShipHiddenAsync()
    {
        var orderId = shipping?.Id;
        var wasShipped = shipped;

        shipping = null;
        shipped = false;

        if (wasShipped || errorMessage is not null)
            await EcommerceDisplay.FocusAsync(statusRegion);
        else if (orderId is int id && shipButtons.TryGetValue(id, out var button))
            await EcommerceDisplay.FocusAsync(button.Element);
    }

    private Task OnShipShownAsync() => EcommerceDisplay.FocusAsync(trackingInput?.Element);

    /// <summary>Re-queries the grid (which doesn't do so on its own) and re-renders the board and the counts.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusChanged(ChangeEventArgs args)
    {
        statusFilter = args.Value?.ToString() ?? string.Empty;
        await RefreshAsync();
    }

    /// <summary>Opens the ship dialog with the order's tracking number filled in, so the demo is quick.</summary>
    private async Task OpenShipAsync(EcommerceOrder order)
    {
        errorMessage = null;
        shipping = order;
        shipped = false;
        shipForm = new ShipForm { TrackingNumber = EcommerceSeedData.TrackingNumberFor(order.Id) };
        shipContext = new EditContext(shipForm);

        await shipModal.ShowAsync();
    }

    /// <summary>The page of orders the grid asked for: newest first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<OrderRow>> ProvideOrdersAsync(GridDataProviderRequest<OrderRow> request) =>
        view.ApplyAsync(request, FilteredOrders().Select(ToRow).ToList());

    /// <summary>A filter changed: back to the first page of the filtered orders.</summary>
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
        selected = new List<OrderRow>();
        gridVersion++;
        Announce("The view is reset: newest orders first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    private string RowClass(OrderRow row) => selected.Any(item => item.Id == row.Id) ? "ea-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private static OrderRow ToRow(EcommerceOrder order) => new(
        order.Id,
        $"#{order.Id.ToString(CultureInfo.InvariantCulture)}",
        order.PlacedOn,
        order.ShipTo.FullName,
        order.ShipTo.Email,
        order.Totals.ItemCount,
        order.Totals.Total,
        order.Status.ToString(),
        order.Status,
        order.Delivery.ToString(),
        order.PromoCode ?? string.Empty,
        order.RefundedAmount);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0;

    private string MatchText => FilteredOrders().Count() is var matches && matches == State.Orders.Count
        ? $"Showing all {EcommerceDisplay.Plural(matches, "order")}."
        : $"{EcommerceDisplay.Count(matches)} of {EcommerceDisplay.Plural(State.Orders.Count, "order")} match.";

    private string OrdersSummary
    {
        get
        {
            var toPack = CountOf(EcommerceOrderStatus.Placed);
            var toShip = CountOf(EcommerceOrderStatus.Packed);
            var inTransit = CountOf(EcommerceOrderStatus.Shipped);

            return $"{EcommerceDisplay.Plural(State.Orders.Count, "order")} in the last year · {toPack} to pack · {toShip} to ship · {inTransit} in transit";
        }
    }

    [Inject] private EcommerceState State { get; set; } = default!;

    /// <summary>Gets or sets the status the list opens filtered to (<c>?status=placed</c>, for example); empty shows all.</summary>
    [SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>One order as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record OrderRow(
        int Id,
        string Number,
        DateTime PlacedOn,
        string Customer,
        string Email,
        int Items,
        decimal Total,
        string Status,
        EcommerceOrderStatus StatusValue,
        string Delivery,
        string Coupon,
        decimal Refunded)
    {
        /// <summary>Gets the placed time as it is exported, like "2026-11-09 08:12".</summary>
        public string PlacedStamp => PlacedOn.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }

    private sealed class ShipForm
    {
        private string trackingNumber = string.Empty;

        [Required(ErrorMessage = "Enter the tracking number.")]
        [StringLength(40, MinimumLength = 4, ErrorMessage = "Use 4 to 40 characters for the tracking number.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9 \-]*$", ErrorMessage = "Use letters, digits, spaces, and dashes only in the tracking number.")]
        public string TrackingNumber
        {
            get => trackingNumber;
            set => trackingNumber = value?.Trim() ?? string.Empty;
        }
    }

    #endregion
}
