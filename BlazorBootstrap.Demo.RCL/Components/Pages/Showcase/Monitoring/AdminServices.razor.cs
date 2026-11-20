using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Admins' list of every monitored service, in a <see cref="Grid{TItem}" /> with sorting, the column chooser, CSV export,
/// and a saved view that can be reset: name (a link to its edit page; the whole row opens it), ID, kind, region, team,
/// whether it is on the public status page (in words), and how many alert rules, incidents, and subscribers (who follow it
/// by name) it has.
/// "Show on" or "Hide from the status page" saves the service with only that changed, in one click, and says what changes
/// publicly. Delete opens a <see cref="Modal" /> that names what goes with the service (its alert rules and failure drills);
/// when the service can't be deleted (incidents, subscribers, maintenance, or the last service) the dialog says why up
/// front and offers to hide it from the status page instead. The action runs once the dialog is hidden; focus then returns
/// to the Delete button, or to the status message when the row is gone. The shared helpers here are also used by
/// <see cref="AdminServiceEdit" />.
/// </summary>
public partial class AdminServices : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The Delete buttons by service ID, for focus after the dialog.</summary>
    private readonly Dictionary<string, Button> deleteButtons = new();

    private ElementReference? countRegion;

    private Modal dialogModal = default!;

    /// <summary>The service the dialog is about.</summary>
    private string? dialogServiceId;

    private bool focusStatus;

    private Grid<ServiceRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The confirmed action, run once the dialog is hidden. It returns whether the Delete button is still there to take focus back.</summary>
    private Func<bool>? pendingAction;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The saved view lives in the tab's session storage, so it loads once the page is interactive; then the grid shows.
        if (firstRender)
        {
            await view.LoadAsync();
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await MonitoringDisplay.FocusAsync(target());
        }

        if (focusStatus)
        {
            focusStatus = false;
            await MonitoringDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        // With no column sorted, the rows keep the site's order of services.
        view = GridViews.Get(State.SiteSlug, "manage-services", 25);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns why <see cref="MonitoringState.DeleteService" /> would refuse a service, in words; empty when it can be deleted.</summary>
    internal static IReadOnlyList<string> DeleteBlockers(MonitoringState state, MonitoringService service)
    {
        var reasons = new List<string>();

        if (state.Services.Count == 1)
            reasons.Add("It's the only service left: the console needs at least one.");

        var incidents = state.Incidents.Where(incident => incident.ServiceId == service.Id).ToList();

        if (incidents.Count > 0)
        {
            var active = incidents.Count(incident => incident.Status != MonitoringIncidentStatus.Resolved);
            reasons.Add($"It has {Plural(incidents.Count, "incident")} ({string.Join(", ", incidents.Take(4).Select(incident => $"#{incident.Id}"))}{(incidents.Count > 4 ? ", …" : null)})"
                + (active > 0 ? $", {active} still active" : null) + ". Incident history stays with the service it happened to.");
        }

        var followers = FollowerCount(state, service.Id);

        if (followers > 0)
            reasons.Add($"{Plural(followers, "subscriber")} {(followers == 1 ? "follows" : "follow")} it by name on the status page.");

        foreach (var window in state.MaintenanceWindows.Where(window => window.ServiceIds.Contains(service.Id)
                     && window.StatusAt(state.Clock.Now) is MonitoringMaintenanceStatus.Scheduled or MonitoringMaintenanceStatus.InProgress))
        {
            reasons.Add(window.StatusAt(state.Clock.Now) == MonitoringMaintenanceStatus.InProgress
                ? $"It's in the maintenance window \"{window.Title}\", which is in progress."
                : $"It's in the maintenance window \"{window.Title}\", scheduled for {MonitoringDisplay.Stamp(window.Start)}.");
        }

        return reasons;
    }

    /// <summary>Says what is deleted with a service: its alert rules (by name) and failure drills.</summary>
    internal static string DeleteConsequence(MonitoringState state, MonitoringService service)
    {
        var rules = state.Rules.Where(rule => rule.ServiceId == service.Id).ToList();
        var drills = state.Faults.Count(fault => fault.ServiceId == service.Id);
        var parts = new List<string>();

        if (rules.Count > 0)
            parts.Add($"{Plural(rules.Count, "alert rule")} ({string.Join(", ", rules.Select(rule => $"\"{rule.Name}\""))})");

        if (drills > 0)
            parts.Add(Plural(drills, "failure drill"));

        return parts.Count == 0
            ? "It has no alert rules or failure drills, so nothing else is deleted."
            : $"Its {string.Join(" and its ", parts)} {(rules.Count + drills == 1 ? "is" : "are")} deleted with it.";
    }

    /// <summary>Returns the message for a delete's outcome.</summary>
    internal static string DeleteMessage(MonitoringResult result, MonitoringService service, IReadOnlyList<string> blockers, string consequence, bool wasPublic) => result switch
    {
        MonitoringResult.Ok => $"{service.Name} is deleted. {consequence}" + (wasPublic ? " It's no longer on the status page." : string.Empty),
        MonitoringResult.WrongState => $"{service.Name} can't be deleted: {string.Join(" ", blockers)} Hide it from the status page instead. Nothing changed.",
        MonitoringResult.NotFound => $"{service.Name} was already deleted, so nothing changed.",
        MonitoringResult.NotAllowed => "Sign in as an admin to manage services. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    /// <summary>Returns the route of a service's edit page.</summary>
    internal static string EditUrl(string serviceId) => $"{ListUrl}/{Uri.EscapeDataString(serviceId)}";

    /// <summary>Counts the subscriptions that follow a service by name (an empty list follows every service and doesn't count).</summary>
    internal static int FollowerCount(MonitoringState state, string serviceId) => state.Subscriptions.Count(subscription => subscription.ServiceIds.Contains(serviceId));

    /// <summary>Formats a count with its noun, like "1 alert rule" or "3 alert rules".</summary>
    internal static string Plural(int count, string noun) => count == 1 ? $"1 {noun}" : $"{count} {noun}s";

    /// <summary>Says what changes for customers when a service is shown on or hidden from the status page.</summary>
    internal static string PublicChangeText(string name, bool isPublic) => isPublic
        ? $"{name} is now on the public status page: customers see its status and uptime, and can subscribe to its updates."
        : $"{name} is hidden from the public status page: customers no longer see its status or uptime, or can subscribe to it. Its published incidents leave the status page too (they come back if you show it again).";

    /// <summary>Saves a service with the same values and only <paramref name="isPublic" /> changed; returns the outcome in words.</summary>
    internal static (MonitoringResult Result, string Message) SetPublic(MonitoringState state, MonitoringService service, bool isPublic)
    {
        var result = state.SaveService(service.Id, service.Name, service.Kind, service.Region, service.Team, service.Description, service.BaseLatencyMs, service.BaseRequests,
            service.Instances, isPublic, out _);

        return (result, result switch
        {
            MonitoringResult.Ok => PublicChangeText(service.Name, isPublic),
            MonitoringResult.NoChange => isPublic ? $"{service.Name} is already on the status page, so nothing changed." : $"{service.Name} is already hidden from the status page, so nothing changed.",
            MonitoringResult.NotFound => $"{service.Name} no longer exists, so nothing changed.",
            MonitoringResult.NotAllowed => "Sign in as an admin to manage services. Nothing changed.",
            MonitoringResult.Duplicate => $"{service.Name} couldn't be saved: another service has the same name. Rename one of them first. Nothing changed.",
            MonitoringResult.Invalid => $"{service.Name} couldn't be saved: one of its values is out of range, or this tab's storage is full. Open it to check. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        });
    }

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Deletes the service once the dialog is gone; returns whether its row (and Delete button) is still there.</summary>
    private bool ApplyDelete(string serviceId)
    {
        if (State.FindService(serviceId) is not { } service)
        {
            Announce(false, "That service was already deleted, so nothing changed.");

            return false;
        }

        var blockers = DeleteBlockers(State, service);
        var consequence = DeleteConsequence(State, service);
        var result = State.DeleteService(serviceId);
        Announce(result == MonitoringResult.Ok, DeleteMessage(result, service, blockers, consequence, service.IsPublic));

        if (result == MonitoringResult.Ok)
            deleteButtons.Remove(serviceId);

        return result != MonitoringResult.Ok;
    }

    private bool ApplyHide(string serviceId)
    {
        if (State.FindService(serviceId) is not { } service)
        {
            Announce(false, "That service no longer exists, so nothing changed.");

            return false;
        }

        var (result, message) = SetPublic(State, service, false);
        Announce(result == MonitoringResult.Ok, message);

        return true;
    }

    private string CountText()
    {
        var total = State.Services.Count;
        var shown = State.StatusPageServices.Count;

        return $"{Plural(total, "service")}, {shown} on the status page.";
    }

    /// <summary>Runs a confirmed action now that the dialog is hidden; focus goes back to the Delete button, or to the status message when it is gone.</summary>
    private void OnDialogHidden()
    {
        var action = pendingAction;
        pendingAction = null;
        var serviceId = dialogServiceId;

        if (action is not null && !action())
        {
            focusStatus = true;

            return;
        }

        focusTarget = () => serviceId is not null && deleteButtons.TryGetValue(serviceId, out var button) ? button.Element : null;
    }

    /// <summary>A service, rule, incident, or subscription changed (here or elsewhere): the grid reads the rows again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenDeleteAsync(MonitoringService service)
    {
        dialogServiceId = service.Id;
        pendingAction = null;

        await dialogModal.ShowAsync();
    }

    private async Task ConfirmDeleteAsync()
    {
        if (pendingAction is not null || dialogServiceId is not { } serviceId)
            return;

        pendingAction = () => ApplyDelete(serviceId);
        await dialogModal.HideAsync();
    }

    private async Task ConfirmHideAsync()
    {
        if (pendingAction is not null || dialogServiceId is not { } serviceId)
            return;

        pendingAction = () => ApplyHide(serviceId);
        await dialogModal.HideAsync();
    }

    /// <summary>The rows the grid asked for, in the site's order of services unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ServiceRow>> ProvideRowsAsync(GridDataProviderRequest<ServiceRow> request) => view.ApplyAsync(request, Rows);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        gridVersion++;
        Announce(true, "The view is reset: services in the site's order, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await MonitoringDisplay.FocusAsync(countRegion);
    }

    /// <summary>Shows or hides a service on the status page in one click; the button stays and its text changes, so focus stays on it.</summary>
    private void TogglePublic(MonitoringService service)
    {
        var (result, message) = SetPublic(State, service, !service.IsPublic);
        Announce(result is MonitoringResult.Ok or MonitoringResult.NoChange, message);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private MonitoringService? DialogService => State.FindService(dialogServiceId);

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>Gets the route of the admins' services list.</summary>
    internal static string ListUrl => $"{ConsoleLayout.AdminBase}/services";

    /// <summary>Gets the route of the add-service page.</summary>
    internal static string NewUrl => $"{ListUrl}/new";

    private IReadOnlyList<ServiceRow> Rows => State.Services
        .Select(service => new ServiceRow(
            service,
            State.Rules.Count(rule => rule.ServiceId == service.Id),
            State.Incidents.Count(incident => incident.ServiceId == service.Id),
            State.Incidents.Count(incident => incident.ServiceId == service.Id && incident.Status != MonitoringIncidentStatus.Resolved),
            FollowerCount(State, service.Id)))
        .ToList();

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One row of the list: a service and its counts, with properties the CSV export can read.</summary>
    private sealed record ServiceRow(MonitoringService Service, int Rules, int Incidents, int ActiveIncidents, int Followers)
    {
        public string Id => Service.Id;

        public string Kind => MonitoringDisplay.KindLabel(Service.Kind);

        public string Name => Service.Name;

        public string OnStatusPage => Service.IsPublic ? "Yes" : "No, engineers only";

        public string Region => Service.Region;

        public string Team => Service.Team;
    }

    #endregion
}
