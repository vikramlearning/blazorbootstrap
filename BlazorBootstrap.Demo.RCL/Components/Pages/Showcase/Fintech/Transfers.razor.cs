using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The transfers hub: every transfer of the customer in a <see cref="Grid{TItem}" />, coming up and held first by date,
/// then the closed ones newest first, with paging, sorting, the column chooser, and CSV export. It is searched by recipient,
/// reference, or note and filtered by status ("Coming up and held", or one status). Each row has the recipient (linking to
/// the receipt; the whole row opens it) with the account it's sent from and the hold or reject reason, the reference, the
/// date, the amount, the status in words, and, for scheduled and held transfers, a Cancel action (a dialog says the money set
/// aside is released). The grid's view is saved for the visit and can be reset; a prominent "New transfer" button opens the
/// transfer form. The status texts and the cancel messages are shared with <see cref="TransferDetail" />.
/// </summary>
public partial class Transfers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string WaitingValue = "waiting";

    private readonly Dictionary<string, ElementReference> cancelButtons = new(StringComparer.Ordinal);

    /// <summary>Set when the dialog's action ran, so focus moves to the status message once it is hidden.</summary>
    private bool actionDone;

    private bool cancelBusy;

    /// <summary>The transfer the dialog is about, captured when it opens, so the text stays put while the dialog closes.</summary>
    private FintechTransfer? cancelling;

    private Modal cancelModal = default!;

    private string? dialogError;

    private Grid<TransferRow>? grid;

    private int gridVersion;

    /// <summary>The button that opened the dialog, so focus can go back to it.</summary>
    private ElementReference? opener;

    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    private string searchBoxValue = string.Empty;

    private string statusFilter = string.Empty;

    private string? statusMessage;

    private ElementReference statusRegion;

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
        view = GridViews.Get(State.SiteSlug, "transfers", DefaultPageSize, "status", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the message for the result of cancelling a transfer.</summary>
    internal static string CancelResultText(FintechResult result, FintechTransfer transfer, string fromName) => result switch
    {
        FintechResult.Ok => $"The {FintechDisplay.Money(transfer.Amount)} transfer to {transfer.ToName} is cancelled. The money is available in {fromName} again.",
        FintechResult.WrongState => "This transfer can't be cancelled any more: it was already sent, rejected, or cancelled.",
        FintechResult.NotFound => "We couldn't find this transfer on your account, so nothing changed.",
        FintechResult.NotAllowed => "Sign in again to cancel transfers. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    /// <summary>Says what cancelling a waiting transfer stops.</summary>
    internal static string CancelText(FintechTransfer transfer) => transfer.Status == FintechTransferStatus.Held
        ? $"{FintechDisplay.Money(transfer.Amount)} to {transfer.ToName}, held for a check, won't be sent."
        : $"{FintechDisplay.Money(transfer.Amount)} to {transfer.ToName}, scheduled for {DateText(transfer.Date)}, won't be sent.";

    /// <summary>Formats a transfer date, like "Tue, Dec 1, 2026".</summary>
    internal static string DateText(DateOnly date) => date.ToString("ddd, MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Returns the receipt's URL of a transfer.</summary>
    internal static string ReceiptUrl(string reference) => $"{FintechLayout.Base}/transfers/{Uri.EscapeDataString(reference)}";

    /// <summary>Formats a date and time, like "Nov 24, 2026 at 09:00".</summary>
    internal static string StampText(DateTime at) => at.ToString("MMM d, yyyy 'at' HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Returns the badge color of a status; the badge's text carries the meaning.</summary>
    internal static BadgeColor StatusColor(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => BadgeColor.Success,
        FintechTransferStatus.Scheduled => BadgeColor.Info,
        FintechTransferStatus.Held => BadgeColor.Warning,
        FintechTransferStatus.Rejected => BadgeColor.Danger,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns the icon of a status.</summary>
    internal static IconName StatusIcon(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => IconName.CheckCircle,
        FintechTransferStatus.Scheduled => IconName.CalendarEvent,
        FintechTransferStatus.Held => IconName.ShieldExclamation,
        FintechTransferStatus.Rejected => IconName.XCircle,
        _ => IconName.SlashCircle,
    };

    /// <summary>Returns the words of a status.</summary>
    internal static string StatusText(FintechTransferStatus status) => status switch
    {
        FintechTransferStatus.Completed => "Sent",
        FintechTransferStatus.Scheduled => "Scheduled",
        FintechTransferStatus.Held => "Held for a check",
        FintechTransferStatus.Rejected => "Not sent",
        _ => "Cancelled",
    };

    #endregion

    #region Private Methods

    private static bool IsWaiting(FintechTransfer transfer) => IsWaiting(transfer.Status);

    private static bool IsWaiting(FintechTransferStatus status) => status is FintechTransferStatus.Scheduled or FintechTransferStatus.Held;

    private string AccountName(string accountId) => State.FindAccount(accountId)?.Name ?? "your account";

    private async Task AskCancelAsync(string reference)
    {
        if (State.FindTransfer(reference) is not { } transfer)
            return;

        cancelling = transfer;
        dialogError = null;
        actionDone = false;
        cancelBusy = false;
        opener = cancelButtons.TryGetValue(transfer.Reference, out var button) ? button : null;

        await cancelModal.ShowAsync();
    }

    /// <summary>The link disappears with the filters it clears, so the count announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = statusFilter = string.Empty;
        await FiltersChangedAsync();
    }

    private async Task ConfirmCancelAsync()
    {
        // Guards a double click: the first click cancels and closes the dialog.
        if (cancelBusy || cancelling is not { } transfer)
            return;

        cancelBusy = true;
        var result = State.CancelTransfer(transfer.Reference);
        var message = CancelResultText(result, transfer, AccountName(transfer.FromAccountId));

        if (result == FintechResult.Ok)
        {
            actionDone = true;
            statusMessage = message;
            await cancelModal.HideAsync();
        }
        else
        {
            dialogError = message;
            cancelBusy = false;
        }
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. After a cancellation, focus goes to the status
    /// message (the row has moved to "Recent"); otherwise back to the Cancel button that opened the dialog.
    /// </summary>
    private async Task OnCancelHiddenAsync()
    {
        dialogError = null;
        cancelBusy = false;

        if (actionDone)
        {
            actionDone = false;
            cancelling = null;
            await Task.Yield();
            await FintechDisplay.FocusAsync(statusRegion);
        }
        else
        {
            cancelling = null;
            await FintechDisplay.FocusAsync(opener);
        }

        opener = null;
    }

    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

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

    private Task<GridDataProviderResult<TransferRow>> ProvideRowsAsync(GridDataProviderRequest<TransferRow> request) => view.ApplyAsync(request, Rows);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private async Task ResetViewAsync()
    {
        const string message = "The view is reset: coming up first, 25 transfers a page, and every column shown.";

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

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private IReadOnlyList<FintechTransfer> Mine => State.MyTransfers();

    /// <summary>
    /// Gets the transfers that pass the filters as grid rows: coming up and held ones first, by date, then the closed ones,
    /// newest first (the status column's order keeps that).
    /// </summary>
    private IReadOnlyList<TransferRow> Rows
    {
        get
        {
            var term = search.Trim();

            return Mine
                .Where(transfer => (statusFilter.Length == 0 || (statusFilter == WaitingValue ? IsWaiting(transfer) : transfer.Status.ToString() == statusFilter))
                    && (term.Length == 0 || transfer.ToName.Contains(term, StringComparison.OrdinalIgnoreCase) || transfer.Reference.Contains(term, StringComparison.OrdinalIgnoreCase)
                        || (transfer.Note?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)))
                .OrderBy(transfer => IsWaiting(transfer) ? 0 : 1)
                .ThenBy(transfer => IsWaiting(transfer) ? transfer.Date.DayNumber : -(transfer.ClosedAt ?? transfer.CreatedAt).Ticks / TimeSpan.TicksPerMinute)
                .Select((transfer, index) => new TransferRow(transfer.Reference, transfer.ToName, AccountName(transfer.FromAccountId), transfer.Date, transfer.Amount,
                    StatusText(transfer.Status), transfer.Status, index,
                    transfer.HoldReason is { } held ? $"Held for a check: {held}." : transfer.Status == FintechTransferStatus.Rejected && transfer.RejectReason is { } why ? $"Not sent: {why}" : string.Empty))
                .ToList();
        }
    }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// One transfer as the grid shows and exports it. The status column sorts by <see cref="Order" />, so coming up and held
    /// transfers stay first.
    /// </summary>
    private sealed record TransferRow(string Reference, string To, string From, DateOnly Date, decimal Amount, string Status, FintechTransferStatus StatusValue, int Order, string Reason);

    #endregion
}
