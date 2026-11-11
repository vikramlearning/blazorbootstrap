namespace BlazorBootstrap;

/// <summary>
/// Defines the views of a <see cref="Calendar{TItem}" />.
/// </summary>
public enum CalendarView
{
    /// <summary>
    /// Shows one month as a grid of weeks.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows one month as a grid of weeks.")]
    Month,

    /// <summary>
    /// Shows one week as day columns over an hourly grid.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows one week as day columns over an hourly grid.")]
    Week,

    /// <summary>
    /// Shows the working days of one week as day columns over an hourly grid.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the working days of one week as day columns over an hourly grid.")]
    WorkWeek,

    /// <summary>
    /// Shows one day over an hourly grid.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows one day over an hourly grid.")]
    Day,

    /// <summary>
    /// Shows the events of a period as a list grouped by day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Shows the events of a period as a list grouped by day.")]
    Agenda
}
