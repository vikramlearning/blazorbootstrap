using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in doctor's day (<see cref="HcState.AppointmentsOn" />): Previous day, Today, and Next day links and a native
/// date field keep the day in <c>?date=yyyy-MM-dd</c>. A summary line counts the visits by status, and each visit shows the
/// time, the patient (linked to their chart) and age, the reason, the clinic, and the status in words; a checked-in patient
/// stands out ("Checked in at 08:22, waiting") with <b>Start visit</b> (<see cref="HcState.StartVisit" />, then the visit
/// page). Visits in progress or completed open the visit page. The page re-renders on <see cref="HcState.Changed" />, so a
/// check-in by the front desk shows up, and a side panel counts the visits on each of the next seven days.
/// </summary>
public partial class Schedule : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The order of the statuses in the summary line: who is waiting first.</summary>
    private static readonly HcAppointmentStatus[] summaryOrder =
    {
        HcAppointmentStatus.CheckedIn, HcAppointmentStatus.InVisit, HcAppointmentStatus.Booked, HcAppointmentStatus.Completed, HcAppointmentStatus.NoShow,
        HcAppointmentStatus.Cancelled,
    };

    private readonly Dictionary<int, ElementReference> startButtons = new();

    private string? dateError;

    private ElementReference? dateField;

    /// <summary>The date field's value since the last change, or <see langword="null" /> when untouched.</summary>
    private string? dateInput;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HcDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        dateInput = null;
        dateError = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string DayUrl(DateTime date) => $"{HcStaffLayout.Base}/schedule?date={date.ToString(HcDisplay.QueryDate, CultureInfo.InvariantCulture)}";

    /// <summary>Returns the status detail of a visit, or <see langword="null" /> when the badge says it all.</summary>
    private static string? StateText(HcAppointment appointment) => appointment.Status switch
    {
        HcAppointmentStatus.CheckedIn => appointment.CheckedInAt is { } at ? $"Checked in at {HcDisplay.Time(at)}, waiting" : "Checked in, waiting",
        HcAppointmentStatus.InVisit => "Visit in progress",
        HcAppointmentStatus.Completed => appointment.CompletedAt is { } done ? $"Completed at {HcDisplay.Time(done)}" : null,
        HcAppointmentStatus.NoShow => "The patient didn't come",
        _ => null,
    };

    private static string StatusWord(HcAppointmentStatus status) => status switch
    {
        HcAppointmentStatus.Booked => "booked",
        HcAppointmentStatus.CheckedIn => "checked in",
        HcAppointmentStatus.InVisit => "in progress",
        HcAppointmentStatus.Completed => "completed",
        HcAppointmentStatus.Cancelled => "cancelled",
        _ => "no-show",
    };

    /// <summary>Returns a line like "5 visits today: 1 checked in, 4 booked".</summary>
    private static string SummaryText(IReadOnlyList<HcAppointment> list, DateTime day, DateTime today)
    {
        var when = day == today ? "today" : day == today.AddDays(1) ? "tomorrow" : day == today.AddDays(-1) ? "yesterday" : $"on {HcDisplay.Day(day)}";

        if (list.Count == 0)
            return $"No visits {when}.";

        var parts = list.GroupBy(item => item.Status).OrderBy(group => Array.IndexOf(summaryOrder, group.Key)).Select(group => $"{group.Count()} {StatusWord(group.Key)}");

        return $"{VisitCount(list.Count)} {when}: {string.Join(", ", parts)}.";
    }

    private static string VisitCount(int count) => count switch
    {
        0 => "No visits",
        1 => "1 visit",
        _ => $"{count} visits",
    };

    /// <summary>Opens the day in the date field.</summary>
    private void GoToDate()
    {
        var value = dateInput ?? Day.ToString(HcDisplay.QueryDate, CultureInfo.InvariantCulture);

        if (!DateTime.TryParseExact(value, HcDisplay.QueryDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) || date.Year is < 2000 or > 2100)
        {
            dateError = "Enter a full date, like 2026-11-24.";
            focusTarget = () => dateField;

            return;
        }

        dateError = null;

        if (date.Date == Day)
        {
            statusIsError = false;
            statusMessage = $"{HcDisplay.DayLong(date)} is already showing.";
            focusTarget = () => statusRegion;

            return;
        }

        Navigation.NavigateTo(DayUrl(date));
    }

    private void OnDateChanged(ChangeEventArgs args) => dateInput = args.Value?.ToString() ?? string.Empty;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Starts the visit with a checked-in patient, then opens the visit page.</summary>
    private void StartVisit(int appointmentId)
    {
        var appointment = State.FindAppointment(appointmentId);
        var name = State.FindPatient(appointment?.PatientId ?? 0)?.Name ?? "the patient";
        var result = State.StartVisit(appointmentId);

        if (result == HcResult.Ok)
        {
            Navigation.NavigateTo(HcDisplay.StaffAppointmentUrl(appointmentId));

            return;
        }

        statusIsError = true;
        statusMessage = result switch
        {
            HcResult.WrongState when appointment?.Status is HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed => $"The visit with {name} has already started. Open it from the list.",
            HcResult.WrongState => $"{name} isn't checked in anymore, so the visit can't start. Ask the front desk.",
            HcResult.NotFound => "That visit isn't on your schedule anymore.",
            HcResult.NotAllowed => "Only a signed-in doctor can start their own visits. Sign in again.",
            _ => "That didn't work, and the visit didn't start. Try again.",
        };
        focusTarget = () => startButtons.TryGetValue(appointmentId, out var button) && appointment?.Status == HcAppointmentStatus.CheckedIn ? button : statusRegion;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the day from the query string (<c>yyyy-MM-dd</c>).</summary>
    [SupplyParameterFromQuery(Name = "date")] public string? DateQuery { get; set; }

    /// <summary>The day shown: the query's date, or today.</summary>
    private DateTime Day =>
        DateTime.TryParseExact(DateQuery, HcDisplay.QueryDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) && date.Year is >= 2000 and <= 2100 ? date.Date : State.Clock.Today;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HcState State { get; set; } = default!;

    #endregion
}
