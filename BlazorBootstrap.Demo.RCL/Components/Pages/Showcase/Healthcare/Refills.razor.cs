using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Refill requests waiting for a doctor (<see cref="HcState.RefillRequests" />) in a <see cref="Grid{TItem}" />: when it was
/// requested, the medication (the row opens the answer dialog), the patient and their record number, the quantity, pick-up
/// day, and pharmacy asked for, the prescribing doctor, and the last fill, oldest first, with paging, sorting, the column
/// chooser, and CSV export. A filter shows the signed-in doctor's prescriptions (the default) or any doctor's, and a search
/// narrows by patient, record number, or medication; the grid's view is saved for the visit and can be reset. The dialog's
/// <see cref="EditForm" /> answers one request (rule WR-002: an answer is required; denying needs a reason of
/// <see cref="HcState.MinDenyReasonLength" /> to <see cref="HcState.MaxReasonLength" /> characters, a cross-field rule;
/// <see cref="HcState.DecideRefill" />), and the bulk bar's answers several at once (<see cref="HcState.BulkDecideRefills" />).
/// Outcomes are announced in the status region, which takes focus because answered requests leave the list.
/// </summary>
public partial class Refills : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string ApproveValue = "approve";

    private const int DefaultPageSize = 25;

    private const string DenyValue = "deny";

    private const string MineFilter = "mine";

    /// <summary>The rows' buttons, by prescription ID, so focus returns to one when its dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    private EditContext bulkContext;

    private DecisionForm bulkForm = new();

    private ElementReference? countRegion;

    private ElementReference? dialogCancel;

    private int dialogErrorCount;

    private string? dialogError;

    private EditContext decisionContext;

    private DecisionForm decisionForm = new();

    private Modal decisionModal = default!;

    private string? errorMessage;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private Grid<RequestRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The request the dialog is about.</summary>
    private int openedId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The answer sent from the dialog, announced once the dialog is hidden.</summary>
    private (string Message, bool IsError)? pendingAnnouncement;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The requests selected in the grid (on its current page).</summary>
    private List<RequestRow> selected = new();

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    private string whoseFilter = MineFilter;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with empty answer forms.</summary>
    public Refills()
    {
        bulkContext = new EditContext(bulkForm);
        decisionContext = new EditContext(decisionForm);
    }

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HcDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "refill-requests", DefaultPageSize, "requested", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Answers the selected requests in one <see cref="HcState.BulkDecideRefills" /> call once the bar's form is valid.</summary>
    private async Task ApplyBulkAsync()
    {
        if (!await bulkContext.ValidateAsync())
            return;

        errorMessage = null;
        var approve = bulkForm.Decision == ApproveValue;
        var outcome = State.BulkDecideRefills(selected.Select(row => row.Id).ToList(), approve, bulkForm.Note);

        switch (outcome.Result)
        {
            case HcResult.Ok:
                SetStatus((approve ? $"Approved {HcDisplay.Plural(outcome.Changed, "refill", "refills")}: one refill added to each." : $"Denied {HcDisplay.Plural(outcome.Changed, "refill", "refills")}.")
                    + " Each patient sees your answer under Prescriptions."
                    + (outcome.Unchanged > 0 ? $" {HcDisplay.Plural(outcome.Unchanged, "request was", "requests were")} answered meanwhile or at the most refills, and left as they were." : string.Empty), false);
                break;

            case HcResult.NoChange:
                SetStatus("No requests changed: they were answered meanwhile, or already have the most refills.", false);
                break;

            case HcResult.NotAllowed:
                errorMessage = "Only doctors can answer refill requests. Sign in again; nothing changed.";
                break;

            default:
                errorMessage = "Those answers couldn't be saved. Nothing changed.";
                break;
        }

        bulkForm = new DecisionForm();
        bulkContext = new EditContext(bulkForm);

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<RequestRow>();
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        whoseFilter = MineFilter;
        await FiltersChangedAsync();
        await HcDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<RequestRow>();
        errorMessage = null;
        SetStatus("Selection cleared.", false);
        focusTarget = () => statusRegion;
    }

    private string CountText(int total)
    {
        var shown = Shown.Count;

        return shown == total ? $"{HcDisplay.Plural(total, "request is", "requests are")} waiting." : $"Showing {shown.ToString(CultureInfo.InvariantCulture)} of {HcDisplay.Plural(total, "waiting request", "waiting requests")}.";
    }

    /// <summary>Answers the dialog's request once its form is valid; the answer is announced when the dialog is hidden, a refusal in the dialog.</summary>
    private async Task DecideAsync()
    {
        if (!await decisionContext.ValidateAsync())
            return;

        if (Opened is not { } prescription)
        {
            pendingAnnouncement = ("That request was already answered, so nothing changed.", true);
            await decisionModal.HideAsync();

            return;
        }

        var approve = decisionForm.Decision == ApproveValue;
        var label = $"{prescription.Medication} {prescription.Dose}";
        var patientName = State.FindPatient(prescription.PatientId)?.Name ?? "the patient";
        var result = State.DecideRefill(prescription.Id, approve, decisionForm.Note ?? string.Empty);

        switch (result)
        {
            case HcResult.Ok:
                pendingAnnouncement = (approve
                    ? $"Approved {label} for {patientName}: one refill added. They see it under Prescriptions."
                    : $"Denied {label} for {patientName}. They see your reason under Prescriptions.", false);
                await decisionModal.HideAsync();

                return;

            case HcResult.Invalid when approve && prescription.Refills >= HcState.MaxRefills:
                ShowDialogError($"{label} already has the most refills ({HcState.MaxRefills}), so it can't get another. Deny with a reason instead.");

                return;

            case HcResult.Invalid:
                ShowDialogError("The answer couldn't be saved: the storage for this demo tab is full. Nothing changed.");

                return;

            case HcResult.WrongState:
                pendingAnnouncement = ($"The request for {label} was already answered, or the prescription was stopped. Nothing changed.", true);
                await decisionModal.HideAsync();

                return;

            case HcResult.NotAllowed:
                ShowDialogError("Only doctors can answer refill requests. Sign in again; nothing changed.");

                return;

            default:
                ShowDialogError("That didn't work, and nothing was answered. Try again.");

                return;
        }
    }

    /// <summary>A filter changed: back to the first page of the filtered requests.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private Task OnDialogShownAsync() => HcDisplay.FocusAsync(dialogCancel);

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: after an answer focus moves to the announcement, otherwise back to the row.</summary>
    private Task OnDialogHiddenAsync()
    {
        var id = openedId;

        if (pendingAnnouncement is { } announcement)
        {
            pendingAnnouncement = null;
            SetStatus(announcement.Message, announcement.IsError);
            focusTarget = () => statusRegion;
        }
        else
        {
            focusTarget = () => openers.TryGetValue(id, out var opener) ? opener : statusRegion;
        }

        StateHasChanged();

        return Task.CompletedTask;
    }

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    /// <summary>Filters on every keystroke without changing the box's bound value, so a slower render never overwrites newer typing.</summary>
    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<RequestRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            errorMessage = null;
    }

    /// <summary>Another screen or a change here changed the state: the grid reads the requests again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnWhoseChangedAsync(ChangeEventArgs args)
    {
        whoseFilter = args.Value?.ToString() == MineFilter ? MineFilter : string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>Opens the answer dialog of one request, with an empty form.</summary>
    private async Task OpenAsync(int prescriptionId)
    {
        openedId = prescriptionId;
        decisionForm = new DecisionForm();
        decisionContext = new EditContext(decisionForm);
        dialogError = null;
        dialogErrorCount = 0;
        pendingAnnouncement = null;
        await decisionModal.ShowAsync();
    }

    /// <summary>The page of requests the grid asked for: oldest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<RequestRow>> ProvideRequestsAsync(GridDataProviderRequest<RequestRow> request) => view.ApplyAsync(request, Shown);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<RequestRow>();
        gridVersion++;
        SetStatus("The view is reset: oldest request first, 25 a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(countRegion);
    }

    private string RowClass(RequestRow row) => selected.Any(item => item.Id == row.Id) ? "hc-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Shows a refusal in the dialog's alert; the same one again gets a counter, so it is announced again.</summary>
    private void ShowDialogError(string problem)
    {
        dialogErrorCount = dialogError?.StartsWith(problem, StringComparison.Ordinal) == true ? dialogErrorCount + 1 : 1;
        dialogError = dialogErrorCount == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({dialogErrorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || whoseFilter != MineFilter;

    private HcPrescription? Opened => State.RefillRequests.FirstOrDefault(item => item.Id == openedId);

    /// <summary>The waiting requests that pass the filters, as grid rows, oldest first.</summary>
    private IReadOnlyList<RequestRow> Shown
    {
        get
        {
            var term = search.Trim();
            var me = State.CurrentDoctor?.Id;
            var doctors = State.Doctors.ToDictionary(doctor => doctor.Id, doctor => doctor.Name);

            return State.RefillRequests
                .Where(item => whoseFilter != MineFilter || item.DoctorId == me)
                .Select(item => (Prescription: item, Patient: State.FindPatient(item.PatientId)))
                .Where(pair => term.Length == 0 || pair.Prescription.Medication.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (pair.Patient is not null && (pair.Patient.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || pair.Patient.Mrn.Contains(term, StringComparison.OrdinalIgnoreCase))))
                .Select(pair => new RequestRow(
                    pair.Prescription.Id,
                    pair.Prescription.RefillRequestedAt ?? State.Clock.Now,
                    HcDisplay.When(pair.Prescription.RefillRequestedAt ?? State.Clock.Now),
                    $"{pair.Prescription.Medication} {pair.Prescription.Dose}",
                    pair.Patient?.Name ?? "A former patient",
                    pair.Patient?.Mrn ?? string.Empty,
                    pair.Prescription.RefillQuantity,
                    pair.Prescription.RefillPickupOn ?? State.Clock.Today,
                    HcDisplay.Day(pair.Prescription.RefillPickupOn ?? State.Clock.Today),
                    State.FindPharmacy(pair.Prescription.RefillPharmacyId)?.Name ?? "Not chosen",
                    (doctors.GetValueOrDefault(pair.Prescription.DoctorId) ?? "A former doctor") + (pair.Prescription.DoctorId == me ? " (you)" : null),
                    pair.Prescription.LastFilledOn,
                    HcDisplay.Date(pair.Prescription.LastFilledOn)))
                .ToList();
        }
    }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>An answer to one or more refill requests: approve or deny, and a note that a denial needs.</summary>
    private sealed class DecisionForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose whether to approve or deny the refill.")]
        public string Decision { get; set; } = string.Empty;

        [StringLength(HcState.MaxReasonLength, ErrorMessage = "Use 200 characters or fewer for the note.")]
        public string? Note { get; set; }

        /// <summary>Runs once the fields are filled in: a denial needs a reason the patient can read.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Decision == DenyValue && (Note?.Trim().Length ?? 0) < HcState.MinDenyReasonLength)
                yield return new ValidationResult("Tell the patient why the refill is denied, in 5 to 200 characters.", new[] { nameof(Note) });
        }
    }

    /// <summary>One waiting request as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record RequestRow(
        int Id,
        DateTime Requested,
        string RequestedText,
        string Medication,
        string Patient,
        string Mrn,
        int Quantity,
        DateTime Pickup,
        string PickupText,
        string Pharmacy,
        string Doctor,
        DateTime LastFilled,
        string LastFilledText);

    #endregion
}
