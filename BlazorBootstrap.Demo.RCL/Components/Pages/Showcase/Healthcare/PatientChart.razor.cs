using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// A patient's chart for doctors: name, date of birth and age, record number, phone, and email (as text), the allergies
/// prominently, then tabs. The health summary lists allergies and conditions, each with a labelled Add field
/// (<see cref="HcState.AddAllergy" />, <see cref="HcState.AddCondition" />) and Remove buttons that name their item, and the
/// immunizations. Visits lists every visit with any doctor, with the status in words and the notes and summary in a
/// disclosure. Lab results lists every order with its status, and the values with flags once entered. Prescriptions lists
/// active and stopped ones; Stop asks first and runs once the dialog is hidden (<see cref="HcState.StopPrescription" />).
/// Every result is announced in an always-rendered status region. An unknown ID shows <see cref="HcNotFound" />.
/// </summary>
public partial class PatientChart : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<ListKind, ElementReference> addInputs = new();

    private readonly Dictionary<ListKind, string?> errors = new();

    private readonly Dictionary<ListKind, string> newText = new() { [ListKind.Allergy] = string.Empty, [ListKind.Condition] = string.Empty };

    private readonly Dictionary<string, ElementReference> removeButtons = new();

    private readonly Dictionary<int, ElementReference> stopButtons = new();

    /// <summary>The patient this page was set up for.</summary>
    private int builtFor;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>Whether Stop was confirmed, so it runs once the dialog is hidden.</summary>
    private bool pendingStop;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    /// <summary>The prescription the Stop dialog is about.</summary>
    private int stopId;

    private string stopLabel = string.Empty;

    private Modal stopModal = default!;

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
        if (builtFor == Id)
            return;

        builtFor = Id;
        statusMessage = null;
        errors.Clear();
        newText[ListKind.Allergy] = newText[ListKind.Condition] = string.Empty;
    }

    #endregion

    #region Public Methods

    /// <summary>Returns an age like "38 years old", or "Age not recorded".</summary>
    public static string AgeText(int? age) => age switch
    {
        null => "Age not recorded",
        1 => "1 year old",
        _ => FormattableString.Invariant($"{age} years old"),
    };

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>Returns the medical record number, like "MRN 2" (the patient ID, as on the lab report PDF).</summary>
    public static string Mrn(int patientId) => $"MRN {patientId.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>Returns a refill count like "no refills", "1 refill", or "3 refills".</summary>
    public static string RefillsText(int refills) => refills switch
    {
        0 => "no refills",
        1 => "1 refill",
        _ => FormattableString.Invariant($"{refills} refills"),
    };

    #endregion

    #region Private Methods

    private static string Initials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0])));

    private static string ItemKey(ListKind kind, string item) => $"{kind}:{item.ToLowerInvariant()}";

    private static List<string> Items(HcPatient patient, ListKind kind) => kind == ListKind.Allergy ? patient.Allergies : patient.Conditions;

    private static string Noun(ListKind kind) => kind == ListKind.Allergy ? "allergy" : "condition";

    /// <summary>Adds an allergy or a condition from its Add field.</summary>
    private void AddItem(ListKind kind)
    {
        if (State.FindPatient(Id) is not { } patient)
            return;

        var text = newText[kind].Trim();
        var noun = Noun(kind);
        var items = Items(patient, kind);

        errors[kind] = text.Length == 0 ? $"Enter the {noun} first."
            : text.Length > HcState.MaxListItemLength ? $"Shorten it to {HcState.MaxListItemLength} characters or fewer (now {text.Length})."
            : items.Count >= HcState.MaxListItems ? $"The list is full ({HcState.MaxListItems} items). Remove one first."
            : null;

        if (errors[kind] is not null)
        {
            Announce(false, $"The {noun} wasn't added. Fix the field, then add it again.");
            focusTarget = () => addInputs.TryGetValue(kind, out var input) ? input : statusRegion;

            return;
        }

        var result = kind == ListKind.Allergy ? State.AddAllergy(patient.Id, text) : State.AddCondition(patient.Id, text);

        switch (result)
        {
            case HcResult.Ok:
                newText[kind] = string.Empty;
                Announce(true, $"Added the {noun} “{text}” to {patient.Name}'s chart.");
                break;

            case HcResult.NoChange:
                newText[kind] = string.Empty;
                Announce(true, $"“{text}” is already on {patient.Name}'s {(kind == ListKind.Allergy ? "allergies" : "conditions")}, so nothing changed.");
                break;

            case HcResult.Invalid:
                errors[kind] = items.Count >= HcState.MaxListItems
                    ? $"The list is full ({HcState.MaxListItems} items). Remove one first."
                    : "The storage for this demo tab is full, so it wasn't added. Remove something, then try again.";
                Announce(false, $"The {noun} wasn't added.");
                break;

            default:
                Announce(false, OtherText(result, "added"));
                break;
        }

        focusTarget = () => addInputs.TryGetValue(kind, out var input) ? input : statusRegion;
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private async Task ConfirmStopAsync()
    {
        pendingStop = true;

        await stopModal.HideAsync();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Stops a confirmed prescription now that the dialog is hidden, or returns focus to its Stop button.</summary>
    private Task OnStopHiddenAsync()
    {
        var accepted = pendingStop;
        var id = stopId;
        pendingStop = false;

        if (!accepted)
        {
            focusTarget = () => stopButtons.TryGetValue(id, out var button) ? button : statusRegion;
            StateHasChanged();

            return Task.CompletedTask;
        }

        var result = State.StopPrescription(id);

        Announce(result is HcResult.Ok or HcResult.NoChange, result switch
        {
            HcResult.Ok => $"Stopped {stopLabel}. The patient sees it as stopped.",
            HcResult.NoChange => $"{stopLabel} was already stopped, so nothing changed.",
            HcResult.NotFound => $"{stopLabel} isn't on this chart any more, so nothing was stopped.",
            _ => OtherText(result, "stopped"),
        });
        stopButtons.Remove(id);
        focusTarget = () => statusRegion;
        StateHasChanged();

        return Task.CompletedTask;
    }

    private async Task OpenStopAsync(HcPrescription prescription)
    {
        stopId = prescription.Id;
        stopLabel = $"{prescription.Medication} {prescription.Dose}";
        pendingStop = false;

        await stopModal.ShowAsync();
    }

    /// <summary>The message for a refusal every chart change can return.</summary>
    private static string OtherText(HcResult result, string verb) => result switch
    {
        HcResult.NotAllowed => $"Only doctors can change charts. Sign in as a doctor; nothing was {verb}.",
        HcResult.NotFound => $"This patient's record wasn't found, so nothing was {verb}.",
        _ => $"That didn't work, and nothing was {verb}. Try again.",
    };

    /// <summary>Removes an allergy or a condition; focus moves to the next item's Remove button, or the Add field.</summary>
    private void RemoveItem(ListKind kind, string item)
    {
        if (State.FindPatient(Id) is not { } patient)
            return;

        var items = Items(patient, kind);
        var index = items.IndexOf(item);
        var noun = Noun(kind);
        var result = kind == ListKind.Allergy ? State.RemoveAllergy(patient.Id, item) : State.RemoveCondition(patient.Id, item);

        Announce(result == HcResult.Ok, result switch
        {
            HcResult.Ok => $"Removed the {noun} “{item}” from {patient.Name}'s chart.",
            HcResult.NotFound => $"“{item}” was already removed.",
            _ => OtherText(result, "removed"),
        });

        removeButtons.Remove(ItemKey(kind, item));
        var next = items.Count == 0 ? null : items[Math.Clamp(index, 0, items.Count - 1)];
        focusTarget = next is not null && removeButtons.TryGetValue(ItemKey(kind, next), out var button)
            ? () => button
            : () => addInputs.TryGetValue(kind, out var input) ? input : statusRegion;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the patient ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Which chart list an action is about.</summary>
    private enum ListKind
    {
        Allergy,
        Condition,
    }

    #endregion
}
