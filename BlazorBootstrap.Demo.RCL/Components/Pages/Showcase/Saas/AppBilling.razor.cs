using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The workspace's billing: the current plan, a plan picker (self-serve plans, monthly or annual, and seats) with a live
/// prorated quote from <see cref="SaasState.QuotePlan" />, the card on file with a test-card form to add or replace it,
/// and the invoices. Open invoices show a "Payment due" panel whose "Pay now" calls <see cref="SaasState.PayOpenInvoices" />
/// (without a card it opens the card form first). On trial, the same picker converts the trial. Owners and billing members make changes; everyone else
/// sees the plan and invoices read-only. While a payment is failing (dunning), a "Payment failed" panel says what failed,
/// the next retry or the end of grace, and offers "Update card", "Pay now", and a demo control that moves the clock a day;
/// the card form and "Pay now" also work while the workspace is suspended for the unpaid invoice. The card form follows rule
/// WR-002 (name, a test card number that passes the Luhn check, a future expiry, the security code, and the billing postal
/// code; only the brand, last four digits, and expiry are kept). <c>?card=1</c> opens it, for the banner's "Update card".
/// The invoices (the billing history) are a <see cref="Grid{TItem}" /> with a saved
/// view, sorting, paging, the column chooser, and CSV export; a row opens the invoice. Every action ends in a status message
/// or an error, and focus returns to the button that opened the card form.
/// </summary>
public partial class AppBilling : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Where Enterprise is sold.</summary>
    internal const string ContactPath = SaasLayout.Base + "/contact";

    private const int InvoicePageSize = 10;

    /// <summary>The annual discount, such as ", save 20%".</summary>
    private static readonly string annualSavingText = $", save {(SaasPricing.AnnualDiscount * 100).ToString("0", CultureInfo.InvariantCulture)}%";

    private ShowcaseCardInput card = new();

    private EditContext cardContext = default!;

    private CardUpdateForm cardForm = new();

    /// <summary>Whether <c>?card=1</c> asked for the card form, opened once the page is interactive.</summary>
    private bool openCardOnLoad;

    private string? appliedCardQuery;

    private Button? cardButton;

    private string? cardError;

    private int cardErrors;

    private ElementReference cardFormHeading;

    private bool cardFormOpen;

    /// <summary>Whether the card form was opened because a plan change needed a card, so focus goes back to the plan's button.</summary>
    private bool cardOpenedFromPlan;

    /// <summary>Whether the card form was opened because paying the open invoices needed a card, so focus goes back to "Pay now".</summary>
    private bool cardOpenedFromPay;

    private Button? confirmButton;

    private Button? payButton;

    private string payError = string.Empty;

    private int payErrors;

    private SaasBillingPeriod period = SaasBillingPeriod.Annual;

    private int? pickerWorkspaceId;

    private string planError = string.Empty;

    private int planErrors;

    private string planId = string.Empty;

    private int seats = 1;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private ElementReference? invoiceCountRegion;

    private Grid<SaasInvoice>? invoiceGrid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int invoiceGridVersion;

    /// <summary>The invoice grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int invoicePageSize = InvoicePageSize;

    private ShowcaseGridView invoiceView = default!;

    private bool invoiceViewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the invoice list's saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await invoiceView.LoadAsync();
            invoicePageSize = (await invoiceView.GetSettingsAsync()).PageSize;
            invoiceViewLoaded = true;
            StateHasChanged();
        }

        if (openCardOnLoad && State.CanFixPayment)
        {
            openCardOnLoad = false;
            await OpenCardFormAsync(fromPlan: false, fromPay: State.CurrentWorkspace is { } workspace && State.OpenDunningCase(workspace.Id) is not null);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary><c>?card=1</c> (the banner's "Update card") opens the card form, also when the page is already open.</summary>
    protected override void OnParametersSet()
    {
        if (CardQuery == appliedCardQuery)
            return;

        appliedCardQuery = CardQuery;
        openCardOnLoad = CardQuery == "1";
    }

    protected override void OnInitialized()
    {
        NewCard();
        invoiceView = GridViews.Get(State.SiteSlug, "billing-invoices", InvoicePageSize, "date", SortDirection.Descending);
        EnsurePicker();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the badge color of an invoice status.</summary>
    internal static BadgeColor InvoiceColor(SaasInvoiceStatus status) => status switch
    {
        SaasInvoiceStatus.Paid => BadgeColor.Success,
        SaasInvoiceStatus.Open => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns "Visa ending in 4242", or <see langword="null" /> without a card.</summary>
    internal static string? CardText(string? brand, string? last4) => last4 is null ? null : $"{brand ?? "Card"} ending in {last4}";

    /// <summary>Returns "Paid", "Open", "Refunded", or "Partially refunded ($12.50)".</summary>
    internal static string InvoiceStatusText(SaasInvoice invoice) => invoice is { Status: SaasInvoiceStatus.Paid, RefundedAmount: > 0 }
        ? $"Partially refunded ({SaasDisplay.MoneyCents(invoice.RefundedAmount)})"
        : invoice.Status.ToString();

    #endregion

    #region Private Methods

    private static string AmountText(SaasPlan plan, SaasBillingPeriod period, int seats) => plan.MonthlyPricePerSeat == 0
        ? "Free"
        : $"{SaasDisplay.MoneyCents(SaasPricing.PeriodAmount(plan, period, seats))} per {(period == SaasBillingPeriod.Annual ? "year" : "month")}";

    private static bool IsExpired(SaasWorkspace workspace, DateTime today) =>
        workspace.CardLast4 is not null && (workspace.CardExpiryYear < today.Year || (workspace.CardExpiryYear == today.Year && workspace.CardExpiryMonth < today.Month));

    private static string SeatPriceText(SaasPlan plan, SaasBillingPeriod period) => plan.MonthlyPricePerSeat == 0
        ? "Free"
        : $"{SaasDisplay.Money(SaasPricing.MonthlySeatPrice(plan, period))} per seat / month";

    private static string SeatsWord(int count) => string.Create(CultureInfo.InvariantCulture, $"{count} {(count == 1 ? "seat" : "seats")}");

    private static bool TryParseExpiry(string expiry, out int month, out int year)
    {
        month = 0;
        year = 0;
        var parts = expiry.Trim().Split('/');

        if (parts.Length != 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out month)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var shortYear))
            return false;

        year = 2000 + shortYear;

        return month is >= 1 and <= 12;
    }

    private async Task CancelCardAsync()
    {
        cardFormOpen = false;
        NewCard();
        cardError = null;
        StateHasChanged();

        await Task.Yield();
        await FocusCardOpenerAsync();
    }

    private async Task ConfirmPlanAsync()
    {
        if (State.CurrentWorkspace is not { } before || State.FindPlan(planId) is not { } plan)
            return;

        var wasTrial = before.Status == SaasWorkspaceStatus.Trial;
        var quote = State.QuotePlan(planId, period, seats);
        var result = State.ChangePlan(planId, period, seats);

        if (result == SaasResult.Ok)
        {
            planError = string.Empty;
            SetStatus(PlanChangedText(before, plan, quote, wasTrial));
            ResetPicker();
            StateHasChanged();

            await Task.Yield();
            await SaasDisplay.FocusAsync(statusRegion);

            return;
        }

        var (min, max) = SeatRange(plan, before);
        SetPlanError(result switch
        {
            SaasResult.CardRequired => $"Add a card first: {plan.Name} is a paid plan, so it needs a card on file. The card form is open below; once the card is saved, confirm the change again.",
            SaasResult.NoChange => "That's already your plan, billing period, and number of seats. Change one of them to see a new price.",
            SaasResult.SeatsOutOfRange => min > max
                ? $"{plan.Name} allows up to {SeatsWord(max)}, fewer than your {State.SeatsUsed(before.Id)} members and open invitations."
                : string.Create(CultureInfo.InvariantCulture, $"Choose between {min} and {max} seats for {plan.Name}."),
            SaasResult.NotFound => "That plan can't be chosen here. For Enterprise, talk to sales.",
            _ => "Only owners and billing members change the plan, and not while the workspace is suspended or archived.",
        });

        if (result == SaasResult.CardRequired)
            await OpenCardFormAsync(fromPlan: true);
    }

    /// <summary>Starts the picker from the workspace's plan once the workspace is known (the tab's state may be restored after the first render).</summary>
    private void EnsurePicker()
    {
        if (State.CurrentWorkspace is { } workspace && pickerWorkspaceId != workspace.Id)
            ResetPicker();
    }

    private async Task FocusCardOpenerAsync()
    {
        if (cardOpenedFromPlan && confirmButton is not null)
            await SaasDisplay.FocusAsync(confirmButton.Element);
        else if (cardOpenedFromPay && payButton is not null)
            await SaasDisplay.FocusAsync(payButton.Element);
        else if (cardButton is not null)
            await SaasDisplay.FocusAsync(cardButton.Element);
        else
            await SaasDisplay.FocusAsync(statusRegion);

        cardOpenedFromPlan = cardOpenedFromPay = false;
    }

    private void OnSeatsChanged(int value)
    {
        // NumberInput raises ValueChanged on its first render too.
        if (value == seats)
            return;

        seats = value;
        planError = string.Empty;
    }

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        EnsurePicker();
        await RefreshInvoicesAsync();
        StateHasChanged();
    });

    private static string InvoiceCountText(IReadOnlyList<SaasInvoice> invoices)
    {
        var open = invoices.Count(invoice => invoice.Status == SaasInvoiceStatus.Open);
        var all = invoices.Count == 1 ? "1 invoice" : $"{invoices.Count.ToString(CultureInfo.InvariantCulture)} invoices";

        return open == 0 ? $"{all}, all paid or refunded." : $"{all}, {open.ToString(CultureInfo.InvariantCulture)} open.";
    }

    /// <summary>The page of the workspace's invoices the grid asked for, newest first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<SaasInvoice>> ProvideInvoicesAsync(GridDataProviderRequest<SaasInvoice> request) =>
        invoiceView.ApplyAsync(request, State.CurrentWorkspace is { } workspace ? State.InvoicesOf(workspace.Id) : Array.Empty<SaasInvoice>());

    private async Task RefreshInvoicesAsync()
    {
        if (invoiceGrid is not null)
            await invoiceGrid.RefreshDataAsync();
    }

    /// <summary>Forgets the invoice list's saved view and builds it again with its defaults; focus moves to the count above it.</summary>
    private async Task ResetInvoiceViewAsync()
    {
        await invoiceView.ResetAsync();
        invoicePageSize = InvoicePageSize;
        invoiceGridVersion++;
        StateHasChanged();

        await Task.Yield();
        await SaasDisplay.FocusAsync(invoiceCountRegion);
    }

    /// <summary>Keeps the invoice grid's settings in its saved view, and follows the page size the selector chose.</summary>
    private async Task SaveInvoiceSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            invoicePageSize = settings.PageSize;

        await invoiceView.SaveSettingsAsync(settings);
    }

    private Task OpenCardAsync() => OpenCardFormAsync(fromPlan: false);

    private Task OpenCardFromFailureAsync() => OpenCardFormAsync(fromPlan: false, fromPay: true);

    /// <summary>Moves the site's clock a day (a demo control while a payment is failing), so the next retry or the end of grace comes.</summary>
    private async Task AdvanceDayAsync()
    {
        var result = State.AdvanceDemoDay();
        SetStatus(result == SaasResult.Ok
            ? $"It's now {State.Clock.Now.ToString("dddd, MMM d", CultureInfo.InvariantCulture)} on {SaasSeedData.ProductName}'s clock. "
                + (State.CurrentWorkspace is { } workspace && State.OpenDunningCase(workspace.Id) is { } item
                    ? $"The payment is still failing: {FailingNextText(workspace, item)}"
                    : "The payment went through, and the workspace is in good standing.")
            : string.Create(CultureInfo.InvariantCulture, $"The demo clock is already {SaasState.MaxDemoDays} days ahead, the most it goes."));
        StateHasChanged();

        await Task.Yield();
        await SaasDisplay.FocusAsync(statusRegion);
    }

    /// <summary>What happens next to a failing payment, in words.</summary>
    private static string FailingNextText(SaasWorkspace workspace, SaasDunningCase failing) => failing.Stage switch
    {
        SaasDunningStage.Retrying when failing.NextRetryAt is { } next => $"We'll try again on {SaasDisplay.Date(next)}. Update the card first, or the retry fails too.",
        SaasDunningStage.Grace when failing.GraceEndsAt is { } ends => $"Every retry failed: {workspace.Name} is suspended on {SaasDisplay.Date(ends)} unless it's paid.",
        _ => $"{workspace.Name} is read-only until it's paid. Update the card, then select Pay now to restore it at once.",
    };

    /// <summary>Starts a fresh card form: the card fields and the billing postal code, in a new edit context.</summary>
    private void NewCard()
    {
        card = new ShowcaseCardInput();
        cardForm = new CardUpdateForm { Card = card };
        cardContext = new EditContext(cardForm);
    }

    private async Task OpenCardFormAsync(bool fromPlan, bool fromPay = false)
    {
        cardOpenedFromPlan = fromPlan;
        cardOpenedFromPay = fromPay;
        cardError = null;
        cardErrors = 0;

        if (!cardFormOpen)
            NewCard();

        cardFormOpen = true;
        StateHasChanged();

        await Task.Yield();
        await SaasDisplay.FocusAsync(cardFormHeading);
    }

    /// <summary>Pays the open invoices with the card on file; without a card, opens the card form the way a plan change does.</summary>
    private async Task PayNowAsync()
    {
        if (State.CurrentWorkspace is not { } before)
            return;

        var open = State.InvoicesOf(before.Id).Where(invoice => invoice.Status == SaasInvoiceStatus.Open).ToList();
        var result = State.PayOpenInvoices();

        if (result == SaasResult.Ok)
        {
            payError = string.Empty;
            var total = SaasDisplay.MoneyCents(open.Sum(invoice => invoice.Amount));
            var text = open.Count == 1
                ? $"Invoice {open[0].Number} is paid: {total} with the {CardText(before.CardBrand, before.CardLast4)}."
                : $"{open.Count.ToString(CultureInfo.InvariantCulture)} invoices are paid: {total} with the {CardText(before.CardBrand, before.CardLast4)}.";

            if (before.Status == SaasWorkspaceStatus.PastDue && State.CurrentWorkspace is { Status: SaasWorkspaceStatus.Active })
                text += " The workspace is no longer past due.";

            SetStatus(text + " Nothing real is charged in this demo.");
            StateHasChanged();

            // The panel and its button are gone once nothing is open.
            await Task.Yield();
            await SaasDisplay.FocusAsync(statusRegion);

            return;
        }

        SetPayError(result switch
        {
            SaasResult.CardRequired => "Add a card first: there's no card on file to pay with. The card form is open below; once the card is saved, select Pay now again.",
            SaasResult.CardDeclined => "The card on file failed again, so nothing was paid. Update the card, then select Pay now.",
            SaasResult.NoChange => "There's nothing to pay: no invoice is open.",
            _ => "Only owners and billing members pay invoices, and not while the workspace is suspended or archived.",
        });

        if (result == SaasResult.CardRequired)
            await OpenCardFormAsync(fromPlan: false, fromPay: true);
    }

    private string PlanChangedText(SaasWorkspace before, SaasPlan plan, SaasPlanQuote? quote, bool wasTrial)
    {
        var paid = plan.MonthlyPricePerSeat > 0;
        var summary = $"{plan.Name}, {SeatsWord(seats)}{(paid ? $", {SaasSeedData.PeriodLabel(period)}" : string.Empty)}";
        var text = wasTrial ? $"Your trial is over and your plan is now {summary}." : $"Your plan is now {summary}.";

        if (quote is null)
            return text;

        if (quote.ChargeToday > 0)
            text += $" An invoice for {SaasDisplay.MoneyCents(quote.ChargeToday)}, paid with the {CardText(before.CardBrand, before.CardLast4)}, is in your invoices. Nothing real is charged in this demo.";
        else
            text += " Nothing is due today.";

        if (quote.CreditAfter > before.CreditBalance)
            text += $" {SaasDisplay.MoneyCents(quote.CreditAfter - before.CreditBalance)} is credited to the workspace.";

        return text;
    }

    private void ResetPicker()
    {
        if (State.CurrentWorkspace is not { } workspace)
            return;

        pickerWorkspaceId = workspace.Id;
        var current = State.PlanOf(workspace);
        planId = current.IsSelfServe ? current.Id : State.Plans.FirstOrDefault(plan => plan.IsSelfServe)?.Id ?? current.Id;
        period = workspace.Period;
        seats = workspace.Seats;
    }

    private async Task SaveCardAsync()
    {
        // The card fields and the postal code are checked together, and a failed save shows the summary (rule WR-002).
        if (!cardContext.Validate() || ShowcaseCards.Validate(card, State.Clock.Today).Count > 0)
            return;

        var charge = ShowcaseCards.Charge(card);

        if (!charge.IsApproved)
        {
            SetCardError(charge.FailureMessage ?? "The card was declined. Try another card.");

            return;
        }

        if (!TryParseExpiry(card.Expiry, out var month, out var year))
        {
            SetCardError("Enter the expiry as MM/YY, for example 08/29.");

            return;
        }

        var result = State.UpdatePaymentMethod(charge, month, year);

        if (result != SaasResult.Ok)
        {
            SetCardError(result switch
            {
                SaasResult.CardDeclined => "The card was declined. Try another card.",
                SaasResult.Invalid => "This card can't be saved. Use one of the listed test cards with a future expiry.",
                _ => "Only owners and billing members change the payment method, and not while the workspace is suspended or archived.",
            });

            return;
        }

        var fromPlan = cardOpenedFromPlan;
        var fromPay = cardOpenedFromPay;
        SetStatus(fromPlan
            ? $"The {CardText(charge.Brand, charge.Last4)} is saved as the payment method. Confirm the plan change to continue."
            : fromPay
                ? State.CurrentWorkspace is { } current && State.OpenDunningCase(current.Id) is { } failing
                    ? $"The {CardText(charge.Brand, charge.Last4)} is saved. Select Pay now to pay {SaasDisplay.MoneyCents(failing.Amount)} at once"
                        + (failing.NextRetryAt is { } next ? $", or the retry on {SaasDisplay.Date(next)} charges it." : ".")
                    : $"The {CardText(charge.Brand, charge.Last4)} is saved as the payment method. Select Pay now to pay the open invoices."
                : $"The {CardText(charge.Brand, charge.Last4)} is saved as the payment method and pays the next invoices. It was checked with a zero-amount test charge; nothing real is charged.");

        if (fromPlan)
            planError = string.Empty;

        if (fromPay)
            payError = string.Empty;

        NewCard();
        cardError = null;
        cardFormOpen = false;
        StateHasChanged();

        await Task.Yield();
        await FocusCardOpenerAsync();
    }

    /// <summary>Returns the seats a plan allows for the workspace: from its members and open invitations to the plan's limit.</summary>
    private (int Min, int Max) SeatRange(SaasPlan plan, SaasWorkspace workspace) =>
        (Math.Max(1, State.SeatsUsed(workspace.Id)), plan.SeatLimit == 0 ? SaasSeedData.MaxSeats : plan.SeatLimit);

    private void SelectPeriod(SaasBillingPeriod value)
    {
        period = value;
        planError = string.Empty;
    }

    /// <summary>Chooses a plan and keeps the seats inside its range.</summary>
    private void SelectPlan(SaasPlan plan, SaasWorkspace workspace)
    {
        planId = plan.Id;
        planError = string.Empty;
        var (min, max) = SeatRange(plan, workspace);

        if (min <= max)
            seats = Math.Clamp(seats, min, max);
    }

    private void SetCardError(string message)
    {
        cardErrors++;
        cardError = cardErrors == 1 ? message : $"{message} (attempt {cardErrors})";
    }

    private void SetPayError(string message)
    {
        payErrors++;
        payError = payErrors == 1 ? message : $"{message} ({payErrors})";
    }

    private void SetPlanError(string message)
    {
        planErrors++;
        planError = planErrors == 1 ? message : $"{message} ({planErrors})";
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>Explains why <see cref="SaasState.QuotePlan" /> returned no quote for the chosen plan and seats.</summary>
    private string UnavailableText(SaasPlan plan, SaasWorkspace workspace)
    {
        var used = State.SeatsUsed(workspace.Id);
        var (min, max) = SeatRange(plan, workspace);

        if (!plan.IsSelfServe)
            return $"{plan.Name} is sold through sales.";

        if (min > max)
            return $"{plan.Name} allows up to {SeatsWord(max)}, fewer than your {used} members and open invitations. Remove someone or cancel an invitation first.";

        if (seats < min)
            return $"{SeatsWord(seats)} is fewer than your {used} members and open invitations. Choose at least {SeatsWord(min)}, or remove someone first.";

        if (seats > max)
            return $"{plan.Name} allows up to {SeatsWord(max)}.";

        return "This choice isn't available.";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets <c>1</c> to open the card form, like <c>?card=1</c> from the failed-payment banner.</summary>
    [SupplyParameterFromQuery(Name = "card")] public string? CardQuery { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The card form: the shared card fields (checked by <see cref="ShowcaseCardForm" />) and the billing postal code.</summary>
    private sealed class CardUpdateForm
    {
        /// <summary>The card fields; their messages come from the shared card form.</summary>
        public ShowcaseCardInput Card { get; set; } = new();

        [Required(ErrorMessage = "Enter the billing postal code.")]
        [RegularExpression(@"^\s*[A-Za-z0-9][A-Za-z0-9 -]{1,8}[A-Za-z0-9]\s*$", ErrorMessage = "Enter a postal code of 3 to 10 letters and digits, like 94107 or SW1A 1AA.")]
        public string PostalCode { get; set; } = string.Empty;
    }

    #endregion
}
