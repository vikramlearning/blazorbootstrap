using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The Visits tab of a patient's chart: every visit with any doctor in a <see cref="Grid{TItem}" /> (when, linking to the
/// visit; number, doctor, clinic, visit type, reason, and status in words), newest first, with paging, sorting, the column
/// chooser, and CSV export. The notes stay on the visit page, out of the list and the export. The grid's view is saved
/// for the visit and can be reset.
/// </summary>
public partial class PatientChartVisits : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private Grid<VisitRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private int statusCount;

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
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "chart-visits", DefaultPageSize, "when", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of visits the grid asked for: newest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<VisitRow>> ProvideVisitsAsync(GridDataProviderRequest<VisitRow> request)
    {
        var doctors = State.Doctors.ToDictionary(doctor => doctor.Id, doctor => doctor.Name);
        var clinics = State.Clinics.ToDictionary(clinic => clinic.Id, clinic => clinic.Name);
        var me = State.CurrentDoctor?.Id;
        var rows = State.PatientAppointments(PatientId).Select(visit => new VisitRow(
            visit.Id,
            visit.Start,
            HcDisplay.When(visit.Start),
            (doctors.GetValueOrDefault(visit.DoctorId) ?? "A former doctor") + (visit.DoctorId == me ? " (you)" : null),
            clinics.GetValueOrDefault(visit.ClinicId) ?? "A former clinic",
            HcDisplay.VisitTypeText(visit.VisitType),
            visit.Reason,
            HcDisplay.StatusLabel(visit.Status),
            visit.Status));

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
        SetStatus("The visits view is reset: newest first, 10 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(statusRegion);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>Gets or sets the patient whose visits are listed.</summary>
    [Parameter, EditorRequired] public int PatientId { get; set; }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One visit as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record VisitRow(int Id, DateTime Start, string WhenText, string Doctor, string Clinic, string VisitType, string Reason, string Status, HcAppointmentStatus StatusValue);

    #endregion
}
