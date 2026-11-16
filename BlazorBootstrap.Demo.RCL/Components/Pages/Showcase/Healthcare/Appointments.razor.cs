using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's appointments: upcoming ones (soonest first, including a visit checked in or in progress today)
/// with "View or change", and past ones (completed, cancelled, no-show; newest first) with "Visit summary" for a completed
/// visit. Each shows the date and time, the doctor, the clinic, the reason, and the status in words. Empty lists link to
/// "Find a doctor".
/// </summary>
public partial class Appointments : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>
    /// Returns whether an appointment is still ahead: booked and not over (it counts until <see cref="HcState.SlotMinutes" />
    /// after its start, like <see cref="HcState.UpcomingOf" />), or checked in or with the doctor.
    /// </summary>
    internal static bool IsUpcoming(HcAppointment appointment, DateTime now) =>
        appointment.Status is HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit
        || (appointment.Status == HcAppointmentStatus.Booked && appointment.Start >= now.AddMinutes(-HcState.SlotMinutes));

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns the upcoming and past sections of the signed-in patient's appointments.</summary>
    private IReadOnlyList<AppointmentSection> Sections()
    {
        var all = State.CurrentPatient is { } patient ? State.PatientAppointments(patient.Id) : Array.Empty<HcAppointment>();
        var now = State.Clock.Now;
        var upcoming = all.Where(item => IsUpcoming(item, now)).OrderBy(item => item.Start).ToList();
        var past = all.Where(item => !IsUpcoming(item, now)).OrderByDescending(item => item.Start).ToList();

        return new[]
        {
            new AppointmentSection("upcoming", "Upcoming", upcoming, false, "You have no upcoming appointments."),
            new AppointmentSection("past", "Past", past, true, "You have no past visits with us yet."),
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private static string FindDoctorUrl => $"{HcLayout.Base}/doctors";

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A list of appointments with its heading and empty-state text.</summary>
    private sealed record AppointmentSection(string Key, string Title, IReadOnlyList<HcAppointment> Items, bool IsPast, string Empty);

    #endregion
}
