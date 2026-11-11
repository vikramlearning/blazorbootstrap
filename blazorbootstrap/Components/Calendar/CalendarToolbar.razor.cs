namespace BlazorBootstrap;

/// <summary>
/// The toolbar of a <see cref="Calendar{TItem}" />: previous, next, Today, the title of the visible period, and the view switch.
/// The calendar renders it; apps do not use it on its own.
/// </summary>
[AddedVersion("4.1.0")]
public partial class CalendarToolbar : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isPickerOpen;

    /// <summary>
    /// The element inside the toolbar to move keyboard focus to after the next render, as a CSS selector, or <see langword="null" />.
    /// </summary>
    private string? pendingFocusSelector;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (pendingFocusSelector is null)
            return;

        var selector = pendingFocusSelector;
        pendingFocusSelector = null;

        await SafeInvokeVoidAsync("window.blazorBootstrap.calendar.focusIn", Id, selector);
    }

    #endregion

    #region Private Methods

    private void ClosePicker() => isPickerOpen = false;

    private Task OnViewSelectChangedAsync(ChangeEventArgs args) =>
        Enum.TryParse<CalendarView>(args.Value?.ToString(), out var view)
            ? OnViewClick.InvokeAsync(view)
            : Task.CompletedTask;

    private Task SelectDateAsync(DateOnly date)
    {
        ClosePicker();
        pendingFocusSelector = ".bb-calendar-title";

        return OnDateSelected.InvokeAsync(date);
    }

    private void TogglePicker()
    {
        isPickerOpen = !isPickerOpen;

        // Focus moves into the picker, to the day the Tab key stops at.
        if (isPickerOpen)
            pendingFocusSelector = ".bb-calendar-date-picker-grid > [tabindex='0']";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the text of the Agenda view button.
    /// <para>
    /// Default value is 'Agenda'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Agenda")]
    [Description("Gets or sets the text of the Agenda view button.")]
    [Parameter]
    public string AgendaText { get; set; } = "Agenda";

    protected override string? ClassNames =>
        BuildClassNames(Class, ("bb-calendar-toolbar d-flex flex-wrap align-items-center gap-2 mb-2", true));

    /// <summary>
    /// Gets or sets the culture the jump-to-date picker uses.
    /// <para>
    /// Default value is <see langword="null" />, which uses <see cref="CultureInfo.CurrentCulture" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the culture the jump-to-date picker uses. When null, the current culture is used.")]
    [Parameter]
    public CultureInfo? Culture { get; set; }

    /// <summary>
    /// Gets or sets the date the calendar shows. The jump-to-date picker opens on its month.
    /// <para>
    /// Default value is today.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Gets or sets the date the calendar shows. The jump-to-date picker opens on its month. Default value is today.")]
    [Parameter]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Gets or sets the text of the Day view button.
    /// <para>
    /// Default value is 'Day'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Day")]
    [Description("Gets or sets the text of the Day view button.")]
    [Parameter]
    public string DayText { get; set; } = "Day";

    /// <summary>
    /// Gets or sets the first day of the week the jump-to-date picker uses. When <see langword="null" />, the culture's first day is used.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the first day of the week the jump-to-date picker uses. When null, the culture's first day is used.")]
    [Parameter]
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the text of the Month view button.
    /// <para>
    /// Default value is 'Month'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Month")]
    [Description("Gets or sets the text of the Month view button.")]
    [Parameter]
    public string MonthText { get; set; } = "Month";

    /// <summary>
    /// Gets or sets the accessible name of the next button.
    /// <para>
    /// Default value is 'Next'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Next")]
    [Description("Gets or sets the accessible name of the next button.")]
    [Parameter]
    public string NextText { get; set; } = "Next";

    /// <summary>
    /// This event fires when the user chooses a date in the jump-to-date picker.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user chooses a date in the jump-to-date picker.")]
    [Parameter]
    public EventCallback<DateOnly> OnDateSelected { get; set; }

    /// <summary>
    /// This event fires when the user activates the next button.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user activates the next button.")]
    [Parameter]
    public EventCallback OnNextClick { get; set; }

    /// <summary>
    /// This event fires when the user activates the previous button.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user activates the previous button.")]
    [Parameter]
    public EventCallback OnPreviousClick { get; set; }

    /// <summary>
    /// This event fires when the user activates the Today button.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user activates the Today button.")]
    [Parameter]
    public EventCallback OnTodayClick { get; set; }

    /// <summary>
    /// This event fires when the user chooses a view.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("This event fires when the user chooses a view.")]
    [Parameter]
    public EventCallback<CalendarView> OnViewClick { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of the previous button.
    /// <para>
    /// Default value is 'Previous'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Previous")]
    [Description("Gets or sets the accessible name of the previous button.")]
    [Parameter]
    public string PreviousText { get; set; } = "Previous";

    /// <summary>
    /// Gets or sets the title of the visible period.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the title of the visible period.")]
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets today's date, which the jump-to-date picker outlines.
    /// <para>
    /// Default value is <see langword="null" />, which uses today on the clock of the machine the code runs on.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets today's date, which the jump-to-date picker outlines. When null, today on the clock of the machine the code runs on is used.")]
    [Parameter]
    public DateOnly? Today { get; set; }

    /// <summary>
    /// Gets or sets the text of the Today button.
    /// <para>
    /// Default value is 'Today'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Today")]
    [Description("Gets or sets the text of the Today button.")]
    [Parameter]
    public string TodayText { get; set; } = "Today";

    /// <summary>
    /// Gets or sets the view shown as selected.
    /// <para>
    /// Default value is <see cref="CalendarView.Month" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(CalendarView.Month)]
    [Description("Gets or sets the view shown as selected.")]
    [Parameter]
    public CalendarView View { get; set; } = CalendarView.Month;

    private IEnumerable<(CalendarView View, string Text)> ViewItems =>
        new[]
        {
            (CalendarView.Month, MonthText),
            (CalendarView.Week, WeekText),
            (CalendarView.WorkWeek, WorkWeekText),
            (CalendarView.Day, DayText),
            (CalendarView.Agenda, AgendaText)
        };

    /// <summary>
    /// Gets or sets the accessible name of the view switch.
    /// <para>
    /// Default value is 'View'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("View")]
    [Description("Gets or sets the accessible name of the view switch.")]
    [Parameter]
    public string ViewText { get; set; } = "View";

    /// <summary>
    /// Gets or sets the text of the Week view button.
    /// <para>
    /// Default value is 'Week'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Week")]
    [Description("Gets or sets the text of the Week view button.")]
    [Parameter]
    public string WeekText { get; set; } = "Week";

    /// <summary>
    /// Gets or sets the text of the Work week view button.
    /// <para>
    /// Default value is 'Work week'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Work week")]
    [Description("Gets or sets the text of the Work week view button.")]
    [Parameter]
    public string WorkWeekText { get; set; } = "Work week";

    #endregion
}
