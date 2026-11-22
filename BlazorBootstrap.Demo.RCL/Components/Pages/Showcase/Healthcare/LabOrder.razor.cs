using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// One lab order for doctors: the test and patient, its status in words, who ordered it and in which visit, and its
/// results. For the ordering doctor, a waiting order has an entry <see cref="EditForm" /> (rule WR-002): one number field per
/// analyte, labelled with its name and unit, required, from 0 up to the most the lab can report for it, with the reference
/// range and the flag the value gets (low, normal, or high) as help while typing, and a comment for the patient of up to
/// 1,000 characters; a failed submit shows a summary that links to each field (<see cref="HcState.EnterLabResults" />). An
/// entered order shows its values with flags in words, Correct results (which reopens the form), and Release to patient,
/// which asks first (the patient sees the results and is billed) and releases once the dialog is hidden
/// (<see cref="HcState.ReleaseLab" />). Released orders, and other doctors' orders, are read-only. Every result is announced
/// in an always-rendered status region. An unknown ID shows <see cref="HcNotFound" />.
/// </summary>
public partial class LabOrder : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The order the form was made for.</summary>
    private int draftFor;

    private ElementReference? editButton;

    /// <summary>Whether an entered order's form is open for a correction.</summary>
    private bool editing;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private ElementReference? heading;

    private EditContext labContext;

    private LabResultForm labForm = new();

    /// <summary>Whether Release was confirmed, so it runs once the dialog is hidden.</summary>
    private bool pendingRelease;

    private ElementReference? releaseButton;

    private Modal releaseModal = default!;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with an empty form.</summary>
    public LabOrder() => labContext = new EditContext(labForm);

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
        if (draftFor == Id)
            return;

        statusMessage = null;
        editing = false;
        ResetForm();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns the flag a typed value gets, like "Flag: High", or nothing before a value is typed.</summary>
    private static string? FlagText(HcAnalyte analyte, double? value) =>
        value is { } number && double.IsFinite(number) && number >= 0 && number <= analyte.MaxValue ? $"Flag: {HcState.Flag(analyte, number)}." : null;

    /// <summary>Returns each value with its flag, like "HbA1c 6.8 % (High)".</summary>
    private static string FlagsText(HcLabPanel panel, IReadOnlyList<double> values) =>
        string.Join("; ", panel.Analytes.Select((analyte, index) => $"{analyte.Name} {HcDisplay.Number(values[index])} {analyte.Unit} ({HcState.Flag(analyte, values[index])})"));

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Closes the correction form of an entered order without saving.</summary>
    private void CancelEdit()
    {
        editing = false;
        ResetForm();
        Announce(true, "Correction cancelled. The entered results are unchanged.");
        focusTarget = () => editButton ?? statusRegion;
    }

    private async Task ConfirmReleaseAsync()
    {
        pendingRelease = true;

        await releaseModal.HideAsync();
    }

    /// <summary>Releases a confirmed order now that the dialog is hidden, or returns focus to the Release button.</summary>
    private Task OnReleaseHiddenAsync()
    {
        var accepted = pendingRelease;
        pendingRelease = false;

        if (!accepted)
        {
            focusTarget = () => releaseButton ?? statusRegion;

            return Task.CompletedTask;
        }

        var order = State.FindLabOrder(Id);
        var label = State.LabPanel(order?.PanelId)?.Name ?? "the test";
        var patient = order is null ? "the patient" : State.FindPatient(order.PatientId)?.Name ?? "the patient";
        var result = State.ReleaseLab(Id);

        Announce(result == HcResult.Ok, result switch
        {
            HcResult.Ok => $"Released {label} to {patient}. They see it in Lab reports with a PDF, and were billed {HcDisplay.Money(HcSeedData.LabFeeCents)}.",
            HcResult.WrongState when order?.Status == HcLabStatus.Released => $"{label} was already released.",
            HcResult.WrongState => $"Enter the results of {label} before releasing them.",
            HcResult.Invalid => "The storage for this demo tab is full, so nothing was released and nothing was billed. Try again after removing something.",
            HcResult.NotFound => "That lab order isn't one of yours, so nothing was released.",
            HcResult.NotAllowed => "Only the ordering doctor can release results. Sign in again; nothing was released.",
            _ => "That didn't work, and nothing was released. Try again.",
        });

        focusTarget = () => result == HcResult.Ok ? heading : statusRegion;

        return Task.CompletedTask;
    }

    /// <summary>Another screen changed the state: a form that was set up before the order was found starts from it now.</summary>
    private void OnStateChanged() => InvokeAsync(() =>
    {
        if (State.FindLabOrder(Id) is { } order && State.LabPanel(order.PanelId) is { } panel && labForm.Count != panel.Analytes.Count)
            ResetForm();

        StateHasChanged();
    });

    private async Task OpenReleaseAsync()
    {
        pendingRelease = false;

        await releaseModal.ShowAsync();
    }

    /// <summary>Starts the form from the entered results (or empty fields for a waiting order).</summary>
    private void ResetForm()
    {
        draftFor = Id;
        labForm = State.FindLabOrder(Id) is { } order && State.LabPanel(order.PanelId) is { } panel
            ? new LabResultForm
            {
                Analytes = panel.Analytes,
                Comment = order.Comment,
                Value0 = order.Values.Count > 0 ? order.Values[0] : null,
                Value1 = order.Values.Count > 1 ? order.Values[1] : null,
                Value2 = order.Values.Count > 2 ? order.Values[2] : null,
                Value3 = order.Values.Count > 3 ? order.Values[3] : null,
            }
            : new LabResultForm();
        labContext = new EditContext(labForm);
    }

    /// <summary>Saves the order's results once the form is valid; the flags are announced in words.</summary>
    private async Task SaveResults()
    {
        if (State.FindLabOrder(Id) is not { } order || State.LabPanel(order.PanelId) is not { } panel || order.DoctorId != State.CurrentDoctor?.Id)
        {
            Announce(false, "That lab order isn't one of yours, so nothing was saved.");
            focusTarget = () => statusRegion;

            return;
        }

        if (!await labContext.ValidateAsync())
            return;

        var values = labForm.Values.Take(panel.Analytes.Count).Select(value => value!.Value).ToList();
        var patientName = State.FindPatient(order.PatientId)?.Name ?? "the patient";
        var wasEntered = order.Status == HcLabStatus.Resulted;
        var result = State.EnterLabResults(order.Id, values, labForm.Comment ?? string.Empty);

        switch (result)
        {
            case HcResult.Ok:
                editing = false;
                ResetForm();
                Announce(true, $"{(wasEntered ? "Corrected" : "Saved")} {panel.Name} results for {patientName}: {FlagsText(panel, order.Values)}. {patientName} sees nothing until you release them.");
                focusTarget = () => releaseButton ?? statusRegion;

                return;

            case HcResult.WrongState:
                editing = false;
                Announce(false, $"These {panel.Name} results were already released, so they can't change.");
                break;

            case HcResult.Invalid:
                Announce(false, "The results weren't saved: a value is out of range, or this demo tab's storage is full.");
                break;

            case HcResult.NotFound:
                Announce(false, "That lab order isn't one of yours, so nothing was saved.");
                break;

            case HcResult.NotAllowed:
                Announce(false, "Only the ordering doctor can enter results. Sign in again; nothing was saved.");
                break;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                break;
        }

        focusTarget = () => statusRegion;
    }

    /// <summary>Reopens the form of an entered order so its results can be corrected.</summary>
    private void StartEdit()
    {
        editing = true;
        ResetForm();
        statusMessage = null;
        focusTarget = () => statusRegion;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the lab order ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The results of a lab order: one value per analyte of the panel (at most four), each required, from 0 up to the most the
    /// lab reports for it, and a comment for the patient.
    /// </summary>
    private sealed class LabResultForm : IValidatableObject
    {
        /// <summary>The panel's analytes, in order, set by the page.</summary>
        public IReadOnlyList<HcAnalyte> Analytes { get; init; } = Array.Empty<HcAnalyte>();

        [StringLength(HcState.MaxCommentLength, ErrorMessage = "Shorten the comment to 1,000 characters or fewer.")]
        public string? Comment { get; set; } = string.Empty;

        /// <summary>How many values the panel has.</summary>
        public int Count => Analytes.Count;

        public double? Value0 { get; set; }

        public double? Value1 { get; set; }

        public double? Value2 { get; set; }

        public double? Value3 { get; set; }

        /// <summary>The values in the panel's order.</summary>
        public IReadOnlyList<double?> Values => new[] { Value0, Value1, Value2, Value3 };

        /// <summary>Runs on every change: each of the panel's values is entered and within what the lab can report.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            for (var index = 0; index < Analytes.Count && index < 4; index++)
            {
                var analyte = Analytes[index];
                var field = $"Value{index.ToString(CultureInfo.InvariantCulture)}";

                if (Values[index] is not { } value)
                    yield return new ValidationResult($"Enter the {analyte.Name} result.", new[] { field });
                else if (!double.IsFinite(value) || value < 0 || value > analyte.MaxValue)
                    yield return new ValidationResult($"Enter a {analyte.Name} result from 0 to {HcDisplay.Number(analyte.MaxValue)} {analyte.Unit}: anything else is a typing error.", new[] { field });
            }
        }
    }

    #endregion
}
