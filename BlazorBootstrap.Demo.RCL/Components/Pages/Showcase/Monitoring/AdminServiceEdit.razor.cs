using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Adds a monitored service (<c>/new</c>) or edits one (admins): name (with a counter and, for a new service, the ID made
/// from it, which can't change later), kind, region, team, description, normal p95 latency and traffic, instances (devices
/// for a device fleet), and whether it is on the public status page. Each problem is shown under its field, tied to it,
/// and the first invalid field takes focus after the render that shows it; the state's refusals (Duplicate, Invalid,
/// NotFound, NoChange) are mapped to the field or the status message. Adding a service moves to its edit route and keeps
/// the status message. For an existing service a side panel shows its live status (a timer adds a sample every two
/// seconds unless paused), its alert rules and incidents, links to its console page and logs, and Delete, with the same
/// rules as the list: the dialog names what goes with it, or says why it can't be deleted and offers to hide it from the
/// status page instead. An unknown ID shows a not-found panel.
/// </summary>
public partial class AdminServiceEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID the router moves to after adding a service: the form and its status message stay.</summary>
    private string? createdId;

    private Button? deleteButton;

    private string description = string.Empty;

    private string? descriptionError;

    private ElementReference? descriptionInput;

    private Modal dialogModal = default!;

    private bool disposed;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (the first field with an error, or the Delete button).</summary>
    private Func<ElementReference?>? focusTarget;

    private string? instancesError;

    private ElementReference? instancesInput;

    private string instancesText = string.Empty;

    private bool isPublic;

    private MonitoringServiceKind kind = MonitoringServiceKind.Api;

    private string? latencyError;

    private ElementReference? latencyInput;

    private string latencyText = string.Empty;

    /// <summary>The service was just deleted and the list is loading: no not-found flash in between.</summary>
    private bool leaving;

    /// <summary>The route ID the form was loaded for ("" for a new service).</summary>
    private string? loadedId;

    private string name = string.Empty;

    private string? nameError;

    private ElementReference? nameInput;

    /// <summary>The confirmed action, run once the dialog is hidden. It returns whether the Delete button is still there to take focus back.</summary>
    private Func<bool>? pendingAction;

    private string region = string.Empty;

    private string? regionError;

    private ElementReference? regionInput;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string team = string.Empty;

    private string? teamError;

    private ElementReference? teamInput;

    private MonitoringLiveTimer? timer;

    private string? trafficError;

    private ElementReference? trafficInput;

    private string trafficText = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !disposed)
            timer = new MonitoringLiveTimer(TimeSpan.FromSeconds(MonitoringSeedData.TickSeconds), OnTickAsync);

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

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        var routeId = Id ?? string.Empty;

        if (loadedId == routeId)
            return;

        loadedId = routeId;

        // The first save of a new service moves to its edit route: the form and the status message stay as they are.
        if (createdId is not null && createdId == routeId)
        {
            createdId = null;

            return;
        }

        statusMessage = null;
        statusIsError = false;
        pendingAction = null;
        Load(Service);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops the live timer and listening to state changes.</summary>
    public void Dispose()
    {
        disposed = true;
        timer?.Dispose();
        State.Changed -= OnStateChanged;
    }

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-service route has no ID, and the router reuses this page when moving from a
    /// service to "Add service", so the previous service's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Returns the ID <see cref="MonitoringState.SaveService" /> makes from a new service's name: lower case, every run of
    /// other characters a hyphen, at most 30 characters. (The state's own helper is private; this mirrors it.)
    /// </summary>
    internal static string IdFromName(string value)
    {
        var slug = SlugPattern().Replace(value.Trim().ToLowerInvariant(), "-").Trim('-');

        return slug.Length > 30 ? slug[..30].TrimEnd('-') : slug;
    }

    #endregion

    #region Private Methods

    private static string CounterText(string value, int max)
    {
        var length = value.Trim().Length;

        return length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters: {length - max} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters");
    }

    private static string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    private static string? TextProblem(string value, int max, string missing, string what)
    {
        var length = value.Trim().Length;

        return length == 0 ? missing
            : length > max ? string.Create(CultureInfo.InvariantCulture, $"Shorten the {what} to {max} characters or fewer (now {length}).")
            : null;
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Deletes the service once the dialog is gone and goes back to the list; returns whether the Delete button is still there.</summary>
    private bool ApplyDelete()
    {
        if (Service is not { } service)
        {
            Announce(false, "This service was already deleted, so nothing changed.");

            return false;
        }

        var blockers = AdminServices.DeleteBlockers(State, service);
        var consequence = AdminServices.DeleteConsequence(State, service);
        var result = State.DeleteService(service.Id);
        var message = AdminServices.DeleteMessage(result, service, blockers, consequence, service.IsPublic);

        if (result == MonitoringResult.Ok)
        {
            leaving = true;
            Toasts.Notify(new ToastMessage(ToastType.Success, IconName.Trash3, "Service deleted", message));
            Navigation.NavigateTo(AdminServices.ListUrl);

            return true;
        }

        Announce(false, message);

        return true;
    }

    private bool ApplyHide()
    {
        if (Service is not { } service)
        {
            Announce(false, "This service no longer exists, so nothing changed.");

            return false;
        }

        var (result, message) = AdminServices.SetPublic(State, service, false);
        Announce(result == MonitoringResult.Ok, message);

        if (result is MonitoringResult.Ok or MonitoringResult.NoChange)
            isPublic = false;

        return true;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (pendingAction is not null)
            return;

        pendingAction = ApplyDelete;
        await dialogModal.HideAsync();
    }

    private async Task ConfirmHideAsync()
    {
        if (pendingAction is not null)
            return;

        pendingAction = ApplyHide;
        await dialogModal.HideAsync();
    }

    /// <summary>Fills the form from a service, or with a new service's starting values.</summary>
    private void Load(MonitoringService? service)
    {
        name = service?.Name ?? string.Empty;
        kind = service?.Kind ?? MonitoringServiceKind.Api;
        region = service?.Region ?? string.Empty;
        team = service?.Team ?? string.Empty;
        description = service?.Description ?? string.Empty;
        latencyText = service is null ? "100" : Format(service.BaseLatencyMs);
        trafficText = service is null ? "100" : Format(service.BaseRequests);
        instancesText = service is null ? "2" : service.Instances.ToString(CultureInfo.InvariantCulture);
        isPublic = service?.IsPublic ?? false;
        nameError = regionError = teamError = descriptionError = null;
        latencyError = trafficError = instancesError = null;
    }

    private void OnDialogHidden()
    {
        var action = pendingAction;
        pendingAction = null;

        if (action is not null && !action())
            focusStatus = true;
        else if (!leaving)
            focusTarget = () => deleteButton?.Element;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OnTickAsync()
    {
        // Only an existing service's side panel shows live values.
        if (disposed || State.IsPaused || Service is null)
            return;

        try
        {
            await InvokeAsync(() =>
            {
                if (disposed)
                    return;

                State.Tick();
                StateHasChanged();
            });
        }
        catch (Exception ex) when (ex is JSDisconnectedException or ObjectDisposedException or TaskCanceledException)
        {
            // The page or circuit went away between ticks.
        }
    }

    private async Task OpenDeleteAsync()
    {
        pendingAction = null;
        await dialogModal.ShowAsync();
    }

    /// <summary>Checks the form, saves it, and maps the state's answer to a field or the status message.</summary>
    private void Save()
    {
        var existing = Service;

        if (Id is not null && existing is null)
            return;

        var trimmedName = name.Trim();
        var newId = existing is null ? IdFromName(trimmedName) : null;
        var unit = MonitoringDisplay.RequestUnit(kind);
        var countWord = kind == MonitoringServiceKind.Devices ? "devices" : "instances";

        nameError = TextProblem(name, MonitoringState.MaxServiceNameLength, "Give the service a name.", "name");

        if (nameError is null && State.Services.FirstOrDefault(service => service.Id != existing?.Id && string.Equals(service.Name, trimmedName, StringComparison.OrdinalIgnoreCase)) is { } sameName)
            nameError = $"Another service is already called \"{sameName.Name}\". Choose a different name.";

        if (nameError is null && newId is not null)
        {
            nameError = newId.Length < 2 ? "Use at least two letters or digits in the name: the service ID is made from them."
                : newId == "new" ? "The ID \"new\" is reserved for the Add service page. Choose a different name."
                : State.FindService(newId) is { } sameId ? $"The ID \"{newId}\" made from this name is taken by {sameId.Name}. Choose a different name."
                : null;
        }

        regionError = TextProblem(region, MonitoringState.MaxServiceTagLength, "Enter the region it runs in, like us-east-1.", "region");
        teamError = TextProblem(team, MonitoringState.MaxServiceTagLength, "Enter the team that owns it.", "team");
        descriptionError = TextProblem(description, MonitoringState.MaxServiceDescriptionLength, "Say in a sentence what the service does.", "description");

        var latencyOk = double.TryParse(latencyText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var latency) && double.IsFinite(latency);
        latencyError = !latencyOk ? "Enter the normal p95 latency in milliseconds, like 120."
            : latency is < 0.1 or > 10_000 ? "Use a latency from 0.1 to 10,000 ms."
            : null;

        var trafficOk = double.TryParse(trafficText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var traffic) && double.IsFinite(traffic);
        trafficError = !trafficOk ? $"Enter the normal traffic in {unit}, like 250."
            : traffic is < 1 or > 100_000 ? $"Use from 1 to 100,000 {unit}."
            : null;

        var instancesOk = int.TryParse(instancesText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var instances);
        instancesError = !instancesOk ? $"Enter the number of {countWord} as a whole number, like 3."
            : instances is < 1 or > 10_000 ? $"Use from 1 to 10,000 {countWord}."
            : null;

        focusTarget = nameError is not null ? () => nameInput
            : regionError is not null ? () => regionInput
            : teamError is not null ? () => teamInput
            : descriptionError is not null ? () => descriptionInput
            : latencyError is not null ? () => latencyInput
            : trafficError is not null ? () => trafficInput
            : instancesError is not null ? () => instancesInput
            : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then save again.");

            return;
        }

        if (existing is null && State.Services.Count >= MonitoringState.MaxServices)
        {
            Announce(false, $"This tab already holds {MonitoringState.MaxServices} services, the most it can. Delete one before adding another. Nothing was saved.");
            focusStatus = true;

            return;
        }

        var wasPublic = existing?.IsPublic;
        var result = State.SaveService(existing?.Id, trimmedName, kind, region, team, description, latency, traffic, instances, isPublic, out var saved);

        switch (result)
        {
            case MonitoringResult.Ok when saved is not null:
                Load(saved);

                if (existing is null)
                {
                    Announce(true, $"{saved.Name} is added with the ID \"{saved.Id}\"."
                        + (saved.IsPublic ? " It's on the public status page now, where customers can subscribe to it." : " It's engineers-only: customers don't see it on the status page.")
                        + " Its metrics start with the next sample. Add alert rules for it on the Alert rules and drills page.");

                    // The edit route renders the same page, so the form and the status message stay; the router then focuses the heading.
                    createdId = saved.Id;
                    Navigation.NavigateTo(AdminServices.EditUrl(saved.Id), replace: true);

                    return;
                }

                Announce(true, $"{saved.Name} is saved." + (wasPublic != saved.IsPublic ? " " + AdminServices.PublicChangeText(saved.Name, saved.IsPublic) : string.Empty));

                return;

            case MonitoringResult.Duplicate:
                nameError = newId is not null && State.FindService(newId) is { } taken && !string.Equals(taken.Name, trimmedName, StringComparison.OrdinalIgnoreCase)
                    ? $"The ID \"{newId}\" made from this name is taken by {taken.Name}. Choose a different name."
                    : $"Another service is already called \"{trimmedName}\". Choose a different name.";
                focusTarget = () => nameInput;
                Announce(false, "That name is taken. Nothing was saved.");

                return;

            case MonitoringResult.NoChange:
                Announce(true, "Nothing changed: the service is already saved like this.");

                return;

            case MonitoringResult.NotFound:
                Announce(false, "This service was deleted, so the changes couldn't be saved.");
                focusStatus = true;

                return;

            case MonitoringResult.NotAllowed:
                Announce(false, "Sign in as an admin to manage services. Nothing was saved.");
                focusStatus = true;

                return;

            case MonitoringResult.Invalid:
                Announce(false, existing is null && State.Services.Count >= MonitoringState.MaxServices
                    ? $"This tab already holds {MonitoringState.MaxServices} services, the most it can. Delete one before adding another. Nothing was saved."
                    : "The service couldn't be saved: a value is missing or out of range, or this tab's storage is full. Nothing was saved.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                return;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Manage services", Href = AdminServices.ListUrl },
        new() { Text = Service?.Name ?? "Add service", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the service ID from the route, or <see langword="null" /> on the add-service route.</summary>
    [Parameter] public string? Id { get; set; }

    private string IdPreview => IdFromName(name);

    private string InstancesLabel => kind == MonitoringServiceKind.Devices ? "Devices" : "Instances";

    private static string AlertsUrl => $"{ConsoleLayout.AdminBase}/alerts";

    private string LogsUrl => $"{ConsoleLayout.Base}/logs?service={Uri.EscapeDataString(Service?.Id ?? string.Empty)}";

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private MonitoringService? Service => Id is null ? null : State.FindService(Id);

    private IReadOnlyList<MonitoringIncident> ServiceIncidents => Service is { } service ? State.Incidents.Where(incident => incident.ServiceId == service.Id).ToList() : Array.Empty<MonitoringIncident>();

    private IReadOnlyList<MonitoringAlertRule> ServiceRules => Service is { } service ? State.Rules.Where(rule => rule.ServiceId == service.Id).ToList() : Array.Empty<MonitoringAlertRule>();

    [Inject] private MonitoringState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion
}
