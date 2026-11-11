namespace BlazorBootstrap;

/// <summary>
/// Defines how a <see cref="Calendar{TItem}" /> numbers the weeks of a year.
/// </summary>
public enum CalendarWeekNumberRule
{
    /// <summary>
    /// Uses the week rule and first day of the week of the calendar's culture.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Uses the week rule and first day of the week of the calendar's culture.")]
    Culture,

    /// <summary>
    /// Uses ISO 8601 week numbers: weeks start on Monday and week 1 holds the first Thursday of the year.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Uses ISO 8601 week numbers: weeks start on Monday and week 1 holds the first Thursday of the year.")]
    Iso8601
}
