using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The patients of the clinics for a doctor (<see cref="HcState.Patients" />), as a table with a caption: name (linked to the
/// chart), age, record number, the next visit with the signed-in doctor, and the last completed visit with any doctor. A
/// labelled search filters by name, record number, phone, or email, and the number of matches is announced. The search
/// stays on the page; names never go into the URL.
/// </summary>
public partial class Patients : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string query = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Plural(int count) => count == 1 ? "1 patient" : FormattableString.Invariant($"{count} patients");

    /// <summary>Returns the patient's last completed visit with any doctor, or <see langword="null" />.</summary>
    private HcAppointment? LastVisit(int patientId) =>
        State.PatientAppointments(patientId).Where(item => item.Status == HcAppointmentStatus.Completed).MaxBy(item => item.Start);

    private bool Matches(HcPatient patient)
    {
        var text = query.Trim();

        if (text.Length == 0)
            return true;

        var number = text.Replace("MRN", string.Empty, StringComparison.OrdinalIgnoreCase).TrimStart('#', ' ');

        return patient.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
            || patient.Email.Contains(text, StringComparison.OrdinalIgnoreCase)
            || (patient.Phone.Length > 0 && patient.Phone.Contains(text, StringComparison.OrdinalIgnoreCase))
            || patient.Id.ToString(CultureInfo.InvariantCulture) == number;
    }

    /// <summary>Returns the patient's next booked or current visit with the signed-in doctor, or <see langword="null" />.</summary>
    private HcAppointment? NextWithMe(int patientId)
    {
        if (State.CurrentDoctor is not { } doctor)
            return null;

        var today = State.Clock.Today;

        return State.PatientAppointments(patientId)
            .Where(item => item.DoctorId == doctor.Id && item.Start >= today && item.Status is HcAppointmentStatus.Booked or HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit)
            .MinBy(item => item.Start);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HcState State { get; set; } = default!;

    #endregion
}
