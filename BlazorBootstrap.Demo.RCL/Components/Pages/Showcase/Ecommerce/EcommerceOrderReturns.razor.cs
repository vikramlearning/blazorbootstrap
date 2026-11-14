using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Returns on the customer's order page: the order's return requests with the store's decision and note, and for a
/// delivered order a form that picks quantities per item (up to what can still be returned), a reason, and a comment, and
/// sends the request to the store's Returns queue.
/// </summary>
public partial class EcommerceOrderReturns : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly string[] reasons =
    {
        "Doesn't match the photos",
        "Arrived damaged",
        "Wrong item or color",
        "Too big or too small",
        "Changed my mind",
    };

    private string? error;

    private int errors;

    private ReturnForm form = new();

    private ElementReference heading;

    private readonly Dictionary<string, int> quantities = new();

    private bool showForm;

    private string? status;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Key(EcommerceOrderLine line) => FormattableString.Invariant($"{line.ProductId}-{line.Color}");

    private static BadgeColor StatusColor(EcommerceReturnStatus status) => status switch
    {
        EcommerceReturnStatus.Requested => BadgeColor.Warning,
        EcommerceReturnStatus.Approved => BadgeColor.Success,
        _ => BadgeColor.Secondary,
    };

    private static string StatusText(EcommerceReturnStatus status) => status switch
    {
        EcommerceReturnStatus.Requested => "Waiting for the store",
        EcommerceReturnStatus.Approved => "Approved and refunded",
        _ => "Declined",
    };

    private async Task CloseFormAsync()
    {
        showForm = false;
        error = null;
        StateHasChanged();
        await Task.Yield();
        await EcommerceDisplay.FocusAsync(heading);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenFormAsync()
    {
        form = new ReturnForm();
        quantities.Clear();
        error = null;
        showForm = true;
        StateHasChanged();
        await Task.Yield();
        await EcommerceDisplay.FocusAsync(heading);
    }

    private int Quantity(string key) => quantities.TryGetValue(key, out var value) ? value : 0;

    private void SetQuantity(string key, object? value)
    {
        if (int.TryParse(value?.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var quantity))
            quantities[key] = quantity;
    }

    private async Task Submit()
    {
        var items = ReturnableLines
            .Select(entry => (entry.Line, Quantity: Math.Min(Quantity(Key(entry.Line)), entry.Max)))
            .Where(entry => entry.Quantity > 0)
            .Select(entry => (entry.Line.ProductId, entry.Line.Color, entry.Quantity))
            .ToList();

        if (items.Count == 0)
        {
            errors++;
            error = errors == 1 ? "Choose how many of at least one item to return." : $"Choose how many of at least one item to return (attempt {errors}).";

            return;
        }

        if (State.RequestReturn(Order.Id, items, form.Reason, form.Comment) is not { } request)
        {
            errors++;
            error = $"The request couldn't be sent. Check the quantities and try again (attempt {errors}).";

            return;
        }

        showForm = false;
        error = null;
        status = $"Return R-{request.Id.ToString("0000", CultureInfo.InvariantCulture)} is sent. {EcommerceSeedData.StoreName} will review it.";
        StateHasChanged();
        await Task.Yield();
        await EcommerceDisplay.FocusAsync(heading);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the order.</summary>
    [Parameter, EditorRequired] public EcommerceOrder Order { get; set; } = default!;

    private IReadOnlyList<EcommerceReturn> Requests => State.Returns.Where(item => item.OrderId == Order.Id).OrderByDescending(item => item.RequestedOn).ToList();

    private IReadOnlyList<(EcommerceOrderLine Line, int Max)> ReturnableLines => Order.Lines
        .Select(line => (Line: line, Max: State.ReturnableQuantity(Order, line)))
        .Where(entry => entry.Max > 0)
        .ToList();

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class ReturnForm
    {
        [StringLength(500, ErrorMessage = "Use 500 characters or fewer.")]
        public string? Comment { get; set; }

        [Required(ErrorMessage = "Choose a reason.")]
        public string Reason { get; set; } = string.Empty;
    }

    #endregion
}
