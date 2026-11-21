using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One transaction: the amount with its sign and "Money in" or "Money out", the description, date and time, account,
/// status (Pending or Posted), category, and the transfer it belongs to. The customer can move it to another category
/// (not a transfer, which says why), keep a private note with a character counter, and dispute it: an open dispute links
/// to its page with its status, a transaction that can't be disputed says why, and otherwise an <see cref="EditForm" />
/// (rule WR-002) asks what's wrong (required), how much of the payment is disputed (more than $0 and at most the payment; less
/// than all of it when the amount is wrong, a cross-field rule), and what happened (10 to 1,000 characters), with a linked
/// summary on a failed submit; it says what happens next, and then shows the new dispute's number. Each section announces its
/// outcome in a status message, and focus moves to the first problem or to the outcome. An unknown transaction shows a
/// not-found screen.
/// </summary>
public partial class TransactionDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The shortest dispute explanation the bank accepts.</summary>
    private const int MinDisputeDetails = FintechState.MinReason;

    /// <summary>The dispute reasons in plain words, in the order the form lists them.</summary>
    private static readonly IReadOnlyList<(FintechDisputeReason Reason, string Label, string Hint)> disputeReasons = new List<(FintechDisputeReason, string, string)>
    {
        (FintechDisputeReason.NotRecognized, "I don't recognize this payment", "I didn't make it, and I don't know the merchant."),
        (FintechDisputeReason.NotReceived, "I didn't get what I paid for", "The goods or service never arrived, and the merchant hasn't put it right."),
        (FintechDisputeReason.Duplicate, "I was charged more than once", "The same purchase was taken twice."),
        (FintechDisputeReason.WrongAmount, "The amount is wrong", "I was charged more than I agreed to pay."),
        (FintechDisputeReason.Cancelled, "I cancelled, but was still charged", "A subscription or booking I cancelled was charged anyway."),
    };

    private bool categoryIsError;

    private string? categoryMessage;

    private InputTextArea? detailsInput;

    private int detailsLength;

    private EditContext disputeContext = default!;

    private DisputeForm disputeForm = new();

    private ElementReference disputeHeading;

    private bool disputeIsError;

    private string? disputeMessage;

    /// <summary>Whether the dispute form is showing.</summary>
    private bool disputeOpen;

    private ElementReference disputeStatus;

    private int? loadedId;

    private EditContext noteContext = default!;

    private NoteForm noteForm = new();

    private InputTextArea? noteInput;

    private bool noteIsError;

    private int noteLength;

    private string? noteMessage;

    private FocusTarget pendingFocus;

    private string selectedCategory = string.Empty;

    private Button? startDisputeButton;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var target = pendingFocus;
        pendingFocus = FocusTarget.None;

        switch (target)
        {
            // The form's radio buttons are rendered by InputRadio, which has no element reference: the section's heading takes
            // focus, and Tab moves on to the first reason.
            case FocusTarget.FirstReason:
                await FintechDisplay.FocusAsync(disputeHeading);
                break;

            case FocusTarget.Details:
                await FintechDisplay.FocusAsync(detailsInput?.Element);
                break;

            case FocusTarget.DisputeStatus:
                await FintechDisplay.FocusAsync(disputeStatus);
                break;

            case FocusTarget.StartDispute:
                await FintechDisplay.FocusAsync(startDisputeButton?.Element ?? disputeHeading);
                break;

            case FocusTarget.Note:
                await FintechDisplay.FocusAsync(noteInput?.Element);
                break;
        }
    }

    protected override void OnInitialized()
    {
        disputeContext = new EditContext(disputeForm);
        noteContext = new EditContext(noteForm);
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        selectedCategory = Item?.CategoryId ?? string.Empty;
        ResetNoteForm(Item?.Note);
        categoryMessage = null;
        noteMessage = null;
        disputeMessage = null;
        categoryIsError = false;
        noteIsError = false;
        disputeIsError = false;
        ResetDisputeForm();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string AccountPath(string accountId) => $"{FintechLayout.Base}/accounts/{Uri.EscapeDataString(accountId)}";

    private static BadgeColor DisputeColor(FintechDisputeStatus status) => status switch
    {
        FintechDisputeStatus.UnderReview => BadgeColor.Info,
        FintechDisputeStatus.Refunded => BadgeColor.Success,
        FintechDisputeStatus.Declined => BadgeColor.Danger,
        _ => BadgeColor.Secondary,
    };

    private static string DisputePath(int disputeId) => string.Create(CultureInfo.InvariantCulture, $"{FintechLayout.Base}/disputes/{disputeId}");

    /// <summary>Returns where a dispute stands and what happens next, in a sentence.</summary>
    private static string DisputeStatusSentence(FintechDispute dispute) => dispute.Status switch
    {
        FintechDisputeStatus.Submitted => $"Sent on {FintechDisplay.Day(dispute.CreatedAt)}. We'll look into it with the merchant, usually within 10 business days.",
        FintechDisputeStatus.UnderReview => "We're looking into it with the merchant. Most disputes are resolved within 10 business days.",
        FintechDisputeStatus.Refunded => $"Resolved in your favor: {FintechDisplay.Money(dispute.Amount)} was credited back.{(dispute.Resolution is null ? string.Empty : $" {dispute.Resolution}")}",
        _ => $"Resolved: the charge stands.{(dispute.Resolution is null ? string.Empty : $" {dispute.Resolution}")}",
    };

    private static string DisputeStatusText(FintechDisputeStatus status) => status switch
    {
        FintechDisputeStatus.UnderReview => "Under review",
        FintechDisputeStatus.Refunded => "Refunded",
        FintechDisputeStatus.Declined => "Declined",
        _ => "Submitted",
    };

    /// <summary>
    /// Returns the message for an action's result. <paramref name="specific" /> gives the action's own wording; any result
    /// it doesn't cover gets a message of its own here.
    /// </summary>
    private static string ResultText(FintechResult result, Func<FintechResult, string?> specific) => specific(result) ?? result switch
    {
        FintechResult.NotAllowed => "Sign in again with your password and a one-time code to do that. Nothing changed.",
        FintechResult.NotFound => "This transaction isn't on your accounts any more, so nothing changed.",
        FintechResult.Invalid => "Something in that isn't valid, so nothing changed. Check it and try again.",
        FintechResult.WrongState => "This transaction can't be changed that way now, so nothing changed.",
        FintechResult.WrongCode => "The one-time code is wrong or expired. Nothing changed.",
        FintechResult.Duplicate => "That already exists, so nothing changed.",
        FintechResult.LimitReached => "You've reached the limit for this, so nothing changed.",
        FintechResult.InsufficientFunds => "There isn't enough money available for that. Nothing changed.",
        FintechResult.NoChange => "Nothing changed.",
        _ => "Done.",
    };

    /// <summary>Formats a date and time, like "Tuesday, November 24, 2026 at 9:00 AM".</summary>
    private static string Stamp(DateTime value) => value.ToString("dddd, MMMM d, yyyy 'at' h:mm tt", CultureInfo.InvariantCulture);

    private static string TransferPath(string reference) => $"{FintechLayout.Base}/transfers/{Uri.EscapeDataString(reference)}";

    private void CancelDispute()
    {
        ResetDisputeForm();
        pendingFocus = FocusTarget.StartDispute;
    }

    private void OnCategorySelected(ChangeEventArgs args)
    {
        selectedCategory = args.Value?.ToString() ?? string.Empty;
        categoryMessage = null;
        categoryIsError = false;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void ResetDisputeForm()
    {
        disputeOpen = false;
        disputeForm = new DisputeForm { Amount = Item is { } item ? -item.Amount : null, MaxAmount = Item is { } charge ? -charge.Amount : 0 };
        disputeContext = new EditContext(disputeForm);
        detailsLength = 0;
    }

    private void ResetNoteForm(string? note)
    {
        noteForm = new NoteForm { Text = note ?? string.Empty };
        noteContext = new EditContext(noteForm);
        noteLength = noteForm.Text?.Length ?? 0;
    }

    private void SaveCategory()
    {
        if (Item is not { } item)
            return;

        var name = State.Categories.FirstOrDefault(category => category.Id == selectedCategory)?.Name;
        var result = State.Recategorize(item.Id, selectedCategory);

        categoryIsError = result is not (FintechResult.Ok or FintechResult.NoChange);
        categoryMessage = ResultText(result, value => value switch
        {
            FintechResult.Ok => State.FindCategory(selectedCategory).IsSpending
                ? $"Moved to {name}. This month's budgets now count it under {name}."
                : $"Moved to {name}. It no longer counts as spending in your budgets.",
            FintechResult.NoChange => $"It's already in {name ?? "that category"}.",
            FintechResult.Invalid => "Choose one of the listed categories. Transfers keep the Transfers category, so nothing changed.",
            _ => null,
        });
    }

    /// <summary>
    /// Checks the reason, the amount (and that it fits the reason), and the explanation; a failed check shows the summary.
    /// Then it opens the dispute; each result gets its own message.
    /// </summary>
    private void SendDispute()
    {
        if (Item is not { } item)
            return;

        disputeForm.MaxAmount = -item.Amount;

        if (!disputeContext.Validate())
            return;

        var details = disputeForm.Details.Trim();
        var result = State.OpenDispute(item.Id, disputeForm.Reason!.Value, disputeForm.Amount!.Value, details, out var dispute);

        if (result == FintechResult.Ok && dispute is not null)
        {
            ResetDisputeForm();
            disputeIsError = false;
            disputeMessage = $"Dispute {dispute.Id.ToString(CultureInfo.InvariantCulture)} sent. We'll update you within 10 business days; follow it on the dispute's page.";
            pendingFocus = FocusTarget.DisputeStatus;
            return;
        }

        if (result == FintechResult.Invalid)
        {
            disputeIsError = true;
            disputeMessage = $"Choose what's wrong, dispute up to {FintechDisplay.Money(item.Amount)}, and tell us what happened in {MinDisputeDetails} to {FintechState.MaxDisputeDetails.ToString("#,0", CultureInfo.InvariantCulture)} characters. Nothing was sent.";
            pendingFocus = FocusTarget.Details;
            return;
        }

        // The transaction can no longer be disputed (or is gone): the form closes and the reason shows instead.
        ResetDisputeForm();
        disputeIsError = true;
        disputeMessage = ResultText(result, value => value switch
        {
            FintechResult.WrongState => State.FindTransaction(item.Id) is { } current && State.CannotDispute(current) is { } reason
                ? $"{reason} Nothing was sent."
                : "This transaction can't be disputed now. Nothing was sent.",
            FintechResult.NotFound => "This transaction isn't on your accounts any more, so nothing was sent.",
            FintechResult.NotAllowed => "Sign in again with your password and a one-time code to dispute a transaction. Nothing was sent.",
            _ => null,
        });
        pendingFocus = FocusTarget.DisputeStatus;
    }

    private void SaveNote()
    {
        if (Item is not { } item || !noteContext.Validate())
            return;

        var result = State.SetNote(item.Id, noteForm.Text);

        noteIsError = result is not (FintechResult.Ok or FintechResult.NoChange);
        noteMessage = ResultText(result, value => value switch
        {
            FintechResult.Ok => State.FindTransaction(item.Id)?.Note is null ? "Note removed." : "Note saved.",
            FintechResult.NoChange => "The note hasn't changed.",
            FintechResult.Invalid => $"Keep the note to {FintechState.MaxNoteLength} characters, so it wasn't saved.",
            _ => null,
        });

        if (result == FintechResult.Ok)
            ResetNoteForm(State.FindTransaction(item.Id)?.Note);

        if (noteIsError)
            pendingFocus = FocusTarget.Note;
    }

    private void StartDispute()
    {
        ResetDisputeForm();
        disputeOpen = true;
        disputeMessage = null;
        disputeIsError = false;
        pendingFocus = FocusTarget.FirstReason;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Transactions", Href = TransactionsPath },
        new() { Text = Item?.Description ?? "Transaction", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the transaction number from the route.</summary>
    [Parameter] public int Id { get; set; }

    private FintechTransaction? Item => State.FindTransaction(Id);

    [Inject] private FintechState State { get; set; } = default!;

    private static string TransactionsPath => $"{FintechLayout.Base}/transactions";

    #endregion

    #region Nested Types

    /// <summary>
    /// The dispute form's fields, with the bank's limits (<see cref="FintechState.OpenDispute" />): a reason, an amount of more
    /// than $0 and at most the payment, and an explanation of 10 to 1,000 characters. Cross-field: a wrong amount disputes
    /// only the overcharge, so less than the whole payment.
    /// </summary>
    private sealed class DisputeForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter how much of the payment you dispute, like $24.99.")]
        [Range(typeof(decimal), "0.01", "50000", ParseLimitsInInvariantCulture = true, ErrorMessage = "Enter an amount greater than $0.00.")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "Tell us what happened.")]
        [StringLength(FintechState.MaxDisputeDetails, MinimumLength = FintechState.MinReason, ErrorMessage = "Tell us what happened in 10 to 1,000 characters.")]
        public string Details { get; set; } = string.Empty;

        /// <summary>The payment's amount, the most that can be disputed; set by the page, not by a field.</summary>
        public decimal MaxAmount { get; set; }

        [Required(ErrorMessage = "Choose what's wrong with this payment.")]
        public FintechDisputeReason? Reason { get; set; }

        /// <summary>Runs once the fields are filled in: the amount fits the payment and the reason.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Amount is { } amount && amount > MaxAmount)
                yield return new ValidationResult($"You paid {FintechDisplay.Money(MaxAmount)}, so dispute at most that.", new[] { nameof(Amount) });
            else if (Amount is { } part && decimal.Round(part, 2) != part)
                yield return new ValidationResult("Enter the amount in dollars and cents.", new[] { nameof(Amount) });
            else if (Reason == FintechDisputeReason.WrongAmount && Amount == MaxAmount)
                yield return new ValidationResult($"If the amount is wrong, dispute only what you were overcharged: less than the {FintechDisplay.Money(MaxAmount)} you paid.", new[] { nameof(Amount) });
        }
    }

    /// <summary>The note form's field: up to 200 characters, empty to remove the note.</summary>
    private sealed class NoteForm
    {
        [StringLength(FintechState.MaxNoteLength, ErrorMessage = "Keep the note to 200 characters.")]
        public string? Text { get; set; }
    }

    /// <summary>Where focus goes after the next render.</summary>
    private enum FocusTarget
    {
        None,
        FirstReason,
        Details,
        DisputeStatus,
        StartDispute,
        Note,
    }

    #endregion
}
