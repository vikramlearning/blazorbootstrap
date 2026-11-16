using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Check-in (front desk): today's appointments at every clinic by time, with the patient, doctor, clinic, and status in
/// words, and counts for booked, checked in, with the doctor, and done. "Check in" works on a booked appointment and
/// announces the outcome ("Checked in Sofia Alvarez for 09:00 with Dr. Samuel Okoye"); "No-show" appears once a booked
/// appointment's start has passed and is confirmed in a <see cref="Modal" />, applied after the dialog is hidden. A search
/// by patient name and a clinic filter narrow the list. The page re-renders on every state change.
/// </summary>
public partial class CheckIn : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? clinicFilter;

    private ElementReference? dialogDismiss;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private Modal noShowModal = default!;

    /// <summary>The appointment the no-show dialog is about.</summary>
    private HcAppointment? noShowAppointment;

    /// <summary>The key of the No-show button that opened the dialog, so Go back returns focus to it.</summary>
    private string? noShowOpenerKey;

    /// <summary>The Check in and No-show buttons, by key.</summary>
    private readonly Dictionary<string, ElementReference> openers = new();

    /// <summary>The confirmed no-show, run once the dialog is hidden.</summary>
    private HcAppointment? pendingNoShow;

    private string search = string.Empty;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

    private ElementReference summaryRegion;

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

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string PatientName(IReadOnlyDictionary<int, string> names, int patientId) => names.TryGetValue(patientId, out var name) ? name : "Unknown patient";

    private static string SummaryText(int shown, int total)
    {
        var count = total == 1 ? "1 appointment" : FormattableString.Invariant($"{total} appointments");

        return shown == total ? $"{count} today." : FormattableString.Invariant($"Showing {shown} of {count} today.");
    }

    private async Task AskNoShowAsync(HcAppointment appointment, string openerKey)
    {
        noShowAppointment = appointment;
        noShowOpenerKey = openerKey;
        pendingNoShow = null;
        StateHasChanged();

        await noShowModal.ShowAsync();
    }

    /// <summary>Checks a patient in; the button disappears, so focus moves to the announced outcome.</summary>
    private void CheckInPatient(HcAppointment appointment)
    {
        var result = State.CheckIn(appointment.Id);
        SetStatus(Calendar.CheckInMessage(result, State, appointment), result != HcResult.Ok);
    }

    /// <summary>Clears the search and clinic; the pressed button disappears, so focus moves to the announced count.</summary>
    private void ClearFilters()
    {
        search = string.Empty;
        clinicFilter = null;
        focusTarget = () => summaryRegion;
    }

    private Task ConfirmNoShowAsync()
    {
        if (noShowAppointment is not { } appointment || pendingNoShow is not null)
            return Task.CompletedTask;

        pendingNoShow = appointment;

        return noShowModal.HideAsync();
    }

    /// <summary>Returns the counts above the list: booked (not arrived), checked in, with the doctor, done, and closed.</summary>
    private static IReadOnlyList<StatusCount> Counts(IReadOnlyList<HcAppointment> appointments)
    {
        int Count(params HcAppointmentStatus[] statuses) => appointments.Count(item => statuses.Contains(item.Status));

        return new List<StatusCount>
        {
            new(Count(HcAppointmentStatus.Booked), "Booked, not arrived"),
            new(Count(HcAppointmentStatus.CheckedIn), "Checked in"),
            new(Count(HcAppointmentStatus.InVisit), "With the doctor"),
            new(Count(HcAppointmentStatus.Completed), "Done"),
            new(Count(HcAppointmentStatus.NoShow, HcAppointmentStatus.Cancelled), "No-show or cancelled"),
        };
    }

    private bool Matches(HcAppointment appointment, IReadOnlyDictionary<int, string> names)
    {
        var text = search.Trim();

        return (clinicFilter is null || appointment.ClinicId == clinicFilter)
            && (text.Length == 0 || PatientName(names, appointment.PatientId).Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    private void OnClinicChanged(ChangeEventArgs e) => clinicFilter = State.FindClinic(e.Value?.ToString())?.Id;

    private Task OnDialogShownAsync() => HcDisplay.FocusAsync(dialogDismiss);

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus. A confirmed no-show is marked now that the dialog is
    /// gone and focus moves to its outcome; Go back or Escape returns focus to the No-show button.
    /// </summary>
    private void OnDialogHidden()
    {
        var appointment = pendingNoShow;
        var openerKey = noShowOpenerKey;
        pendingNoShow = null;
        noShowAppointment = null;
        noShowOpenerKey = null;

        if (appointment is not null)
        {
            var result = State.MarkNoShow(appointment.Id);
            SetStatus(Calendar.NoShowMessage(result, State, appointment), result != HcResult.Ok);
        }
        else if (openerKey is not null && openers.TryGetValue(openerKey, out var opener))
        {
            focusTarget = () => opener;
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns staff's view of the patients' names, by ID.</summary>
    private IReadOnlyDictionary<int, string> PatientNames() => State.Patients.ToDictionary(patient => patient.Id, patient => patient.Name);

    private void SetStatus(string message, bool isError)
    {
        statusMessage = message;
        statusIsError = isError;
        focusTarget = () => statusRegion;
    }

    /// <summary>Returns today's appointments at every clinic, by time, then doctor.</summary>
    private IReadOnlyList<HcAppointment> TodayAppointments() =>
        State.AppointmentsOn(State.Clock.Today).OrderBy(item => item.Start).ThenBy(item => State.FindDoctor(item.DoctorId)?.Name, StringComparer.Ordinal).ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One of the counts above the list.</summary>
    private sealed record StatusCount(int Number, string Label);

    #endregion
}
