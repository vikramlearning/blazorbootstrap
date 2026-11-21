using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's health record: the summaries of completed visits in a <see cref="Grid{TItem}" /> (the visit's
/// date and reason, linking to the visit; the doctor and specialty, the clinic, and the summary), newest first, with paging,
/// sorting, the column chooser, and CSV export; then the conditions, allergies, and vaccinations on their chart, each with
/// a note that the doctor keeps it up to date; and who at the clinics looked at their record (<see cref="RecordsAccess" />).
/// Everything is plain text; the doctor's own visit notes are never shown.
/// </summary>
public partial class Records : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private Grid<SummaryRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

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
        view = GridViews.Get(State.SiteSlug, "my-visit-summaries", DefaultPageSize, "date", SortDirection.Descending);
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

    /// <summary>The page of summaries the grid asked for: newest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<SummaryRow>> ProvideSummariesAsync(GridDataProviderRequest<SummaryRow> request)
    {
        var rows = State.CurrentPatient is { } patient
            ? Visits(patient).Select(visit =>
            {
                var doctor = State.FindDoctor(visit.DoctorId);

                return new SummaryRow(
                    visit.Id,
                    visit.Start,
                    $"{HcDisplay.Date(visit.Start)}: {visit.Reason}",
                    doctor is null ? "Unknown doctor" : $"{doctor.Name}, {doctor.Specialty}",
                    State.FindClinic(visit.ClinicId)?.Name ?? string.Empty,
                    string.IsNullOrWhiteSpace(visit.Summary) ? "The doctor didn't write a summary for this visit." : visit.Summary);
            })
            : Enumerable.Empty<SummaryRow>();

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
        statusCount++;
        statusMessage = "The visit summaries view is reset: newest first, 10 a page, and every column shown." + (statusCount == 1 ? null : string.Create(CultureInfo.InvariantCulture, $" ({statusCount})"));
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

    /// <summary>Returns the patient's completed visits, newest first.</summary>
    private IReadOnlyList<HcAppointment> Visits(HcPatient patient) =>
        State.PatientAppointments(patient.Id).Where(item => item.Status == HcAppointmentStatus.Completed).OrderByDescending(item => item.Start).ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One visit summary as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record SummaryRow(int Id, DateTime Start, string Title, string Doctor, string Clinic, string Summary);

    #endregion
}
