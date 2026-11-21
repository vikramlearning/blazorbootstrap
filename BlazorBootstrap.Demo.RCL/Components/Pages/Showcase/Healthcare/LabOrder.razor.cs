using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// One lab order for doctors: the test and patient, its status in words, who ordered it and in which visit, and its
/// results. For the ordering doctor, a waiting order has an entry form: one native number field per analyte, labelled with
/// its name and unit, with the reference range as help, and a comment for the patient (<see cref="HcState.EnterLabResults" />).
/// An entered order shows its values with flags in words, Correct results (which reopens the form), and Release to patient,
/// which asks first (the patient sees the results and is billed) and releases once the dialog is hidden
/// (<see cref="HcState.ReleaseLab" />). Released orders, and other doctors' orders, are read-only. Every result is announced
/// in an always-rendered status region. An unknown ID shows <see cref="HcNotFound" />.
/// </summary>
public partial class LabOrder : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<string, ElementReference> valueInputs = new();

    private LabDraft draft = new(0);

    /// <summary>The order the draft was made for.</summary>
    private int draftFor;

    private ElementReference? editButton;

    /// <summary>Whether an entered order's form is open for a correction.</summary>
    private bool editing;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private ElementReference? heading;

    /// <summary>Whether Release was confirmed, so it runs once the dialog is hidden.</summary>
    private bool pendingRelease;

    private ElementReference? releaseButton;

    private Modal releaseModal = default!;

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
        if (draftFor == Id)
            return;

        statusMessage = null;
        editing = false;
        ResetDraft();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

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
        ResetDraft();
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
            StateHasChanged();

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
        StateHasChanged();

        return Task.CompletedTask;
    }

    /// <summary>Another screen changed the state: a form that was set up before the order was found starts from it now.</summary>
    private void OnStateChanged() => InvokeAsync(() =>
    {
        if (State.FindLabOrder(Id) is { } order && State.LabPanel(order.PanelId) is { } panel && draft.Values.Length != panel.Analytes.Count)
            ResetDraft();

        StateHasChanged();
    });

    private async Task OpenReleaseAsync()
    {
        pendingRelease = false;

        await releaseModal.ShowAsync();
    }

    /// <summary>Starts the form from the entered results (or empty fields for a waiting order).</summary>
    private void ResetDraft()
    {
        draftFor = Id;

        if (State.FindLabOrder(Id) is not { } order || State.LabPanel(order.PanelId) is not { } panel)
        {
            draft = new LabDraft(0);

            return;
        }

        draft = new LabDraft(panel.Analytes.Count) { Comment = order.Comment };

        if (order.Values.Count == panel.Analytes.Count)
        {
            for (var index = 0; index < order.Values.Count; index++)
                draft.Values[index] = HcDisplay.Number(order.Values[index]);
        }
    }

    /// <summary>Checks and saves the order's results; flags are announced in words.</summary>
    private void SaveResults()
    {
        if (State.FindLabOrder(Id) is not { } order || State.LabPanel(order.PanelId) is not { } panel || order.DoctorId != State.CurrentDoctor?.Id)
        {
            Announce(false, "That lab order isn't one of yours, so nothing was saved.");
            focusTarget = () => statusRegion;

            return;
        }

        if (draft.Values.Length != panel.Analytes.Count)
            ResetDraft();

        var values = new List<double>();
        string? firstInvalid = null;

        for (var index = 0; index < panel.Analytes.Count; index++)
        {
            var analyte = panel.Analytes[index];
            var text = draft.Values[index]?.Trim() ?? string.Empty;
            var parsed = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value);

            draft.Errors[index] = text.Length == 0 ? $"Enter the {analyte.Name} result."
                : !parsed || !double.IsFinite(value) ? "Enter a number, like 5.4."
                : value is < 0 or > 100_000 ? "Enter a value from 0 to 100,000."
                : null;

            if (draft.Errors[index] is not null)
                firstInvalid ??= $"hc-lo-value-{index}";
            else
                values.Add(value);
        }

        var comment = draft.Comment.Trim();
        draft.CommentError = comment.Length > HcState.MaxCommentLength ? $"Shorten the comment to {HcState.MaxCommentLength} characters or fewer (now {comment.Length})." : null;

        if (draft.CommentError is not null)
            firstInvalid ??= "hc-lo-comment";

        if (firstInvalid is not null)
        {
            Announce(false, $"The {panel.Name} results weren't saved. Fix the marked fields, then save again.");
            focusTarget = () => valueInputs.TryGetValue(firstInvalid, out var input) ? input : statusRegion;

            return;
        }

        var patientName = State.FindPatient(order.PatientId)?.Name ?? "the patient";
        var wasEntered = order.Status == HcLabStatus.Resulted;
        var result = State.EnterLabResults(order.Id, values, comment);

        switch (result)
        {
            case HcResult.Ok:
                editing = false;
                ResetDraft();
                Announce(true, $"{(wasEntered ? "Corrected" : "Saved")} {panel.Name} results for {patientName}: {FlagsText(panel, order.Values)}. {patientName} sees nothing until you release them.");
                focusTarget = () => releaseButton ?? statusRegion;

                return;

            case HcResult.WrongState:
                editing = false;
                Announce(false, $"These {panel.Name} results were already released, so they can't change.");
                break;

            case HcResult.Invalid:
                Announce(false, "The storage for this demo tab is full, so the results weren't saved. Try again after removing something.");
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
        ResetDraft();
        statusMessage = null;
        focusTarget = () => valueInputs.TryGetValue("hc-lo-value-0", out var input) ? input : statusRegion;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the lab order ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The entry form of the lab order.</summary>
    private sealed class LabDraft
    {
        public LabDraft(int count)
        {
            Values = Enumerable.Repeat(string.Empty, count).ToArray();
            Errors = new string?[count];
        }

        /// <summary>Gets or sets the comment for the patient.</summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>Gets or sets the comment's error.</summary>
        public string? CommentError { get; set; }

        /// <summary>Gets each value's error.</summary>
        public string?[] Errors { get; }

        /// <summary>Gets the values as typed, in the panel's analyte order.</summary>
        public string[] Values { get; }
    }

    #endregion
}
