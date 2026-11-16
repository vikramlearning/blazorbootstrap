using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in doctor's lab work (<see cref="HcState.MyLabWork" />) in three groups: waiting for results, entered but not
/// released, and released. A waiting order has an entry form: one native number field per analyte, labelled with its name
/// and unit, with the reference range as help, and a comment for the patient (<see cref="HcState.EnterLabResults" />). An
/// entered order shows its values with flags in words, a Correct results button that reopens the form, and Release to
/// patient, which asks first (the patient sees the results and is billed) and releases once the dialog is hidden
/// (<see cref="HcState.ReleaseLab" />). Released orders are read-only. <c>?order={id}</c> highlights one order and moves
/// focus to it. Every result is announced in an always-rendered status region.
/// </summary>
public partial class Labs : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, LabDraft> drafts = new();

    private readonly Dictionary<int, ElementReference> editButtons = new();

    private readonly Dictionary<int, ElementReference> headings = new();

    private readonly Dictionary<int, ElementReference> releaseButtons = new();

    private readonly Dictionary<string, ElementReference> valueInputs = new();

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The <c>?order=</c> value this page last reacted to.</summary>
    private string? handledOrderQuery;

    /// <summary>The order from <c>?order=</c> that still needs focus once it is on the page.</summary>
    private int? pendingOrderFocus;

    /// <summary>Whether Release was confirmed, so it runs once the dialog is hidden.</summary>
    private bool pendingRelease;

    /// <summary>The order the Release dialog is about.</summary>
    private int releaseId;

    private string releaseLabel = string.Empty;

    private Modal releaseModal = default!;

    private string releasePatient = string.Empty;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (pendingOrderFocus is { } orderId && State.CurrentDoctor is not null)
        {
            if (headings.TryGetValue(orderId, out var heading))
            {
                pendingOrderFocus = null;
                await HcDisplay.FocusAsync(heading);

                return;
            }

            if (State.MyLabWork.All(item => item.Id != orderId))
            {
                pendingOrderFocus = null;
                Announce(false, $"Lab order {orderId.ToString(CultureInfo.InvariantCulture)} isn't one of yours, or the link is wrong. Your lab work is below.");
                StateHasChanged();

                return;
            }
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HcDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (handledOrderQuery == OrderQuery)
            return;

        handledOrderQuery = OrderQuery;
        pendingOrderFocus = TargetOrder;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>Returns the staff lab results route that opens one order.</summary>
    public static string OrderUrl(int orderId) => FormattableString.Invariant($"{HcStaffLayout.Base}/labs?order={orderId}");

    /// <summary>Returns an analyte's reference range like "4–11 ×10⁹/L" or "Up to 200 mg/dL".</summary>
    public static string RangeText(HcAnalyte analyte) =>
        analyte.Low <= 0 ? $"Up to {HcDisplay.Number(analyte.High)} {analyte.Unit}" : $"{HcDisplay.Number(analyte.Low)}–{HcDisplay.Number(analyte.High)} {analyte.Unit}";

    /// <summary>Returns the Bootstrap color of a lab status (always shown with its words).</summary>
    public static string StatusColor(HcLabStatus status) => status switch
    {
        HcLabStatus.Ordered => "secondary",
        HcLabStatus.Resulted => "warning",
        _ => "success",
    };

    /// <summary>Returns a lab status in words for staff.</summary>
    public static string StatusLabel(HcLabStatus status) => status switch
    {
        HcLabStatus.Ordered => "Waiting for results",
        HcLabStatus.Resulted => "Entered, not released",
        _ => "Released to the patient",
    };

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
    private void CancelEdit(int orderId)
    {
        drafts.Remove(orderId);
        Announce(true, "Correction cancelled. The entered results are unchanged.");
        focusTarget = () => editButtons.TryGetValue(orderId, out var button) ? button : statusRegion;
    }

    private async Task ConfirmReleaseAsync()
    {
        pendingRelease = true;

        await releaseModal.HideAsync();
    }

    /// <summary>Returns the order's form values, starting from the entered results.</summary>
    private LabDraft DraftFor(HcLabOrder order, HcLabPanel panel)
    {
        if (drafts.TryGetValue(order.Id, out var draft) && draft.Values.Length == panel.Analytes.Count)
            return draft;

        draft = new LabDraft(panel.Analytes.Count)
        {
            Comment = order.Comment,
        };

        if (order.Values.Count == panel.Analytes.Count)
        {
            for (var index = 0; index < order.Values.Count; index++)
                draft.Values[index] = HcDisplay.Number(order.Values[index]);
        }

        drafts[order.Id] = draft;

        return draft;
    }

    /// <summary>Releases a confirmed order now that the dialog is hidden, or returns focus to its Release button.</summary>
    private Task OnReleaseHiddenAsync()
    {
        var accepted = pendingRelease;
        var id = releaseId;
        pendingRelease = false;

        if (!accepted)
        {
            focusTarget = () => releaseButtons.TryGetValue(id, out var button) ? button : statusRegion;
            StateHasChanged();

            return Task.CompletedTask;
        }

        var order = State.MyLabWork.FirstOrDefault(item => item.Id == id);
        var result = State.ReleaseLab(id);

        Announce(result == HcResult.Ok, result switch
        {
            HcResult.Ok => $"Released {releaseLabel} to {releasePatient}. They see it in Lab reports with a PDF, and were billed {HcDisplay.Money(HcSeedData.LabFeeCents)}.",
            HcResult.WrongState when order?.Status == HcLabStatus.Released => $"{releaseLabel} was already released.",
            HcResult.WrongState => $"Enter the results of {releaseLabel} before releasing them.",
            HcResult.Invalid => "The storage for this demo tab is full, so nothing was released and nothing was billed. Try again after removing something.",
            HcResult.NotFound => "That lab order isn't one of yours any more, so nothing was released.",
            HcResult.NotAllowed => "Only the ordering doctor can release results. Sign in again; nothing was released.",
            _ => "That didn't work, and nothing was released. Try again.",
        });

        if (result == HcResult.Ok)
        {
            drafts.Remove(id);
            focusTarget = () => headings.TryGetValue(id, out var heading) ? heading : statusRegion;
        }
        else
        {
            focusTarget = () => statusRegion;
        }

        StateHasChanged();

        return Task.CompletedTask;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenReleaseAsync(HcLabOrder order)
    {
        releaseId = order.Id;
        releaseLabel = State.LabPanel(order.PanelId)?.Name ?? order.PanelId;
        releasePatient = State.FindPatient(order.PatientId)?.Name ?? "The patient";
        pendingRelease = false;

        await releaseModal.ShowAsync();
    }

    /// <summary>Checks and saves an order's results; flags are announced in words.</summary>
    private void SaveResults(int orderId)
    {
        if (State.MyLabWork.FirstOrDefault(item => item.Id == orderId) is not { } order || State.LabPanel(order.PanelId) is not { } panel)
        {
            Announce(false, "That lab order isn't one of yours any more, so nothing was saved.");
            focusTarget = () => statusRegion;

            return;
        }

        var draft = DraftFor(order, panel);
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
                firstInvalid ??= $"hc-labs-{order.Id}-value-{index}";
            else
                values.Add(value);
        }

        var comment = draft.Comment.Trim();
        draft.CommentError = comment.Length > HcState.MaxCommentLength ? $"Shorten the comment to {HcState.MaxCommentLength} characters or fewer (now {comment.Length})." : null;

        if (draft.CommentError is not null)
            firstInvalid ??= $"hc-labs-{order.Id}-comment";

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
                drafts.Remove(order.Id);
                Announce(true, $"{(wasEntered ? "Corrected" : "Saved")} {panel.Name} results for {patientName}: {FlagsText(panel, order.Values)}. {patientName} sees nothing until you release them.");
                focusTarget = () => releaseButtons.TryGetValue(order.Id, out var button) ? button : statusRegion;

                return;

            case HcResult.WrongState:
                drafts.Remove(order.Id);
                Announce(false, $"These {panel.Name} results were already released, so they can't change.");
                break;

            case HcResult.Invalid:
                Announce(false, "The storage for this demo tab is full, so the results weren't saved. Try again after removing something.");
                break;

            case HcResult.NotFound:
                Announce(false, "That lab order isn't one of yours any more, so nothing was saved.");
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
    private void StartEdit(int orderId)
    {
        if (State.MyLabWork.FirstOrDefault(item => item.Id == orderId) is not { } order || State.LabPanel(order.PanelId) is not { } panel)
            return;

        drafts.Remove(orderId);
        DraftFor(order, panel).Editing = true;
        statusMessage = null;
        focusTarget = () => valueInputs.TryGetValue($"hc-labs-{orderId}-value-0", out var input) ? input : statusRegion;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the order to open, from <c>?order=</c>.</summary>
    [SupplyParameterFromQuery(Name = "order")] public string? OrderQuery { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    /// <summary>The order from <c>?order=</c>, or <see langword="null" />.</summary>
    private int? TargetOrder => int.TryParse(OrderQuery, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : null;

    #endregion

    #region Nested Types

    /// <summary>The entry form of one lab order.</summary>
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

        /// <summary>Gets or sets whether an entered order's form is open for a correction.</summary>
        public bool Editing { get; set; }

        /// <summary>Gets each value's error.</summary>
        public string?[] Errors { get; }

        /// <summary>Gets the values as typed, in the panel's analyte order.</summary>
        public string[] Values { get; }
    }

    #endregion
}
