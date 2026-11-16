using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Leave and onboarding wording shared by My leave, My team, Leave approvals, My onboarding, and New hires: a balance for
/// any year worked out from someone's requests (the same sums <see cref="HrState" /> checks), request labels, overlaps, and
/// lists in words. Formatting uses the invariant culture, like <see cref="HrDisplay" />.
/// </summary>
public static class LeaveText
{
    #region Fields and Constants

    /// <summary>The value format of a native date input.</summary>
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary>How far back an unlimited type (like sick leave) can be recorded, in days.</summary>
    public const int UnlimitedBackdateDays = 30;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Formats a date for a native date input.</summary>
    public static string DateValue(DateTime date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);

    /// <summary>Returns someone's first name, for friendly sentences.</summary>
    public static string FirstName(HrEmployee? employee) =>
        employee?.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "They";

    /// <summary>Returns whether a request covers any day of a range.</summary>
    public static bool Overlaps(HrLeaveRequest request, DateTime from, DateTime to) => request.From <= to.Date && from.Date <= request.To;

    /// <summary>Joins words like "a", "a and b", or "a, b, and c".</summary>
    public static string JoinAnd(IReadOnlyList<string> parts) => parts.Count switch
    {
        0 => string.Empty,
        1 => parts[0],
        2 => $"{parts[0]} and {parts[1]}",
        _ => $"{string.Join(", ", parts.Take(parts.Count - 1))}, and {parts[^1]}",
    };

    /// <summary>Parses a native date input's value, or returns <see langword="null" /> (empty or half-typed).</summary>
    public static DateTime? ParseDate(string? text) =>
        DateTime.TryParseExact(text?.Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date.Date : null;

    /// <summary>Names a request, like "Vacation, Dec 21–23, 2026".</summary>
    public static string RequestLabel(HrLeaveRequest request, HrLeavePolicy? policy) => $"{policy?.Name ?? "Leave"}, {HrDisplay.Range(request.From, request.To)}";

    /// <summary>
    /// Works out a limited type's balance for a year from someone's requests: approved days are used, pending days are
    /// waiting; <paramref name="exceptId" /> leaves one request out (the one being edited or decided).
    /// </summary>
    public static YearBalance BalanceFor(IEnumerable<HrLeaveRequest> requests, HrLeavePolicy policy, int year, int? exceptId = null)
    {
        var mine = requests.Where(item => item.PolicyId == policy.Id && item.From.Year == year && item.Id != exceptId).ToList();
        var used = mine.Where(item => item.Status == HrLeaveStatus.Approved).Sum(item => item.Days);
        var pending = mine.Where(item => item.Status == HrLeaveStatus.Pending).Sum(item => item.Days);

        return new YearBalance(policy.AnnualDays, used, pending);
    }

    /// <summary>Returns the company holidays on weekdays in a range (the days a request skips besides weekends).</summary>
    public static IReadOnlyList<HrHoliday> WeekdayHolidays(IEnumerable<HrHoliday> holidays, DateTime from, DateTime to) =>
        holidays.Where(item => item.Date >= from.Date && item.Date <= to.Date && item.Date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)).OrderBy(item => item.Date).ToList();

    /// <summary>Names holidays, like "Christmas Eve (Dec 24) and Christmas Day (Dec 25)".</summary>
    public static string HolidayNames(IReadOnlyList<HrHoliday> holidays) =>
        JoinAnd(holidays.Select(item => $"{item.Name} ({item.Date.ToString("MMM d", CultureInfo.InvariantCulture)})").ToList());

    /// <summary>Says when someone's approved leave is, relative to today: "away today", "away now, until Dec 4, 2026", or "away Dec 21–23, 2026".</summary>
    public static string AwayText(HrLeaveRequest request, DateTime today) =>
        request.From <= today && request.To >= today
            ? request.To == today ? "away today" : $"away now, until {HrDisplay.Date(request.To)}"
            : $"away {HrDisplay.Range(request.From, request.To)}";

    #endregion

    #endregion

    #region Nested Types

    /// <summary>A limited leave type's days for one year.</summary>
    /// <param name="Allowance">Days a year.</param>
    /// <param name="Used">Approved days.</param>
    /// <param name="Pending">Days waiting for approval.</param>
    public sealed record YearBalance(int Allowance, int Used, int Pending)
    {
        /// <summary>Gets the days left after approved and waiting requests.</summary>
        public int Left => Math.Max(0, Allowance - Used - Pending);
    }

    #endregion
}
