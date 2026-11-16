using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Scheduled postings (operations): every scheduled transfer grouped by its date, soonest first, with the customer, the
/// amount, from and to, the reference (linked to <see cref="OpsTransfer" />), a "Due" marker once its date has come, and
/// each day's count and total. "Run due postings" calls <see cref="FintechState.RunPostings" /> and announces how many
/// were processed; postings also run as pages load. "Posted and rejected" lists the last scheduled transfers that were
/// sent or rejected.
/// </summary>
public partial class OpsScheduled : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many sent or rejected scheduled transfers "Posted and rejected" lists.</summary>
    private const int RecentCount = 10;

    /// <summary>The time of day a scheduled transfer posts on its date.</summary>
    private static readonly TimeOnly PostingTime = new(6, 0);

    private string? statusMessage;

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

    /// <summary>Names the account a transfer is sent from: the demo customer's account name, or another customer's masked label.</summary>
    private string FromText(FintechTransfer transfer) =>
        transfer.CustomerId == FintechSeedData.SeededCustomerId ? State.FindAccount(transfer.FromAccountId)?.Name ?? "a closed account" : transfer.FromAccountId;

    /// <summary>Returns whether a scheduled transfer's posting time has come.</summary>
    private bool IsDue(FintechTransfer transfer) => transfer.Date.ToDateTime(PostingTime) <= State.Clock.Now;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void RunPostings()
    {
        var processed = State.RunPostings();

        statusMessage = processed switch
        {
            0 => $"Nothing was due at {Transfers.StampText(State.Clock.Now)}.",
            1 => "1 due transfer processed. It's in \"Posted and rejected\" below.",
            _ => $"{processed.ToString(CultureInfo.InvariantCulture)} due transfers processed. They're in \"Posted and rejected\" below.",
        };
    }

    /// <summary>Says how far a date is from today, like "today", "tomorrow", "in 6 days", or "2 days ago".</summary>
    private string WhenText(DateOnly date)
    {
        var days = date.DayNumber - State.Today.DayNumber;

        return days switch
        {
            0 => "today",
            1 => "tomorrow",
            -1 => "yesterday",
            > 1 => string.Create(CultureInfo.InvariantCulture, $"in {days} days"),
            _ => string.Create(CultureInfo.InvariantCulture, $"{-days} days ago"),
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the scheduled transfers by date, soonest first; within a day, in the order they were confirmed.</summary>
    private List<ScheduledDay> Days => State.AllTransfers.Where(transfer => transfer.Status == FintechTransferStatus.Scheduled)
        .GroupBy(transfer => transfer.Date)
        .OrderBy(group => group.Key)
        .Select(group => new ScheduledDay(group.Key, group.OrderBy(transfer => transfer.CreatedAt).ToList()))
        .ToList();

    /// <summary>Gets the last scheduled transfers (dated after the day they were confirmed) that were sent or rejected, newest first.</summary>
    private List<FintechTransfer> Recent => State.AllTransfers
        .Where(transfer => transfer.Status is FintechTransferStatus.Completed or FintechTransferStatus.Rejected && transfer.Date > DateOnly.FromDateTime(transfer.CreatedAt))
        .OrderByDescending(transfer => transfer.ClosedAt ?? transfer.CreatedAt)
        .Take(RecentCount)
        .ToList();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The scheduled transfers of one date.</summary>
    private sealed record ScheduledDay(DateOnly Date, List<FintechTransfer> Items);

    #endregion
}
