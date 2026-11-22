using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The retailer's returns queue: return requests in a <see cref="Grid{TItem}" /> (number linking to the return, order,
/// customer, items, amount, reason, requested, status, refunded, and decided) with paging, sorting, the column chooser,
/// and CSV export, searched by return or order number, name, or email and filtered by status (waiting ones by default,
/// oldest first); its view is saved for the visit and can be reset. Selected waiting returns are decided together from a
/// bulk bar: approve with a full refund and an optional note (<see cref="EcommerceState.BulkApproveReturns" />), or reject
/// with a reason the customers read (<see cref="EcommerceState.BulkRejectReturns" />). A return's own page decides it with
/// a partial refund.
/// </summary>
public partial class AdminReturns : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string ApproveValue = "approve";

    private const int DefaultPageSize = 25;

    private const string RejectValue = "reject";

    private EditContext bulkContext = default!;

    private BulkForm bulkForm = new();

    private ElementReference? countRegion;

    private string? errorMessage;

    private bool focusStatus;

    private Grid<ReturnRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The returns selected in the grid (on its current page).</summary>
    private List<ReturnRow> selected = new();

    private int statusCount;

    private string statusFilter = nameof(EcommerceReturnStatus.Requested);

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

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
        view = GridViews.Get(State.SiteSlug, "returns", DefaultPageSize, "requested", SortDirection.Ascending);
        bulkContext = new EditContext(bulkForm);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    internal static BadgeColor StatusColor(EcommerceReturnStatus status) => status switch
    {
        EcommerceReturnStatus.Requested => BadgeColor.Warning,
        EcommerceReturnStatus.Approved => BadgeColor.Success,
        _ => BadgeColor.Danger,
    };

    internal static string StatusLabel(EcommerceReturnStatus status) => status switch
    {
        EcommerceReturnStatus.Requested => "Waiting",
        EcommerceReturnStatus.Approved => "Approved and refunded",
        _ => "Rejected",
    };

    /// <summary>
    /// Decides the selected waiting returns together once the bar is valid (rule WR-002), then refreshes the grid and clears
    /// the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync()
    {
        if (!await bulkContext.ValidateAsync())
            return;

        errorMessage = null;

        var ids = selected.Where(row => row.StatusValue == EcommerceReturnStatus.Requested).Select(row => row.Id).ToList();
        var approve = bulkForm.Decision == ApproveValue;
        var refunded = State.Returns.Where(item => ids.Contains(item.Id)).ToDictionary(item => item.Id, item => item.Status);
        var outcome = approve ? State.BulkApproveReturns(ids, bulkForm.Note) : State.BulkRejectReturns(ids, bulkForm.Note ?? string.Empty);

        switch (outcome.Result)
        {
            case EcommerceBulkResult.Ok:
                var refund = approve ? State.Returns.Where(item => refunded.ContainsKey(item.Id) && item.Status == EcommerceReturnStatus.Approved).Sum(item => item.Refunded) : 0m;
                SetStatus((approve
                        ? $"Approved {EcommerceDisplay.Plural(outcome.Changed, "return")}: items back in stock and {EcommerceDisplay.Money(refund)} refunded."
                        : $"Rejected {EcommerceDisplay.Plural(outcome.Changed, "return")}. Nothing was refunded.")
                    + (outcome.Unchanged > 0 ? $" {EcommerceDisplay.Plural(outcome.Unchanged, "return was", "returns were")} no longer waiting, or {(outcome.Unchanged == 1 ? "its order was" : "their orders were")} refunded already." : string.Empty));
                break;
            case EcommerceBulkResult.NoChange:
                SetStatus("No returns changed: they were decided meanwhile, or their orders were refunded already.");
                break;
            case EcommerceBulkResult.NotAllowed:
                errorMessage = "Only the store team can decide returns. Nothing changed.";
                break;
            default:
                errorMessage = "Those decisions couldn't be saved. Nothing changed.";
                break;
        }

        bulkForm = new BulkForm();
        bulkContext = new EditContext(bulkForm);

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<ReturnRow>();
        focusStatus = true;
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<ReturnRow>();
        SetStatus("Selection cleared.");
        focusStatus = true;
    }

    private IEnumerable<EcommerceReturn> Matching()
    {
        IEnumerable<EcommerceReturn> items = State.Returns;
        var term = search.Trim().TrimStart('#');

        if (term.Length > 0)
        {
            items = items.Where(item =>
            {
                var order = State.FindOrder(item.OrderId);

                return EcommerceDisplay.ReturnNumber(item.Id).Contains(term, StringComparison.OrdinalIgnoreCase)
                    || item.OrderId.ToString(CultureInfo.InvariantCulture).Contains(term, StringComparison.Ordinal)
                    || (order is not null && (order.ShipTo.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) || order.ShipTo.Email.Contains(term, StringComparison.OrdinalIgnoreCase)));
            });
        }

        if (Enum.TryParse<EcommerceReturnStatus>(statusFilter, out var status) && Enum.IsDefined(status))
            items = items.Where(item => item.Status == status);

        return items;
    }

    private async Task OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    /// <summary>Filters on every keystroke without changing the box's bound value, so older text is never written back into it.</summary>
    private async Task OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        await RefreshAsync();
    }

    private void OnSelectionChanged(HashSet<ReturnRow> items) => selected = items.ToList();

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusFilterChanged(ChangeEventArgs args)
    {
        statusFilter = Enum.TryParse<EcommerceReturnStatus>(args.Value?.ToString(), out var status) && Enum.IsDefined(status) ? status.ToString() : string.Empty;
        await RefreshAsync();
    }

    /// <summary>The page of returns the grid asked for: the oldest request first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ReturnRow>> ProvideReturnsAsync(GridDataProviderRequest<ReturnRow> request) =>
        view.ApplyAsync(request, Matching().Select(item => new ReturnRow(
            item.Id,
            EcommerceDisplay.ReturnNumber(item.Id),
            item.OrderId,
            State.FindOrder(item.OrderId)?.ShipTo.FullName ?? "Unknown customer",
            item.Lines.Sum(line => line.Quantity),
            item.Amount,
            item.Reason,
            item.RequestedOn,
            StatusLabel(item.Status),
            item.Status,
            item.Refunded,
            item.DecidedOn)).ToList());

    /// <summary>A filter changed: back to the first page of the filtered returns.</summary>
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
        selected = new List<ReturnRow>();
        gridVersion++;
        SetStatus("The view is reset: the oldest requests first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    private string RowClass(ReturnRow row) => selected.Any(item => item.Id == row.Id) ? "ea-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ShowWaitingAsync()
    {
        search = searchBoxValue = string.Empty;
        statusFilter = nameof(EcommerceReturnStatus.Requested);
        await RefreshAsync();
        await EcommerceDisplay.FocusAsync(countRegion);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string EmptyText => statusFilter == nameof(EcommerceReturnStatus.Requested) && search.Trim().Length == 0
        ? "No returns are waiting for a decision. New requests from customers appear here."
        : "No returns match these filters. Clear the search, or show every status.";

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter != nameof(EcommerceReturnStatus.Requested);

    private string LeadText
    {
        get
        {
            var waiting = State.Returns.Count(item => item.Status == EcommerceReturnStatus.Requested);
            var refunded = State.Returns.Sum(item => item.Refunded);

            return $"{(waiting == 1 ? "1 return waits" : $"{waiting} returns wait")} for a decision · {EcommerceDisplay.Plural(State.Returns.Count, "return")} this year · "
                + $"{EcommerceDisplay.Money(refunded)} refunded on approved returns";
        }
    }

    private string MatchText
    {
        get
        {
            var count = Matching().Count();

            return count == State.Returns.Count ? $"Showing all {EcommerceDisplay.Plural(count, "return")}." : $"{EcommerceDisplay.Count(count)} of {EcommerceDisplay.Plural(State.Returns.Count, "return")} match.";
        }
    }

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The bulk bar's fields: a decision and its note (required, as a reason the customers read, to reject).</summary>
    private sealed class BulkForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose whether to approve or reject the selected returns.")]
        public string Decision { get; set; } = string.Empty;

        [StringLength(EcommerceState.MaxDecisionNoteLength, ErrorMessage = "Use 200 characters or fewer for the note.")]
        public string? Note { get; set; }

        /// <summary>Runs once the fields are filled in: a rejection needs a reason.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Decision == RejectValue && (Note?.Trim().Length ?? 0) < EcommerceState.MinReasonLength)
                yield return new ValidationResult("Tell the customers why the returns are rejected, in 5 to 200 characters.", new[] { nameof(Note) });
        }
    }

    /// <summary>One return as the grid shows and exports it: every exported column is a property.</summary>
    private sealed record ReturnRow(
        int Id,
        string Number,
        int OrderId,
        string Customer,
        int Items,
        decimal Amount,
        string Reason,
        DateTime RequestedOn,
        string Status,
        EcommerceReturnStatus StatusValue,
        decimal Refunded,
        DateTime? DecidedOn)
    {
        /// <summary>Gets the decided time as it is exported, or empty while waiting.</summary>
        public string DecidedText => DecidedOn?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;

        /// <summary>Gets the order number as the grid shows and exports it, like "#10233".</summary>
        public string Order => string.Create(CultureInfo.InvariantCulture, $"#{OrderId}");

        /// <summary>Gets the requested time as it is exported, like "2026-11-09 08:12".</summary>
        public string RequestedText => RequestedOn.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }

    #endregion
}
