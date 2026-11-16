using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The customer's disputes, newest first: number, merchant, amount, the reason in words, the status as a text badge, and
/// the date opened, each linking to its detail page. Beside the list, how disputes work and a link to the transactions,
/// where a dispute is opened. Without disputes the list shows an empty state. The reason and status texts are shared with
/// <see cref="DisputeDetail" />.
/// </summary>
public partial class Disputes : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

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

    private IReadOnlyList<FintechDispute> MyDisputes => State.MyDisputes();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
