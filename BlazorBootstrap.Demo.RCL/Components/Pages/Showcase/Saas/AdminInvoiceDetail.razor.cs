using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Invoice detail in the operator console: the invoice (seller, buyer tenant, number, dates, service period, description,
/// amount, refunds, status, and card), its tenant's owner and plan, and its history from the audit log. A paid invoice with
/// something left to refund has a "Refund" dialog (amount, "Full" and "Half", and a required reason) that calls
/// <see cref="SaasState.RefundInvoice" />; an open invoice has "Retry payment", which calls
/// <see cref="SaasState.CollectInvoice" />. Each result becomes a message, and the dialog says beforehand who is notified.
/// An unknown number renders the not-found screen.
/// </summary>
public partial class AdminInvoiceDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int MaxReasonLength = 200;

    private ElementReference amountInput;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private RefundForm refund = new();

    private Button? refundButton;

    private int refundErrorCount;

    private string? refundError;

    private string? refundErrorText;

    private Modal refundModal = default!;

    /// <summary>Whether a refund was just made, so focus goes to the status message when the dialog closes.</summary>
    private bool refunded;

    private string? shownNumber;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>Messages about one invoice are cleared when the route moves to another invoice.</summary>
    protected override void OnParametersSet()
    {
        if (shownNumber == Number)
            return;

        shownNumber = Number;
        statusMessage = statusText = errorMessage = errorText = null;
        statusCount = errorCount = 0;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string JoinNames(IReadOnlyList<string> names) =>
        names.Count <= 2 ? string.Join(" and ", names) : $"{string.Join(", ", names.Take(names.Count - 1))}, and {names[^1]}";

    private static string TenantPath(int workspaceId) => $"{SaasAdminLayout.Base}/tenants/{workspaceId}";

    private static bool TryParseAmount(string? text, out decimal amount) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out amount);

    private void ClearMessages()
    {
        refundError = refundErrorText = null;
        refundErrorCount = 0;
    }

    private async Task CollectAsync()
    {
        if (Invoice is not { } invoice)
            return;

        var before = State.FindWorkspace(invoice.WorkspaceId);
        var result = State.CollectInvoice(invoice.Number);

        if (result != SaasResult.Ok)
        {
            ShowError(result switch
            {
                SaasResult.CardRequired => "No card on file; the customer must add one in Billing.",
                SaasResult.NotFound => "This invoice is no longer open, so there's nothing to collect.",
                SaasResult.NotAllowed => "Only a signed-in Lumora operator can retry a payment.",
                _ => "The payment couldn't be collected. Try again.",
            });

            return;
        }

        var paid = State.FindInvoice(invoice.Number) ?? invoice;
        var after = State.FindWorkspace(invoice.WorkspaceId);
        var message = $"Collected {SaasDisplay.MoneyCents(paid.Amount)} from the {AppBilling.CardText(paid.CardBrand, paid.CardLast4) ?? "card on file"}. Nothing real is charged in this demo.";

        if (before is { Status: SaasWorkspaceStatus.PastDue } && after is { Status: SaasWorkspaceStatus.Active })
            message += $" {after.Name} is active again.";
        else if (after is { Status: SaasWorkspaceStatus.PastDue })
            message += $" {after.Name} still has open invoices, so it stays past due.";
        else if (after is { Status: SaasWorkspaceStatus.Suspended })
            message += $" {after.Name} stays suspended; change its status on the tenant page.";

        ShowStatus(message);
        StateHasChanged();

        // The button is gone once the invoice is paid.
        await Task.Yield();
        await SaasDisplay.FocusAsync(statusRegion);
    }

    private IReadOnlyList<SaasAuditEntry> History(SaasInvoice invoice)
    {
        var path = $"{SaasAdminLayout.Base}/invoices/{invoice.Number}";

        return State.AuditLog().Where(entry => entry.Link == path).ToList();
    }

    private Task HideRefundAsync() => refundModal.HideAsync();

    /// <summary>The members told about a refund: owners and billing members with a sign-in in this demo, as the state notifies them.</summary>
    private IReadOnlyList<string> NotifiedNames(int workspaceId) =>
        State.MembersOf(workspaceId)
            .Where(member => member.AccountId is not null && member.Role is SaasMemberRole.Owner or SaasMemberRole.Billing)
            .Select(member => member.Name)
            .Distinct()
            .ToList();

    /// <summary>The sentence about who sees a notification of the refund.</summary>
    private string NotifySentence(SaasInvoice invoice, bool done)
    {
        var names = NotifiedNames(invoice.WorkspaceId);

        if (names.Count > 0)
            return done
                ? $"{JoinNames(names)} got a notification in their workspace."
                : $"{JoinNames(names)} {(names.Count == 1 ? "gets" : "get")} a notification in their workspace.";

        var why = State.OwnerOf(invoice.WorkspaceId)?.Name is { } owner ? $"{owner}, the owner, has no sign-in in this demo" : "the tenant has no owner on record";

        return done
            ? $"No one can sign in to see a notification: {why}."
            : $"The owners would get a notification, but no one can sign in to see it: {why}.";
    }

    private async Task OpenRefundAsync()
    {
        if (Invoice is not { } invoice)
            return;

        refunded = false;
        ClearMessages();
        refund = new RefundForm { Amount = (invoice.Amount - invoice.RefundedAmount).ToString("0.00", CultureInfo.InvariantCulture) };

        await refundModal.ShowAsync();
    }

    /// <summary>The modal opens from code, so Bootstrap doesn't move focus back to what opened it; this does (WCAG 2.4.3).</summary>
    private async Task OnRefundHiddenAsync()
    {
        var done = refunded;

        refunded = false;
        refund = new RefundForm();
        ClearMessages();
        StateHasChanged();
        await Task.Yield();

        // A full refund removes the Refund button, so focus goes to the message that says what happened.
        if (done || Invoice is not { Status: SaasInvoiceStatus.Paid } invoice || invoice.Amount - invoice.RefundedAmount <= 0)
            await SaasDisplay.FocusAsync(statusRegion);
        else
            await SaasDisplay.FocusAsync(refundButton?.Element);
    }

    private Task OnRefundShownAsync() => SaasDisplay.FocusAsync(amountInput);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task RefundAsync()
    {
        if (Invoice is not { } invoice)
        {
            ShowRefundError("This invoice no longer exists in this tab. Close the dialog and open the invoice again.");

            return;
        }

        var left = invoice.Amount - invoice.RefundedAmount;

        if (!TryParseAmount(refund.Amount, out var amount))
        {
            ShowRefundError($"Enter the amount as a number, such as {left.ToString("0.00", CultureInfo.InvariantCulture)}.");

            return;
        }

        var result = State.RefundInvoice(invoice.Number, amount, refund.Reason);

        if (result != SaasResult.Ok)
        {
            ShowRefundError(result switch
            {
                SaasResult.Invalid when SaasPricing.Cents(amount) <= 0 || SaasPricing.Cents(amount) > left =>
                    $"Enter an amount from $0.01 to {SaasDisplay.MoneyCents(left)}, what's left to refund on this invoice.",
                SaasResult.Invalid => $"Enter a reason for the refund, up to {MaxReasonLength} characters.",
                SaasResult.NotFound => "This invoice can't be refunded: it isn't paid, or it no longer exists in this tab.",
                SaasResult.NotAllowed => "Only a signed-in Lumora operator can refund an invoice.",
                _ => "The refund couldn't be made. Check the amount and reason, and try again.",
            });

            return;
        }

        var after = State.FindInvoice(invoice.Number) ?? invoice;
        var card = AppBilling.CardText(invoice.CardBrand, invoice.CardLast4);
        var message = $"Refunded {SaasDisplay.MoneyCents(SaasPricing.Cents(amount))} on {invoice.Number}{(card is null ? string.Empty : $" to the {card}")}.";

        message += after.Status == SaasInvoiceStatus.Refunded
            ? " The invoice is now fully refunded."
            : $" {SaasDisplay.MoneyCents(after.Amount - after.RefundedAmount)} can still be refunded.";
        message += $" {NotifySentence(invoice, done: true)} No money moves in this demo.";

        ShowStatus(message);
        refunded = true;

        await refundModal.HideAsync();
    }

    private void SetAmount(decimal amount)
    {
        refund.Amount = amount.ToString("0.00", CultureInfo.InvariantCulture);
        ClearMessages();
    }

    /// <summary>Shows a page error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void ShowError(string message)
    {
        statusMessage = statusText = null;
        statusCount = 0;
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : $"{message} ({errorCount})";
    }

    /// <summary>Shows an error in the refund dialog; the same error again gets a counter.</summary>
    private void ShowRefundError(string message)
    {
        refundErrorCount = message == refundErrorText ? refundErrorCount + 1 : 1;
        refundErrorText = message;
        refundError = refundErrorCount == 1 ? message : $"{message} ({refundErrorCount})";
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void ShowStatus(string message)
    {
        errorMessage = errorText = null;
        errorCount = 0;
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Overview", Href = SaasAdminLayout.Base },
        new() { Text = "Invoices", Href = InvoicesPath },
        new() { Text = Number, IsCurrentPage = true },
    };

    private static string InvoicesPath => $"{SaasAdminLayout.Base}/invoices";

    private SaasInvoice? Invoice => State.FindInvoice(Number);

    /// <summary>Gets or sets the invoice number from the route, such as <c>LUM-001-0002</c>.</summary>
    [Parameter] public string Number { get; set; } = string.Empty;

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The refund dialog's fields; the amount is text so a half-typed value doesn't jump.</summary>
    private sealed class RefundForm
    {
        public string Amount { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }

    #endregion
}
