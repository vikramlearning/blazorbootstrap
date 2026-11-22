using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The operations review queue in a <see cref="Grid{TItem}" />: every transfer held for a check
/// (<see cref="FintechTransferStatus.Held" />), oldest first (first in, first out), with the reference (linking to
/// <see cref="OpsTransfer" />; the whole row opens it), the customer (with a Watch badge), the amount, the payee, the
/// transfer date, the hold reason, and how long it has waited, with paging, sorting, the column chooser, and CSV export. It
/// is searched by reference, customer, or payee and filtered by hold reason; "Show" switches to the scheduled transfers that
/// aren't held yet. The grid's view is saved for the visit and can be reset. Rows are selected with the grid's checkboxes and
/// changed together from a bulk bar, an <see cref="EditForm" /> (rule WR-002): release (approve) with an optional note, or
/// reject with the reason customers read, for held transfers; hold for review with a reason, for scheduled ones. The URL and
/// waiting-time helpers are shared with <see cref="OpsTransfer" /> and <see cref="OpsScheduled" />.
/// </summary>
public partial class OpsQueue : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string HeldValue = "held";

    private const string HoldAction = "hold";

    private const string RejectAction = "reject";

    private const string ReleaseAction = "release";

    private const string ScheduledValue = "scheduled";

    private EditContext bulkContext = default!;

    private BulkForm bulkForm = new();

    private ElementReference? countRegion;

    private bool focusStatus;

    private Grid<QueueRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string reasonFilter = string.Empty;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The transfers selected in the grid (on its current page).</summary>
    private List<QueueRow> selected = new();

    private int statusCount;

    private string statusFilter = HeldValue;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

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
            await FintechDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "review-queue", DefaultPageSize, "waiting", SortDirection.Ascending);
        ResetBulkForm();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the operations page of a customer.</summary>
    internal static string CustomerUrl(string customerId) => $"{FintechOpsLayout.Base}/customers/{Uri.EscapeDataString(customerId)}";

    /// <summary>Returns the operations page of a transfer.</summary>
    internal static string TransferUrl(string reference) => $"{FintechOpsLayout.Base}/transfers/{Uri.EscapeDataString(reference)}";

    /// <summary>Says how long something has waited, like "45 min", "3 h", or "2 d 4 h".</summary>
    internal static string WaitText(DateTime since, DateTime now)
    {
        var waited = now - since;

        if (waited < TimeSpan.FromMinutes(1))
            return "Under 1 min";

        if (waited < TimeSpan.FromHours(1))
            return string.Create(CultureInfo.InvariantCulture, $"{(int)waited.TotalMinutes} min");

        if (waited < TimeSpan.FromDays(1))
            return string.Create(CultureInfo.InvariantCulture, $"{(int)waited.TotalHours} h");

        return waited.Hours == 0
            ? string.Create(CultureInfo.InvariantCulture, $"{waited.Days} d")
            : string.Create(CultureInfo.InvariantCulture, $"{waited.Days} d {waited.Hours} h");
    }

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string CountText(int count) => $"{Count(count)} waiting";

    /// <summary>
    /// Applies the bulk bar's action to the selected transfers in one call, once the form is valid, then refreshes the grid
    /// and clears the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync()
    {
        bulkForm.Scheduled = statusFilter == ScheduledValue;

        if (!bulkContext.Validate())
            return;

        var references = selected.Select(row => row.Reference).ToList();
        var reason = bulkForm.Reason?.Trim() ?? string.Empty;
        var action = bulkForm.Action;
        var outcome = action switch
        {
            ReleaseAction => State.BulkApproveTransfers(references, reason.Length == 0 ? null : reason),
            RejectAction => State.BulkRejectTransfers(references, reason),
            _ => State.BulkHoldTransfers(references, reason),
        };
        var done = action switch { ReleaseAction => "Released", RejectAction => "Rejected", _ => "Held for review" };
        var message = outcome.Result switch
        {
            FintechResult.Ok => $"{done}: {FintechDisplay.Plural(outcome.Changed, "transfer", "transfers")}. The customers are told."
                + (outcome.Unchanged > 0 ? $" {FintechDisplay.Plural(outcome.Unchanged, "transfer was", "transfers were")} left as they were: already decided, or the account no longer holds the money." : string.Empty),
            FintechResult.NoChange => "No transfers changed: they were already decided meanwhile, or the accounts no longer hold the money.",
            FintechResult.NotAllowed => "Sign in again as operations, with a one-time code, to change transfers. Nothing changed.",
            _ => "Those changes couldn't be saved. Nothing changed.",
        };

        SetStatus(message, outcome.Result is not (FintechResult.Ok or FintechResult.NoChange));
        ResetBulkForm();

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<QueueRow>();
        focusStatus = true;
        StateHasChanged();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = reasonFilter = string.Empty;
        await FiltersChangedAsync();
        await FintechDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<QueueRow>();
        ResetBulkForm();
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    /// <summary>A filter changed: back to the first page of the filtered transfers.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    /// <summary>Filters on every keystroke without changing the box's bound value, so a slower render never overwrites newer typing.</summary>
    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<QueueRow> items) => selected = items.ToList();

    /// <summary>Another screen or a bulk change changed the state: the grid reads the transfers again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>Held and scheduled transfers take different actions, so switching clears the selection and the bulk bar.</summary>
    private async Task OnStatusChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() == ScheduledValue ? ScheduledValue : HeldValue;

        if (value == statusFilter)
            return;

        statusFilter = value;
        reasonFilter = string.Empty;

        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<QueueRow>();
        ResetBulkForm();
        await FiltersChangedAsync();
    }

    /// <summary>The page of transfers the grid asked for, from the filtered list, oldest first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<QueueRow>> ProvideRowsAsync(GridDataProviderRequest<QueueRow> request) => view.ApplyAsync(request, Rows());

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private void ResetBulkForm()
    {
        bulkForm = new BulkForm { Scheduled = statusFilter == ScheduledValue };
        bulkContext = new EditContext(bulkForm);
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<QueueRow>();
        gridVersion++;
        SetStatus("The view is reset: oldest first, 25 transfers a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await FintechDisplay.FocusAsync(countRegion);
    }

    private string RowClass(QueueRow row) => selected.Any(item => item.Reference == row.Reference) ? "tm-row-selected" : string.Empty;

    /// <summary>Returns the transfers that pass every filter (only the Show filter when <paramref name="byStatusOnly" /> is set), as grid rows.</summary>
    private List<QueueRow> Rows(bool byStatusOnly = false)
    {
        var status = statusFilter == ScheduledValue ? FintechTransferStatus.Scheduled : FintechTransferStatus.Held;
        var term = byStatusOnly ? string.Empty : search.Trim();
        var now = State.Clock.Now;
        var watched = State.Customers.Where(customer => customer.Status == FintechCustomerStatus.Watch).Select(customer => customer.Id).ToHashSet(StringComparer.Ordinal);

        return State.AllTransfers
            .Where(transfer => transfer.Status == status
                && (term.Length == 0 || transfer.Reference.Contains(term, StringComparison.OrdinalIgnoreCase) || transfer.CustomerName.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || transfer.ToName.Contains(term, StringComparison.OrdinalIgnoreCase))
                && (byStatusOnly || reasonFilter.Length == 0 || ReasonGroup(transfer.HoldReason) == reasonFilter))
            .OrderBy(transfer => transfer.CreatedAt)
            .Select(transfer => new QueueRow(transfer.Reference, transfer.CustomerName, watched.Contains(transfer.CustomerId), transfer.Amount, transfer.ToName, transfer.Date,
                transfer.HoldReason ?? string.Empty, transfer.CreatedAt, WaitText(transfer.CreatedAt, now)))
            .ToList();
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Groups a hold reason for the filter: operations' own holds share one entry.</summary>
    private static string ReasonGroup(string? reason) =>
        reason is null ? string.Empty : reason.StartsWith("Held by operations", StringComparison.Ordinal) ? "Held by operations" : reason;

    #endregion

    #endregion

    #region Properties, Indexers

    private string ActionHelp => statusFilter == HeldValue
        ? "Releasing sends each transfer now, or schedules it for its date. Rejecting stops it for good; customers read your reason."
        : "Holding moves each transfer to the review queue; it isn't sent until someone releases it. Customers are told.";

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private int HeldCount => State.AllTransfers.Count(transfer => transfer.Status == FintechTransferStatus.Held);

    /// <summary>Gets the hold reasons of the held transfers, for the filter.</summary>
    private IReadOnlyList<string> HoldReasons => State.AllTransfers.Where(transfer => transfer.Status == FintechTransferStatus.Held)
        .Select(transfer => ReasonGroup(transfer.HoldReason)).Where(reason => reason.Length > 0).Distinct().Order(StringComparer.Ordinal).ToList();

    private bool IsFiltered => search.Trim().Length > 0 || reasonFilter.Length > 0;

    private string ReasonHelp => bulkForm.Action switch
    {
        RejectAction => $"Required: {FintechState.MinReason} to {FintechState.MaxRejectReason} characters. Every selected customer reads it.",
        HoldAction => $"Required: {FintechState.MinReason} to {FintechState.MaxHoldReason} characters. Customers see it as the hold reason.",
        ReleaseAction => $"Optional internal note, up to {FintechState.MaxApproveNote} characters, for the audit log.",
        _ => "Required to reject or hold; an optional note to release.",
    };

    private string ReasonLabel => bulkForm.Action == ReleaseAction ? "Internal note (optional)" : "Reason";

    private int ScheduledCount => State.AllTransfers.Count(transfer => transfer.Status == FintechTransferStatus.Scheduled);

    private string ShownText
    {
        get
        {
            var shown = Rows().Count;
            var total = Rows(byStatusOnly: true).Count;
            var noun = statusFilter == HeldValue ? "held transfers" : "scheduled transfers";

            return $"Showing {Count(shown)} of {Count(total)} {noun}.";
        }
    }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The bulk bar's fields. The reason is required to reject (10–300 characters) or hold (10–100 characters), and an optional
    /// note of up to 300 characters to release: a cross-field rule on the chosen action.
    /// </summary>
    private sealed class BulkForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose what to do with the selected transfers.")]
        public string Action { get; set; } = string.Empty;

        [StringLength(FintechState.MaxRejectReason, ErrorMessage = "Keep it to 300 characters.")]
        public string? Reason { get; set; }

        /// <summary>Whether the selection is of scheduled transfers (only Hold applies); set by the page.</summary>
        public bool Scheduled { get; set; }

        /// <summary>Runs once the action is chosen: it fits the selection, and the reason fits the action.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var length = Reason?.Trim().Length ?? 0;

            if (Scheduled ? Action != HoldAction : Action is not (ReleaseAction or RejectAction))
                yield return new ValidationResult(Scheduled ? "Scheduled transfers can only be held for review." : "Held transfers can be released or rejected.", new[] { nameof(Action) });
            else if (Action == RejectAction && length is < FintechState.MinReason or > FintechState.MaxRejectReason)
                yield return new ValidationResult(length == 0 ? "Write the reason the customers will read." : "Write a reason of 10 to 300 characters.", new[] { nameof(Reason) });
            else if (Action == HoldAction && length is < FintechState.MinReason or > FintechState.MaxHoldReason)
                yield return new ValidationResult(length == 0 ? "Write why you're holding these transfers." : "Write a reason of 10 to 100 characters.", new[] { nameof(Reason) });
        }
    }

    /// <summary>One transfer as the queue shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record QueueRow(string Reference, string Customer, bool OnWatch, decimal Amount, string To, DateOnly Date, string HoldReason, DateTime ConfirmedAt, string Waiting);

    #endregion
}
