namespace BlazorBootstrap;

/// <summary>
/// Describes how a <see cref="Calendar{TItem}" /> marks one day: a label, a highlight color, and non-working shading.
/// </summary>
public sealed class CalendarDayMark
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the highlight color of the day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(CalendarColor.None)]
    [Description("Gets or sets the highlight color of the day.")]
    public CalendarColor Color { get; set; } = CalendarColor.None;

    /// <summary>
    /// Gets or sets whether the day is shaded as a non-working day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the day is shaded as a non-working day.")]
    public bool IsNonWorking { get; set; }

    /// <summary>
    /// Gets or sets the label shown on the day.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the label shown on the day.")]
    public string? Label { get; set; }

    #endregion
}
