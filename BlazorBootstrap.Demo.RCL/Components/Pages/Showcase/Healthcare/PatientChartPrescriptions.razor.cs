using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The Prescriptions tab of a patient's chart: every prescription in a <see cref="Grid{TItem}" /> (medication, dose,
/// status, quantity, refills left, when and by whom it was prescribed, and the refill request or last answer), active first,
/// with paging, sorting, the column chooser, and CSV export. A prescription has no page of its own, so the row opens a
/// <see cref="Modal" /> with its directions and fills; Stop asks in the same dialog and runs once it is hidden
/// (<see cref="HcState.StopPrescription" />), with the outcome announced in the status region. The grid's view is saved
/// for the visit and can be reset.
/// </summary>
public partial class PatientChartPrescriptions : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    /// <summary>The row buttons, by prescription ID, so focus returns to one when its dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    /// <summary>Whether the dialog asks to confirm Stop.</summary>
    private bool confirmStop;

    private ElementReference? dialogClose;

    private Func<ElementReference?>? focusTarget;

    private Grid<RxRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private Modal modal = default!;

    /// <summary>The prescription the dialog is about.</summary>
    private int openedId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>Whether Stop was confirmed, so it runs once the dialog is hidden.</summary>
    private bool pendingStop;

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

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
        view = GridViews.Get(State.SiteSlug, "chart-prescriptions", DefaultPageSize, null, SortDirection.None);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>The refill request waiting, or the last answer, in words.</summary>
    private string RefillText(HcPrescription prescription) =>
        prescription.RefillRequestedAt is { } requested
            ? $"Requested {HcDisplay.When(requested)}: {prescription.RefillQuantity.ToString(CultureInfo.InvariantCulture)} to pick up {HcDisplay.Day(prescription.RefillPickupOn ?? requested)} at {State.FindPharmacy(prescription.RefillPharmacyId)?.Name ?? "a pharmacy"}, waiting for an answer"
            : prescription.RefillOutcome switch
            {
                HcRefillOutcome.Approved => $"Last request approved: {prescription.RefillNote}",
                HcRefillOutcome.Denied => $"Last request denied: {prescription.RefillNote}",
                _ => "No refill requested",
            };

    private void AskStop() => confirmStop = true;

    private async Task ConfirmStopAsync()
    {
        pendingStop = true;
        await modal.HideAsync();
    }

    private void KeepIt()
    {
        confirmStop = false;
        focusTarget = () => dialogClose;
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus. A confirmed Stop runs now that the dialog is gone and
    /// focus moves to its outcome; otherwise focus returns to the row's button.
    /// </summary>
    private Task OnHiddenAsync()
    {
        var id = openedId;
        var stop = pendingStop;
        pendingStop = false;
        confirmStop = false;

        if (!stop)
        {
            focusTarget = () => openers.TryGetValue(id, out var button) ? button : statusRegion;
            StateHasChanged();

            return Task.CompletedTask;
        }

        var prescription = State.PrescriptionsOf(PatientId).FirstOrDefault(item => item.Id == id);
        var label = prescription is null ? "That prescription" : $"{prescription.Medication} {prescription.Dose}";
        var result = State.StopPrescription(id);

        SetStatus(result switch
        {
            HcResult.Ok => $"Stopped {label}. The patient sees it as stopped.",
            HcResult.NoChange => $"{label} was already stopped, so nothing changed.",
            HcResult.NotFound => $"{label} isn't on this chart any more, so nothing was stopped.",
            HcResult.NotAllowed => "Only doctors can stop prescriptions. Sign in as a doctor; nothing was stopped.",
            _ => "That didn't work, and nothing was stopped. Try again.",
        }, result is not (HcResult.Ok or HcResult.NoChange));
        focusTarget = () => statusRegion;
        StateHasChanged();

        return Task.CompletedTask;
    }

    private Task OnShownAsync() => HcDisplay.FocusAsync(dialogClose);

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenAsync(int prescriptionId)
    {
        openedId = prescriptionId;
        confirmStop = false;
        pendingStop = false;
        await modal.ShowAsync();
    }

    /// <summary>The page of prescriptions the grid asked for: active first, then newest, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<RxRow>> ProvidePrescriptionsAsync(GridDataProviderRequest<RxRow> request)
    {
        var doctors = State.Doctors.ToDictionary(doctor => doctor.Id, doctor => doctor.Name);
        var me = State.CurrentDoctor?.Id;
        var rows = State.PrescriptionsOf(PatientId).Select(prescription => new RxRow(
            prescription.Id,
            prescription.Medication,
            prescription.Dose,
            prescription.Status == HcPrescriptionStatus.Active ? "Active" : "Stopped",
            prescription.Status,
            prescription.Quantity,
            prescription.Refills,
            prescription.PrescribedAt,
            HcDisplay.Date(prescription.PrescribedAt),
            (doctors.GetValueOrDefault(prescription.DoctorId) ?? "A former doctor") + (prescription.DoctorId == me ? " (you)" : null),
            prescription.RefillRequestedAt is not null ? "Waiting for an answer"
                : prescription.RefillOutcome switch { HcRefillOutcome.Approved => "Approved", HcRefillOutcome.Denied => "Denied", _ => "None requested" }));

        return view.ApplyAsync(request, rows);
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the announcement.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        SetStatus("The prescriptions view is reset: active first, 10 a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(statusRegion);
    }

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

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private HcPrescription? Opened => State.PrescriptionsOf(PatientId).FirstOrDefault(item => item.Id == openedId);

    /// <summary>Gets or sets the patient whose prescriptions are listed.</summary>
    [Parameter, EditorRequired] public int PatientId { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One prescription as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record RxRow(
        int Id,
        string Medication,
        string Dose,
        string Status,
        HcPrescriptionStatus StatusValue,
        int Quantity,
        int Refills,
        DateTime Prescribed,
        string PrescribedText,
        string Doctor,
        string Refill);

    #endregion
}
