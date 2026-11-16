using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The admins' alert rules and failure drills. A drill form (service, impact with what it does to the metrics, length)
/// shows before starting which enabled rules watch the service, whether each would post on the status page and how likely
/// it is to fire, and warns when the service already has an active incident or maintenance running (rules don't fire) or a
/// drill running. A drill that could make a rule post on the status page is confirmed in a <see cref="Modal" /> that names
/// the public message and the subscribers it is emailed to. Running drills show their time left from the clock and a Stop
/// button; recent drills show how long they ran and the incident they opened. The rules table shows each rule's
/// condition, severity, state, "posts on the status page", the current value ("Breaching" in words), and when it last
/// fired with a link to the incident; Edit opens the rule form (also for a new rule) in a <see cref="Modal" />, Enable or
/// Disable toggles it, and Delete is confirmed (its incidents stay). Saving and deleting run after the dialog is hidden.
/// Rules are evaluated on every tick, so the page runs the live timer (with <see cref="LivePauseButton" />); a rule that
/// fires while the page is open, and a drill that ends on its own, are announced once in the status region.
/// </summary>
public partial class AdminAlerts : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The drill lengths offered, in seconds (kept within the state's limits).</summary>
    private static readonly int[] drillLengthChoices = { 30, 60, 120, 300, 900 };

    /// <summary>How many finished drills the history shows.</summary>
    private const int PastDrillCount = 5;

    /// <summary>What the confirmation dialog is confirming.</summary>
    private ConfirmKind confirm;

    private Modal confirmModal = default!;

    /// <summary>The button that opened the confirmation, for focus after Cancel or when it is still there after the action.</summary>
    private Button? confirmOpener;

    /// <summary>The rule the delete confirmation is about.</summary>
    private int confirmRuleId;

    private readonly Dictionary<int, Button> deleteButtons = new();

    private bool disposed;

    private MonitoringImpact drillImpact = MonitoringImpact.Errors;

    private int drillSeconds = 60;

    private string drillServiceId = string.Empty;

    private string? drillServiceError;

    private ElementReference? drillServiceInput;

    private readonly Dictionary<int, Button> editButtons = new();

    /// <summary>The rule the form edits; 0 for a new rule.</summary>
    private int editingRuleId;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (a field with an error, or the button that opened a dialog).</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The incidents already on the page, so a rule firing is announced once.</summary>
    private readonly HashSet<int> knownIncidentIds = new();

    private Button? newRuleButton;

    /// <summary>The confirmed action, run once the dialog is hidden (it changes the page under the dialog). It returns whether the opener is still there to take focus back.</summary>
    private Func<bool>? pendingAction;

    /// <summary>The drill waiting for the "it could go public" confirmation.</summary>
    private DrillRequest? pendingDrill;

    /// <summary>The validated rule, saved once the form's dialog is hidden.</summary>
    private RuleRequest? pendingRule;

    private bool ruleAutoPublish;

    private string? ruleDurationError;

    private ElementReference? ruleDurationInput;

    private bool ruleEnabled = true;

    private int ruleForSeconds = 20;

    private string? ruleFormError;

    private string? ruleMessageError;

    private ElementReference? ruleMessageInput;

    private MonitoringAlertMetric ruleMetric = MonitoringAlertMetric.ErrorRate;

    private string? ruleMetricError;

    private ElementReference? ruleMetricInput;

    private Modal ruleModal = default!;

    private string ruleName = string.Empty;

    private string? ruleNameError;

    private ElementReference? ruleNameInput;

    /// <summary>The button that opened the rule form.</summary>
    private Button? ruleOpener;

    private string rulePublicMessage = string.Empty;

    private string ruleServiceId = string.Empty;

    private string? ruleServiceError;

    private ElementReference? ruleServiceInput;

    private MonitoringSeverity ruleSeverity = MonitoringSeverity.Major;

    private double? ruleThreshold = 2;

    private string? ruleThresholdError;

    private ElementReference? ruleThresholdInput;

    /// <summary>The drills running when last checked, so one that ends on its own is announced once.</summary>
    private readonly HashSet<int> runningFaultIds = new();

    private Button? startDrillButton;

    private bool statusIsError;

    private List<StatusNote> statusNotes = new();

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

    protected override void OnInitialized()
    {
        // Catch up with the clock on arrival; what fires now shows in the table rather than as an announcement.
        if (!State.IsPaused)
            State.Tick();

        knownIncidentIds.UnionWith(State.Incidents.Select(incident => incident.Id));
        runningFaultIds.UnionWith(RunningFaults.Select(fault => fault.Id));
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops the live timer and the state subscription, so leaving the page stops all updates.</summary>
    public void Dispose()
    {
        disposed = true;
        timer?.Dispose();
        State.Changed -= OnStateChanged;
    }

    #endregion

    #region Private Methods

    /// <summary>Formats a drill length like "30 s", "1 min", or "2 min 30 s".</summary>
    private static string LengthLabel(int seconds) =>
        seconds < 60 ? FormattableString.Invariant($"{seconds} s")
        : seconds % 60 == 0 ? FormattableString.Invariant($"{seconds / 60} min")
        : FormattableString.Invariant($"{seconds / 60} min {seconds % 60} s");

    /// <summary>Formats the time a drill has left, like "45 s left" or "4 min 12 s left".</summary>
    private static string Left(TimeSpan span)
    {
        var seconds = Math.Max(0, (int)Math.Ceiling(span.TotalSeconds));

        return seconds < 60 ? FormattableString.Invariant($"{seconds} s left") : FormattableString.Invariant($"{seconds / 60} min {seconds % 60} s left");
    }

    /// <summary>Returns a rule's condition in words, like "Error rate above 2% for 20 s".</summary>
    private static string Condition(MonitoringAlertRule rule) =>
        $"{MonitoringDisplay.MetricLabel(rule.Metric)} above {MonitoringDisplay.MetricValue(rule.Metric, rule.Threshold)} for {rule.ForSeconds} s";

    /// <summary>The threshold a new rule starts from for a metric.</summary>
    private static double DefaultThreshold(MonitoringAlertMetric metric) => metric switch
    {
        MonitoringAlertMetric.Latency => 500,
        MonitoringAlertMetric.Cpu => 90,
        _ => 2,
    };

    private static string MetricHelp(MonitoringAlertMetric metric) => metric switch
    {
        MonitoringAlertMetric.ErrorRate => "The share of requests that fail, in percent.",
        MonitoringAlertMetric.Latency => "How long the slowest 5% of requests take, in milliseconds.",
        _ => "The average CPU use across the service's instances, in percent.",
    };

    /// <summary>The typical value of a metric while a failure with this impact runs (the same bend the state applies to the samples).</summary>
    private static double ExpectedDuring(MonitoringService service, MonitoringImpact impact, MonitoringAlertMetric metric) => (metric, impact) switch
    {
        (MonitoringAlertMetric.ErrorRate, MonitoringImpact.Errors) => 6.5,
        (MonitoringAlertMetric.ErrorRate, MonitoringImpact.Outage) => 47,
        (MonitoringAlertMetric.ErrorRate, _) => service.BaseErrorRate,
        (MonitoringAlertMetric.Latency, MonitoringImpact.Latency) => service.BaseLatencyMs * 2.6,
        (MonitoringAlertMetric.Latency, MonitoringImpact.Outage) => service.BaseLatencyMs * 4,
        (MonitoringAlertMetric.Latency, _) => service.BaseLatencyMs,
        (_, MonitoringImpact.Latency) => Math.Min(100, service.BaseCpu * 1.5),
        _ => service.BaseCpu,
    };

    /// <summary>Says whether a rule is likely to fire during a drill, and why not when it can't.</summary>
    private static string Outlook(MonitoringAlertRule rule, MonitoringService service, MonitoringImpact impact, int seconds, bool silenced)
    {
        if (silenced)
            return "Won't fire while the service has an active incident or maintenance.";

        if (seconds < rule.ForSeconds)
            return $"Won't fire: the drill is shorter than the rule's {rule.ForSeconds} s window.";

        var expected = ExpectedDuring(service, impact, rule.Metric);
        var around = MonitoringDisplay.MetricValue(rule.Metric, expected);

        return expected > rule.Threshold * 1.15 ? $"Likely to fire about {rule.ForSeconds} s after the start ({MonitoringDisplay.MetricLabel(rule.Metric).ToLowerInvariant()} around {around})."
            : expected > rule.Threshold * 0.85 ? $"Might fire: {MonitoringDisplay.MetricLabel(rule.Metric).ToLowerInvariant()} hovers around {around}, close to the threshold."
            : $"Unlikely to fire: this impact keeps {MonitoringDisplay.MetricLabel(rule.Metric).ToLowerInvariant()} around {around}.";
    }

    /// <summary>Returns whether a rule could plausibly fire during a drill (for the public confirmation).</summary>
    private static bool CouldFire(MonitoringAlertRule rule, MonitoringService service, MonitoringImpact impact, int seconds) =>
        seconds >= rule.ForSeconds && ExpectedDuring(service, impact, rule.Metric) > rule.Threshold * 0.85;

    private static string UnitSymbol(MonitoringAlertMetric metric) => metric == MonitoringAlertMetric.Latency ? "ms" : "%";

    private static string UnitWords(MonitoringAlertMetric metric) => metric == MonitoringAlertMetric.Latency ? "milliseconds" : "percent";

    private void Announce(bool ok, string message, int? incidentId = null, bool isPublic = false)
    {
        statusIsError = !ok;
        statusNotes = new List<StatusNote> { new(message, incidentId, isPublic) };
    }

    private bool ApplyDelete(int ruleId)
    {
        var rule = State.Rules.FirstOrDefault(item => item.Id == ruleId);
        var name = rule?.Name ?? "The rule";
        var result = State.DeleteRule(ruleId);

        Announce(result == MonitoringResult.Ok, result switch
        {
            MonitoringResult.Ok => $"Deleted rule \"{name}\". Incidents it opened stay.",
            MonitoringResult.NotFound => "That rule was already deleted, so nothing changed.",
            _ => SharedRefusal(result) ?? "The rule couldn't be deleted. Nothing changed.",
        });

        // The row is gone after a delete, so focus goes to the status message.
        return result != MonitoringResult.Ok && rule is not null;
    }

    /// <summary>Injects the failure and announces the outcome; returns whether it started.</summary>
    private bool ApplyDrill(DrillRequest request)
    {
        var serviceName = ServiceName(request.ServiceId);
        var result = State.InjectFailure(request.ServiceId, request.Impact, request.Seconds, out var fault);

        if (result == MonitoringResult.Ok && fault is not null)
            runningFaultIds.Add(fault.Id);

        Announce(result == MonitoringResult.Ok, result switch
        {
            MonitoringResult.Ok => $"Drill started: {MonitoringDisplay.ImpactLabel(request.Impact).ToLowerInvariant()} on {serviceName} for {LengthLabel(request.Seconds)}, until {MonitoringDisplay.Time(fault!.End)}."
                + (State.IsPaused ? " Live updates are paused, so no rule fires until you resume them." : " A rule that fires is announced here."),
            MonitoringResult.WrongState => $"A drill is already running on {serviceName}. Stop it first. No drill started.",
            MonitoringResult.NotFound => "That service no longer exists. No drill started.",
            MonitoringResult.Invalid => $"The drill couldn't start: its length must be from {LengthLabel(MonitoringState.MinFaultSeconds)} to {LengthLabel(MonitoringState.MaxFaultSeconds)}, or too many drills are kept in this tab. No drill started.",
            _ => SharedRefusal(result) ?? "That didn't work. No drill started.",
        });

        return result == MonitoringResult.Ok;
    }

    private bool ApplyRule(RuleRequest request)
    {
        var result = State.SaveRule(request.RuleId, request.Name, request.ServiceId, request.Metric, request.Threshold, request.ForSeconds, request.Severity,
            request.AutoPublish, request.PublicMessage, request.IsEnabled, out var saved);
        var serviceName = ServiceName(request.ServiceId);

        Announce(result is MonitoringResult.Ok or MonitoringResult.NoChange, result switch
        {
            MonitoringResult.Ok when request.RuleId == 0 => $"Created rule \"{saved!.Name}\" on {serviceName}." + (saved.IsEnabled ? " It checks every sample from now on." : " It's disabled until you turn it on."),
            MonitoringResult.Ok => $"Saved rule \"{saved!.Name}\". Incidents it already opened don't change.",
            MonitoringResult.NoChange => $"Nothing changed: \"{request.Name}\" already has these settings.",
            MonitoringResult.NotFound => request.RuleId != 0 && State.Rules.All(rule => rule.Id != request.RuleId)
                ? "That rule was deleted meanwhile, so nothing was saved."
                : "The service was deleted meanwhile, so nothing was saved.",
            MonitoringResult.Invalid => request.RuleId == 0 && State.Rules.Count >= MonitoringState.MaxRules
                ? $"The rule couldn't be created: this tab already has {MonitoringState.MaxRules} rules. Delete one first."
                : "The rule couldn't be saved: " + (MonitoringState.RuleProblem(request.Name, request.Metric, request.Threshold, request.ForSeconds, request.Severity, request.AutoPublish, request.PublicMessage) ?? "this tab's storage is full."),
            _ => SharedRefusal(result) ?? "That didn't work, and nothing was saved.",
        });

        // A new rule's row is added below; the opener (New rule, or the row's Edit) is still there.
        return result is MonitoringResult.Ok or MonitoringResult.NoChange;
    }

    private async Task ConfirmAsync()
    {
        if (pendingAction is not null)
            return;

        switch (confirm)
        {
            case ConfirmKind.DeleteRule:
                var ruleId = confirmRuleId;
                pendingAction = () => ApplyDelete(ruleId);
                break;

            case ConfirmKind.StartDrill when pendingDrill is { } drill:
                pendingAction = () => ApplyDrill(drill);
                break;
        }

        await confirmModal.HideAsync();
    }

    /// <summary>Announces once each incident a rule opened while the page was open, and each drill that ended on its own.</summary>
    private void DetectEvents()
    {
        var notes = new List<StatusNote>();

        foreach (var incident in State.Incidents.Where(incident => !knownIncidentIds.Contains(incident.Id)).OrderBy(incident => incident.Id))
        {
            knownIncidentIds.Add(incident.Id);

            if (incident.RuleId is not { } ruleId)
                continue;

            var ruleName = State.Rules.FirstOrDefault(rule => rule.Id == ruleId)?.Name ?? incident.Title;
            var text = $"Alert fired: \"{ruleName}\" opened #{incident.Id} ({incident.Severity}) on {ServiceName(incident.ServiceId)}."
                + (incident.IsPublished ? $" It's posted on the status page and emailed to {SubscribersText(incident.ServiceId)}." : " It's engineers-only until someone posts on the status page.");
            notes.Add(new StatusNote(text, incident.Id, incident.IsPublished));
        }

        var running = RunningFaults.Select(fault => fault.Id).ToHashSet();

        foreach (var faultId in runningFaultIds.Where(id => !running.Contains(id)).ToList())
        {
            runningFaultIds.Remove(faultId);

            if (State.Faults.FirstOrDefault(fault => fault.Id == faultId) is { } ended)
                notes.Add(new StatusNote($"The {MonitoringDisplay.ImpactLabel(ended.Impact).ToLowerInvariant()} drill on {ServiceName(ended.ServiceId)} ended after {LengthLabel((int)Math.Round((ended.End - ended.Start).TotalSeconds))}.", null, false));
        }

        runningFaultIds.UnionWith(running);

        if (notes.Count > 0)
        {
            statusIsError = false;
            statusNotes = notes;
        }
    }

    /// <summary>Returns the incident a rule opened on the drill's service while the drill ran, if any.</summary>
    private MonitoringIncident? DrillIncident(MonitoringFault fault) =>
        State.Incidents
            .Where(incident => incident.ServiceId == fault.ServiceId && incident.RuleId is not null && incident.Updates.Count > 0)
            .Where(incident => incident.Updates[0].Time >= fault.Start && incident.Updates[0].Time <= fault.End.AddSeconds(MonitoringSeedData.TickSeconds))
            .OrderBy(incident => incident.Id)
            .FirstOrDefault();

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. A confirmed action runs now that the dialog is gone;
    /// focus then goes back to the button that opened it, or to the status message when that button is gone.
    /// </summary>
    private void OnConfirmHidden()
    {
        var action = pendingAction;
        pendingAction = null;
        var opener = confirmOpener;

        if (action is not null && !action())
            focusStatus = true;
        else
            focusTarget = () => opener?.Element;
    }

    private void OnRuleHidden()
    {
        var request = pendingRule;
        pendingRule = null;
        var opener = ruleOpener;

        if (request is not null && !ApplyRule(request))
            focusStatus = true;
        else
            focusTarget = () => opener?.Element;
    }

    private void OnRuleMetricChanged()
    {
        ruleThreshold = DefaultThreshold(ruleMetric);
        ruleThresholdError = null;
    }

    private void OnRuleShown() => focusTarget = () => ruleNameInput;

    private void OnStateChanged() => InvokeAsync(() =>
    {
        if (disposed)
            return;

        DetectEvents();
        StateHasChanged();
    });

    private async Task OnTickAsync()
    {
        if (disposed || State.IsPaused)
            return;

        try
        {
            await InvokeAsync(() =>
            {
                if (disposed)
                    return;

                State.Tick();
                DetectEvents();
                StateHasChanged();
            });
        }
        catch (Exception ex) when (ex is JSDisconnectedException or ObjectDisposedException or TaskCanceledException)
        {
            // The page or circuit went away between ticks.
        }
    }

    private async Task OpenDeleteRuleAsync(MonitoringAlertRule rule)
    {
        confirm = ConfirmKind.DeleteRule;
        confirmRuleId = rule.Id;
        confirmOpener = deleteButtons.GetValueOrDefault(rule.Id);
        pendingAction = null;

        await confirmModal.ShowAsync();
    }

    private async Task OpenEditRuleAsync(MonitoringAlertRule rule)
    {
        editingRuleId = rule.Id;
        ruleOpener = editButtons.GetValueOrDefault(rule.Id);
        ruleName = rule.Name;
        ruleServiceId = rule.ServiceId;
        ruleMetric = rule.Metric;
        ruleThreshold = rule.Threshold;
        ruleForSeconds = rule.ForSeconds;
        ruleSeverity = rule.Severity;
        ruleAutoPublish = rule.AutoPublish;
        rulePublicMessage = rule.PublicMessage ?? string.Empty;
        ruleEnabled = rule.IsEnabled;

        await ShowRuleFormAsync();
    }

    private async Task OpenNewRuleAsync()
    {
        editingRuleId = 0;
        ruleOpener = newRuleButton;
        ruleName = string.Empty;
        ruleServiceId = string.Empty;
        ruleMetric = MonitoringAlertMetric.ErrorRate;
        ruleThreshold = DefaultThreshold(ruleMetric);
        ruleForSeconds = 20;
        ruleSeverity = MonitoringSeverity.Major;
        ruleAutoPublish = false;
        rulePublicMessage = string.Empty;
        ruleEnabled = true;

        await ShowRuleFormAsync();
    }

    /// <summary>The enabled rules on a service that post on the status page and could fire during the pending drill.</summary>
    private IReadOnlyList<MonitoringAlertRule> PublishingRules(string serviceId) =>
        State.FindService(serviceId) is { } service && pendingDrill is { } drill
            ? State.Rules.Where(rule => rule.ServiceId == serviceId && rule.IsEnabled && rule.AutoPublish && CouldFire(rule, service, drill.Impact, drill.Seconds)).ToList()
            : Array.Empty<MonitoringAlertRule>();

    private string ServiceName(string serviceId) => State.FindService(serviceId)?.Name ?? serviceId;

    /// <summary>The message for a refusal every action shares.</summary>
    private static string? SharedRefusal(MonitoringResult result) => result switch
    {
        MonitoringResult.NotAllowed => "Sign in as an admin to change alert rules and run drills. Nothing changed.",
        MonitoringResult.Duplicate => "That already exists, so nothing changed.",
        _ => null,
    };

    private async Task ShowRuleFormAsync()
    {
        pendingRule = null;
        ruleFormError = ruleNameError = ruleServiceError = ruleMetricError = ruleThresholdError = ruleDurationError = ruleMessageError = null;

        await ruleModal.ShowAsync();
    }

    private void StopDrill(MonitoringFault fault)
    {
        var serviceName = ServiceName(fault.ServiceId);
        var result = State.StopFailure(fault.Id);

        if (result is MonitoringResult.Ok or MonitoringResult.NoChange)
            runningFaultIds.Remove(fault.Id);

        var opened = DrillIncident(fault);

        Announce(result is MonitoringResult.Ok or MonitoringResult.NoChange, result switch
        {
            MonitoringResult.Ok => $"Stopped the drill on {serviceName}. Its metrics return to normal from the next sample."
                + (opened is not null ? $" Incident #{opened.Id} stays open until someone resolves it." : string.Empty),
            MonitoringResult.NoChange => $"The drill on {serviceName} had already ended.",
            MonitoringResult.NotFound => "That drill no longer exists, so nothing changed.",
            _ => SharedRefusal(result) ?? "The drill couldn't be stopped. Try again.",
        }, opened?.Id, opened?.IsPublished == true);

        // The row is gone, so focus goes to the status message.
        focusStatus = true;
    }

    /// <summary>Checks the drill form; a drill that could post on the status page is confirmed first.</summary>
    private async Task SubmitDrillAsync()
    {
        drillServiceError = State.FindService(drillServiceId) is null ? "Choose the service to inject the failure into."
            : State.ActiveFault(drillServiceId) is not null ? $"A drill is already running on {ServiceName(drillServiceId)}. Stop it first, or choose another service."
            : null;

        if (drillServiceError is not null)
        {
            focusTarget = () => drillServiceInput;
            return;
        }

        var request = new DrillRequest(drillServiceId, drillImpact, Math.Clamp(drillSeconds, MonitoringState.MinFaultSeconds, MonitoringState.MaxFaultSeconds));
        var silenced = State.ActiveIncident(request.ServiceId) is not null || State.ActiveMaintenance(request.ServiceId) is not null;
        pendingDrill = request;

        if (!silenced && PublishingRules(request.ServiceId).Count > 0)
        {
            confirm = ConfirmKind.StartDrill;
            confirmOpener = startDrillButton;
            pendingAction = null;
            await confirmModal.ShowAsync();
            return;
        }

        pendingDrill = null;
        ApplyDrill(request);
    }

    /// <summary>Checks the rule form with the state's own rule check, field by field; the dialog closes and the rule is saved once it is valid.</summary>
    private async Task SubmitRuleAsync()
    {
        if (pendingRule is not null)
            return;

        var name = ruleName.Trim();
        var message = rulePublicMessage.Trim();
        var (low, high) = MonitoringState.ThresholdRange(ruleMetric);
        const MonitoringAlertMetric okMetric = MonitoringAlertMetric.ErrorRate;
        const MonitoringSeverity okSeverity = MonitoringSeverity.Major;

        // Each field is checked with valid values for the others, so every error shows at once in the state's own words.
        ruleNameError = MonitoringState.RuleProblem(name, okMetric, 2, 20, okSeverity, false, null);
        ruleServiceError = State.FindService(ruleServiceId) is null ? "Choose the service this rule watches." : null;
        ruleMetricError = MonitoringState.RuleProblem("x", ruleMetric, low, 20, ruleSeverity, false, null);
        ruleThresholdError = ruleThreshold is { } threshold
            ? (ruleMetricError is null ? MonitoringState.RuleProblem("x", ruleMetric, threshold, 20, okSeverity, false, null) : null)
            : FormattableString.Invariant($"Enter a threshold from {low} to {high} {UnitWords(ruleMetric)}.");
        ruleDurationError = MonitoringState.RuleProblem("x", okMetric, 2, ruleForSeconds, okSeverity, false, null);
        ruleMessageError = ruleAutoPublish && message.Length == 0
            ? "Write the message customers will read, or turn off posting on the status page."
            : MonitoringState.RuleProblem("x", okMetric, 2, 20, okSeverity, ruleAutoPublish, message);
        ruleFormError = editingRuleId == 0 && State.Rules.Count >= MonitoringState.MaxRules
            ? $"This tab already has {MonitoringState.MaxRules} rules, the most it keeps. Delete one first."
            : editingRuleId != 0 && State.Rules.All(rule => rule.Id != editingRuleId) ? "This rule was deleted meanwhile. Cancel and create a new one." : null;

        focusTarget = ruleNameError is not null ? () => ruleNameInput
            : ruleServiceError is not null ? () => ruleServiceInput
            : ruleMetricError is not null ? () => ruleMetricInput
            : ruleThresholdError is not null ? () => ruleThresholdInput
            : ruleDurationError is not null ? () => ruleDurationInput
            : ruleMessageError is not null ? () => ruleMessageInput
            : ruleFormError is not null ? () => ruleNameInput
            : null;

        if (focusTarget is not null)
            return;

        // The same check as a whole, in case a combination is refused.
        if (MonitoringState.RuleProblem(name, ruleMetric, ruleThreshold!.Value, ruleForSeconds, ruleSeverity, ruleAutoPublish, message) is { } problem)
        {
            ruleFormError = problem;
            focusTarget = () => ruleNameInput;
            return;
        }

        pendingRule = new RuleRequest(editingRuleId, name, ruleServiceId, ruleMetric, ruleThreshold.Value, ruleForSeconds, ruleSeverity, ruleAutoPublish, message, ruleEnabled);
        await ruleModal.HideAsync();
    }

    /// <summary>Says who a status page post on a service is emailed to.</summary>
    private string SubscribersText(string serviceId)
    {
        if (State.FindService(serviceId) is null)
            return "the subscribers who follow the service";

        var count = State.Subscriptions.Count(subscription => subscription.ServiceIds.Count == 0 || subscription.ServiceIds.Contains(serviceId));

        return count switch
        {
            0 => "the subscribers who follow the service (none in this tab yet)",
            1 => "1 subscriber",
            _ => $"{count} subscribers",
        };
    }

    /// <summary>The Enable/Disable button stays (its text changes), so focus stays on it.</summary>
    private void ToggleRule(MonitoringAlertRule rule)
    {
        var enable = !rule.IsEnabled;
        var result = State.SetRuleEnabled(rule.Id, enable);
        var serviceName = ServiceName(rule.ServiceId);

        Announce(result is MonitoringResult.Ok or MonitoringResult.NoChange, result switch
        {
            MonitoringResult.Ok when enable => $"\"{rule.Name}\" is enabled: it checks {serviceName} on every sample."
                + (rule.AutoPublish ? " When it fires, it posts on the status page." : string.Empty),
            MonitoringResult.Ok => $"\"{rule.Name}\" is disabled: it won't open incidents until you enable it again.",
            MonitoringResult.NoChange => $"\"{rule.Name}\" is already {(enable ? "enabled" : "disabled")}, so nothing changed.",
            MonitoringResult.NotFound => "That rule was deleted meanwhile, so nothing changed.",
            _ => SharedRefusal(result) ?? "That didn't work, and nothing changed.",
        });
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Whether the rule in the form would fire right after saving: its service has no active incident and no maintenance, and
    /// the latest samples of the window are all past the threshold.
    /// </summary>
    private bool WouldFireOnSave =>
        State.FindService(ruleServiceId) is { } service && ruleThreshold is double threshold && State.ActiveIncident(service.Id) is null && State.ActiveMaintenance(service.Id) is null
        && State.History(service.Id).TakeLast(Math.Max(1, ruleForSeconds / MonitoringSeedData.TickSeconds)).All(sample => MonitoringDisplay.MetricOf(ruleMetric, sample) > threshold);

    private string ConfirmTitle => confirm switch
    {
        ConfirmKind.DeleteRule => "Delete this alert rule?",
        ConfirmKind.StartDrill => "This drill could post on the status page",
        _ => "Confirm",
    };

    /// <summary>The drill lengths within the state's limits.</summary>
    private static IEnumerable<int> DrillLengths => drillLengthChoices.Where(seconds => seconds is >= MonitoringState.MinFaultSeconds and <= MonitoringState.MaxFaultSeconds);

    /// <summary>The finished drills, newest first.</summary>
    private IReadOnlyList<MonitoringFault> PastFaults => State.Faults.Where(fault => fault.End <= State.Clock.Now).Take(PastDrillCount).ToList();

    /// <summary>The durations a rule may use: even seconds over the samples kept.</summary>
    private static IEnumerable<int> RuleDurations =>
        Enumerable.Range(2, MonitoringSeedData.HistoryLength - 1).Select(samples => samples * MonitoringSeedData.TickSeconds);

    /// <summary>The drills running now, newest first.</summary>
    private IReadOnlyList<MonitoringFault> RunningFaults => State.Faults.Where(fault => fault.Start <= State.Clock.Now && State.Clock.Now < fault.End).ToList();

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>What the confirmation dialog confirms.</summary>
    private enum ConfirmKind
    {
        None,
        DeleteRule,
        StartDrill,
    }

    /// <summary>A validated drill to start.</summary>
    private sealed record DrillRequest(string ServiceId, MonitoringImpact Impact, int Seconds);

    /// <summary>A validated rule to save.</summary>
    private sealed record RuleRequest(int RuleId, string Name, string ServiceId, MonitoringAlertMetric Metric, double Threshold, int ForSeconds, MonitoringSeverity Severity,
        bool AutoPublish, string PublicMessage, bool IsEnabled);

    /// <summary>One line of the status region, with a link to an incident when it is about one.</summary>
    private sealed record StatusNote(string Text, int? IncidentId, bool IsPublic);

    #endregion
}
