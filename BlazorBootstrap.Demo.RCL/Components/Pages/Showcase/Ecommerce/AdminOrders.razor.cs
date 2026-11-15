using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's Orders module, in two views. The list is a searchable, sortable, paged grid of every order with a status
/// filter (preselected by <c>?status=</c>). The fulfilment board shows the orders to pack, to ship, and in transit, each
/// with its one next step; shipping asks for a tracking number in a <see cref="Modal" />. Every change goes through
/// <see cref="EcommerceState" />, so the grid, the board, and the dashboard stay in step.
/// </summary>
public partial class AdminOrders : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly (EcommerceOrderStatus Status, string Title, string EmptyText)[] boardColumns =
    {
        (EcommerceOrderStatus.Placed, "To pack", "Nothing to pack. New orders land here."),
        (EcommerceOrderStatus.Packed, "To ship", "Nothing waiting for the carrier."),
        (EcommerceOrderStatus.Shipped, "In transit", "No orders on the way."),
    };

    private readonly Dictionary<int, Button> shipButtons = new();

    private string? appliedStatusQuery;

    private string? errorMessage;

    private Grid<EcommerceOrder>? grid;

    private string? lastAnnouncement;

    private int repeatCount;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

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

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
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

    private static string OrderUrl(int orderId) => $"{EcommerceAdminLayout.Base}/orders/{orderId}";

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

    private List<EcommerceOrder> BoardOrders(EcommerceOrderStatus status) => State.Orders
        .Where(order => order.Status == status)
        .OrderBy(order => order.PlacedOn)
        .ThenBy(order => order.Id)
        .ToList();

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
        if (grid is not null)
            await grid.RefreshDataAsync();

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

    private Task<GridDataProviderResult<EcommerceOrder>> OrdersDataProvider(GridDataProviderRequest<EcommerceOrder> request) =>
        Task.FromResult(request.ApplyTo(FilteredOrders().ToList()));

    private async Task RefreshAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();

        StateHasChanged();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string MatchText => FilteredOrders().Count() is var matches && matches == State.Orders.Count
        ? $"Showing all {matches} orders"
        : $"{matches} of {State.Orders.Count} orders match";

    private string OrdersSummary
    {
        get
        {
            var toPack = CountOf(EcommerceOrderStatus.Placed);
            var toShip = CountOf(EcommerceOrderStatus.Packed);
            var inTransit = CountOf(EcommerceOrderStatus.Shipped);

            return $"{State.Orders.Count} orders · {toPack} to pack · {toShip} to ship · {inTransit} in transit";
        }
    }

    [Inject] private EcommerceState State { get; set; } = default!;

    /// <summary>Gets or sets the status the list opens filtered to (<c>?status=placed</c>, for example); empty shows all.</summary>
    [SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }

    #endregion

    #region Nested Types

    private sealed class ShipForm
    {
        private string trackingNumber = string.Empty;

        [Required(ErrorMessage = "Enter the tracking number.")]
        [StringLength(40, MinimumLength = 4, ErrorMessage = "Use 4 to 40 characters.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9 \-]*$", ErrorMessage = "Use letters, digits, spaces, and dashes only.")]
        public string TrackingNumber
        {
            get => trackingNumber;
            set => trackingNumber = value?.Trim() ?? string.Empty;
        }
    }

    #endregion
}
