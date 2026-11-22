using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Billing's claims work queue: counts of what needs doing (visits to code, claims to submit, with the plans, denied, and
/// appeals due within two weeks), the open claims by age since the visit (0–30, 31–60, 61–90, and over 90 days) with what
/// they bill, the visits to code in a <see cref="Grid{TItem}" /> (the row opens the coding dialog, <see cref="HcCodingForm" />,
/// which creates the draft claim), and every claim in a <see cref="Grid{TItem}" /> (number, linking to the claim; patient,
/// plan, visit day, status in words, billed, paid, and patient amounts, age, codes, and version) with paging, sorting, the
/// column chooser, and CSV export. Filters narrow by status (open by default), plan, age, and a search by claim number,
/// patient, or record number; both grids' views are saved for the visit and can be reset. Selected drafts and corrected
/// claims are submitted together (<see cref="HcState.BulkSubmitClaims" />). Outcomes are announced in the status region.
/// </summary>
public partial class Claims : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The route of the claims list.</summary>
    public const string ListUrl = HcSeedData.StaffBase + "/claims";

    /// <summary>The route of the denials list.</summary>
    public const string DenialsUrl = HcSeedData.StaffBase + "/claims/denials";

    private const string ClosedFilter = "closed";

    private const int DefaultPageSize = 25;

    private const string DeniedFilter = "denied";

    private const string OpenFilter = "open";

    private const string PaidFilter = "paid";

    private const string ToSubmitFilter = "submit";

    private const string WithPlanFilter = "plan";

    /// <summary>The age buckets of the open claims, by days since the visit.</summary>
    private static readonly string[] Buckets = { "0–30", "31–60", "61–90", "Over 90" };

    /// <summary>The rows' buttons of the visits to code, by visit ID, so focus returns to one when its dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> visitOpeners = new();

    private string ageFilter = string.Empty;

    private Grid<VisitRow>? codeGrid;

    private int codeGridVersion;

    private int codePageSize = 5;

    private ShowcaseGridView codeView = default!;

    private bool codeViewLoaded;

    private ElementReference? codingHeading;

    private Modal codingModal = default!;

    /// <summary>The visit the coding dialog is about.</summary>
    private int codingVisitId;

    private ElementReference? countRegion;

    private bool focusStatus;

    private Grid<ClaimRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string payerFilter = string.Empty;

    /// <summary>The coding dialog's outcome, announced once it is hidden.</summary>
    private string? pendingMessage;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The claims selected in the grid (on its current page).</summary>
    private List<ClaimRow> selected = new();

    private int statusCount;

    private string statusFilter = OpenFilter;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    /// <summary>The visits' start times by ID, or <see langword="null" /> until needed after a change.</summary>
    private Dictionary<int, DateTime>? visitStarts;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved views once the page is interactive (they live in the tab's session storage), then shows the grids.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            await codeView.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            codePageSize = (await codeView.GetSettingsAsync()).PageSize;
            viewLoaded = codeViewLoaded = true;
            StateHasChanged();
        }

        if (focusStatus)
        {
            focusStatus = false;
            await HcDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "claims", DefaultPageSize, "visit", SortDirection.Ascending);
        codeView = GridViews.Get(State.SiteSlug, "visits-to-code", 5, "when", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Returns the age bucket of a claim by days since its visit.</summary>
    private string BucketOf(HcClaim claim) => AgeDays(claim) switch
    {
        <= 30 => Buckets[0],
        <= 60 => Buckets[1],
        <= 90 => Buckets[2],
        _ => Buckets[3],
    };

    private int AgeDays(HcClaim claim) => (State.Clock.Today - VisitStart(claim).Date).Days;

    /// <summary>Returns a claim's visit day, from a lookup built once per change of the state (claims are many).</summary>
    private DateTime VisitStart(HcClaim claim)
    {
        visitStarts ??= State.AllAppointments.ToDictionary(item => item.Id, item => item.Start);

        return visitStarts.TryGetValue(claim.AppointmentId, out var start) ? start : claim.CreatedAt;
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = payerFilter = ageFilter = string.Empty;
        statusFilter = OpenFilter;
        await FiltersChangedAsync();
        await HcDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<ClaimRow>();
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    private string CountText(int total)
    {
        var shown = Shown.Count;

        return string.Create(CultureInfo.InvariantCulture, $"Showing {shown:#,0} of {HcDisplay.Plural(total, "claim", "claims")}.");
    }

    /// <summary>A filter changed: back to the first page of the filtered claims.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private static bool IsOpen(HcClaim claim) => claim.Status is not (HcClaimStatus.Paid or HcClaimStatus.PartlyPaid or HcClaimStatus.WrittenOff);

    private async Task OnAgeChangedAsync(ChangeEventArgs args)
    {
        ageFilter = Buckets.Contains(args.Value?.ToString()) ? args.Value!.ToString()! : string.Empty;
        await FiltersChangedAsync();
    }

    private async Task OnCodedAsync(string message)
    {
        pendingMessage = message;
        await codingModal.HideAsync();
    }

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: after coding it moves to the announcement, otherwise back to the row.</summary>
    private async Task OnCodingHidden()
    {
        if (pendingMessage is { } message)
        {
            pendingMessage = null;
            SetStatus(message, false);
            focusStatus = true;
            await RefreshGridsAsync();
        }
        else if (visitOpeners.TryGetValue(codingVisitId, out var opener))
        {
            await HcDisplay.FocusAsync(opener);
        }
    }

    private Task OnCodingShownAsync() => HcDisplay.FocusAsync(codingHeading);

    private async Task OnPayerChangedAsync(ChangeEventArgs args)
    {
        payerFilter = State.FindPayer(args.Value?.ToString())?.Id ?? string.Empty;
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

    private void OnSelectionChanged(HashSet<ClaimRow> items) => selected = items.ToList();

    /// <summary>Another screen or a change here changed the state: the grids read the claims again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        visitStarts = null;
        await RefreshGridsAsync();
        StateHasChanged();
    });

    private async Task OnStatusChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        statusFilter = value is OpenFilter or ToSubmitFilter or WithPlanFilter or DeniedFilter or PaidFilter or ClosedFilter ? value : string.Empty;
        await FiltersChangedAsync();
    }

    private async Task OpenCodingAsync(int visitId)
    {
        codingVisitId = visitId;
        pendingMessage = null;
        await codingModal.ShowAsync();
    }

    /// <summary>The page of claims the grid asked for: oldest visit first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ClaimRow>> ProvideClaimsAsync(GridDataProviderRequest<ClaimRow> request) => view.ApplyAsync(request, Shown);

    /// <summary>The page of visits to code the grid asked for: oldest first (closest to the filing deadline), unless a column is sorted.</summary>
    private Task<GridDataProviderResult<VisitRow>> ProvideVisitsAsync(GridDataProviderRequest<VisitRow> request)
    {
        var rows = State.VisitsToCode.Select(visit =>
        {
            var patient = State.FindPatient(visit.PatientId);
            var fileBy = visit.Start.Date.AddDays(HcSeedData.FilingDays);

            return new VisitRow(visit.Id, visit.Start, HcDisplay.When(visit.Start), patient?.Name ?? "A former patient", State.FindDoctor(visit.DoctorId)?.Name ?? "A former doctor", visit.Reason,
                State.FindPayer(patient?.InsurancePayerId)?.Name ?? string.Empty, fileBy, HcDisplay.Date(fileBy));
        });

        return codeView.ApplyAsync(request, rows);
    }

    private async Task RefreshGridsAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();

        if (codeGrid is not null)
            await codeGrid.RefreshDataAsync();
    }

    /// <summary>Forgets both saved views and builds the grids again with their defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        await codeView.ResetAsync();
        pageSize = DefaultPageSize;
        codePageSize = 5;
        selected = new List<ClaimRow>();
        gridVersion++;
        codeGridVersion++;
        SetStatus("The views are reset: oldest first, 25 claims and 5 visits a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(countRegion);
    }

    private string RowClass(ClaimRow row) => selected.Any(item => item.Id == row.Id) ? "hc-row-selected" : string.Empty;

    private async Task SaveCodeSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            codePageSize = settings.PageSize;

        await codeView.SaveSettingsAsync(settings);
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

    /// <summary>Submits the selected drafts and corrected claims in one <see cref="HcState.BulkSubmitClaims" /> call.</summary>
    private async Task SubmitSelectedAsync()
    {
        var ids = selected.Where(row => row.StatusValue is HcClaimStatus.Draft or HcClaimStatus.Corrected).Select(row => row.Id).ToList();
        var outcome = State.BulkSubmitClaims(ids);

        SetStatus(outcome.Result switch
        {
            HcResult.Ok => $"Submitted {HcDisplay.Plural(outcome.Changed, "claim", "claims")} to the plans."
                + (outcome.Unchanged > 0 ? $" {HcDisplay.Plural(outcome.Unchanged, "claim was", "claims were")} left as they were: past the filing deadline, or already sent." : string.Empty),
            HcResult.NoChange => "No claims were sent: they're past the filing deadline, or already sent.",
            HcResult.NotAllowed => "Only billing can submit claims. Sign in again; nothing was sent.",
            _ => "Those claims couldn't be sent. Nothing changed.",
        }, outcome.Result is not (HcResult.Ok or HcResult.NoChange));

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<ClaimRow>();
        focusStatus = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private HcAppointment? CodingVisit => State.VisitsToCode.FirstOrDefault(item => item.Id == codingVisitId);

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter != OpenFilter || payerFilter.Length > 0 || ageFilter.Length > 0;

    /// <summary>The claims that pass the filters, as grid rows.</summary>
    private IReadOnlyList<ClaimRow> Shown
    {
        get
        {
            var term = search.Trim();
            var patients = State.Patients.ToDictionary(patient => patient.Id);
            var payers = State.Payers.ToDictionary(payer => payer.Id, payer => payer.Name);

            return State.Claims
                .Where(claim => statusFilter switch
                {
                    OpenFilter => IsOpen(claim),
                    ToSubmitFilter => claim.Status is HcClaimStatus.Draft or HcClaimStatus.Corrected,
                    WithPlanFilter => claim.Status is HcClaimStatus.Submitted or HcClaimStatus.Appealed,
                    DeniedFilter => claim.Status == HcClaimStatus.Denied,
                    PaidFilter => claim.Status is HcClaimStatus.Paid or HcClaimStatus.PartlyPaid,
                    ClosedFilter => claim.Status == HcClaimStatus.WrittenOff,
                    _ => true,
                })
                .Where(claim => payerFilter.Length == 0 || claim.PayerId == payerFilter)
                .Where(claim => ageFilter.Length == 0 || BucketOf(claim) == ageFilter)
                .Select(claim => (Claim: claim, Patient: patients.GetValueOrDefault(claim.PatientId)))
                .Where(pair => term.Length == 0 || pair.Claim.Number.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (pair.Patient is not null && (pair.Patient.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || pair.Patient.Mrn.Contains(term, StringComparison.OrdinalIgnoreCase))))
                .Select(pair =>
                {
                    var claim = pair.Claim;
                    var answer = claim.Responses.LastOrDefault();
                    var visit = VisitStart(claim);
                    var closed = claim.Status is HcClaimStatus.Paid or HcClaimStatus.PartlyPaid;

                    return new ClaimRow(
                        claim.Id,
                        claim.Number,
                        pair.Patient?.Name ?? "A former patient",
                        payers.GetValueOrDefault(claim.PayerId) ?? claim.PayerId,
                        visit,
                        HcDisplay.Date(visit),
                        ClaimDetail.StatusLabel(claim),
                        claim.Status,
                        claim.BilledCents,
                        closed ? answer?.PaidCents ?? 0 : 0,
                        closed ? answer?.PatientCents ?? 0 : claim.BilledToPatient ? claim.BilledCents : 0,
                        (State.Clock.Today - visit.Date).Days,
                        BucketOf(claim),
                        claim.Status == HcClaimStatus.Denied && answer is not null ? string.Join(", ", answer.DenialCodes) : string.Join(", ", claim.ProcedureCodes),
                        claim.Versions.Count);
                })
                .ToList();
        }
    }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One claim as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record ClaimRow(
        int Id,
        string Number,
        string Patient,
        string Payer,
        DateTime Visit,
        string VisitText,
        string Status,
        HcClaimStatus StatusValue,
        int Billed,
        int Paid,
        int PatientOwes,
        int AgeDays,
        string Bucket,
        string Codes,
        int Version)
    {
        /// <summary>Gets the billed amount in words.</summary>
        public string BilledText => HcDisplay.Money(Billed);

        /// <summary>Gets the plan's payment in words.</summary>
        public string PaidText => HcDisplay.Money(Paid);

        /// <summary>Gets the patient's share in words.</summary>
        public string PatientText => HcDisplay.Money(PatientOwes);
    }

    /// <summary>One visit to code as the grid shows and exports it.</summary>
    private sealed record VisitRow(int Id, DateTime Start, string WhenText, string Patient, string Doctor, string Reason, string Payer, DateTime FileBy, string FileByText);

    #endregion
}
