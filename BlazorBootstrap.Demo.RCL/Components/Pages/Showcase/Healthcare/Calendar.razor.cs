using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The front desk's calendar: a day view (today unless <c>?date=</c> says otherwise; Previous, Today, Next, and a native
/// date input) with one column per doctor working that day — a list grouped by doctor on narrow screens — each appointment
/// a block with its time, patient, reason, and status in words. Filters for clinic and doctor live in the query
/// (<c>?clinic=</c>, <c>?doctor=</c>). An appointment opens an actions panel (a <see cref="Modal" />): check in (today,
/// booked), mark a no-show (booked and started), or cancel (with an optional reason); the last two are confirmed in the
/// panel, and every action runs once the panel is hidden. Booked appointments now outside the doctor's hours or in their
/// time off carry a warning in words, so the desk can call the patient. Below, a week overview counts appointments per
/// doctor per day. The page re-renders on every state change, so patients' new bookings show up at once. Also holds the
/// outcome wording for check-in, no-show, and cancel that the check-in page shares.
/// </summary>
public partial class Calendar : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The <c>?clinic=</c>, <c>?date=</c>, and <c>?doctor=</c> values applied last, so a re-render keeps the selection.</summary>
    private (string? Clinic, string? Date, string? Doctor)? appliedQuery;

    private ElementReference? cancelButton;

    private string cancelReason = string.Empty;

    private string? cancelReasonError;

    private ElementReference? cancelReasonInput;

    private ElementReference? checkInButton;

    /// <summary>The day shown; <see langword="null" /> means today (which moves with the site clock).</summary>
    private DateTime? chosenDate;

    private string? clinicFilter;

    private ElementReference? confirmText;

    private ElementReference daySummary;

    private string? doctorFilter;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>What the panel shows: the actions, or one of the confirmations.</summary>
    private PanelMode mode;

    private ElementReference? noShowButton;

    /// <summary>The appointment blocks, by appointment ID, so closing the panel returns focus to the one that opened it.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    private ElementReference? panelClose;

    private Modal panelModal = default!;

    /// <summary>The confirmed action, run once the panel is hidden (it changes the page under the panel).</summary>
    private Action? pendingAction;

    /// <summary>The appointment the panel is about.</summary>
    private HcAppointment? selected;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

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

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    /// <summary>Reads the day and filters from the query; unknown or out-of-range values are ignored.</summary>
    protected override void OnParametersSet()
    {
        var query = (ClinicQuery, DateQuery, DoctorQuery);

        if (appliedQuery == query)
            return;

        appliedQuery = query;
        chosenDate = ParseDate(DateQuery);
        clinicFilter = State.FindClinic(ClinicQuery)?.Id;
        doctorFilter = State.FindDoctor(DoctorQuery)?.Id;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Says the outcome of cancelling an appointment (front desk).</summary>
    internal static string CancelMessage(HcResult result, HcState state, HcAppointment appointment)
    {
        var (patient, doctor) = Names(state, appointment);

        return result switch
        {
            HcResult.Ok => $"Cancelled {patient}'s appointment on {HcDisplay.When(appointment.Start)} with {doctor}. The time is open for booking again; call the patient to let them know.",
            HcResult.WrongState => $"{patient}'s appointment on {HcDisplay.When(appointment.Start)} is “{HcDisplay.StatusLabel(appointment.Status)}”, so it can't be cancelled. Nothing changed.",
            HcResult.Invalid => $"Keep the reason to {HcState.MaxReasonLength} characters or fewer. Nothing changed.",
            _ => CommonMessage(result),
        };
    }

    /// <summary>Says the outcome of checking a patient in (front desk), like "Checked in Sofia Alvarez for 09:00 with Dr. Samuel Okoye."</summary>
    internal static string CheckInMessage(HcResult result, HcState state, HcAppointment appointment)
    {
        var (patient, doctor) = Names(state, appointment);

        return result switch
        {
            HcResult.Ok => $"Checked in {patient} for {HcDisplay.Time(appointment.Start)} with {doctor}.",
            HcResult.WrongState when appointment.Status != HcAppointmentStatus.Booked =>
                $"{patient}'s {HcDisplay.Time(appointment.Start)} appointment is already “{HcDisplay.StatusLabel(appointment.Status)}”, so there's nothing to check in. Nothing changed.",
            HcResult.WrongState => $"{patient}'s appointment is on {HcDisplay.Day(appointment.Start)}. Patients can only be checked in on the day of their visit. Nothing changed.",
            _ => CommonMessage(result),
        };
    }

    /// <summary>Says the outcome of marking a no-show (front desk).</summary>
    internal static string NoShowMessage(HcResult result, HcState state, HcAppointment appointment)
    {
        var (patient, doctor) = Names(state, appointment);

        return result switch
        {
            HcResult.Ok => $"Marked {patient} as a no-show for {HcDisplay.Time(appointment.Start)} with {doctor}.",
            HcResult.WrongState when appointment.Status != HcAppointmentStatus.Booked =>
                $"{patient}'s {HcDisplay.Time(appointment.Start)} appointment is “{HcDisplay.StatusLabel(appointment.Status)}”, so it can't be marked as a no-show. Nothing changed.",
            HcResult.WrongState => $"{patient}'s appointment at {HcDisplay.When(appointment.Start)} hasn't started yet, so it can't be a no-show. Nothing changed.",
            _ => CommonMessage(result),
        };
    }

    /// <summary>
    /// Returns the warning for a booked appointment now outside its doctor's hours or in their time off, like "Outside Dr.
    /// Rossi's hours — call the patient to rebook".
    /// </summary>
    internal static string OutsideText(HcState state, HcAppointment appointment)
    {
        var doctor = state.FindDoctor(appointment.DoctorId) is { } found ? DoctorProfile.ShortName(found) : "the doctor";
        var away = state.TimeOffOf(appointment.DoctorId).FirstOrDefault(entry => appointment.Start.Date >= entry.From && appointment.Start.Date <= entry.To);

        return away is not null
            ? $"{doctor} is away {DoctorProfile.AwayText(away)} — call the patient to rebook"
            : $"Outside {doctor}'s hours — call the patient to rebook";
    }

    /// <summary>Returns the week's Monday.</summary>
    internal static DateTime WeekStart(DateTime date) => date.Date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    #endregion

    #region Private Methods

    private static string CommonMessage(HcResult result) => result switch
    {
        HcResult.NotFound => "That appointment no longer exists. Nothing changed.",
        HcResult.NotAllowed => "Only the front desk can do this. Sign in again as the front desk; nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    private static (string Patient, string Doctor) Names(HcState state, HcAppointment appointment) =>
        (state.FindPatient(appointment.PatientId)?.Name ?? "The patient", state.FindDoctor(appointment.DoctorId)?.Name ?? "the doctor");

    private static string PatientName(IReadOnlyDictionary<int, string> names, int patientId) => names.TryGetValue(patientId, out var name) ? name : "Unknown patient";

    private static string QueryValue(DateTime date) => date.ToString(HcDisplay.QueryDate, CultureInfo.InvariantCulture);

    /// <summary>Explains what can be done with an appointment now, or why nothing can.</summary>
    private string ActionsHint(HcAppointment appointment)
    {
        var now = State.Clock.Now;

        return appointment.Status switch
        {
            HcAppointmentStatus.Booked when appointment.Start.Date > State.Clock.Today => $"Check-in opens on {HcDisplay.Day(appointment.Start)}, the day of the visit.",
            HcAppointmentStatus.Booked when appointment.Start.Date < State.Clock.Today => "This day has passed. Mark a no-show if the patient never came.",
            HcAppointmentStatus.Booked when appointment.Start > now => "Mark a no-show once the start time has passed.",
            HcAppointmentStatus.Booked => "The start time has passed: check the patient in if they're here, or mark a no-show.",
            HcAppointmentStatus.CheckedIn => "The patient is here and waiting for the doctor. You can still cancel if they leave.",
            HcAppointmentStatus.InVisit => "The patient is with the doctor, so there's nothing for the front desk to do.",
            HcAppointmentStatus.Completed => "The visit is finished.",
            HcAppointmentStatus.Cancelled => "This appointment was cancelled; its time is open for booking again.",
            _ => "The patient didn't come to this appointment.",
        };
    }

    /// <summary>Opens a confirmation in the panel and moves focus to its question.</summary>
    private void Ask(PanelMode confirmation)
    {
        mode = confirmation;
        cancelReason = string.Empty;
        cancelReasonError = null;
        focusTarget = confirmation == PanelMode.ConfirmCancel ? () => cancelReasonInput : () => confirmText;
    }

    private bool CanCancel(HcAppointment appointment) => appointment.Status is HcAppointmentStatus.Booked or HcAppointmentStatus.CheckedIn;

    private bool CanCheckIn(HcAppointment appointment) => appointment.Status == HcAppointmentStatus.Booked && appointment.Start.Date == State.Clock.Today;

    private bool CanNoShow(HcAppointment appointment) => appointment.Status == HcAppointmentStatus.Booked && appointment.Start <= State.Clock.Now;

    /// <summary>Clears the clinic and doctor filters; the pressed button disappears, so focus moves to the day's summary.</summary>
    private void ClearFilters()
    {
        clinicFilter = doctorFilter = null;
        UpdateQuery(push: false);
        focusTarget = () => daySummary;
    }

    /// <summary>
    /// Returns the doctors to show on a day, each with that day's blocks, time off, and appointments (in the clinic filter):
    /// those with hours or appointments that day, or the one doctor the filter names.
    /// </summary>
    private IReadOnlyList<DoctorColumn> Columns(DateTime date)
    {
        var appointments = State.AppointmentsOn(date).Where(item => clinicFilter is null || item.ClinicId == clinicFilter).ToList();

        return State.Doctors
            .Where(doctor => doctorFilter is null || doctor.Id == doctorFilter)
            .Select(doctor => new DoctorColumn(
                doctor,
                State.AvailabilityOf(doctor.Id).Where(block => block.Day == date.DayOfWeek && (clinicFilter is null || block.ClinicId == clinicFilter)).ToList(),
                State.TimeOffOf(doctor.Id).FirstOrDefault(entry => date >= entry.From && date <= entry.To),
                appointments.Where(item => item.DoctorId == doctor.Id).ToList()))
            .Where(column => doctorFilter is not null || column.Blocks.Count > 0 || column.Appointments.Count > 0)
            .ToList();
    }

    private Task ConfirmCancelAsync()
    {
        if (selected is not { } appointment || pendingAction is not null)
            return Task.CompletedTask;

        var reason = cancelReason.Trim();

        if (reason.Length > HcState.MaxReasonLength)
        {
            cancelReasonError = $"Keep the reason to {HcState.MaxReasonLength} characters or fewer.";
            focusTarget = () => cancelReasonInput;

            return Task.CompletedTask;
        }

        pendingAction = () =>
        {
            var result = State.CancelAppointment(appointment.Id, reason);
            SetStatus(CancelMessage(result, State, appointment), result != HcResult.Ok);
        };

        return panelModal.HideAsync();
    }

    private Task ConfirmCheckInAsync()
    {
        if (selected is not { } appointment || pendingAction is not null)
            return Task.CompletedTask;

        pendingAction = () =>
        {
            var result = State.CheckIn(appointment.Id);
            SetStatus(CheckInMessage(result, State, appointment), result != HcResult.Ok);
        };

        return panelModal.HideAsync();
    }

    private Task ConfirmNoShowAsync()
    {
        if (selected is not { } appointment || pendingAction is not null)
            return Task.CompletedTask;

        pendingAction = () =>
        {
            var result = State.MarkNoShow(appointment.Id);
            SetStatus(NoShowMessage(result, State, appointment), result != HcResult.Ok);
        };

        return panelModal.HideAsync();
    }

    private string DateUrl(DateTime date) => Navigation.GetUriWithQueryParameter("date", QueryValue(date));

    /// <summary>Says how many appointments the day has, like "7 appointments with 3 doctors; 1 cancelled."</summary>
    private static string DaySummary(IReadOnlyList<HcAppointment> appointments, int doctors)
    {
        var live = appointments.Count(item => item.Status != HcAppointmentStatus.Cancelled);
        var cancelled = appointments.Count - live;
        var count = live == 1 ? "1 appointment" : FormattableString.Invariant($"{live} appointments");
        var with = doctors == 1 ? "1 doctor" : FormattableString.Invariant($"{doctors} doctors");

        return cancelled == 0 ? $"{count} with {with}." : FormattableString.Invariant($"{count} with {with}; {cancelled} cancelled.");
    }

    /// <summary>Moves the panel back to the actions, with focus on the button that opened the confirmation.</summary>
    private void GoBack(PanelMode from)
    {
        mode = PanelMode.Actions;
        cancelReasonError = null;
        focusTarget = from == PanelMode.ConfirmCancel ? () => cancelButton ?? panelClose : () => noShowButton ?? panelClose;
    }

    /// <summary>Says a doctor's day under their name: away, their hours, or none.</summary>
    private string HoursLine(DoctorColumn column, DateTime date)
    {
        if (column.Away is { } away)
            return $"Away {DoctorProfile.AwayText(away)}{(away.Reason.Length > 0 ? $": {away.Reason}" : null)}";

        return column.Blocks.Count > 0
            ? DoctorProfile.DayHoursText(State, column.Blocks)
            : $"No hours on {date.DayOfWeek}s{(clinicFilter is not null ? " at this clinic" : null)}";
    }

    private bool IsOutside(HcAppointment appointment) =>
        appointment.Status == HcAppointmentStatus.Booked && appointment.Start > State.Clock.Now && State.IsOutsideHours(appointment);

    private string NoDoctorsText(DateTime date)
    {
        var where = clinicFilter is not null ? $" at {State.FindClinic(clinicFilter)?.Name}" : null;

        return $"No doctor has hours or appointments{where} on {HcDisplay.DayLong(date)}. Pick another day, or clear the filters.";
    }

    private void OnClinicChanged(ChangeEventArgs e)
    {
        clinicFilter = State.FindClinic(e.Value?.ToString())?.Id;
        UpdateQuery(push: false);
    }

    private void OnDateChanged(ChangeEventArgs e)
    {
        if (ParseDate(e.Value?.ToString()) is { } date)
            SetDate(date == State.Clock.Today ? null : date);
    }

    private void OnDoctorChanged(ChangeEventArgs e)
    {
        doctorFilter = State.FindDoctor(e.Value?.ToString())?.Id;
        UpdateQuery(push: false);
    }

    /// <summary>
    /// The panel opens from code, so Bootstrap doesn't return focus. A confirmed action runs now that the panel is gone and
    /// focus moves to its outcome; Close or Escape returns focus to the appointment that opened the panel.
    /// </summary>
    private void OnPanelHidden()
    {
        var action = pendingAction;
        var openerId = selected?.Id;
        pendingAction = null;
        selected = null;
        mode = PanelMode.Actions;

        if (action is not null)
            action();
        else if (openerId is { } id && openers.TryGetValue(id, out var opener))
            focusTarget = () => opener;
        else
            focusTarget = () => daySummary;
    }

    private Task OnPanelShownAsync() => HcDisplay.FocusAsync(checkInButton ?? noShowButton ?? cancelButton ?? panelClose);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenAsync(HcAppointment appointment)
    {
        selected = appointment;
        mode = PanelMode.Actions;
        pendingAction = null;
        cancelReason = string.Empty;
        cancelReasonError = null;
        checkInButton = noShowButton = cancelButton = null;
        StateHasChanged();

        await panelModal.ShowAsync();
    }

    private string PanelTitle(IReadOnlyDictionary<int, string> names) =>
        selected is { } appointment ? $"{PatientName(names, appointment.PatientId)}, {HcDisplay.Time(appointment.Start)}" : "Appointment";

    /// <summary>Reads a <c>yyyy-MM-dd</c> date within a year of today, or returns <see langword="null" />.</summary>
    private DateTime? ParseDate(string? text) =>
        DateTime.TryParseExact(text?.Trim(), HcDisplay.QueryDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) && date >= MinDate && date <= MaxDate
            ? date.Date
            : null;

    /// <summary>Returns staff's view of the patients' names, by ID.</summary>
    private IReadOnlyDictionary<int, string> PatientNames() => State.Patients.ToDictionary(patient => patient.Id, patient => patient.Name);

    /// <summary>Shows another day (<see langword="null" /> for today) and adds it to the history.</summary>
    private void SetDate(DateTime? date)
    {
        chosenDate = date is { } value && value.Date != State.Clock.Today ? Clamp(value.Date) : null;
        UpdateQuery(push: true);
    }

    private void SetStatus(string message, bool isError)
    {
        statusMessage = message;
        statusIsError = isError;
        focusTarget = () => statusRegion;
    }

    private void UpdateQuery(bool push)
    {
        var date = chosenDate is { } value ? QueryValue(value) : null;
        appliedQuery = (clinicFilter, date, doctorFilter);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["date"] = date,
            ["clinic"] = clinicFilter,
            ["doctor"] = doctorFilter,
        }), replace: !push);
    }

    /// <summary>Counts each shown doctor's appointments (not cancelled, in the clinic filter) for each day of a week.</summary>
    private IReadOnlyList<WeekRow> WeekRows(DateTime weekStart)
    {
        var days = Enumerable.Range(0, 7).Select(offset => weekStart.AddDays(offset)).ToList();
        var appointments = days.SelectMany(day => State.AppointmentsOn(day))
            .Where(item => item.Status != HcAppointmentStatus.Cancelled && (clinicFilter is null || item.ClinicId == clinicFilter))
            .ToList();

        return State.Doctors
            .Where(doctor => doctorFilter is null || doctor.Id == doctorFilter)
            .Select(doctor =>
            {
                var blocks = State.AvailabilityOf(doctor.Id);
                var timeOff = State.TimeOffOf(doctor.Id);

                return new WeekRow(doctor, days.Select(day => new WeekCell(
                    appointments.Count(item => item.DoctorId == doctor.Id && item.Start.Date == day),
                    blocks.Any(block => block.Day == day.DayOfWeek && (clinicFilter is null || block.ClinicId == clinicFilter)),
                    timeOff.Any(entry => day >= entry.From && day <= entry.To))).ToList());
            })
            .ToList();
    }

    private DateTime Clamp(DateTime date) => date < MinDate ? MinDate : date > MaxDate ? MaxDate : date;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the clinic ID from the query.</summary>
    [SupplyParameterFromQuery(Name = "clinic")] public string? ClinicQuery { get; set; }

    /// <summary>Gets the day shown.</summary>
    private DateTime Date => chosenDate ?? State.Clock.Today;

    /// <summary>Gets or sets the day from the query (<c>yyyy-MM-dd</c>).</summary>
    [SupplyParameterFromQuery(Name = "date")] public string? DateQuery { get; set; }

    /// <summary>Gets or sets the doctor ID from the query.</summary>
    [SupplyParameterFromQuery(Name = "doctor")] public string? DoctorQuery { get; set; }

    /// <summary>Gets the latest day the calendar shows.</summary>
    private DateTime MaxDate => State.Clock.Today.AddYears(1);

    /// <summary>Gets the earliest day the calendar shows.</summary>
    private DateTime MinDate => State.Clock.Today.AddYears(-1);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>What the actions panel shows.</summary>
    private enum PanelMode
    {
        /// <summary>The appointment and the actions it allows.</summary>
        Actions,

        /// <summary>Confirming a no-show.</summary>
        ConfirmNoShow,

        /// <summary>Confirming a cancellation, with a reason.</summary>
        ConfirmCancel,
    }

    /// <summary>A doctor's column for the day: their blocks, time off, and appointments.</summary>
    private sealed record DoctorColumn(HcDoctor Doctor, IReadOnlyList<HcAvailability> Blocks, HcTimeOff? Away, IReadOnlyList<HcAppointment> Appointments);

    /// <summary>One day of a doctor's week: appointments, whether they have hours, and whether they're away.</summary>
    private sealed record WeekCell(int Count, bool Works, bool Away);

    /// <summary>A doctor's week in the overview.</summary>
    private sealed record WeekRow(HcDoctor Doctor, IReadOnlyList<WeekCell> Cells);

    #endregion
}
