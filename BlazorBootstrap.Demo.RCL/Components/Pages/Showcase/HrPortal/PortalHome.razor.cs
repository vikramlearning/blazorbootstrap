using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Portal home (everyone signed in): a greeting with the person's name and title, then cards that each link to their page:
/// the leave balances this year (the main limited type with a meter of used and pending days, the others in words), the next
/// approved leave, the onboarding progress when there is a plan ("6 of 10 done, 2 overdue"), who's away today and later this
/// week among the people whose leave the signed-in person may see (themselves and their reports; HR sees everyone's), the next company holidays, and, for managers and HR, the leave requests and new hires waiting; HR also gets
/// quick links to its tools.
/// </summary>
public partial class PortalHome : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Says a balance in words: "18 days available: 5 used and 2 waiting for approval, of 25" or "1 day used (no limit)".</summary>
    private static string BalanceText(HrBalance balance) =>
        balance.Policy.IsUnlimited
            ? $"{HrDisplay.Days(balance.Used)} used{(balance.Pending > 0 ? $", {HrDisplay.Days(balance.Pending)} waiting for approval" : null)} (no yearly limit)"
            : string.Create(CultureInfo.InvariantCulture,
                $"{HrDisplay.Days(balance.Available)} available: {HrDisplay.Days(balance.Used)} used and {HrDisplay.Days(balance.Pending)} waiting for approval, of {balance.Allowance} this year.");

    /// <summary>Says when a due date is: "due today", "due tomorrow", "due Dec 1", or "overdue since Nov 5".</summary>
    private static string DueText(DateTime due, DateTime today) =>
        due.Date == today ? "due today"
        : due.Date == today.AddDays(1) ? "due tomorrow"
        : due.Date < today ? "overdue since " + due.ToString("MMM d", CultureInfo.InvariantCulture)
        : "due " + due.ToString("MMM d", CultureInfo.InvariantCulture);

    private static string FirstName(string name) => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;

    private static string Greeting(DateTime now) => now.Hour < 12 ? "Good morning" : now.Hour < 18 ? "Good afternoon" : "Good evening";

    /// <summary>Says how far away a day is: "today", "tomorrow", or "in 9 days".</summary>
    private static string InDays(DateTime date, DateTime today)
    {
        var days = (int)(date.Date - today).TotalDays;

        return days switch { 0 => "today", 1 => "tomorrow", _ => "in " + HrDisplay.Days(days) };
    }

    /// <summary>Says onboarding progress, like "6 of 10 done, 2 overdue".</summary>
    private static string OnboardingText(int done, int total, int overdue) =>
        string.Create(CultureInfo.InvariantCulture, $"{done} of {total} done{(overdue > 0 ? $", {overdue} overdue" : null)}");

    private static double Percent(int part, int whole) => whole <= 0 ? 0 : Math.Clamp(part * 100d / whole, 0, 100);

    /// <summary>Says when approved leave starts: "Starts in 4 days", "Starts tomorrow", or "You're on leave until Dec 31".</summary>
    private static string StartsText(HrLeaveRequest request, DateTime today) =>
        request.From <= today ? $"You're on leave until {HrDisplay.Date(request.To)}."
        : request.From == today.AddDays(1) ? "Starts tomorrow."
        : $"Starts in {HrDisplay.Days((int)(request.From - today).TotalDays)}.";

    /// <summary>
    /// Approved leave this week (today to Sunday) of the signed-in person's team: their manager's reports and their own
    /// reports. Only people whose leave they may see are listed; <c>Hidden</c> counts the others.
    /// </summary>
    private AwayWeek AwayThisWeek(HrEmployee me, DateTime today)
    {
        var weekEnd = today.AddDays(((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7);
        var team = new List<HrEmployee>();

        if (me.ManagerId is { } managerId)
            team.AddRange(State.DirectReports(managerId));

        team.AddRange(State.DirectReports(me.Id));
        team.Add(me);
        team = team.DistinctBy(person => person.Id).ToList();

        var visible = team.Where(person => person.Id == me.Id || State.IsHrAdmin || person.ManagerId == me.Id).ToList();
        var leave = visible.SelectMany(person => State.LeaveOf(person.Id)
                .Where(request => request.Status == HrLeaveStatus.Approved && request.From <= weekEnd && request.To >= today)
                .Select(request => new AwayEntry(person, request)))
            .OrderBy(entry => entry.Request.From)
            .ThenBy(entry => entry.Person.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new AwayWeek(
            leave.Where(entry => entry.Request.From <= today).ToList(),
            leave.Where(entry => entry.Request.From > today).ToList(),
            team.Count - visible.Count);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Someone's approved leave that falls in this week.</summary>
    private sealed record AwayEntry(HrEmployee Person, HrLeaveRequest Request);

    /// <summary>Who's away today and later this week, and how many teammates' leave isn't visible.</summary>
    private sealed record AwayWeek(IReadOnlyList<AwayEntry> Today, IReadOnlyList<AwayEntry> Later, int Hidden);

    #endregion
}
