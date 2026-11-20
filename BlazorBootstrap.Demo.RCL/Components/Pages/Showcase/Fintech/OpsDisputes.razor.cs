using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Operations' list of every customer's disputes in a <see cref="Grid{TItem}" />, newest first, with the count per status
/// (and overdue), paging, sorting, the column chooser, and CSV export. It is searched by number, customer, or merchant and
/// filtered by status (open, the default; overdue; one status; or all) and reason. Each row has the number (linking to the
/// dispute; the whole row opens it), the customer, the merchant, the amount, the reason in words, the status as a text
/// badge with "Overdue" for an open dispute past <see cref="FintechState.DisputeTargetDays" />, the date opened, and the age
/// (how long it has been open, or how long it took to close). The grid's view is saved for the visit and can be reset. The
/// reason and age texts are shared with <see cref="OpsDispute" /> and <see cref="OpsCustomer" />.
/// </summary>
public partial class OpsDisputes : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string OpenValue = "open";

    private const string OverdueValue = "overdue";

    private ElementReference? countRegion;

    private Grid<DisputeRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private int pageSize = DefaultPageSize;

    private string reasonFilter = string.Empty;

    private string search = string.Empty;

    private string searchBoxValue = string.Empty;

    private string statusFilter = OpenValue;

    private string? statusMessage;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await view.LoadAsync();
        pageSize = (await view.GetSettingsAsync()).PageSize;
        viewLoaded = true;
        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "ops-disputes", DefaultPageSize, "opened", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Says how long a dispute has been open, or, once closed, how long it took, in whole days.</summary>
    internal static string AgeText(FintechDispute dispute, DateTime now)
    {
        var days = (int)Math.Floor(((dispute.ClosedAt ?? now) - dispute.CreatedAt).TotalDays);
        var span = days switch
        {
            < 1 => "under a day",
            1 => "1 day",
            _ => $"{days.ToString(CultureInfo.InvariantCulture)} days",
        };

        return dispute.ClosedAt is null ? char.ToUpperInvariant(span[0]) + span[1..] : $"Closed in {span}";
    }

    /// <summary>Returns the operations route of a dispute.</summary>
    internal static string DisputePath(int disputeId) => $"{FintechOpsLayout.Base}/disputes/{disputeId.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>Returns whether a dispute is still open: submitted or under review.</summary>
    internal static bool IsOpen(FintechDispute dispute) => dispute.Status is FintechDisputeStatus.Submitted or FintechDisputeStatus.UnderReview;

    /// <summary>Returns whether a dispute is open and was escalated for passing the answer target.</summary>
    internal static bool IsOverdue(FintechDispute dispute) => IsOpen(dispute) && dispute.EscalatedAt is not null;

    /// <summary>Returns a dispute reason in words, as operations read it.</summary>
    internal static string ReasonText(FintechDisputeReason reason) => reason switch
    {
        FintechDisputeReason.NotRecognized => "Doesn't recognize the charge",
        FintechDisputeReason.NotReceived => "Didn't get what they paid for",
        FintechDisputeReason.Duplicate => "Charged twice",
        FintechDisputeReason.WrongAmount => "Wrong amount",
        FintechDisputeReason.Cancelled => "Cancelled, but still charged",
        _ => "Other",
    };

    #endregion

    #region Private Methods

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = reasonFilter = string.Empty;
        statusFilter = OpenValue;
        await FiltersChangedAsync();
        await FintechDisplay.FocusAsync(countRegion);
    }

    private int Count(string status) => All.Count(dispute => MatchesStatus(dispute, status));

    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private static bool MatchesStatus(FintechDispute dispute, string status) => status switch
    {
        OpenValue => IsOpen(dispute),
        OverdueValue => IsOverdue(dispute),
        "" => true,
        _ => dispute.Status.ToString() == status,
    };

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

    private Task<GridDataProviderResult<DisputeRow>> ProvideRowsAsync(GridDataProviderRequest<DisputeRow> request) => view.ApplyAsync(request, Shown);

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
        statusMessage = statusMessage == "The view is reset: newest first, 25 disputes a page, and every column shown."
            ? "The view is reset again: newest first, 25 disputes a page, and every column shown."
            : "The view is reset: newest first, 25 disputes a page, and every column shown.";
        StateHasChanged();

        await Task.Yield();
        await FintechDisplay.FocusAsync(countRegion);
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

    private IReadOnlyList<FintechDispute> All => State.AllDisputes;

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || reasonFilter.Length > 0 || statusFilter != OpenValue;

    /// <summary>Gets the disputes that pass every filter, as grid rows.</summary>
    private IReadOnlyList<DisputeRow> Shown
    {
        get
        {
            var term = search.Trim();
            var now = State.Clock.Now;

            return All
                .Where(dispute => MatchesStatus(dispute, statusFilter) && (reasonFilter.Length == 0 || dispute.Reason.ToString() == reasonFilter))
                .Select(dispute => (Dispute: dispute, Customer: State.FindCustomer(dispute.CustomerId)?.Name ?? dispute.CustomerId))
                .Where(item => term.Length == 0 || item.Dispute.Id.ToString(CultureInfo.InvariantCulture).Contains(term, StringComparison.Ordinal)
                    || item.Customer.Contains(term, StringComparison.OrdinalIgnoreCase) || item.Dispute.Merchant.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(item => new DisputeRow(item.Dispute.Id, item.Customer, item.Dispute.Merchant, item.Dispute.Amount, ReasonText(item.Dispute.Reason),
                    Disputes.StatusText(item.Dispute.Status), item.Dispute.Status, IsOverdue(item.Dispute), item.Dispute.CreatedAt,
                    ((item.Dispute.ClosedAt ?? now) - item.Dispute.CreatedAt).TotalDays, AgeText(item.Dispute, now)))
                .ToList();
        }
    }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One dispute as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record DisputeRow(int Id, string Customer, string Merchant, decimal Amount, string Reason, string Status, FintechDisputeStatus StatusValue, bool Overdue,
        DateTime Opened, double AgeDays, string Age);

    #endregion
}
