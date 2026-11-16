using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The console's incidents: a summary (open, acknowledged, critical active), filters by status (active by default),
/// severity, service, and "on the status page", each option with its count, and a table of the matching incidents
/// (number and title linking to the incident, service, severity and status in words, assignee, start, duration, and the
/// public stage). "Declare incident" opens a <see cref="Modal" /> form (service, title, summary, severity, impact on the
/// metrics); the incident is created after the dialog closes, and the page then goes to it. The filters can start from
/// the <c>status</c> (<c>active</c>, <c>resolved</c>, <c>all</c>) and <c>service</c> query values.
/// </summary>
public partial class Incidents : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string AllValue = "all";

    private static readonly (string Value, string Label)[] statusOptions = { ("active", "Active"), ("resolved", "Resolved"), (AllValue, "All") };

    private Button? declareButton;

    private MonitoringImpact declareImpact = MonitoringImpact.Errors;

    private Modal declareModal = default!;

    /// <summary>The validated form, waiting for the dialog to close before the incident is created (it changes the page).</summary>
    private DeclareRequest? pendingDeclare;

    private string declareServiceId = string.Empty;

    private MonitoringSeverity declareSeverity = MonitoringSeverity.Major;

    private string declareSummary = string.Empty;

    private string declareTitle = string.Empty;

    private bool focusStatus;

    private (string? Status, string? Service) loadedQuery = ("\0", "\0");

    /// <summary>Returns the field to focus after the next render: the dialog's first field, or the first one with an error (WCAG 3.3.1).</summary>
    private Func<ElementReference?>? pendingFocus;

    private bool publicOnly;

    private ElementReference? serviceInput;

    private string? serviceError;

    private string serviceFilter = AllValue;

    private string severityFilter = AllValue;

    private string statusFilter = "active";

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? summaryError;

    private ElementReference? summaryInput;

    private string? titleError;

    private ElementReference? titleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (pendingFocus is { } field)
        {
            pendingFocus = null;
            await MonitoringDisplay.FocusAsync(field());
        }

        if (focusStatus)
        {
            focusStatus = false;
            await MonitoringDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (loadedQuery == (StatusQuery, ServiceQuery))
            return;

        loadedQuery = (StatusQuery, ServiceQuery);
        statusFilter = statusOptions.Any(option => option.Value == StatusQuery) ? StatusQuery! : "active";
        serviceFilter = State.FindService(ServiceQuery) is { } service ? service.Id : AllValue;
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

    private void ClearFilters()
    {
        statusFilter = "active";
        severityFilter = AllValue;
        serviceFilter = AllValue;
        publicOnly = false;
    }

    /// <summary>Returns the incidents that pass every filter except the ones switched off, for each option's count.</summary>
    private IEnumerable<MonitoringIncident> Matching(bool byStatus = true, bool bySeverity = true, bool byService = true, bool byPublic = true) =>
        State.Incidents.Where(incident =>
            (!byStatus || MatchesStatus(incident, statusFilter))
            && (!bySeverity || severityFilter == AllValue || incident.Severity.ToString() == severityFilter)
            && (!byService || serviceFilter == AllValue || incident.ServiceId == serviceFilter)
            && (!byPublic || !publicOnly || incident.IsPublished));

    private static bool MatchesStatus(MonitoringIncident incident, string status) => status switch
    {
        "active" => incident.Status != MonitoringIncidentStatus.Resolved,
        "resolved" => incident.Status == MonitoringIncidentStatus.Resolved,
        _ => true,
    };

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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

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

        statusIsError = true;
        statusMessage = DeclareRefusedText(result);
        focusStatus = true;
        StateHasChanged();
    }

    private void OnDeclareShown() => pendingFocus = () => serviceInput;

    private async Task OpenDeclareAsync()
    {
        pendingDeclare = null;
        declareServiceId = serviceFilter == AllValue ? string.Empty : serviceFilter;
        declareTitle = string.Empty;
        declareSummary = string.Empty;
        declareSeverity = MonitoringSeverity.Major;
        declareImpact = MonitoringImpact.Errors;
        serviceError = titleError = summaryError = null;

        await declareModal.ShowAsync();
    }

    private int ServiceCount(string? serviceId) => Matching(byService: false).Count(incident => serviceId is null || incident.ServiceId == serviceId);

    private void SetStatusFilter(string value) => statusFilter = value;

    private int SeverityCount(MonitoringSeverity? severity) => Matching(bySeverity: false).Count(incident => severity is null || incident.Severity == severity);

    private int StatusCount(string status) => Matching(byStatus: false).Count(incident => MatchesStatus(incident, status));

    /// <summary>Checks the form; the first invalid field gets focus after the render that shows its error.</summary>
    private async Task SubmitDeclareAsync()
    {
        if (pendingDeclare is not null)
            return;

        var title = declareTitle.Trim();
        var summary = declareSummary.Trim();

        serviceError = State.FindService(declareServiceId) is null ? "Choose the affected service." : null;
        titleError = title.Length == 0 ? "Give the incident a title."
            : title.Length > MonitoringState.MaxIncidentTitleLength ? $"Shorten the title to {MonitoringState.MaxIncidentTitleLength} characters." : null;
        summaryError = summary.Length == 0 ? "Say what you see and who it affects."
            : summary.Length > MonitoringState.MaxIncidentSummaryLength ? $"Shorten the summary to {MonitoringState.MaxIncidentSummaryLength} characters." : null;

        pendingFocus = serviceError is not null ? () => serviceInput : titleError is not null ? () => titleInput : summaryError is not null ? () => summaryInput : null;

        if (pendingFocus is not null)
            return;

        pendingDeclare = new DeclareRequest(declareServiceId, title, summary, declareSeverity, declareImpact);

        await declareModal.HideAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private int AcknowledgedCount => State.Incidents.Count(incident => incident.Status == MonitoringIncidentStatus.Acknowledged);

    private int CriticalActiveCount => State.Incidents.Count(incident => incident.Status != MonitoringIncidentStatus.Resolved && incident.Severity == MonitoringSeverity.Critical);

    private bool IsFiltered => statusFilter != "active" || severityFilter != AllValue || serviceFilter != AllValue || publicOnly;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private int OpenCount => State.Incidents.Count(incident => incident.Status == MonitoringIncidentStatus.Open);

    private int PublicCount => Matching(byPublic: false).Count(incident => incident.IsPublished);

    /// <summary>Gets or sets the service filter from the query string, like <c>?service=payments</c>.</summary>
    [SupplyParameterFromQuery(Name = "service")] public string? ServiceQuery { get; set; }

    private IReadOnlyList<MonitoringIncident> Shown => Matching().ToList();

    [Inject] private MonitoringState State { get; set; } = default!;

    /// <summary>Gets or sets the status filter from the query string: <c>active</c> (the default), <c>resolved</c>, or <c>all</c>.</summary>
    [SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }

    #endregion

    #region Nested Types

    /// <summary>A validated declare form.</summary>
    private sealed record DeclareRequest(string ServiceId, string Title, string Summary, MonitoringSeverity Severity, MonitoringImpact Impact);

    #endregion
}
