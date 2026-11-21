using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The console's incident queue: a summary (open, acknowledged, critical active), filters by status (active by default),
/// severity, service, and "on the status page", each option with its count, and a <see cref="Grid{TItem}" /> of the
/// matching incidents (number, title linking to the incident, service, severity and status in words, the escalation
/// policy's paging, assignee, start, duration, and the public stage) with paging, sorting, the column chooser, and CSV
/// export. A quick filter shows the incidents waiting for acknowledgement (<c>?paging=1</c> starts with it on). The grid's view (page size,
/// sort, columns) is saved for the visit and can be reset. Rows can be selected with the grid's checkboxes and changed
/// together from a bulk bar: assign, severity (with a reason), and acknowledge or resolve (an incident on the status page is
/// resolved on its own page, with a public message). "Declare incident" opens a validated <see cref="Modal" /> form
/// (rule WR-002); the incident is created after the dialog closes, and the page then goes to it. The filters can start
/// from the <c>status</c> (<c>active</c>, <c>resolved</c>, <c>all</c>) and <c>service</c> query values.
/// </summary>
public partial class Incidents : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string AllValue = "all";

    private const int DefaultPageSize = 25;

    private static readonly (string Value, string Label)[] statusOptions = { ("active", "Active"), ("resolved", "Resolved"), (AllValue, "All") };

    private string bulkAssignee = string.Empty;

    private string? bulkError;

    private int bulkErrors;

    private string bulkReason = string.Empty;

    private string bulkSeverity = string.Empty;

    private string bulkStatus = string.Empty;

    private ElementReference? countRegion;

    private Button? declareButton;

    private EditContext declareContext = new(new DeclareForm());

    private DeclareForm declareForm = new();

    private Modal declareModal = default!;

    private bool focusStatus;

    private Grid<MonitoringIncident>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private (string? Status, string? Service, string? Paging) loadedQuery = ("\0", "\0", "\0");

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The validated form, waiting for the dialog to close before the incident is created (it changes the page).</summary>
    private DeclareRequest? pendingDeclare;

    private bool publicOnly;

    /// <summary>Whether only incidents an escalation policy is paging for are shown.</summary>
    private bool waitingOnly;

    /// <summary>The incidents selected in the grid (on its current page).</summary>
    private List<MonitoringIncident> selected = new();

    private InputSelect<string>? serviceInput;

    private string serviceFilter = AllValue;

    private string severityFilter = AllValue;

    private int statusCount;

    private string statusFilter = "active";

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

        if (focusStatus)
        {
            focusStatus = false;
            await MonitoringDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "incidents", DefaultPageSize, "started", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (loadedQuery == (StatusQuery, ServiceQuery, PagingQuery))
            return;

        loadedQuery = (StatusQuery, ServiceQuery, PagingQuery);
        statusFilter = statusOptions.Any(option => option.Value == StatusQuery) ? StatusQuery! : "active";
        waitingOnly = PagingQuery == "1";
        serviceFilter = State.FindService(ServiceQuery) is { } service ? service.Id : AllValue;
        await FiltersChangedAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns what an impact does to the metrics, for the declare form and the incident page.</summary>
    internal static string ImpactHelp(MonitoringImpact impact) => impact switch
    {
        MonitoringImpact.Errors => "About 6% of requests fail; traffic and latency stay normal.",
        MonitoringImpact.Latency => "Requests take about 2.6 times as long, and CPU runs hot.",
        _ => "Around 45% of requests fail, traffic drops to 40%, and latency quadruples.",
    };

    /// <summary>Returns an impact in words.</summary>
    internal static string ImpactLabel(MonitoringImpact impact) => impact switch
    {
        MonitoringImpact.Errors => "Elevated errors",
        MonitoringImpact.Latency => "Slow responses",
        _ => "Outage",
    };

    /// <summary>Returns what a severity means, for the declare form and the incident page.</summary>
    internal static string SeverityHelp(MonitoringSeverity severity) => severity switch
    {
        MonitoringSeverity.Critical => "Customers can't use a core feature. Shown as a major outage once public.",
        MonitoringSeverity.Major => "A feature is slow or partly failing. Shown as a partial outage once public.",
        _ => "Little or no customer impact. Shown as degraded performance once public.",
    };

    #endregion

    #region Private Methods

    private static string Count(int count, string singular, string plural) =>
        count == 1 ? $"1 {singular}" : string.Create(CultureInfo.InvariantCulture, $"{count} {plural}");

    /// <summary>Returns the message for an incident that couldn't be declared.</summary>
    private static string DeclareRefusedText(MonitoringResult result) => result switch
    {
        MonitoringResult.NotAllowed => "Sign in as an engineer to declare incidents. Nothing was declared.",
        MonitoringResult.NotFound => "That service isn't monitored any more, so nothing was declared.",
        MonitoringResult.Invalid => "This tab can't hold another incident (its storage is full), so nothing was declared.",
        MonitoringResult.WrongState => "The incident couldn't be declared right now. Nothing was declared.",
        MonitoringResult.NoChange => "Nothing changed: the incident wasn't declared.",
        MonitoringResult.Duplicate => "That incident already exists, so nothing was declared.",
        _ => "That didn't work, and nothing was declared. Try again.",
    };

    private static bool MatchesStatus(MonitoringIncident incident, string status) => status switch
    {
        "active" => incident.Status != MonitoringIncidentStatus.Resolved,
        "resolved" => incident.Status == MonitoringIncidentStatus.Resolved,
        _ => true,
    };

    /// <summary>
    /// Applies the bulk bar's choices to the selected incidents in one <see cref="MonitoringState.BulkUpdate" /> call, then
    /// refreshes the grid and clears the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync()
    {
        var assignee = State.FindEngineer(bulkAssignee)?.Id;
        MonitoringSeverity? severity = Enum.TryParse<MonitoringSeverity>(bulkSeverity, out var parsedSeverity) && Enum.IsDefined(parsedSeverity) ? parsedSeverity : null;
        MonitoringIncidentStatus? status = Enum.TryParse<MonitoringIncidentStatus>(bulkStatus, out var parsedStatus) && Enum.IsDefined(parsedStatus) ? parsedStatus : null;
        var reason = bulkReason.Trim();

        var problem = assignee is null && severity is null && status is null ? "Choose an assignee, a severity, or a status to apply."
            : severity is not null && reason.Length == 0 ? "Give a reason for the severity change: it goes on each incident's timeline."
            : reason.Length > MonitoringState.MaxUpdateLength ? string.Create(CultureInfo.InvariantCulture, $"Shorten the reason to {MonitoringState.MaxUpdateLength} characters.")
            : null;

        if (problem is not null)
        {
            bulkErrors++;
            bulkError = bulkErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkErrors})");

            return;
        }

        bulkError = null;
        bulkErrors = 0;

        var outcome = State.BulkUpdate(selected.Select(incident => incident.Id).ToList(), assignee, severity, status, reason);
        var parts = new List<string>();

        parts.Add(outcome.Result switch
        {
            MonitoringResult.Ok => $"Updated {Count(outcome.Changed, "incident", "incidents")}.",
            MonitoringResult.NoChange => "No incidents changed: the selected incidents already had those values, or are resolved.",
            MonitoringResult.NotAllowed => "Sign in as an engineer to change incidents. Nothing changed.",
            MonitoringResult.NotFound => "That engineer is no longer on the rotation. Nothing changed.",
            _ => "Those changes couldn't be saved (this tab's storage may be full). Nothing changed.",
        });

        if (outcome.Result == MonitoringResult.Ok && outcome.Unchanged > 0)
            parts.Add($"{Count(outcome.Unchanged, "incident already had", "incidents already had")} those values or {(outcome.Unchanged == 1 ? "is" : "are")} resolved.");

        if (outcome.PublishedNotResolved > 0)
            parts.Add($"{Count(outcome.PublishedNotResolved, "incident is", "incidents are")} on the status page, so not resolved here: resolve {(outcome.PublishedNotResolved == 1 ? "it" : "each")} on its page with a public \"Resolved\" message.");

        SetStatus(string.Join(" ", parts), outcome.Result is not (MonitoringResult.Ok or MonitoringResult.NoChange));

        bulkAssignee = bulkSeverity = bulkStatus = bulkReason = string.Empty;

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<MonitoringIncident>();
        focusStatus = true;
        StateHasChanged();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        statusFilter = "active";
        severityFilter = AllValue;
        serviceFilter = AllValue;
        publicOnly = false;
        waitingOnly = false;
        await FiltersChangedAsync();
        await MonitoringDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<MonitoringIncident>();
        bulkError = null;
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    /// <summary>A filter changed: back to the first page of the filtered incidents.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Returns the incidents that pass every filter except the ones switched off, for each option's count.</summary>
    private IEnumerable<MonitoringIncident> Matching(bool byStatus = true, bool bySeverity = true, bool byService = true, bool byPublic = true, bool byWaiting = true) =>
        State.Incidents.Where(incident =>
            (!byStatus || MatchesStatus(incident, statusFilter))
            && (!bySeverity || severityFilter == AllValue || incident.Severity.ToString() == severityFilter)
            && (!byService || serviceFilter == AllValue || incident.ServiceId == serviceFilter)
            && (!byPublic || !publicOnly || incident.IsPublished)
            && (!byWaiting || !waitingOnly || incident.Paging is { IsActive: true }));

    /// <summary>Sorts paging: still paging first (the later the step, the more urgent), then nobody answered, stopped, and not paged.</summary>
    private static int PagingRank(MonitoringIncident incident) => incident.Paging switch
    {
        { IsActive: true } paging => 10 - (paging.Pages.LastOrDefault()?.Step ?? 1),
        { StopReason: MonitoringPagingStop.NobodyAnswered } => 20,
        not null => 30,
        _ => 40,
    };

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. A validated form is declared now that the dialog is
    /// gone: the page then goes to the new incident, or a refusal is announced in the status message. Cancel or Escape
    /// returns focus to the Declare button.
    /// </summary>
    private async Task OnDeclareHiddenAsync()
    {
        if (pendingDeclare is not { } request)
        {
            await MonitoringDisplay.FocusAsync(declareButton?.Element);
            return;
        }

        pendingDeclare = null;
        var result = State.DeclareIncident(request.ServiceId, request.Title, request.Summary, request.Severity, request.Impact, out var incident);

        if (result == MonitoringResult.Ok && incident is not null)
        {
            Navigation.NavigateTo($"{MonitoringDisplay.IncidentUrl(incident.Id)}?declared=1");
            return;
        }

        SetStatus(DeclareRefusedText(result), true);
        focusStatus = true;
        StateHasChanged();
    }

    private Task OnDeclareShownAsync() => MonitoringDisplay.FocusAsync(serviceInput?.Element);

    private void OnSelectionChanged(HashSet<MonitoringIncident> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            bulkError = null;
    }

    /// <summary>Another screen, a rule firing, or a bulk change changed the state: the grid reads the incidents again (the selection keeps the ones still shown).</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenDeclareAsync()
    {
        pendingDeclare = null;
        declareForm = new DeclareForm { ServiceId = serviceFilter == AllValue ? string.Empty : serviceFilter };
        declareContext = new EditContext(declareForm);

        await declareModal.ShowAsync();
    }

    /// <summary>The page of incidents the grid asked for, from the filtered list, newest first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<MonitoringIncident>> ProvideIncidentsAsync(GridDataProviderRequest<MonitoringIncident> request) => view.ApplyAsync(request, Shown);

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
        selected = new List<MonitoringIncident>();
        gridVersion++;
        SetStatus("The view is reset: newest first, 25 incidents a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await MonitoringDisplay.FocusAsync(countRegion);
    }

    private string RowClass(MonitoringIncident incident) => selected.Contains(incident) ? "mw-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private string ServiceName(string serviceId) => State.FindService(serviceId)?.Name ?? serviceId;

    private int ServiceCount(string? serviceId) => Matching(byService: false).Count(incident => serviceId is null || incident.ServiceId == serviceId);

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private async Task SetStatusFilterAsync(string value)
    {
        statusFilter = value;
        await FiltersChangedAsync();
    }

    private int SeverityCount(MonitoringSeverity? severity) => Matching(bySeverity: false).Count(incident => severity is null || incident.Severity == severity);

    private int StatusCount(string status) => Matching(byStatus: false).Count(incident => MatchesStatus(incident, status));

    /// <summary>Declares once the form is valid (a failed submit shows the summary and moves focus to it, rule WR-002).</summary>
    private async Task SubmitDeclareAsync()
    {
        if (pendingDeclare is not null || !declareContext.Validate())
            return;

        if (State.FindService(declareForm.ServiceId) is null)
            return;

        pendingDeclare = new DeclareRequest(declareForm.ServiceId, declareForm.Title.Trim(), declareForm.Summary.Trim(), declareForm.Severity, declareForm.Impact);

        await declareModal.HideAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private int AcknowledgedCount => State.Incidents.Count(incident => incident.Status == MonitoringIncidentStatus.Acknowledged);

    private int CriticalActiveCount => State.Incidents.Count(incident => incident.Status != MonitoringIncidentStatus.Resolved && incident.Severity == MonitoringSeverity.Critical);

    private string EmptyText =>
        waitingOnly && statusFilter == "active" && !publicOnly && severityFilter == AllValue && serviceFilter == AllValue
            ? "Nothing is waiting for acknowledgement: no escalation policy is paging anyone."
            : statusFilter == "active" && !publicOnly && severityFilter == AllValue && serviceFilter == AllValue
                ? "No active incidents. All services are operating normally."
                : "No incidents match these filters.";

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => statusFilter != "active" || severityFilter != AllValue || serviceFilter != AllValue || publicOnly || waitingOnly;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private int OpenCount => State.Incidents.Count(incident => incident.Status == MonitoringIncidentStatus.Open);

    private int PublicCount => Matching(byPublic: false).Count(incident => incident.IsPublished);

    /// <summary>Gets or sets <c>1</c> to start with only the incidents waiting for acknowledgement, like <c>?paging=1</c>.</summary>
    [SupplyParameterFromQuery(Name = "paging")] public string? PagingQuery { get; set; }

    private int WaitingCount => State.Incidents.Count(incident => incident.Paging is { IsActive: true });

    private int WaitingMatchCount => Matching(byWaiting: false).Count(incident => incident.Paging is { IsActive: true });

    /// <summary>Gets or sets the service filter from the query string, like <c>?service=payments</c>.</summary>
    [SupplyParameterFromQuery(Name = "service")] public string? ServiceQuery { get; set; }

    private IReadOnlyList<MonitoringIncident> Shown => Matching().ToList();

    [Inject] private MonitoringState State { get; set; } = default!;

    /// <summary>Gets or sets the status filter from the query string: <c>active</c> (the default), <c>resolved</c>, or <c>all</c>.</summary>
    [SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>The declare dialog's fields, with the limits of <see cref="MonitoringState.DeclareIncident" />.</summary>
    private sealed class DeclareForm
    {
        public MonitoringImpact Impact { get; set; } = MonitoringImpact.Errors;

        [Required(ErrorMessage = "Choose the affected service.")]
        public string ServiceId { get; set; } = string.Empty;

        public MonitoringSeverity Severity { get; set; } = MonitoringSeverity.Major;

        [Required(ErrorMessage = "Say what you see and who it affects.")]
        [StringLength(MonitoringState.MaxIncidentSummaryLength, ErrorMessage = "Use 300 characters or fewer for the summary.")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Give the incident a title.")]
        [StringLength(MonitoringState.MaxIncidentTitleLength, ErrorMessage = "Use 120 characters or fewer for the title.")]
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>A validated declare form.</summary>
    private sealed record DeclareRequest(string ServiceId, string Title, string Summary, MonitoringSeverity Severity, MonitoringImpact Impact);

    #endregion
}
