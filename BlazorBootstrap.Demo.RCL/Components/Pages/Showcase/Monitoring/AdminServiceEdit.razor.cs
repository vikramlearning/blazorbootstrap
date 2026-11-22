using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Adds a monitored service (<c>/new</c>) or edits one (admins), in an <see cref="EditForm" /> (rule WR-002): name (unique,
/// checked as you type, together with the ID made from it for a new service, which can't change later), kind, region (a
/// region code like us-east-1), team, description, normal p95 latency and traffic, instances (devices for a device
/// fleet; other kinds run at most <see cref="MonitoringState.MaxServiceInstances" />), and whether it is on the public
/// status page. Messages show after a field is touched or on submit, and a failed submit shows a summary that links to
/// each field; the state's refusals (Duplicate, Invalid, NotFound, NoChange) are mapped to the field or the status
/// message. Adding a service moves to its edit route and keeps the status message. For an existing service a side panel
/// shows its live status (a timer adds a sample every two seconds unless paused), its alert rules and incidents, links to
/// its console page and logs, and Delete, with the same rules as the list: the dialog names what goes with it, or says why
/// it can't be deleted and offers to hide it from the status page instead. An unknown ID shows a not-found panel.
/// </summary>
public partial class AdminServiceEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID the router moves to after adding a service: the form and its status message stay.</summary>
    private string? createdId;

    private Button? deleteButton;

    private Modal dialogModal = default!;

    private bool disposed;

    private EditContext editContext = new(new ServiceForm());

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (the Delete button after its dialog).</summary>
    private Func<ElementReference?>? focusTarget;

    private ServiceForm form = new();

    /// <summary>The service was just deleted and the list is loading: no not-found flash in between.</summary>
    private bool leaving;

    /// <summary>The route ID the form was loaded for ("" for a new service).</summary>
    private string? loadedId;

    private ShowcaseUniqueCheck? nameCheck;

    private InputText? nameInput;

    /// <summary>The name as typed, before the field's change event, so the ID preview follows each keystroke.</summary>
    private string? namePreview;

    /// <summary>The confirmed action, run once the dialog is hidden. It returns whether the Delete button is still there to take focus back.</summary>
    private Func<bool>? pendingAction;

    /// <summary>The state's answers that belong to a field (a taken name), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private MonitoringLiveTimer? timer;

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
            form.IsPublic = false;

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

    /// <summary>Whether another service has this name, or (for a new service) the ID made from it; the service being edited doesn't count.</summary>
    private Task<bool> IsNameTakenAsync(string name) => Task.FromResult(TakenBy(name) is not null);

    /// <summary>Fills the form from a service, or with a new service's starting values, in a new edit context.</summary>
    private void Load(MonitoringService? service)
    {
        form = new ServiceForm
        {
            IsNew = service is null,
            Name = service?.Name ?? string.Empty,
            Kind = service?.Kind ?? MonitoringServiceKind.Api,
            Region = service?.Region ?? string.Empty,
            Team = service?.Team ?? string.Empty,
            Description = service?.Description ?? string.Empty,
            LatencyMs = service?.BaseLatencyMs ?? 100,
            Traffic = service?.BaseRequests ?? 100,
            Instances = service?.Instances ?? 2,
            IsPublic = service?.IsPublic ?? false,
        };
        namePreview = null;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += (_, args) => serverErrors?.Clear(args.FieldIdentifier);
    }

    /// <summary>Says why a name can't be used: another service has it, or the ID made from it.</summary>
    private string NameTakenMessage(string name) => TakenBy(name) switch
    {
        { } same when string.Equals(same.Name, name.Trim(), StringComparison.OrdinalIgnoreCase) => $"Another service is already called \"{same.Name}\". Choose a different name.",
        { } other => $"The ID \"{IdFromName(name)}\" made from this name is taken by {other.Name}. Choose a different name.",
        _ => "That name is taken. Choose a different name.",
    };

    private void OnDialogHidden()
    {
        var action = pendingAction;
        pendingAction = null;

        if (action is not null && !action())
            focusStatus = true;
        else if (!leaving)
            focusTarget = () => deleteButton?.Element;
    }

    private void OnNameInput(ChangeEventArgs args) => namePreview = args.Value?.ToString();

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

    /// <summary>
    /// Saves the service once the form is valid: the name check finishes first, then every rule runs, and a failed submit
    /// shows the summary (rule WR-002). The state checks again; its answer maps to the name or the status message.
    /// </summary>
    private async Task SaveAsync()
    {
        var existing = Service;

        if (Id is not null && existing is null)
            return;

        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!await editContext.ValidateAsync() || !nameFree)
            return;

        if (existing is null && State.Services.Count >= MonitoringState.MaxServices)
        {
            Announce(false, $"This tab already holds {MonitoringState.MaxServices} services, the most it can. Delete one before adding another. Nothing was saved.");
            focusStatus = true;

            return;
        }

        var trimmedName = form.Name.Trim();
        var wasPublic = existing?.IsPublic;
        var result = State.SaveService(existing?.Id, trimmedName, form.Kind, form.Region.Trim(), form.Team.Trim(), form.Description.Trim(), form.LatencyMs!.Value, form.Traffic!.Value,
            form.Instances!.Value, form.IsPublic, out var saved);

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
                ServerError(() => form.Name, NameTakenMessage(trimmedName));
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

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(field, message);
        editContext.NotifyValidationStateChanged();
    }

    /// <summary>The service whose name (or, for a new service, whose ID) this name would clash with, or <see langword="null" />.</summary>
    private MonitoringService? TakenBy(string name)
    {
        var trimmed = name.Trim();
        var current = Service?.Id;

        if (State.Services.FirstOrDefault(service => service.Id != current && string.Equals(service.Name, trimmed, StringComparison.OrdinalIgnoreCase)) is { } same)
            return same;

        return current is null && IdFromName(trimmed) is { Length: >= 2 } id ? State.FindService(id) : null;
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

    private string IdPreview => IdFromName(namePreview ?? form.Name);

    private string InstancesLabel => form.Kind == MonitoringServiceKind.Devices ? "Devices" : "Instances";

    private static string AlertsUrl => $"{ConsoleLayout.AdminBase}/alerts";

    private string LogsUrl => $"{ConsoleLayout.Base}/logs?service={Uri.EscapeDataString(Service?.Id ?? string.Empty)}";

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private MonitoringService? Service => Id is null ? null : State.FindService(Id);

    private IReadOnlyList<MonitoringIncident> ServiceIncidents => Service is { } service ? State.Incidents.Where(incident => incident.ServiceId == service.Id).ToList() : Array.Empty<MonitoringIncident>();

    private IReadOnlyList<MonitoringAlertRule> ServiceRules => Service is { } service ? State.Rules.Where(rule => rule.ServiceId == service.Id).ToList() : Array.Empty<MonitoringAlertRule>();

    [Inject] private MonitoringState State { get; set; } = default!;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The service form's fields, with the limits of <see cref="MonitoringState.SaveService" />: required text with lengths, a
    /// region code, ranges for the normal values, and two cross-field rules (a new service's ID, and instances by kind).
    /// </summary>
    private sealed class ServiceForm : IValidatableObject
    {
        [Required(ErrorMessage = "Say in a sentence what the service does.")]
        [StringLength(MonitoringState.MaxServiceDescriptionLength, ErrorMessage = "Use 300 characters or fewer for the description.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the number of instances or devices.")]
        [Range(1, 10_000, ErrorMessage = "Use from 1 to 10,000.")]
        public int? Instances { get; set; }

        /// <summary>Whether the form adds a service (its ID is made from the name); set by the page.</summary>
        public bool IsNew { get; set; }

        public bool IsPublic { get; set; }

        public MonitoringServiceKind Kind { get; set; }

        [Required(ErrorMessage = "Enter the normal p95 latency in milliseconds, like 120.")]
        [Range(0.1, 10_000, ErrorMessage = "Use a latency from 0.1 to 10,000 ms.")]
        public double? LatencyMs { get; set; }

        [Required(ErrorMessage = "Give the service a name.")]
        [StringLength(MonitoringState.MaxServiceNameLength, ErrorMessage = "Use 60 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the region it runs in, like us-east-1.")]
        [StringLength(MonitoringState.MaxServiceTagLength, ErrorMessage = "Use 40 characters or fewer for the region.")]
        [RegularExpression(@"^\s*[a-z][a-z0-9]*(-[a-z0-9]+)+\s*$", ErrorMessage = "Use a region code like us-east-1: lower-case letters and digits joined by hyphens.")]
        public string Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the team that owns it.")]
        [StringLength(MonitoringState.MaxServiceTagLength, ErrorMessage = "Use 40 characters or fewer for the team.")]
        public string Team { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the normal traffic, like 250.")]
        [Range(1, 100_000, ErrorMessage = "Use from 1 to 100,000 a second.")]
        public double? Traffic { get; set; }

        /// <summary>Runs once the fields are filled in: a new service needs an ID from its name, and only a device fleet counts more than 500.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsNew)
            {
                var id = IdFromName(Name);

                if (id.Length < 2)
                    yield return new ValidationResult("Use at least two letters or digits in the name: the service ID is made from them.", new[] { nameof(Name) });
                else if (id == "new")
                    yield return new ValidationResult("The ID \"new\" is reserved for the Add service page. Choose a different name.", new[] { nameof(Name) });
            }

            if (Kind != MonitoringServiceKind.Devices && Instances > MonitoringState.MaxServiceInstances)
                yield return new ValidationResult(
                    string.Create(CultureInfo.InvariantCulture, $"Only a device fleet counts more than {MonitoringState.MaxServiceInstances}: use up to {MonitoringState.MaxServiceInstances} instances, or choose the kind Devices."),
                    new[] { nameof(Instances) });
        }
    }

    #endregion
}
