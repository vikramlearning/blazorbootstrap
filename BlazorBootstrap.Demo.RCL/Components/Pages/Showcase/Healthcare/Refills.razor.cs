using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Refill requests waiting for a doctor (<see cref="HcState.RefillRequests" />), oldest first: the patient (linked to their
/// chart), medication and dose, when it was requested, the prescribing doctor, and the directions. Each has a note for the
/// patient and Approve (the note is optional; one refill is added) or Decline (the note is required)
/// (<see cref="HcState.DecideRefill" />). The answer is announced in an always-rendered status region, which takes focus as
/// the request leaves the list; an empty list says nothing is waiting.
/// </summary>
public partial class Refills : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, string?> noteErrors = new();

    private readonly Dictionary<int, ElementReference> noteInputs = new();

    private readonly Dictionary<int, string> notes = new();

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

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Approves or declines a refill request with the note for the patient.</summary>
    private void Decide(int prescriptionId, bool approve)
    {
        if (State.RefillRequests.FirstOrDefault(item => item.Id == prescriptionId) is not { } prescription)
        {
            Announce(false, "That request was already answered.");
            focusTarget = () => statusRegion;

            return;
        }

        var label = $"{prescription.Medication} {prescription.Dose}";
        var patientName = State.FindPatient(prescription.PatientId)?.Name ?? "the patient";
        var note = (notes.GetValueOrDefault(prescriptionId) ?? string.Empty).Trim();

        noteErrors[prescriptionId] = !approve && note.Length == 0 ? $"Write a note telling {patientName} why the refill is declined."
            : note.Length > HcState.MaxReasonLength ? $"Shorten the note to {HcState.MaxReasonLength} characters or fewer (now {note.Length})."
            : null;

        if (noteErrors[prescriptionId] is not null)
        {
            Announce(false, $"The request wasn't {(approve ? "approved" : "declined")}. Fix the note, then try again.");
            focusTarget = () => noteInputs.TryGetValue(prescriptionId, out var input) ? input : statusRegion;

            return;
        }

        var result = State.DecideRefill(prescriptionId, approve, note);

        switch (result)
        {
            case HcResult.Ok:
                notes.Remove(prescriptionId);
                noteErrors.Remove(prescriptionId);
                noteInputs.Remove(prescriptionId);
                Announce(true, approve
                    ? $"Approved {label} for {patientName}: one refill added. They see it under Prescriptions."
                    : $"Declined {label} for {patientName}. They see your note under Prescriptions.");
                focusTarget = () => statusRegion;

                return;

            case HcResult.Invalid when approve && prescription.Refills >= HcState.MaxRefills:
                Announce(false, $"{label} already has the most refills ({HcState.MaxRefills}), so it can't get another. Decline with a note instead.");
                break;

            case HcResult.Invalid when !approve && note.Length == 0:
                noteErrors[prescriptionId] = $"Write a note telling {patientName} why the refill is declined.";
                Announce(false, "The request wasn't declined. Fix the note, then try again.");
                focusTarget = () => noteInputs.TryGetValue(prescriptionId, out var input) ? input : statusRegion;

                return;

            case HcResult.Invalid:
                Announce(false, "The storage for this demo tab is full, so nothing was answered. Try again after removing something.");
                break;

            case HcResult.WrongState:
                Announce(false, $"The request for {label} was already answered, or the prescription was stopped. Nothing changed.");
                break;

            case HcResult.NotFound:
                Announce(false, $"{label} isn't on {patientName}'s record any more, so nothing was answered.");
                break;

            case HcResult.NotAllowed:
                Announce(false, "Only doctors can answer refill requests. Sign in again; nothing changed.");
                break;

            default:
                Announce(false, "That didn't work, and nothing was answered. Try again.");
                break;
        }

        focusTarget = () => statusRegion;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HcState State { get; set; } = default!;

    #endregion
}
