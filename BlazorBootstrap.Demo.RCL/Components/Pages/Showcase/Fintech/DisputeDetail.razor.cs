using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// One of the customer's disputes: the number, the status with a short timeline (Submitted, Under review, then Refunded or
/// Declined; the current step is said in text, not only shown), the merchant and amount, the disputed transaction with a
/// link to it, the reason in words, what the customer told us, the bank's decision once closed, and what happens next for
/// the status. The customer has no actions here besides navigation: bank operations resolve disputes. An unknown number, or
/// someone else's dispute, shows a not-found screen.
/// </summary>
public partial class DisputeDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The business days within which disputes are usually answered.</summary>
    private const int AnswerBusinessDays = 10;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns the date a number of business days (Monday to Friday) after a date.</summary>
    private static DateTime AddBusinessDays(DateTime start, int days)
    {
        var date = start.Date;

        while (days > 0)
        {
            date = date.AddDays(1);

            if (date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                days--;
        }

        return date;
    }

    private static string DefaultResolution(FintechDisputeStatus status) => status == FintechDisputeStatus.Refunded
        ? "We found in your favor and refunded the amount."
        : "We found that the charge was valid, so it stands.";

    private string NextText(FintechDispute dispute)
    {
        var answerBy = AddBusinessDays(dispute.CreatedAt, AnswerBusinessDays).ToString("MMMM d", CultureInfo.InvariantCulture);

        return dispute.Status switch
        {
            FintechDisputeStatus.Submitted =>
                $"We've received your dispute and will start reviewing it with {dispute.Merchant}. We usually answer within {AnswerBusinessDays} business days, so expect to hear from us by {answerBy}. You'll get an alert when it changes; you don't need to do anything.",
            FintechDisputeStatus.UnderReview =>
                $"We're looking into it with {dispute.Merchant}. We usually answer within {AnswerBusinessDays} business days, so expect to hear from us by {answerBy}. You'll get an alert as soon as we decide; you don't need to do anything.",
            FintechDisputeStatus.Refunded =>
                $"{FintechDisplay.Money(dispute.Amount)} was credited back to your account. You'll see the refund in your transactions. There's nothing else to do.",
            _ =>
                "The charge stands and this dispute is closed. If the merchant refunds you directly, the refund will show in your transactions.",
        };
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private static string StatusSentence(FintechDispute dispute) => dispute.Status switch
    {
        FintechDisputeStatus.Submitted => "We've received it and will start reviewing it soon.",
        FintechDisputeStatus.UnderReview => "We're reviewing it with the merchant.",
        FintechDisputeStatus.Refunded => "Closed in your favor: the amount was refunded.",
        _ => "Closed: the charge stands.",
    };

    private static IReadOnlyList<Step> Steps(FintechDispute dispute)
    {
        var closed = dispute.Status is FintechDisputeStatus.Refunded or FintechDisputeStatus.Declined;
        var reviewing = dispute.Status == FintechDisputeStatus.UnderReview;
        var closedOn = dispute.ClosedAt is { } closedAt ? $" on {Disputes.DayText(closedAt)}" : string.Empty;

        return new List<Step>
        {
            new("Submitted", $"You opened it on {Disputes.DayText(dispute.CreatedAt)}.", Done: true, IsCurrent: dispute.Status == FintechDisputeStatus.Submitted),
            new("Under review", reviewing ? "We're checking it with the merchant." : closed ? "We checked it with the merchant." : "We'll check it with the merchant.",
                Done: closed, IsCurrent: reviewing),
            closed
                ? new(Disputes.StatusText(dispute.Status), dispute.Status == FintechDisputeStatus.Refunded ? $"Refunded{closedOn}." : $"Declined{closedOn}.", Done: true, IsCurrent: true)
                : new("Refunded or declined", "We refund you, or explain why the charge stands.", Done: false, IsCurrent: false),
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private FintechDispute? Dispute => State.FindDispute(Id);

    private static string DisputesPath => $"{FintechLayout.Base}/disputes";

    /// <summary>Gets or sets the dispute number from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One step of the dispute timeline: done steps show a check; the current one is named in text.</summary>
    private sealed record Step(string Title, string Note, bool Done, bool IsCurrent);

    #endregion
}
