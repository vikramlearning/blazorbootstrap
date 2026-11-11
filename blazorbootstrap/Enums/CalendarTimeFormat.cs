namespace BlazorBootstrap;

/// <summary>
/// Defines how a <see cref="Calendar{TItem}" /> shows times of day.
/// </summary>
public enum CalendarTimeFormat
{
    /// <summary>
    /// Uses the time format of the calendar's culture.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Uses the time format of the calendar's culture.")]
    Culture,

    /// <summary>
    /// Shows times on a 12-hour clock.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows times on a 12-hour clock.")]
    TwelveHour,

    /// <summary>
    /// Shows times on a 24-hour clock.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows times on a 24-hour clock.")]
    TwentyFourHour
}
