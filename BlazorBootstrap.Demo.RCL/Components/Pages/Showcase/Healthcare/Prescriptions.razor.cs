using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's prescriptions, active then stopped: each with the dose, directions, quantity, refills left, the
/// prescribing doctor, and the date. An active one with no refills left and no request waiting has "Request refill"
/// (<see cref="HcState.RequestRefill" />); a waiting request reads "Refill requested on …, waiting for Dr. …", and the
/// doctor's answer (<see cref="HcPrescription.RefillNote" />) is shown in words as approved or not approved. Results are
/// announced in an always-rendered status region, which takes focus.
/// </summary>
public partial class Prescriptions : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly IReadOnlyList<RxSection> Sections = new[]
    {
        new RxSection("active", "Active", HcPrescriptionStatus.Active, "You have no active prescriptions."),
        new RxSection("stopped", "Stopped", HcPrescriptionStatus.Stopped, "None of your prescriptions has been stopped."),
    };

    private bool focusStatus;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await HcDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private IReadOnlyList<HcPrescription> All() => State.CurrentPatient is { } patient ? State.PrescriptionsOf(patient.Id) : Array.Empty<HcPrescription>();

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
        focusStatus = true;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private static string RefillsText(int refills) => refills == 1 ? "1 refill" : string.Create(CultureInfo.InvariantCulture, $"{refills} refills");

    /// <summary>Asks the doctor for a refill and maps every result.</summary>
    private void RequestRefill(int prescriptionId)
    {
        var prescription = All().FirstOrDefault(item => item.Id == prescriptionId);
        var name = prescription is null ? "this medicine" : $"{prescription.Medication} {prescription.Dose}";
        var doctor = State.FindDoctor(prescription?.DoctorId)?.Name ?? "your doctor";

        switch (State.RequestRefill(prescriptionId))
        {
            case HcResult.Ok:
                Announce(true, $"Refill requested for {name}. {doctor} will answer here.");
                break;

            case HcResult.WrongState:
                Announce(false, $"A refill of {name} can't be requested right now: it still has refills, it was stopped, or a request is already waiting.");
                break;

            case HcResult.NotFound:
                Announce(false, "This prescription can't be found any more, so no refill was requested.");
                break;

            case HcResult.NotAllowed:
                Announce(false, "Only the patient's own account can request refills. Nothing was requested.");
                break;

            default:
                Announce(false, $"The refill of {name} couldn't be requested. Try again.");
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A list of prescriptions by status, with its heading and empty-state text.</summary>
    private sealed record RxSection(string Key, string Title, HcPrescriptionStatus Status, string Empty);

    #endregion
}
