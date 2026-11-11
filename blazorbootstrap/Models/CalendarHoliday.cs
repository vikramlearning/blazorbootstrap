namespace BlazorBootstrap;

/// <summary>
/// A holiday a <see cref="Calendar{TItem}" /> labels and shades.
/// </summary>
public sealed class CalendarHoliday
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarHoliday" /> class.
    /// </summary>
    /// <param name="date">The date of the holiday.</param>
    /// <param name="name">The name shown on the day.</param>
    public CalendarHoliday(DateOnly date, string name)
    {
        Date = date;
        Name = name;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the date of the holiday.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the date of the holiday.")]
    public DateOnly Date { get; }

    /// <summary>
    /// Gets the name shown on the day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the name shown on the day.")]
    public string Name { get; }

    #endregion
}
