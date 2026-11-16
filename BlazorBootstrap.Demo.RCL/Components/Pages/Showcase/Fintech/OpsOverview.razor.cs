using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Operations overview: a greeting with the operations user's first name and the site's date; figure cards that open
/// their modules (transfers waiting for review with their total and the oldest wait, open disputes and how many aren't
/// started, scheduled postings in the next 7 days with their total and how many are due now, customers on Watch, and
/// today's audit entries); a "Needs attention" list of the 5 longest-waiting held transfers and the submitted disputes,
/// each linking to its operations page; and the last 8 audit entries with a link to the audit log. Every list has an
/// empty state.
/// </summary>
public partial class OpsOverview : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many held transfers "Needs attention" lists.</summary>
    private const int AttentionCount = 5;

    /// <summary>How many days ahead the scheduled postings figure looks.</summary>
    private const int PostingDays = 7;

    /// <summary>How many audit entries "Recent activity" lists.</summary>
    private const int RecentCount = 8;

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

    private string CustomerName(string customerId) => State.FindCustomer(customerId)?.Name ?? "Unknown customer";

    private static string DisputePath(int disputeId) => $"{FintechOpsLayout.Base}/disputes/{disputeId.ToString(CultureInfo.InvariantCulture)}";

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Formats an audit time: the time alone for today, the date and time before that.</summary>
    private string TimeText(DateTime at) => at.Date == State.Clock.Now.Date
        ? at.ToString("HH:mm", CultureInfo.InvariantCulture)
        : at.ToString("MMM d, HH:mm", CultureInfo.InvariantCulture);

    private static string TransferPath(string reference) => $"{FintechOpsLayout.Base}/transfers/{Uri.EscapeDataString(reference)}";

    /// <summary>Says how long something has waited, like "45 min", "3 h 10 min", or "2 days".</summary>
    private static string WaitText(TimeSpan wait)
    {
        if (wait < TimeSpan.FromMinutes(1))
            return "under a minute";

        if (wait < TimeSpan.FromHours(1))
            return string.Create(CultureInfo.InvariantCulture, $"{(int)wait.TotalMinutes} min");

        if (wait < TimeSpan.FromDays(1))
            return wait.Minutes == 0
                ? string.Create(CultureInfo.InvariantCulture, $"{(int)wait.TotalHours} h")
                : string.Create(CultureInfo.InvariantCulture, $"{(int)wait.TotalHours} h {wait.Minutes} min");

        var days = (int)wait.TotalDays;

        return string.Create(CultureInfo.InvariantCulture, $"{days} {(days == 1 ? "day" : "days")}");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets how many audit entries were recorded since midnight on the site's clock.</summary>
    private int ActionsToday => State.AuditLog.Count(entry => entry.At >= State.Clock.Now.Date);

    private static string AuditPath => $"{FintechOpsLayout.Base}/audit";

    private static string CustomersPath => $"{FintechOpsLayout.Base}/customers";

    private static string DisputesPath => $"{FintechOpsLayout.Base}/disputes";

    private string DueNowText
    {
        get
        {
            var due = UpcomingPostings.Count(transfer => transfer.Date.ToDateTime(new TimeOnly(6, 0)) <= State.Clock.Now);

            return due switch
            {
                0 => "None due now",
                1 => "1 due now",
                _ => string.Create(CultureInfo.InvariantCulture, $"{due} due now"),
            };
        }
    }

    /// <summary>Gets the operations user's first name for the greeting.</summary>
    private string FirstName =>
        State.Identity.Current?.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "there";

    /// <summary>Gets the greeting for the time of day on the site's clock.</summary>
    private string Greeting => State.Clock.Now.Hour switch
    {
        < 12 => "Good morning",
        < 18 => "Good afternoon",
        _ => "Good evening",
    };

    /// <summary>Gets the transfers held for a check, longest waiting first.</summary>
    private IReadOnlyList<FintechTransfer> Held => State.AllTransfers
        .Where(transfer => transfer.Status == FintechTransferStatus.Held)
        .OrderBy(transfer => transfer.CreatedAt)
        .ToList();

    private string NotStartedText
    {
        get
        {
            var count = SubmittedDisputes.Count;

            return count switch
            {
                0 => "All started",
                1 => "1 not yet started",
                _ => string.Create(CultureInfo.InvariantCulture, $"{count} not yet started"),
            };
        }
    }

    /// <summary>Gets the held transfers "Needs attention" lists.</summary>
    private IReadOnlyList<FintechTransfer> OldestHeld => Held.Take(AttentionCount).ToList();

    private string OldestHeldText => Held.Count == 0
        ? "Nothing waiting"
        : $"Oldest waiting {WaitText(State.Clock.Now - Held[0].CreatedAt)}";

    /// <summary>Gets the disputes that are submitted or under review.</summary>
    private IReadOnlyList<FintechDispute> OpenDisputes => State.AllDisputes
        .Where(dispute => dispute.Status is FintechDisputeStatus.Submitted or FintechDisputeStatus.UnderReview)
        .ToList();

    private static string QueuePath => $"{FintechOpsLayout.Base}/queue";

    private IReadOnlyList<FintechAuditEntry> Recent => State.AuditLog.Take(RecentCount).ToList();

    private static string ScheduledPath => $"{FintechOpsLayout.Base}/scheduled";

    [Inject] private FintechState State { get; set; } = default!;

    /// <summary>Gets the disputes no one has started reviewing, oldest first.</summary>
    private IReadOnlyList<FintechDispute> SubmittedDisputes => State.AllDisputes
        .Where(dispute => dispute.Status == FintechDisputeStatus.Submitted)
        .OrderBy(dispute => dispute.CreatedAt)
        .ToList();

    /// <summary>Gets the scheduled transfers due now or in the next <see cref="PostingDays" /> days.</summary>
    private IReadOnlyList<FintechTransfer> UpcomingPostings => State.AllTransfers
        .Where(transfer => transfer.Status == FintechTransferStatus.Scheduled && transfer.Date <= State.Today.AddDays(PostingDays))
        .ToList();

    private int WatchCount => State.Customers.Count(customer => customer.Status == FintechCustomerStatus.Watch);

    #endregion
}
