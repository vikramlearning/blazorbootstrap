using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Operations' list of every customer's disputes, newest first, with the count per status and a status filter (open, that
/// is submitted or under review, the default; closed; or all, each with its count). Each row has the number linking to the
/// dispute, the customer linking to their profile, the merchant, the amount, the reason in words, the status as a text
/// badge, the date opened, and the age (how long it has been open, or how long it took to close). The reason and age texts
/// are shared with <see cref="OpsDispute" /> and <see cref="OpsCustomer" />.
/// </summary>
public partial class OpsDisputes : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly IReadOnlyList<FilterOption> FilterOptions = new List<FilterOption>
    {
        new(DisputeFilter.Open, "Open"),
        new(DisputeFilter.Closed, "Closed"),
        new(DisputeFilter.All, "All"),
    };

    private DisputeFilter filter = DisputeFilter.Open;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Says how long a dispute has been open, or, once closed, how long it took, in whole days.</summary>
    internal static string AgeText(FintechDispute dispute, DateTime now)
    {
        var days = (int)Math.Floor(((dispute.ClosedAt ?? now) - dispute.CreatedAt).TotalDays);
        var span = days switch
        {
            < 1 => "under a day",
            1 => "1 day",
            _ => $"{days.ToString(CultureInfo.InvariantCulture)} days",
        };

        return dispute.ClosedAt is null ? char.ToUpperInvariant(span[0]) + span[1..] : $"Closed in {span}";
    }

    /// <summary>Returns the operations route of a dispute.</summary>
    internal static string DisputePath(int disputeId) => $"{FintechOpsLayout.Base}/disputes/{disputeId.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>Returns whether a dispute is still open: submitted or under review.</summary>
    internal static bool IsOpen(FintechDispute dispute) => dispute.Status is FintechDisputeStatus.Submitted or FintechDisputeStatus.UnderReview;

    /// <summary>Returns a dispute reason in words, as operations read it.</summary>
    internal static string ReasonText(FintechDisputeReason reason) => reason switch
    {
        FintechDisputeReason.NotRecognized => "Doesn't recognize the charge",
        FintechDisputeReason.NotReceived => "Didn't get what they paid for",
        FintechDisputeReason.Duplicate => "Charged twice",
        FintechDisputeReason.WrongAmount => "Wrong amount",
        FintechDisputeReason.Cancelled => "Cancelled, but still charged",
        _ => "Other",
    };

    #endregion

    #region Private Methods

    private int Count(DisputeFilter value) => All.Count(dispute => Matches(dispute, value));

    private string CustomerName(string customerId) => State.FindCustomer(customerId)?.Name ?? customerId;

    private static bool Matches(FintechDispute dispute, DisputeFilter value) => value switch
    {
        DisputeFilter.Open => IsOpen(dispute),
        DisputeFilter.Closed => !IsOpen(dispute),
        _ => true,
    };

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    private IReadOnlyList<FintechDispute> All => State.AllDisputes;

    private string CaptionText
    {
        get
        {
            var count = Shown.Count.ToString(CultureInfo.InvariantCulture);
            var noun = Shown.Count == 1 ? "dispute" : "disputes";

            return filter switch
            {
                DisputeFilter.Open => $"{count} open {noun}, newest first.",
                DisputeFilter.Closed => $"{count} closed {noun}, newest first.",
                _ => $"All {count} {noun}, newest first.",
            };
        }
    }

    private string EmptyText => filter switch
    {
        DisputeFilter.Open => "No open disputes: every dispute has been refunded or declined.",
        DisputeFilter.Closed => "No closed disputes yet.",
        _ => "No customer has opened a dispute.",
    };

    private IReadOnlyList<FintechDispute> Shown => All.Where(dispute => Matches(dispute, filter)).ToList();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Which disputes the list shows.</summary>
    private enum DisputeFilter
    {
        /// <summary>Submitted or under review.</summary>
        Open,

        /// <summary>Refunded or declined.</summary>
        Closed,

        /// <summary>Every dispute.</summary>
        All,
    }

    /// <summary>One choice of the status filter.</summary>
    private sealed record FilterOption(DisputeFilter Filter, string Label);

    #endregion
}
