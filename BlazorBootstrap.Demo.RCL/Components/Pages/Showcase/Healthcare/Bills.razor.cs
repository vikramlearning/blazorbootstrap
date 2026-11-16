using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's bills: the total due at the top, bills to pay (amount, description, issue and due dates, overdue
/// in words, and a link to the visit or lab report), and paid bills in a table (paid date, card brand and last four). "Pay"
/// opens an inline test-card form (<see cref="ShowcaseCardForm" />, D-026) under that bill: field errors are shown and the
/// first invalid field takes focus; the card is charged with <see cref="ShowcaseCards.Charge" /> and the bill paid with
/// <see cref="HcState.PayBill" />. A decline is shown in an alert whose text changes with each attempt; success closes the
/// form and announces "Paid $40.00 with Visa ending 4242" in the status region, which takes focus.
/// </summary>
public partial class Bills : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID of the element around the card fields, for focusing the first invalid one.</summary>
    private const string CardFieldsId = "hc-bills-card-fields";

    /// <summary>The Pay buttons by bill ID, so focus returns to one when its form is cancelled.</summary>
    private readonly Dictionary<int, ElementReference> payButtons = new();

    private ShowcaseCardInput card = new();

    /// <summary>How many times the card was declined since the form opened (varies the alert so it is announced again).</summary>
    private int declines;

    /// <summary>The bill whose Pay button takes focus after the next render.</summary>
    private int? focusPayButtonId;

    private FocusTarget focusTarget;

    private EditContext payContext;

    private string? payError;

    private ElementReference? payHeading;

    /// <summary>The bill whose payment form is open, or <see langword="null" />.</summary>
    private int? payingId;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with no payment form open.</summary>
    public Bills() => payContext = new EditContext(card);

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var target = focusTarget;
        var payButtonId = focusPayButtonId;
        focusTarget = FocusTarget.None;
        focusPayButtonId = null;

        switch (target)
        {
            case FocusTarget.Status:
                await HcDisplay.FocusAsync(statusRegion);
                break;

            case FocusTarget.PayHeading:
                await HcDisplay.FocusAsync(payHeading);
                break;

            case FocusTarget.PayButton when payButtonId is { } id && payButtons.TryGetValue(id, out var button):
                await HcDisplay.FocusAsync(button);
                break;

            case FocusTarget.FirstInvalidField:
                await FocusFirstInvalidFieldAsync();
                break;
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

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

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
        focusTarget = FocusTarget.Status;
    }

    /// <summary>Closes the payment form and returns focus to its Pay button.</summary>
    private void CancelPay()
    {
        focusPayButtonId = payingId;
        focusTarget = FocusTarget.PayButton;
        ClosePay();
    }

    /// <summary>Closes the payment form; the typed card is dropped.</summary>
    private void ClosePay()
    {
        payingId = null;
        payError = null;
        declines = 0;
        card = new ShowcaseCardInput();
        payContext = new EditContext(card);
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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Opens the payment form under a bill (closing any other) and moves focus to its heading.</summary>
    private void OpenPay(int billId)
    {
        ClosePay();
        payingId = billId;
        statusMessage = null;
        focusTarget = FocusTarget.PayHeading;
    }

    /// <summary>Checks the card, charges it, pays the bill, and maps every result.</summary>
    private void Pay()
    {
        if (payingId is not { } billId)
            return;

        payError = null;

        // Validate() runs the card form's checks (ShowcaseCards.Validate) and shows each field's message.
        if (!payContext.Validate() || ShowcaseCards.Validate(card, State.Clock.Today).Count > 0)
        {
            focusTarget = FocusTarget.FirstInvalidField;

            return;
        }

        var amount = All().FirstOrDefault(bill => bill.Id == billId) is { } due ? HcDisplay.Money(due.AmountCents) : "the bill";
        var charge = ShowcaseCards.Charge(card);

        switch (State.PayBill(billId, charge))
        {
            case HcResult.Ok:
                var paid = All().FirstOrDefault(bill => bill.Id == billId);
                ClosePay();
                Announce(true, paid is null ? $"Paid {amount}." : $"Paid {amount} with {CardText(paid)}.");
                break;

            case HcResult.CardDeclined:
                // A different text each attempt, so the alert is announced again.
                declines++;
                var failure = charge.FailureMessage ?? "Your card was declined. Try another card.";
                payError = declines == 1 ? $"{failure} Nothing was paid." : string.Create(CultureInfo.InvariantCulture, $"{failure} Nothing was paid. (attempt {declines})");
                break;

            case HcResult.WrongState:
                ClosePay();
                Announce(false, "This bill is already paid, so nothing was charged.");
                break;

            case HcResult.NotFound:
                ClosePay();
                Announce(false, "This bill can't be found any more, so nothing was charged.");
                break;

            case HcResult.NotAllowed:
                ClosePay();
                Announce(false, "Only the patient's own account can pay this bill. Nothing was charged.");
                break;

            default:
                payError = "The payment couldn't be made, and nothing was charged. Try again.";
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Where focus goes after the next render.</summary>
    private enum FocusTarget
    {
        None,
        Status,
        PayHeading,
        PayButton,
        FirstInvalidField,
    }

    #endregion
}
