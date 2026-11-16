using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// A doctor's public profile: biography, specialty, languages, whether they take new patients, their main clinic (address,
/// phone, hours), weekly hours in words grouped by day, upcoming time away (dates only, never the reason), and the open
/// times a week at a time — each day a heading with its times as links to book them, named in full ("Tuesday, November 24
/// at 09:30"). "Show the next week" pages forward up to <see cref="HcState.BookingDays" /> days. Also holds the wording
/// helpers for hours and time off that the front desk pages share. An unknown ID shows the site's not-found screen.
/// </summary>
public partial class DoctorProfile : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The days of the week, Monday first, as the clinics plan them.</summary>
    internal static readonly IReadOnlyList<DayOfWeek> Week = new[]
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday,
    };

    /// <summary>The doctor whose open times are shown, so a new doctor starts at this week.</summary>
    private string? shownId;

    /// <summary>How many days after today the shown week starts (0, 7, 14, …).</summary>
    private int weekOffset;

    /// <summary>Whether the week line takes focus after the next render (paging may remove the pressed button).</summary>
    private bool focusWindow;

    private ElementReference windowHeading;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusWindow)
        {
            focusWindow = false;
            await HcDisplay.FocusAsync(windowHeading);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (shownId == Id)
            return;

        shownId = Id;
        weekOffset = 0;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Formats a time off's days like "Dec 7–9", "Nov 30 – Dec 2", or "Dec 7" (never the reason).</summary>
    internal static string AwayText(HcTimeOff entry)
    {
        var from = entry.From.Date;
        var to = entry.To.Date;

        if (from == to)
            return from.ToString("MMM d", CultureInfo.InvariantCulture);

        if (from.Year != to.Year)
            return $"{from.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)} – {to.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";

        return from.Month == to.Month
            ? $"{from.ToString("MMM d", CultureInfo.InvariantCulture)}–{to.ToString("%d", CultureInfo.InvariantCulture)}"
            : $"{from.ToString("MMM d", CultureInfo.InvariantCulture)} – {to.ToString("MMM d", CultureInfo.InvariantCulture)}";
    }

    /// <summary>Formats a block of hours like "08:30–12:00".</summary>
    internal static string BlockText(HcAvailability block) => $"{HcDisplay.Minutes(block.StartMinute)}–{HcDisplay.Minutes(block.EndMinute)}";

    /// <summary>
    /// Says one day's blocks in words, like "08:30–12:00 and 13:00–16:30 at Cedarline Downtown", naming each clinic once for
    /// its run of blocks.
    /// </summary>
    internal static string DayHoursText(HcState state, IEnumerable<HcAvailability> blocks)
    {
        var runs = new List<(string ClinicId, List<string> Times)>();

        foreach (var block in blocks.OrderBy(item => item.StartMinute))
        {
            if (runs.Count > 0 && runs[^1].ClinicId == block.ClinicId)
                runs[^1].Times.Add(BlockText(block));
            else
                runs.Add((block.ClinicId, new List<string> { BlockText(block) }));
        }

        return string.Join(", then ", runs.Select(run => $"{JoinAnd(run.Times)} at {state.FindClinic(run.ClinicId)?.Name ?? "a clinic"}"));
    }

    /// <summary>Formats a day like "Tuesday, November 24".</summary>
    internal static string FullDay(DateTime date) => date.ToString("dddd, MMMM d", CultureInfo.InvariantCulture);

    /// <summary>Formats an open time in full, like "Tuesday, November 24 at 09:30", for accessible names.</summary>
    internal static string FullTime(DateTime time) => time.ToString("dddd, MMMM d 'at' HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Joins words like "a", "a and b", or "a, b, and c".</summary>
    internal static string JoinAnd(IReadOnlyList<string> items) => items.Count switch
    {
        0 => string.Empty,
        1 => items[0],
        2 => $"{items[0]} and {items[1]}",
        _ => $"{string.Join(", ", items.Take(items.Count - 1))}, and {items[^1]}",
    };

    /// <summary>Returns a doctor's short name, like "Dr. Okoye".</summary>
    internal static string ShortName(HcDoctor doctor)
    {
        var parts = doctor.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length > 1 && parts[0] == "Dr." ? $"Dr. {parts[^1]}" : doctor.Name;
    }

    /// <summary>Returns a doctor's weekly hours in words, one line per working day, Monday first.</summary>
    internal static IReadOnlyList<DayHours> WeeklyHours(HcState state, string doctorId)
    {
        var blocks = state.AvailabilityOf(doctorId);

        return Week
            .Select(day => (Day: day, Blocks: blocks.Where(block => block.Day == day).ToList()))
            .Where(item => item.Blocks.Count > 0)
            .Select(item => new DayHours(item.Day.ToString(), DayHoursText(state, item.Blocks)))
            .ToList();
    }

    #endregion

    #region Private Methods

    private static string OpenTimesText(IReadOnlyList<OpenDay> days)
    {
        var times = days.Sum(day => day.Slots.Count);

        return times switch
        {
            0 => "no open times",
            1 => "1 open time",
            _ => FormattableString.Invariant($"{times} open times on {(days.Count == 1 ? "1 day" : $"{days.Count} days")}"),
        };
    }

    /// <summary>Returns the days of the window that have open times.</summary>
    private IReadOnlyList<OpenDay> OpenDays(string doctorId, DateTime from, DateTime to)
    {
        var list = new List<OpenDay>();

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var slots = State.Slots(doctorId, date);

            if (slots.Count > 0)
                list.Add(new OpenDay(date, slots));
        }

        return list;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void ShowNextWeek()
    {
        if (!CanShowNext)
            return;

        weekOffset += 7;
        focusWindow = true;
    }

    private void ShowPreviousWeek()
    {
        weekOffset = Math.Max(0, weekOffset - 7);
        focusWindow = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets whether a later week is still inside the booking window.</summary>
    private bool CanShowNext => weekOffset + 7 <= HcState.BookingDays;

    private HcDoctor? Doctor => State.FindDoctor(Id);

    /// <summary>Gets or sets the doctor ID from the route.</summary>
    [Parameter] public string? Id { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    /// <summary>Gets the last day of the shown week, never past the booking window.</summary>
    private DateTime WindowEnd
    {
        get
        {
            var end = WindowStart.AddDays(6);
            var last = State.Clock.Today.AddDays(HcState.BookingDays);

            return end > last ? last : end;
        }
    }

    /// <summary>Gets the first day of the shown week.</summary>
    private DateTime WindowStart => State.Clock.Today.AddDays(weekOffset);

    #endregion

    #region Nested Types

    /// <summary>One working day's hours in words.</summary>
    internal sealed record DayHours(string Day, string Text);

    /// <summary>A day with open times.</summary>
    private sealed record OpenDay(DateTime Date, IReadOnlyList<HcSlot> Slots);

    #endregion
}
