using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's bills: the total due at the top, then every bill in a <see cref="Grid{TItem}" /> (what it's for,
/// the amount, status in words — overdue, to pay, or paid —, issue and due dates, when and how it was paid, and its
/// number), to pay first, with paging, sorting, the column chooser, and CSV export; a filter shows the bills to pay or the
/// paid ones, and the grid's view is saved for the visit and can be reset. A bill has no page of its own, so the row opens a
/// dialog: its facts, a link to the visit or lab report, and for a due bill the test-card form
/// (<see cref="ShowcaseCardForm" />, D-026): field errors are shown and the first invalid field takes focus; the card is
/// charged with <see cref="ShowcaseCards.Charge" /> and the bill paid with <see cref="HcState.PayBill" />. A decline is shown
/// in an alert whose text changes with each attempt; success closes the dialog and announces "Paid $40.00 with Visa ending
/// 4242" in the status region, which takes focus. <c>?bill={id}</c> (from a notification or the search) opens that bill.
/// </summary>
public partial class Bills : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID of the element around the card fields, for focusing the first invalid one.</summary>
    private const string CardFieldsId = "hc-bills-card-fields";

    private const int DefaultPageSize = 10;

    private const string DueFilter = "due";

    private const string PaidFilter = "paid";

    /// <summary>The rows' buttons, by bill ID, so focus returns to one when its dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    private Modal billModal = default!;

    private ShowcaseCardInput card = new();

    private ElementReference? countRegion;

    /// <summary>How many times the card was declined since the dialog opened (varies the alert so it is announced again).</summary>
    private int declines;

    private FocusTarget focusTarget;

    private Grid<BillRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The <c>?bill=</c> value this page last opened.</summary>
    private int? handledBillQuery;

    /// <summary>The bill the dialog is about.</summary>
    private int openedId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private EditContext payContext;

    private string? payError;

    private ElementReference? payHeading;

    /// <summary>The payment made in the dialog, announced once the dialog is hidden.</summary>
    private (string Message, bool IsError)? pendingAnnouncement;

    private int statusCount;

    private string statusFilter = string.Empty;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with no payment form open.</summary>
    public Bills() => payContext = new EditContext(card);

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive, opens a bill from <c>?bill=</c>, and moves focus where the last action left it.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (BillQuery is { } billId && handledBillQuery != billId && Session?.IsRestored == true)
        {
            handledBillQuery = billId;

            if (All().Any(bill => bill.Id == billId))
                await OpenAsync(billId);
        }

        var target = focusTarget;
        focusTarget = FocusTarget.None;

        switch (target)
        {
            case FocusTarget.Status:
                await HcDisplay.FocusAsync(statusRegion);
                break;

            case FocusTarget.Opener when openers.TryGetValue(openedId, out var opener):
                await HcDisplay.FocusAsync(opener);
                break;

            case FocusTarget.FirstInvalidField:
                await FocusFirstInvalidFieldAsync();
                break;
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "my-bills", DefaultPageSize, null, SortDirection.None);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns how a paid bill was paid, like "Visa ending 4242".</summary>
    private static string CardText(HcBill bill)
    {
        var brand = bill.CardBrand.Length == 0 ? "Test card" : bill.CardBrand;

        return bill.CardLast4.Length == 0 ? brand : $"{brand} ending {bill.CardLast4}";
    }

    /// <summary>Returns the total line in words, like "2 bills to pay, none overdue." or "2 bills to pay; 1 is overdue."</summary>
    private static string DueSummary(int due, int overdue)
    {
        if (due == 0)
            return "Nothing to pay. Thank you!";

        var bills = due == 1 ? "1 bill to pay" : string.Create(CultureInfo.InvariantCulture, $"{due} bills to pay");

        return overdue switch
        {
            0 => $"{bills}, none overdue.",
            1 => $"{bills}; 1 is overdue.",
            _ => string.Create(CultureInfo.InvariantCulture, $"{bills}; {overdue} are overdue."),
        };
    }

    /// <summary>Returns whether a due bill is past its due date.</summary>
    private static bool IsOverdue(HcBill bill, DateTime today) => bill.Status == HcBillStatus.Due && bill.DueDate.Date < today;

    private IReadOnlyList<HcBill> All() => State.CurrentPatient is { } patient ? State.BillsOf(patient.Id) : Array.Empty<HcBill>();

    private string CountText(int total)
    {
        var shown = Shown.Count;
        var count = total == 1 ? "1 bill" : string.Create(CultureInfo.InvariantCulture, $"{total} bills");

        return shown == total ? $"{count}." : string.Create(CultureInfo.InvariantCulture, $"Showing {shown} of {count}.");
    }

    /// <summary>
    /// Moves focus to the first card field marked invalid. The fields live inside <see cref="ShowcaseCardForm" />, so they
    /// are found by selector with the library's own focus helper (no eval).
    /// </summary>
    private async Task FocusFirstInvalidFieldAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.calendar.focusIn", CardFieldsId, "[aria-invalid=true]");
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException)
        {
            // The page or the circuit has gone; nothing to focus.
        }
    }

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: after a payment focus moves to the announcement, otherwise back to the row.</summary>
    private Task OnHiddenAsync()
    {
        if (pendingAnnouncement is { } announcement)
        {
            pendingAnnouncement = null;
            SetStatus(announcement.Message, announcement.IsError);
            focusTarget = FocusTarget.Status;
        }
        else
        {
            focusTarget = FocusTarget.Opener;
        }

        card = new ShowcaseCardInput();
        payContext = new EditContext(card);
        StateHasChanged();

        return Task.CompletedTask;
    }

    private Task OnShownAsync() => HcDisplay.FocusAsync(payHeading);

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        statusFilter = value is DueFilter or PaidFilter ? value : string.Empty;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Opens a bill's dialog with an empty card form.</summary>
    private async Task OpenAsync(int billId)
    {
        openedId = billId;
        payError = null;
        declines = 0;
        pendingAnnouncement = null;
        card = new ShowcaseCardInput();
        payContext = new EditContext(card);
        await billModal.ShowAsync();
    }

    /// <summary>Checks the card, charges it, pays the bill, and maps every result.</summary>
    private async Task PayAsync()
    {
        if (Opened is not { } bill)
            return;

        payError = null;

        // Validate() runs the card form's checks (ShowcaseCards.Validate) and shows each field's message.
        if (!payContext.Validate() || ShowcaseCards.Validate(card, State.Clock.Today).Count > 0)
        {
            focusTarget = FocusTarget.FirstInvalidField;

            return;
        }

        var amount = HcDisplay.Money(bill.AmountCents);
        var charge = ShowcaseCards.Charge(card);

        switch (State.PayBill(bill.Id, charge))
        {
            case HcResult.Ok:
                var paid = All().FirstOrDefault(item => item.Id == bill.Id);
                pendingAnnouncement = (paid is null ? $"Paid {amount}." : $"Paid {amount} with {CardText(paid)}.", false);
                await billModal.HideAsync();
                break;

            case HcResult.CardDeclined:
                // A different text each attempt, so the alert is announced again.
                declines++;
                var failure = charge.FailureMessage ?? "Your card was declined. Try another card.";
                payError = declines == 1 ? $"{failure} Nothing was paid." : string.Create(CultureInfo.InvariantCulture, $"{failure} Nothing was paid. (attempt {declines})");
                break;

            case HcResult.WrongState:
                pendingAnnouncement = ("This bill is already paid, so nothing was charged.", true);
                await billModal.HideAsync();
                break;

            case HcResult.NotFound:
                pendingAnnouncement = ("This bill can't be found any more, so nothing was charged.", true);
                await billModal.HideAsync();
                break;

            case HcResult.NotAllowed:
                pendingAnnouncement = ("Only the patient's own account can pay this bill. Nothing was charged.", true);
                await billModal.HideAsync();
                break;

            default:
                payError = "The payment couldn't be made, and nothing was charged. Try again.";
                break;
        }
    }

    /// <summary>The page of bills the grid asked for: to pay first, then newest, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<BillRow>> ProvideBillsAsync(GridDataProviderRequest<BillRow> request) => view.ApplyAsync(request, Shown);

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
        SetStatus("The view is reset: bills to pay first, 10 a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(countRegion);
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

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the bill to open, from <c>?bill=</c>.</summary>
    [SupplyParameterFromQuery(Name = "bill")] public int? BillQuery { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private IJSRuntime JS { get; set; } = default!;

    private HcBill? Opened => All().FirstOrDefault(bill => bill.Id == openedId);

    [CascadingParameter] private ShowcaseSession? Session { get; set; }

    /// <summary>The patient's bills that pass the filter, as grid rows.</summary>
    private IReadOnlyList<BillRow> Shown
    {
        get
        {
            var today = State.Clock.Today;

            return All()
                .Where(bill => statusFilter switch { DueFilter => bill.Status == HcBillStatus.Due, PaidFilter => bill.Status == HcBillStatus.Paid, _ => true })
                .Select(bill => new BillRow(
                    bill.Id,
                    bill.Description,
                    bill.AmountCents,
                    HcDisplay.Money(bill.AmountCents),
                    IsOverdue(bill, today) ? "Overdue" : bill.Status == HcBillStatus.Due ? "To pay" : "Paid",
                    IsOverdue(bill, today) ? 0 : bill.Status == HcBillStatus.Due ? 1 : 2,
                    bill.CreatedAt,
                    HcDisplay.Date(bill.CreatedAt),
                    bill.DueDate,
                    HcDisplay.Date(bill.DueDate),
                    bill.PaidAt,
                    bill.Status == HcBillStatus.Paid ? $"{HcDisplay.Date(bill.PaidAt ?? bill.CreatedAt)} with {CardText(bill)}" : "Not yet"))
                .ToList();
        }
    }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Where focus goes after the next render.</summary>
    private enum FocusTarget
    {
        None,
        Status,
        Opener,
        FirstInvalidField,
    }

    /// <summary>One bill as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record BillRow(
        int Id,
        string Description,
        int AmountCents,
        string AmountText,
        string Status,
        int StatusOrder,
        DateTime Issued,
        string IssuedText,
        DateTime Due,
        string DueText,
        DateTime? Paid,
        string PaidText);

    #endregion
}
