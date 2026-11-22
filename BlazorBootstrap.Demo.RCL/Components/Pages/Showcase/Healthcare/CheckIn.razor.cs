using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Check-in (front desk): today's appointments at every clinic in a <see cref="Grid{TItem}" /> (time, patient, record
/// number, doctor, clinic, reason, visit type, and status in words with the arrival time, "not here yet", or a warning when
/// the time is now outside the doctor's hours), by time, with paging, sorting, the column chooser, and CSV export, under
/// counts for booked, checked in, with the doctor, done, and closed. Filters narrow by patient, clinic, and status; the
/// grid's view is saved for the visit and can be reset. "Check in" works on a booked appointment and announces the outcome
/// ("Checked in Sofia Alvarez for 09:00 with Dr. Samuel Okoye"); "No-show" appears once a booked appointment's start has
/// passed and is confirmed in a <see cref="Modal" />, applied after the dialog is hidden. Selected appointments can be
/// checked in or marked as no-shows together (<see cref="HcState.BulkCheckIn" />, <see cref="HcState.BulkMarkNoShow" />).
/// The front desk has no appointment page, so the row opens a dialog with the details and Check in.
/// </summary>
public partial class CheckIn : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string ClosedFilter = "closed";

    private const int DefaultPageSize = 25;

    private const string HereFilter = "here";

    private const string SeenFilter = "seen";

    private const string WaitingFilter = "waiting";

    /// <summary>The rows' patient buttons, by appointment ID, so focus returns to one when its dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    private string clinicFilter = string.Empty;

    private ElementReference? countRegion;

    private ElementReference? detailsClose;

    private Modal detailsModal = default!;

    private ElementReference? dialogDismiss;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private Grid<ArrivalRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The appointments the no-show dialog is about.</summary>
    private List<int> noShowIds = new();

    private Modal noShowModal = default!;

    /// <summary>The appointment the details dialog is about.</summary>
    private int openedId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>Whether Check in was pressed in the details dialog, so it runs once the dialog is hidden.</summary>
    private bool pendingCheckIn;

    /// <summary>Whether the no-show was confirmed, so it runs once the dialog is hidden.</summary>
    private bool pendingNoShow;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The appointments selected in the grid (on its current page).</summary>
    private List<ArrivalRow> selected = new();

    private int statusCount;

    private string statusFilter = string.Empty;

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
        view = GridViews.Get(State.SiteSlug, "check-in", DefaultPageSize, "time", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string SummaryText(int shown, int total)
    {
        var count = total == 1 ? "1 appointment" : FormattableString.Invariant($"{total} appointments");

        return shown == total ? $"{count} today." : FormattableString.Invariant($"Showing {shown} of {count} today.");
    }

    /// <summary>Asks before marking the selected appointments past their time as no-shows.</summary>
    private Task AskBulkNoShowAsync() => AskNoShowAsync(selected.Where(row => row.CanNoShow).Select(row => row.Id).ToList());

    private async Task AskNoShowAsync(IReadOnlyList<int> ids)
    {
        noShowIds = ids.ToList();
        pendingNoShow = false;
        StateHasChanged();

        await noShowModal.ShowAsync();
    }

    /// <summary>Checks the selected booked patients in with one <see cref="HcState.BulkCheckIn" /> call.</summary>
    private async Task BulkCheckInAsync()
    {
        var ids = selected.Where(row => row.StatusValue == HcAppointmentStatus.Booked).Select(row => row.Id).ToList();
        var outcome = State.BulkCheckIn(ids);

        await FinishBulkAsync(outcome, $"Checked in {HcDisplay.Plural(outcome.Changed, "patient", "patients")}: they're in the waiting room, and their doctors can see them.",
            "No one was checked in: the selected patients are checked in already, or their appointments are closed.", "already checked in or closed");
    }

    /// <summary>Checks one patient in from the row; the button disappears, so focus moves to the announced outcome.</summary>
    private void CheckInOne(int appointmentId)
    {
        if (State.FindAppointment(appointmentId) is not { } appointment)
        {
            SetStatus("That appointment isn't on today's list any more, so nothing changed.", true);
            focusTarget = () => statusRegion;

            return;
        }

        var result = State.CheckIn(appointment.Id);
        SetStatus(Calendar.CheckInMessage(result, State, appointment), result != HcResult.Ok);
        focusTarget = () => statusRegion;
    }

    private async Task CheckInFromDialogAsync(int appointmentId)
    {
        openedId = appointmentId;
        pendingCheckIn = true;
        await detailsModal.HideAsync();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = clinicFilter = statusFilter = string.Empty;
        await FiltersChangedAsync();
        await HcDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<ArrivalRow>();
        SetStatus("Selection cleared.", false);
        focusTarget = () => statusRegion;
    }

    private Task ConfirmNoShowAsync()
    {
        if (noShowIds.Count == 0 || pendingNoShow)
            return Task.CompletedTask;

        pendingNoShow = true;

        return noShowModal.HideAsync();
    }

    /// <summary>Returns the counts above the list: booked (not arrived), checked in, with the doctor, done, and closed.</summary>
    private static IReadOnlyList<StatusCount> Counts(IReadOnlyList<HcAppointment> appointments)
    {
        int Count(params HcAppointmentStatus[] statuses) => appointments.Count(item => statuses.Contains(item.Status));

        return new List<StatusCount>
        {
            new(Count(HcAppointmentStatus.Booked), "Booked, not arrived"),
            new(Count(HcAppointmentStatus.CheckedIn), "In the waiting room"),
            new(Count(HcAppointmentStatus.InVisit), "With the doctor"),
            new(Count(HcAppointmentStatus.Completed), "Done"),
            new(Count(HcAppointmentStatus.NoShow, HcAppointmentStatus.Cancelled), "No-show or cancelled"),
        };
    }

    /// <summary>A filter changed: back to the first page of the filtered arrivals.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Announces a bulk change, refreshes the grid, and clears the selection; the bar closes, so focus moves to the announcement.</summary>
    private async Task FinishBulkAsync(HcBulkOutcome outcome, string changedText, string noChangeText, string unchangedReason)
    {
        var message = outcome.Result switch
        {
            HcResult.Ok => changedText + (outcome.Unchanged > 0 ? $" {HcDisplay.Plural(outcome.Unchanged, "appointment was", "appointments were")} left as they were: {unchangedReason}." : string.Empty),
            HcResult.NoChange => noChangeText,
            HcResult.NotAllowed => "Only the front desk can check patients in. Sign in again; nothing changed.",
            _ => "Those changes couldn't be saved. Nothing changed.",
        };

        SetStatus(message, outcome.Result is not (HcResult.Ok or HcResult.NoChange));

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<ArrivalRow>();
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    private bool Matches(HcAppointment appointment, IReadOnlyDictionary<int, HcPatient> patients)
    {
        var text = search.Trim();
        var patient = patients.GetValueOrDefault(appointment.PatientId);

        return (clinicFilter.Length == 0 || appointment.ClinicId == clinicFilter)
            && (statusFilter switch
            {
                WaitingFilter => appointment.Status == HcAppointmentStatus.Booked,
                HereFilter => appointment.Status == HcAppointmentStatus.CheckedIn,
                SeenFilter => appointment.Status is HcAppointmentStatus.InVisit or HcAppointmentStatus.Completed,
                ClosedFilter => appointment.Status is HcAppointmentStatus.NoShow or HcAppointmentStatus.Cancelled,
                _ => true,
            })
            && (text.Length == 0 || (patient is not null && (patient.Name.Contains(text, StringComparison.OrdinalIgnoreCase) || patient.Mrn.Contains(text, StringComparison.OrdinalIgnoreCase))));
    }

    private async Task OnClinicChangedAsync(ChangeEventArgs args)
    {
        clinicFilter = State.FindClinic(args.Value?.ToString())?.Id ?? string.Empty;
        await FiltersChangedAsync();
    }

    private Task OnDetailsShownAsync() => HcDisplay.FocusAsync(detailsClose);

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: a Check in pressed in it runs now; otherwise focus returns to the row.</summary>
    private void OnDetailsHidden()
    {
        var id = openedId;

        if (!pendingCheckIn)
        {
            focusTarget = () => openers.TryGetValue(id, out var opener) ? opener : statusRegion;

            return;
        }

        pendingCheckIn = false;
        CheckInOne(id);
    }

    private Task OnNoShowShownAsync() => HcDisplay.FocusAsync(dialogDismiss);

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus. A confirmed no-show is marked now that the dialog is
    /// gone and focus moves to its outcome; Go back or Escape returns focus to the status line.
    /// </summary>
    private void OnNoShowHidden()
    {
        var ids = noShowIds;
        var confirmed = pendingNoShow;
        pendingNoShow = false;
        noShowIds = new List<int>();

        if (!confirmed)
        {
            focusTarget = () => statusRegion;

            return;
        }

        if (ids.Count == 1 && State.FindAppointment(ids[0]) is { } appointment)
        {
            var result = State.MarkNoShow(appointment.Id);
            SetStatus(Calendar.NoShowMessage(result, State, appointment), result != HcResult.Ok);
            focusTarget = () => statusRegion;

            return;
        }

        _ = InvokeAsync(() => FinishBulkAsync(State.BulkMarkNoShow(ids), $"Marked {HcDisplay.Plural(ids.Count, "patient", "patients")} as no-shows.",
            "No one was marked: the selected appointments haven't started yet, or they aren't booked any more.", "not started yet or not booked"));
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

    private void OnSelectionChanged(HashSet<ArrivalRow> items) => selected = items.ToList();

    /// <summary>Another screen or a change here changed the state: the grid reads today's appointments again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusFilterChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        statusFilter = value is WaitingFilter or HereFilter or SeenFilter or ClosedFilter ? value : string.Empty;
        await FiltersChangedAsync();
    }

    private async Task OpenAsync(int appointmentId)
    {
        openedId = appointmentId;
        pendingCheckIn = false;
        await detailsModal.ShowAsync();
    }

    private string PatientName(int patientId) => State.FindPatient(patientId)?.Name ?? "Unknown patient";

    /// <summary>The page of arrivals the grid asked for: by time, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ArrivalRow>> ProvideArrivalsAsync(GridDataProviderRequest<ArrivalRow> request) => view.ApplyAsync(request, Shown);

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
        selected = new List<ArrivalRow>();
        gridVersion++;
        SetStatus("The view is reset: by time, 25 appointments a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(countRegion);
    }

    private string RowClass(ArrivalRow row) => selected.Any(item => item.Id == row.Id) ? "hc-row-selected" : string.Empty;

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

    /// <summary>Returns today's appointments at every clinic, by time, then doctor.</summary>
    private IReadOnlyList<HcAppointment> TodayAppointments() =>
        State.AppointmentsOn(State.Clock.Today).OrderBy(item => item.Start).ThenBy(item => State.FindDoctor(item.DoctorId)?.Name, StringComparer.Ordinal).ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || clinicFilter.Length > 0 || statusFilter.Length > 0;

    private HcAppointment? Opened => State.FindAppointment(openedId);

    /// <summary>Today's appointments that pass the filters, as grid rows.</summary>
    private IReadOnlyList<ArrivalRow> Shown
    {
        get
        {
            var now = State.Clock.Now;
            var patients = State.Patients.ToDictionary(patient => patient.Id);
            var doctors = State.Doctors.ToDictionary(doctor => doctor.Id, doctor => doctor.Name);
            var clinics = State.Clinics.ToDictionary(clinic => clinic.Id, clinic => clinic.Name);

            return TodayAppointments().Where(item => Matches(item, patients)).Select(appointment =>
            {
                var patient = patients.GetValueOrDefault(appointment.PatientId);
                var late = appointment.Status == HcAppointmentStatus.Booked && appointment.Start <= now;

                return new ArrivalRow(
                    appointment.Id,
                    appointment.Start,
                    HcDisplay.Time(appointment.Start),
                    patient?.Name ?? "Unknown patient",
                    patient?.Mrn ?? string.Empty,
                    doctors.GetValueOrDefault(appointment.DoctorId) ?? "A former doctor",
                    clinics.GetValueOrDefault(appointment.ClinicId) ?? "A former clinic",
                    appointment.Reason,
                    HcDisplay.VisitTypeText(appointment.VisitType),
                    HcDisplay.StatusLabel(appointment.Status),
                    appointment.Status,
                    appointment.Status == HcAppointmentStatus.CheckedIn && appointment.CheckedInAt is { } arrived ? $"Arrived {HcDisplay.Time(arrived)}" : late ? "Not here yet" : string.Empty,
                    appointment.Status == HcAppointmentStatus.Booked && State.IsOutsideHours(appointment) ? Calendar.OutsideText(State, appointment) : string.Empty,
                    late);
            }).ToList();
        }
    }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One of today's appointments as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record ArrivalRow(
        int Id,
        DateTime Start,
        string Time,
        string Patient,
        string Mrn,
        string Doctor,
        string Clinic,
        string Reason,
        string VisitType,
        string Status,
        HcAppointmentStatus StatusValue,
        string Note,
        string Warning,
        bool CanNoShow)
    {
        /// <summary>Gets the status with its note, for the export.</summary>
        public string StatusText => Note.Length > 0 ? $"{Status} ({Note})" : Status;
    }

    /// <summary>One of the counts above the list.</summary>
    private sealed record StatusCount(int Number, string Label);

    #endregion
}
