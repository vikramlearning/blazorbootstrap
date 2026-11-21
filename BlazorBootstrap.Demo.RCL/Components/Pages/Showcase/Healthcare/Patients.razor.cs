using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// The patients of the clinics for staff (<see cref="HcState.Patients" />) in a <see cref="Grid{TItem}" />: name (the row
/// opens the chart for a doctor and the registration details for the front desk), record number, date of birth and age,
/// phone, email, home clinic, health plan, the next booked visit and the last completed one, and when they registered, with
/// paging, sorting, the column chooser, and CSV export. A search narrows by name, record number, phone, email, or date of
/// birth, and a select by home clinic; the grid's view is saved for the visit and can be reset. "Register a patient" opens
/// the registration form. Names never go into the URL.
/// </summary>
public partial class Patients : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private string clinicFilter = string.Empty;

    private ElementReference? countRegion;

    private Grid<PatientRow>? grid;

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
        view = GridViews.Get(State.SiteSlug, "patients", DefaultPageSize, "name", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Plural(int count) => count == 1 ? "1 patient" : string.Create(CultureInfo.InvariantCulture, $"{count:#,0} patients");

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = clinicFilter = string.Empty;
        await FiltersChangedAsync();
        await HcDisplay.FocusAsync(countRegion);
    }

    private string CountText(int total)
    {
        var shown = Shown.Count;

        return shown == total ? $"{Plural(total)}." : $"Showing {shown.ToString("#,0", CultureInfo.InvariantCulture)} of {Plural(total)}.";
    }

    /// <summary>A filter changed: back to the first page of the filtered patients.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Returns the patients that pass the filters, as grid rows, by name (the state's order).</summary>
    private IEnumerable<PatientRow> Matching()
    {
        var today = State.Clock.Today;
        var now = State.Clock.Now;
        var term = search.Trim();
        var number = term.TrimStart('#').Trim();
        var birthDate = DateTime.TryParseExact(term, new[] { "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed.Date : (DateTime?)null;
        var clinics = State.Clinics.ToDictionary(clinic => clinic.Id, clinic => clinic.Name);
        var doctors = State.Doctors.ToDictionary(doctor => doctor.Id, doctor => doctor.Name);
        var visits = State.AllAppointments.ToLookup(appointment => appointment.PatientId);

        foreach (var patient in State.Patients)
        {
            if (clinicFilter.Length > 0 && patient.ClinicId != clinicFilter)
                continue;

            if (term.Length > 0 && !(patient.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || patient.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (patient.Phone.Length > 0 && patient.Phone.Contains(term, StringComparison.OrdinalIgnoreCase)) || patient.Mrn.Contains(number, StringComparison.OrdinalIgnoreCase)
                    || (birthDate is { } dob && patient.DateOfBirth?.Date == dob)))
                continue;

            var mine = visits[patient.Id];
            var next = mine.Where(item => item.Status is HcAppointmentStatus.Booked or HcAppointmentStatus.CheckedIn or HcAppointmentStatus.InVisit && item.Start >= today).MinBy(item => item.Start);
            var last = mine.Where(item => item.Status == HcAppointmentStatus.Completed && item.Start <= now).MaxBy(item => item.Start);
            var age = HcDisplay.Age(patient.DateOfBirth, today);

            yield return new PatientRow(
                patient.Id,
                patient.Name,
                patient.Mrn,
                patient.DateOfBirth,
                patient.DateOfBirth is { } born ? HcDisplay.Date(born) : "Not recorded",
                age,
                patient.Phone.Length > 0 ? patient.Phone : "Not recorded",
                patient.Email,
                clinics.GetValueOrDefault(patient.ClinicId) ?? "Not chosen",
                State.FindPayer(patient.InsurancePayerId)?.Name ?? "Self-pay",
                next?.Start,
                next is null ? "None booked" : $"{HcDisplay.When(next.Start)} with {doctors.GetValueOrDefault(next.DoctorId)}",
                last?.Start,
                last is null ? "No visits yet" : $"{HcDisplay.Date(last.Start)} with {doctors.GetValueOrDefault(last.DoctorId)}",
                patient.RegisteredAt);
        }
    }

    private async Task OnClinicChangedAsync(ChangeEventArgs args)
    {
        clinicFilter = State.FindClinic(args.Value?.ToString())?.Id ?? string.Empty;
        await FiltersChangedAsync();
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

    /// <summary>Another screen or a change here changed the state: the grid reads the patients again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of patients the grid asked for: by name, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<PatientRow>> ProvidePatientsAsync(GridDataProviderRequest<PatientRow> request) => view.ApplyAsync(request, Shown);

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
        SetStatus("The view is reset: by name, 25 patients a page, and every column shown.");
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

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || clinicFilter.Length > 0;

    private IReadOnlyList<PatientRow> Shown => Matching().ToList();

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One patient as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record PatientRow(
        int Id,
        string Name,
        string Mrn,
        DateTime? DateOfBirth,
        string BirthText,
        int? Age,
        string Phone,
        string Email,
        string Clinic,
        string Plan,
        DateTime? Next,
        string NextText,
        DateTime? Last,
        string LastText,
        DateTime Registered)
    {
        /// <summary>Gets the age in words.</summary>
        public string AgeText => Age is { } years ? years.ToString(CultureInfo.InvariantCulture) : "Not recorded";

        /// <summary>Gets the registration day in words.</summary>
        public string RegisteredText => HcDisplay.Date(Registered);
    }

    #endregion
}
