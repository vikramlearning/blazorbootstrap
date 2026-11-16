using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One transaction: the amount with its sign and "Money in" or "Money out", the description, date and time, account,
/// status (Pending or Posted), category, and the transfer it belongs to. The customer can move it to another category
/// (not a transfer, which says why), keep a private note with a character counter, and dispute it: an open dispute links
/// to its page with its status, a transaction that can't be disputed says why, and otherwise a form asks what's wrong in
/// plain words and what happened, says what happens next, and then shows the new dispute's number. Each section announces
/// its outcome in a status message, and focus moves to the first problem or to the outcome. An unknown transaction shows a
/// not-found screen.
/// </summary>
public partial class TransactionDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The shortest dispute explanation the bank accepts.</summary>
    private const int MinDisputeDetails = 10;

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

    private string? detailsError;

    private ElementReference detailsInput;

    private string? disputeDetails;

    private ElementReference disputeHeading;

    private bool disputeIsError;

    private string? disputeMessage;

    /// <summary>Whether the dispute form is showing.</summary>
    private bool disputeOpen;

    private FintechDisputeReason? disputeReason;

    private ElementReference disputeStatus;

    private ElementReference firstReasonInput;

    private int? loadedId;

    private ElementReference noteInput;

    private bool noteIsError;

    private string? noteMessage;

    private string? noteText;

    private FocusTarget pendingFocus;

    private string? reasonError;

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
            case FocusTarget.FirstReason:
                await FintechDisplay.FocusAsync(firstReasonInput);
                break;

            case FocusTarget.Details:
                await FintechDisplay.FocusAsync(detailsInput);
                break;

            case FocusTarget.DisputeStatus:
                await FintechDisplay.FocusAsync(disputeStatus);
                break;

            case FocusTarget.StartDispute:
                await FintechDisplay.FocusAsync(startDisputeButton?.Element ?? disputeHeading);
                break;

            case FocusTarget.Note:
                await FintechDisplay.FocusAsync(noteInput);
                break;
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        selectedCategory = Item?.CategoryId ?? string.Empty;
        noteText = Item?.Note ?? string.Empty;
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
        disputeReason = null;
        disputeDetails = string.Empty;
        reasonError = null;
        detailsError = null;
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

    /// <summary>Checks the reason and the explanation, then opens the dispute; each result gets its own message.</summary>
    private void SendDispute()
    {
        if (Item is not { } item)
            return;

        var details = disputeDetails?.Trim() ?? string.Empty;
        reasonError = disputeReason is null ? "Choose what's wrong with this payment." : null;
        detailsError = details.Length < MinDisputeDetails ? $"Tell us what happened in at least {MinDisputeDetails} characters."
            : details.Length > FintechState.MaxDisputeDetails ? $"Use {FintechState.MaxDisputeDetails.ToString("#,0", CultureInfo.InvariantCulture)} characters or fewer."
            : null;

        if (reasonError is not null || detailsError is not null)
        {
            pendingFocus = reasonError is not null ? FocusTarget.FirstReason : FocusTarget.Details;
            return;
        }

        var result = State.OpenDispute(item.Id, disputeReason!.Value, details, out var dispute);

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
            detailsError = $"Choose what's wrong, and tell us what happened in {MinDisputeDetails} to {FintechState.MaxDisputeDetails.ToString("#,0", CultureInfo.InvariantCulture)} characters.";
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
        if (Item is not { } item)
            return;

        var result = State.SetNote(item.Id, noteText);

        noteIsError = result is not (FintechResult.Ok or FintechResult.NoChange);
        noteMessage = ResultText(result, value => value switch
        {
            FintechResult.Ok => State.FindTransaction(item.Id)?.Note is null ? "Note removed." : "Note saved.",
            FintechResult.NoChange => "The note hasn't changed.",
            FintechResult.Invalid => $"Keep the note to {FintechState.MaxNoteLength} characters, so it wasn't saved.",
            _ => null,
        });

        if (result == FintechResult.Ok)
            noteText = State.FindTransaction(item.Id)?.Note ?? string.Empty;

        if (noteIsError)
            pendingFocus = FocusTarget.Note;
    }

    private void SelectReason(FintechDisputeReason reason)
    {
        disputeReason = reason;
        reasonError = null;
    }

    private void StartDispute()
    {
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
