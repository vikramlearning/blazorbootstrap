using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's health record: the summaries of completed visits (newest first, with the doctor, the date, and a
/// link to the visit), then the conditions, allergies, and vaccinations on their chart, each with a note that the doctor
/// keeps it up to date. Everything is plain text; the doctor's own visit notes are never shown.
/// </summary>
public partial class Records : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Returns the patient's completed visits, newest first.</summary>
    private IReadOnlyList<HcAppointment> Visits(HcPatient patient) =>
        State.PatientAppointments(patient.Id).Where(item => item.Status == HcAppointmentStatus.Completed).OrderByDescending(item => item.Start).ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HcState State { get; set; } = default!;

    #endregion
}
