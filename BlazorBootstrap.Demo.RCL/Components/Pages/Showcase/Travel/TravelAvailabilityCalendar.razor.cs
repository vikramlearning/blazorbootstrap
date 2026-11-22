using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// A read-only availability calendar of one stay: two months side by side (stacked on a phone), with Previous and Next
/// buttons that never go before the current month or past the booking window. Each month is a table with a caption and
/// weekday headers; each date says in words whether its night is available, unavailable (booked, requested, or blocked by
/// the host; also struck through), today, or part of the chosen dates. A booking being changed can be left out of the
/// unavailable nights (<see cref="ExceptReference" />).
/// </summary>
public partial class TravelAvailabilityCalendar : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The number of months shown at once.</summary>
    private const int MonthCount = 2;

    private static readonly string[] weekdays = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;

    private DateOnly? lastCheckIn;

    private int? lastStayId;

    private ElementReference nextButton;

    private ElementReference previousButton;

    /// <summary>The first day of the first month shown; clamped to the allowed months when read (<see cref="FirstMonth" />).</summary>
    private DateOnly shownMonth;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>
    /// A new stay starts at <see cref="Month" />, the check-in month, or this month. A check-in that moves outside the
    /// months shown brings its month into view.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (lastStayId != StayId)
        {
            lastStayId = StayId;
            lastCheckIn = CheckIn;
            shownMonth = StartOfMonth(Month ?? CheckIn ?? State.Today);

            return;
        }

        if (CheckIn != lastCheckIn)
        {
            lastCheckIn = CheckIn;

            if (CheckIn is { } checkIn && (checkIn < FirstMonth || checkIn >= FirstMonth.AddMonths(MonthCount)))
                shownMonth = StartOfMonth(checkIn);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string MonthName(DateOnly month) => month.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    private static DateOnly StartOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    /// <summary>Returns the weeks of a month, Sunday first, with <see langword="null" /> for the days outside it.</summary>
    private static IEnumerable<DateOnly?[]> Weeks(DateOnly month)
    {
        var week = new DateOnly?[7];
        var column = (int)month.DayOfWeek;

        for (var date = month; date.Month == month.Month; date = date.AddDays(1))
        {
            week[column++] = date;

            if (column == 7)
            {
                yield return week;
                week = new DateOnly?[7];
                column = 0;
            }
        }

        if (column > 0)
            yield return week;
    }

    private string DayClass(DateOnly date, bool isUnavailable)
    {
        var classes = new List<string>(5) { "ds-cal-day" };

        if (date < State.Today)
            classes.Add("ds-cal-past");
        else if (isUnavailable)
            classes.Add("ds-cal-off");

        if (date == State.Today)
            classes.Add("ds-cal-today");

        if (date == CheckIn)
            classes.Add("ds-cal-in");
        else if (date == CheckOut)
            classes.Add("ds-cal-out");
        else if (IsInStay(date))
            classes.Add("ds-cal-range");

        return string.Join(' ', classes);
    }

    /// <summary>Returns the words a screen reader reads for a date, like "December 3, today, available, your check-in".</summary>
    private string DayText(DateOnly date, bool isUnavailable)
    {
        var parts = new List<string>(4) { date.ToString("MMMM d", CultureInfo.InvariantCulture) };

        if (date == State.Today)
            parts.Add("today");

        parts.Add(date < State.Today ? "past" : isUnavailable ? "unavailable" : "available");

        if (date == CheckIn)
            parts.Add("your check-in");
        else if (date == CheckOut)
            parts.Add("your check-out");
        else if (IsInStay(date))
            parts.Add("in your stay");

        return string.Join(", ", parts);
    }

    private bool IsInStay(DateOnly date) => HasSelection && date > CheckIn!.Value && date < CheckOut!.Value;

    /// <summary>A button that becomes disabled loses focus, so focus moves to the other one.</summary>
    private async Task NextAsync()
    {
        if (!CanGoForward)
            return;

        shownMonth = FirstMonth.AddMonths(1);

        if (!CanGoForward)
        {
            await TravelDisplay.FocusAsync(previousButton);
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>A button that becomes disabled loses focus, so focus moves to the other one.</summary>
    private async Task PreviousAsync()
    {
        if (!CanGoBack)
            return;

        shownMonth = FirstMonth.AddMonths(-1);

        if (!CanGoBack)
        {
            await TravelDisplay.FocusAsync(nextButton);
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private bool CanGoBack => FirstMonth > EarliestMonth;

    private bool CanGoForward => FirstMonth < LatestMonth;

    /// <summary>Gets or sets the chosen check-in, shown as the start of the shaded dates.</summary>
    [Parameter] public DateOnly? CheckIn { get; set; }

    /// <summary>Gets or sets the chosen check-out, shown as the end of the shaded dates.</summary>
    [Parameter] public DateOnly? CheckOut { get; set; }

    /// <summary>Gets or sets extra CSS classes for the wrapper.</summary>
    [Parameter] public string? Class { get; set; }

    private DateOnly EarliestMonth => StartOfMonth(State.Today);

    /// <summary>Gets or sets a booking whose own nights count as available (for a booking whose dates are being changed).</summary>
    [Parameter] public string? ExceptReference { get; set; }

    /// <summary>The first month shown, kept between this month and the last month a stay can be booked into.</summary>
    private DateOnly FirstMonth => shownMonth < EarliestMonth ? EarliestMonth : shownMonth > LatestMonth ? LatestMonth : shownMonth;

    private bool HasSelection => CheckIn is { } checkIn && CheckOut is { } checkOut && checkOut > checkIn;

    /// <summary>The latest first month: its second month holds the last possible check-out (a year ahead plus the longest stay).</summary>
    private DateOnly LatestMonth => StartOfMonth(State.Today.AddDays(365 + TravelSeedData.MaxNights)).AddMonths(1 - MonthCount);

    /// <summary>Gets or sets the first month to show; defaults to the check-in month, or this month.</summary>
    [Parameter] public DateOnly? Month { get; set; }

    private string RangeText => $"{MonthName(FirstMonth)} and {MonthName(FirstMonth.AddMonths(1))}";

    [Inject] private TravelState State { get; set; } = default!;

    /// <summary>Gets or sets the stay whose nights are shown.</summary>
    [Parameter, EditorRequired] public int StayId { get; set; }

    #endregion
}
