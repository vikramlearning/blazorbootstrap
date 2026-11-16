using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Headcount analytics (HR admin): KPI cards (headcount today, people joining in the next 30 days, hires and leavers in the
/// last 12 calendar months, 12-month attrition as leavers divided by the average month-end headcount, and the average
/// tenure of the people working here), and charts of the headcount by department and by location, hires and leavers by
/// month, tenure bands (under 1 year, 1 to 2, 2 to 4, 4 or more), and this year's leave days by policy (approved and
/// waiting). Figures are computed from <see cref="HrState.AllEmployees" /> and <see cref="HrState.LeaveOf" />, so adding
/// someone, marking someone as left, or deciding leave changes them at once. Each chart is an <see cref="AdminChart" />
/// with a text summary and a data table in a <c>details</c> element.
/// </summary>
public partial class AdminAnalytics : ComponentBase, IDisposable
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

    /// <summary>One series of people counts, one value per group.</summary>
    private static IReadOnlyList<AdminChartSeries> CountSeries(IReadOnlyList<GroupRow> rows, int colorIndex) =>
        new[] { new AdminChartSeries("People", rows.Select(row => (double)row.Count).ToList(), colorIndex) };

    /// <summary>Approved days and days waiting for approval, one value per policy.</summary>
    private static IReadOnlyList<AdminChartSeries> LeaveSeries(Figures figures) => new[]
    {
        new AdminChartSeries("Approved days", figures.Leave.Select(row => (double)row.Approved).ToList(), 0),
        new AdminChartSeries("Days waiting for approval", figures.Leave.Select(row => (double)row.Waiting).ToList(), 2),
    };

    /// <summary>The short month labels of the hires and leavers chart, like "Dec 25".</summary>
    private static IReadOnlyList<string> MonthLabels(Figures figures) =>
        figures.Months.Select(row => row.Month.ToString("MMM yy", CultureInfo.InvariantCulture)).ToList();

    /// <summary>Hires and leavers, one value per month.</summary>
    private static IReadOnlyList<AdminChartSeries> MonthSeries(Figures figures) => new[]
    {
        new AdminChartSeries("Hires", figures.Months.Select(row => (double)row.Hires).ToList(), 0),
        new AdminChartSeries("Leavers", figures.Months.Select(row => (double)row.Leavers).ToList(), 4),
    };

    private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Whether someone worked here on a day: started by then, and hadn't passed their last day.</summary>
    private static bool EmployedOn(HrEmployee employee, DateTime day) =>
        employee.StartDate.Date <= day && (employee.Status != HrEmployeeStatus.Left || employee.EndDate is not { } end || end.Date >= day);

    private static string JoinNames(IReadOnlyList<string> items) => items.Count switch
    {
        0 => string.Empty,
        1 => items[0],
        2 => $"{items[0]} and {items[1]}",
        _ => $"{string.Join(", ", items.Take(items.Count - 1))}, and {items[^1]}",
    };

    private static string MonthText(DateTime month) => month.ToString("MMM yyyy", CultureInfo.InvariantCulture);

    private static string Share(int part, int whole) =>
        whole == 0 ? "0 %" : string.Create(CultureInfo.InvariantCulture, $"{Math.Round(part * 100.0 / whole):0} %");

    /// <summary>Computes every figure of the page from the people and their leave.</summary>
    private Figures Compute()
    {
        var today = State.Clock.Today;
        var people = State.AllEmployees;
        var active = people.Where(item => AdminHelpers.EffectiveStatus(item, today) == HrEmployeeStatus.Active).ToList();
        var joining = people.Where(item => AdminHelpers.EffectiveStatus(item, today) == HrEmployeeStatus.Joining).OrderBy(item => item.StartDate).ToList();
        var windowStart = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
        var hires = people.Where(item => item.StartDate.Date >= windowStart && item.StartDate.Date <= today).ToList();
        var leavers = people.Where(item => item.Status == HrEmployeeStatus.Left && item.EndDate is { } end && end.Date >= windowStart).ToList();

        // Headcount at the end of each of the 12 months (today for this month).
        var points = Enumerable.Range(0, 12).Select(index =>
        {
            var end = windowStart.AddMonths(index + 1).AddDays(-1);

            return end > today ? today : end;
        });
        var averageHeadcount = points.Average(day => people.Count(item => EmployedOn(item, day)));

        var months = Enumerable.Range(0, 12).Select(index => windowStart.AddMonths(index)).Select(month => new MonthRow(
            month,
            hires.Count(item => item.StartDate.Year == month.Year && item.StartDate.Month == month.Month),
            leavers.Count(item => item.EndDate!.Value.Year == month.Year && item.EndDate.Value.Month == month.Month))).ToList();

        var departments = State.Departments.Select(department => new GroupRow(
            department.Name,
            active.Count(item => item.DepartmentId == department.Id),
            joining.Count(item => item.DepartmentId == department.Id))).ToList();

        var locations = State.Locations.Select(location => new GroupRow(
            location,
            active.Count(item => item.Location == location),
            joining.Count(item => item.Location == location))).ToList();

        var tenure = new[]
        {
            new GroupRow("Under 1 year", active.Count(item => Years(item, today) < 1), 0),
            new GroupRow("1 to 2 years", active.Count(item => Years(item, today) is >= 1 and < 2), 0),
            new GroupRow("2 to 4 years", active.Count(item => Years(item, today) is >= 2 and < 4), 0),
            new GroupRow("4 years or more", active.Count(item => Years(item, today) >= 4), 0),
        };

        var year = today.Year;
        var requests = people.SelectMany(item => State.LeaveOf(item.Id)).Where(request => request.From.Year == year).ToList();
        var leave = State.AllPolicies.Select(policy => new LeaveRow(
                policy,
                requests.Where(request => request.PolicyId == policy.Id && request.Status == HrLeaveStatus.Approved).Sum(request => request.Days),
                requests.Where(request => request.PolicyId == policy.Id && request.Status == HrLeaveStatus.Pending).Sum(request => request.Days),
                requests.Count(request => request.PolicyId == policy.Id && request.Status is HrLeaveStatus.Approved or HrLeaveStatus.Pending)))
            .Where(row => row.Policy.IsActive || row.Requests > 0)
            .ToList();

        return new Figures(today, windowStart, active, joining, hires, leavers, averageHeadcount,
            active.Count == 0 ? 0 : active.Average(item => Years(item, today)), months, departments, locations, tenure, leave);

        static double Years(HrEmployee employee, DateTime day) => (day - employee.StartDate.Date).TotalDays / 365.25;
    }

    private static string DepartmentSummary(Figures figures)
    {
        var total = figures.Active.Count;

        if (total == 0)
            return "Nobody works here today.";

        var ranked = figures.Departments.Where(row => row.Count > 0).OrderByDescending(row => row.Count).ThenBy(row => row.Label, StringComparer.Ordinal).ToList();
        var largest = ranked[0];
        var smallest = ranked[^1];
        var joining = figures.Departments.Where(row => row.Joining > 0).Select(row => $"{row.Label} ({row.Joining})").ToList();

        return $"{largest.Label} is the largest department, with {largest.Count} of {HrDisplay.Plural(total, "person", "people")} ({Share(largest.Count, total)})"
               + (ranked.Count > 1 ? $"; {smallest.Label} is the smallest, with {smallest.Count}." : ".")
               + (joining.Count == 0 ? null : $" Joining soon: {JoinNames(joining)}.");
    }

    private static string LeaveSummary(Figures figures)
    {
        var approved = figures.Leave.Sum(row => row.Approved);
        var waiting = figures.Leave.Sum(row => row.Waiting);

        if (approved == 0 && waiting == 0)
            return $"No leave was approved or requested for {figures.Today.Year} yet.";

        var top = figures.Leave.OrderByDescending(row => row.Approved).First();

        return $"{HrDisplay.Days(approved)} approved for {figures.Today.Year} across the company"
               + (top.Approved == 0 ? "." : $", most of them {top.Policy.Name} ({HrDisplay.Days(top.Approved)}).")
               + (waiting == 0 ? " Nothing is waiting for approval." : $" {HrDisplay.Days(waiting)} {(waiting == 1 ? "is" : "are")} waiting for approval.");
    }

    private static string LocationSummary(Figures figures)
    {
        var total = figures.Active.Count;

        return total == 0
            ? "Nobody works here today."
            : string.Join(", ", figures.Locations.OrderByDescending(row => row.Count).Select(row => $"{row.Label} {row.Count} ({Share(row.Count, total)})")) + ".";
    }

    private static string MonthsSummary(Figures figures)
    {
        var hires = figures.Months.Sum(row => row.Hires);
        var leavers = figures.Months.Sum(row => row.Leavers);
        var net = hires - leavers;
        var leaving = figures.Months.Where(row => row.Leavers > 0).Select(row => MonthText(row.Month)).ToList();

        return $"{HrDisplay.Plural(hires, "hire")} and {HrDisplay.Plural(leavers, "leaver")} from {MonthText(figures.WindowStart)} to {MonthText(figures.Today)}, "
               + (net == 0 ? "no net change." : string.Create(CultureInfo.InvariantCulture, $"a net change of {net:+0;-0}."))
               + (leaving.Count == 0 ? " Nobody left." : $" Someone left in {JoinNames(leaving)}.");
    }

    private static string TenureSummary(Figures figures)
    {
        var total = figures.Active.Count;

        if (total == 0)
            return "Nobody works here today.";

        var largest = figures.Tenure.OrderByDescending(row => row.Count).First();

        return $"The largest group is {largest.Label.ToLowerInvariant()}, with {largest.Count} of {total} ({Share(largest.Count, total)}). "
               + $"{HrDisplay.Plural(figures.Tenure[0].Count, "person", "people")} joined less than a year ago.";
    }

    /// <summary>The KPI cards.</summary>
    private static IReadOnlyList<Kpi> Kpis(Figures figures)
    {
        var soon = figures.Joining.Where(item => item.StartDate.Date <= figures.Today.AddDays(30)).ToList();
        var attrition = figures.AverageHeadcount <= 0 ? 0 : figures.Leavers.Count * 100.0 / figures.AverageHeadcount;
        var window = $"{MonthText(figures.WindowStart)} to {MonthText(figures.Today)}";

        return new[]
        {
            new Kpi("Headcount today", Count(figures.Active.Count), $"Working here on {HrDisplay.Date(figures.Today)}"),
            new Kpi("Joining in the next 30 days", Count(soon.Count),
                soon.Count == 0 ? "Nobody is starting soon" : JoinNames(soon.Select(item => $"{item.Name} on {item.StartDate.ToString("MMM d", CultureInfo.InvariantCulture)}").ToList())),
            new Kpi("Hires, last 12 months", Count(figures.Hires.Count), $"Started {window}"),
            new Kpi("Leavers, last 12 months", Count(figures.Leavers.Count), figures.Leavers.Count == 0 ? $"Nobody left {window}" : $"Last day {window}"),
            new Kpi("12-month attrition", string.Create(CultureInfo.InvariantCulture, $"{attrition:0.0} %"),
                string.Create(CultureInfo.InvariantCulture, $"{HrDisplay.Plural(figures.Leavers.Count, "leaver")} ÷ average headcount of {figures.AverageHeadcount:0.0}")),
            new Kpi("Average tenure", string.Create(CultureInfo.InvariantCulture, $"{figures.AverageTenureYears:0.0} years"), "Of the people working here today"),
        };
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Every figure of the page, computed once per render.</summary>
    private sealed record Figures(
        DateTime Today,
        DateTime WindowStart,
        IReadOnlyList<HrEmployee> Active,
        IReadOnlyList<HrEmployee> Joining,
        IReadOnlyList<HrEmployee> Hires,
        IReadOnlyList<HrEmployee> Leavers,
        double AverageHeadcount,
        double AverageTenureYears,
        IReadOnlyList<MonthRow> Months,
        IReadOnlyList<GroupRow> Departments,
        IReadOnlyList<GroupRow> Locations,
        IReadOnlyList<GroupRow> Tenure,
        IReadOnlyList<LeaveRow> Leave);

    /// <summary>A headcount group: people working here, and people joining.</summary>
    private sealed record GroupRow(string Label, int Count, int Joining);

    /// <summary>A KPI card.</summary>
    private sealed record Kpi(string Label, string Value, string Note);

    /// <summary>A policy's leave this year: approved days, days waiting for approval, and requests.</summary>
    private sealed record LeaveRow(HrLeavePolicy Policy, int Approved, int Waiting, int Requests);

    /// <summary>Hires and leavers in a month.</summary>
    private sealed record MonthRow(DateTime Month, int Hires, int Leavers);

    #endregion
}
