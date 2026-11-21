using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The signed-in patient's appointments in a <see cref="Grid{TItem}" />: when (the row opens the appointment, to change or
/// cancel it, or for the visit summary), the doctor and specialty, the clinic, the reason, the visit type, the status in
/// words, and the appointment number, newest first, with paging, sorting, the column chooser, and CSV export. A filter
/// shows upcoming ones (booked and not over, checked in, or with the doctor) or past ones, and a search narrows by doctor,
/// reason, or number; the grid's view is saved for the visit and can be reset. An empty list links to "Find a doctor".
/// </summary>
public partial class Appointments : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private const string PastFilter = "past";

    private const string UpcomingFilter = "upcoming";

    private ElementReference? countRegion;

    private Grid<AppointmentRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    private string whenFilter = string.Empty;

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
        view = GridViews.Get(State.SiteSlug, "my-appointments", DefaultPageSize, "when", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>
    /// Returns whether an appointment is still ahead: booked and not over (it counts until <see cref="HcState.SlotMinutes" />
    /// after its start, like <see cref="HcState.UpcomingOf" />), or checked in or with the doctor.
    /// </summary>
    internal static bool IsUpcoming(HcAppointment appointment, DateTime now) =>
        appointment.Status is HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit
        || (appointment.Status == HcAppointmentStatus.Booked && appointment.Start >= now.AddMinutes(-HcState.SlotMinutes));

    #endregion

    #region Private Methods

    private IReadOnlyList<HcAppointment> All() => State.CurrentPatient is { } patient ? State.PatientAppointments(patient.Id) : Array.Empty<HcAppointment>();

    private string CountText(int total)
    {
        var shown = Shown.Count;
        var count = total == 1 ? "1 appointment" : string.Create(CultureInfo.InvariantCulture, $"{total} appointments");

        return shown == total ? $"{count}." : string.Create(CultureInfo.InvariantCulture, $"Showing {shown} of {count}.");
    }

    /// <summary>A filter changed: back to the first page of the filtered appointments.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
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

    /// <summary>Another screen or a change here changed the state: the grid reads the appointments again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnWhenChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        whenFilter = value is UpcomingFilter or PastFilter ? value : string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>The page of appointments the grid asked for: newest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<AppointmentRow>> ProvideAppointmentsAsync(GridDataProviderRequest<AppointmentRow> request) => view.ApplyAsync(request, Shown);

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
        gridVersion++;
        SetStatus("The view is reset: newest first, 10 appointments a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(countRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private static string FindDoctorUrl => $"{HcLayout.Base}/doctors";

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>The patient's appointments that pass the filters, as grid rows.</summary>
    private IReadOnlyList<AppointmentRow> Shown
    {
        get
        {
            var now = State.Clock.Now;
            var term = search.Trim();
            var number = term.TrimStart('#').Trim();

            return All()
                .Where(item => whenFilter switch { UpcomingFilter => IsUpcoming(item, now), PastFilter => !IsUpcoming(item, now), _ => true })
                .Select(item => (Appointment: item, Doctor: State.FindDoctor(item.DoctorId)))
                .Where(pair => term.Length == 0 || pair.Appointment.Reason.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (pair.Doctor?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) || pair.Appointment.Id.ToString(CultureInfo.InvariantCulture) == number)
                .Select(pair => new AppointmentRow(
                    pair.Appointment.Id,
                    pair.Appointment.Start,
                    HcDisplay.When(pair.Appointment.Start),
                    pair.Doctor?.Name ?? "Unknown doctor",
                    pair.Doctor?.Specialty ?? string.Empty,
                    State.FindClinic(pair.Appointment.ClinicId)?.Name ?? string.Empty,
                    pair.Appointment.Reason,
                    HcDisplay.VisitTypeText(pair.Appointment.VisitType),
                    HcDisplay.StatusLabel(pair.Appointment.Status),
                    pair.Appointment.Status))
                .ToList();
        }
    }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One appointment as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record AppointmentRow(
        int Id,
        DateTime Start,
        string WhenText,
        string Doctor,
        string Specialty,
        string Clinic,
        string Reason,
        string VisitType,
        string Status,
        HcAppointmentStatus StatusValue);

    #endregion
}
