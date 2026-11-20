using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The customer's disputes in a <see cref="Grid{TItem}" />, newest first: number and merchant (linking to the detail page;
/// the whole row opens it), the reason in words, the date opened, the amount, and the status as a text badge, with sorting,
/// paging, the column chooser, and CSV export; the grid's view is saved for the visit and can be reset. Beside the list, how
/// disputes work and a link to the transactions, where a dispute is opened. Without disputes the list shows an empty state.
/// The reason and status texts are shared with <see cref="DisputeDetail" />.
/// </summary>
public partial class Disputes : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private Grid<DisputeRow>? grid;

    private int gridVersion;

    private int pageSize = DefaultPageSize;

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
        view = GridViews.Get(State.SiteSlug, "my-disputes", DefaultPageSize, "opened", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Formats a date like "Nov 19, 2026".</summary>
    internal static string DayText(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Returns the route of a dispute's detail page.</summary>
    internal static string DetailPath(int disputeId) => $"{FintechLayout.Base}/disputes/{disputeId.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>Returns a dispute reason in words.</summary>
    internal static string ReasonText(FintechDisputeReason reason) => reason switch
    {
        FintechDisputeReason.NotRecognized => "I don't recognize this charge",
        FintechDisputeReason.NotReceived => "I didn't get what I paid for",
        FintechDisputeReason.Duplicate => "I was charged twice",
        FintechDisputeReason.WrongAmount => "The amount is wrong",
        FintechDisputeReason.Cancelled => "I cancelled, but was still charged",
        _ => "Other",
    };

    /// <summary>Returns the badge color of a dispute status; the badge always carries the status text too.</summary>
    internal static BadgeColor StatusColor(FintechDisputeStatus status) => status switch
    {
        FintechDisputeStatus.Submitted => BadgeColor.Info,
        FintechDisputeStatus.UnderReview => BadgeColor.Warning,
        FintechDisputeStatus.Refunded => BadgeColor.Success,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns a dispute status in words.</summary>
    internal static string StatusText(FintechDisputeStatus status) => status switch
    {
        FintechDisputeStatus.Submitted => "Submitted",
        FintechDisputeStatus.UnderReview => "Under review",
        FintechDisputeStatus.Refunded => "Refunded",
        _ => "Declined",
    };

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private Task<GridDataProviderResult<DisputeRow>> ProvideRowsAsync(GridDataProviderRequest<DisputeRow> request) =>
        view.ApplyAsync(request, MyDisputes.Select(dispute => new DisputeRow(dispute.Id, dispute.Merchant, ReasonText(dispute.Reason), dispute.CreatedAt, dispute.Amount,
            StatusText(dispute.Status), dispute.Status)).ToList());

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private async Task ResetViewAsync()
    {
        const string message = "The view is reset: newest first, 10 disputes a page, and every column shown.";

        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        statusMessage = statusMessage == message ? message + " (2)" : message;
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

    /// <summary>Gets the route of the transactions page, where disputes are opened.</summary>
    internal static string TransactionsPath => $"{FintechLayout.Base}/transactions";

    private string CountText
    {
        get
        {
            var open = MyDisputes.Count(dispute => dispute.Status is FintechDisputeStatus.Submitted or FintechDisputeStatus.UnderReview);

            return MyDisputes.Count switch
            {
                0 => "Nothing disputed.",
                1 => open == 1 ? "1 dispute, open." : "1 dispute, closed.",
                _ => $"{MyDisputes.Count.ToString(CultureInfo.InvariantCulture)} disputes, {open.ToString(CultureInfo.InvariantCulture)} open.",
            };
        }
    }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private IReadOnlyList<FintechDispute> MyDisputes => State.MyDisputes();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One dispute as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record DisputeRow(int Id, string Merchant, string Reason, DateTime Opened, decimal Amount, string Status, FintechDisputeStatus StatusValue);

    #endregion
}
