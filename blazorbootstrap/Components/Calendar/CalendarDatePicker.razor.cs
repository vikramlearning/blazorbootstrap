namespace BlazorBootstrap;

/// <summary>
/// A small month grid for choosing a date. A <see cref="Calendar{TItem}" /> opens it from its title to jump to a date.
/// The calendar renders it; apps do not use it on its own.
/// </summary>
[AddedVersion("4.1.0")]
public partial class CalendarDatePicker : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// A date in the month the picker shows. It starts at <see cref="Date" /> and changes as the user moves between months.
    /// </summary>
    private DateOnly displayDate;

    private bool hasParameterSnapshot;

    private DateOnly previousDateParameter;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (!hasParameterSnapshot || Date != previousDateParameter)
            displayDate = Date;

        previousDateParameter = Date;
        hasParameterSnapshot = true;
    }

    #endregion

    #region Private Methods

    private string GetDayClass(DateOnly day) =>
        BuildClassNames(
            ("btn btn-sm", true),
            ("btn-primary", day == Date),
            ("btn-outline-primary", day != Date && day == EffectiveToday),
            ("border-0", day != Date && day != EffectiveToday),
            ("text-body-tertiary", day != Date && !CalendarLayout.IsSameMonth(day, displayDate, EffectiveCulture)));

    private void MoveMonth(int direction) =>
        displayDate = CalendarLayout.AddPeriod(CalendarView.Month, displayDate, direction, EffectiveCulture, 1);

    #endregion

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames => BuildClassNames(Class, ("bb-calendar-date-picker", true));

    /// <summary>
    /// Gets or sets the culture that supplies the month and day names, the first day of the week, and the calendar system.
    /// <para>
    /// Default value is <see langword="null" />, which uses <see cref="CultureInfo.CurrentCulture" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the culture that supplies the month and day names, the first day of the week, and the calendar system. When null, the current culture is used.")]
    [Parameter]
    public CultureInfo? Culture { get; set; }

    /// <summary>
    /// Gets or sets the date shown as chosen. The picker opens on its month.
    /// <para>
    /// Default value is today.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the date shown as chosen. The picker opens on its month. Default value is today.")]
    [Parameter]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Gets the days the grid shows: the weeks of the displayed month, completed with days of the neighboring months.
    /// </summary>
    private IReadOnlyList<DateOnly> Days
    {
        get
        {
            var culture = EffectiveCulture;
            var firstDayOfWeek = CalendarLayout.GetFirstDayOfWeek(culture, FirstDayOfWeek);
            var (start, end) = CalendarLayout.GetVisibleRange(CalendarView.Month, displayDate, culture, firstDayOfWeek, Array.Empty<DayOfWeek>(), 1);

            return Enumerable.Range(0, end.DayNumber - start.DayNumber + 1).Select(offset => start.AddDays(offset)).ToList();
        }
    }

    private CultureInfo EffectiveCulture => Culture ?? CultureInfo.CurrentCulture;

    private DateOnly EffectiveToday => Today ?? DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Gets or sets the first day of the week. When <see langword="null" />, the culture's first day is used.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the first day of the week. When null, the culture's first day is used.")]
    [Parameter]
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of the next month button.
    /// <para>
    /// Default value is 'Next'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Next")]
    [Description("Gets or sets the accessible name of the next month button.")]
    [Parameter]
    public string NextText { get; set; } = "Next";

    /// <summary>
    /// This event fires when the user chooses a date.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user chooses a date.")]
    [Parameter]
    public EventCallback<DateOnly> OnDateSelected { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of the previous month button.
    /// <para>
    /// Default value is 'Previous'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Previous")]
    [Description("Gets or sets the accessible name of the previous month button.")]
    [Parameter]
    public string PreviousText { get; set; } = "Previous";

    /// <summary>
    /// Gets or sets today's date, which the picker outlines.
    /// <para>
    /// Default value is <see langword="null" />, which uses today on the clock of the machine the code runs on.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets today's date, which the picker outlines. When null, today on the clock of the machine the code runs on is used.")]
    [Parameter]
    public DateOnly? Today { get; set; }

    #endregion
}
