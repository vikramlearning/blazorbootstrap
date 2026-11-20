using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The admins' alert rules and failure drills. A drill form (an <see cref="EditForm" />, rule WR-002: service, impact with
/// what it does to the metrics, length) shows before starting which enabled rules watch the service, whether each would
/// post on the status page and how likely it is to fire, and warns when the service already has an active incident or
/// maintenance running (rules don't fire) or a drill running. A drill that could make a rule post on the status page is
/// confirmed in a <see cref="Modal" /> that names the public message and the subscribers it is emailed to. Running drills
/// show their time left from the clock and a Stop button; recent drills show how long they ran and the incident they
/// opened. The rules are a <see cref="Grid{TItem}" /> (metric, threshold, duration, severity, enabled, "posts on the
/// status page", the current value with "Breaching" in words, and when it last fired with a link to the incident) with
/// sorting, the column chooser, CSV export, and a saved view; the whole row opens the rule form (also for a new rule) in a
/// <see cref="Modal" />, an <see cref="EditForm" /> with a unique name checked as you type, the metric's threshold range,
/// a threshold above the service's normal level, and posting only for a service on the status page. Enable or Disable
/// toggles a rule, and Delete is confirmed (its incidents stay). Rules are evaluated on every tick, so the page runs the
/// live timer (with <see cref="LivePauseButton" />); a rule that fires while the page is open, and a drill that ends on its
/// own, are announced once in the status region.
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

    private EditContext drillContext = new(new DrillForm());

    private DrillForm drillForm = new();

    /// <summary>The state's answers that belong to a drill field (a drill already running), shown like the form's own messages.</summary>
    private ValidationMessageStore? drillServerErrors;

    private readonly Dictionary<int, Button> editButtons = new();

    /// <summary>The rule the form edits; 0 for a new rule.</summary>
    private int editingRuleId;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (the button that opened a dialog).</summary>
    private Func<ElementReference?>? focusTarget;

    private Grid<MonitoringAlertRule>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The incidents already on the page, so a rule firing is announced once.</summary>
    private readonly HashSet<int> knownIncidentIds = new();

    private Button? newRuleButton;

    /// <summary>The confirmed action, run once the dialog is hidden (it changes the page under the dialog). It returns whether the opener is still there to take focus back.</summary>
    private Func<bool>? pendingAction;

    /// <summary>The drill waiting for the "it could go public" confirmation.</summary>
    private DrillRequest? pendingDrill;

    private EditContext ruleContext = new(new RuleForm());

    private RuleForm ruleForm = new();

    private string? ruleFormError;

    private Modal ruleModal = default!;

    private ShowcaseUniqueCheck? ruleNameCheck;

    private InputText? ruleNameInput;

    /// <summary>The button that opened the rule form.</summary>
    private Button? ruleOpener;

    private ElementReference? rulesCountRegion;

    /// <summary>The state's answers that belong to a rule field (a taken name, posting for a hidden service), shown like the form's own messages.</summary>
    private ValidationMessageStore? ruleServerErrors;

    /// <summary>The drills running when last checked, so one that ends on its own is announced once.</summary>
    private readonly HashSet<int> runningFaultIds = new();

    private Button? startDrillButton;

    private bool statusIsError;

    private List<StatusNote> statusNotes = new();

    private ElementReference? statusRegion;

    private MonitoringLiveTimer? timer;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The saved view lives in the tab's session storage, so it loads once the page is interactive; then the grid shows and the timer starts.
        if (firstRender && !disposed)
        {
            await view.LoadAsync();
            viewLoaded = true;
            StateHasChanged();
            timer = new MonitoringLiveTimer(TimeSpan.FromSeconds(MonitoringSeedData.TickSeconds), OnTickAsync);
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
        // Catch up with the clock on arrival; what fires now shows in the table rather than as an announcement.
        if (!State.IsPaused)
            State.Tick();

        // With no column sorted, the rules keep their number order.
        view = GridViews.Get(State.SiteSlug, "alert-rules", 25);
        knownIncidentIds.UnionWith(State.Incidents.Select(incident => incident.Id));
        runningFaultIds.UnionWith(RunningFaults.Select(fault => fault.Id));
        NewDrillForm(string.Empty, MonitoringImpact.Errors, 60);
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

        if (result == MonitoringResult.Ok)
            NewDrillForm(string.Empty, request.Impact, request.Seconds);

        return result == MonitoringResult.Ok;
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

    /// <summary>Whether another rule already has this name (the rule being edited doesn't count).</summary>
    private Task<bool> IsRuleNameTakenAsync(string name) => Task.FromResult(State.IsRuleNameTaken(name, editingRuleId));

    /// <summary>A fresh drill form in a new edit context, so no old message stays.</summary>
    private void NewDrillForm(string serviceId, MonitoringImpact impact, int seconds)
    {
        drillForm = new DrillForm { ServiceId = serviceId, Impact = impact, Seconds = seconds };
        drillContext = new EditContext(drillForm);
        drillServerErrors = new ValidationMessageStore(drillContext);
        drillContext.OnFieldChanged += (_, args) => drillServerErrors?.Clear(args.FieldIdentifier);
    }

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

    /// <summary>The rule was saved before the dialog closed (or it was cancelled): focus goes back to the button that opened it.</summary>
    private void OnRuleHidden()
    {
        var opener = ruleOpener;
        focusTarget = () => opener?.Element;
    }

    /// <summary>A new metric starts from its usual threshold, which is checked again.</summary>
    private void OnRuleMetricChanged()
    {
        ruleForm.Threshold = DefaultThreshold(ruleForm.Metric);
        ruleContext.NotifyFieldChanged(FieldIdentifier.Create(() => ruleForm.Threshold));
    }

    private Task OnRuleShownAsync() => MonitoringDisplay.FocusAsync(ruleNameInput?.Element);

    /// <summary>A rule fired, a drill ended, or another screen changed the rules: announce it, and the grid reads the rules again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        if (disposed)
            return;

        DetectEvents();
        await RefreshGridAsync();
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

        await ShowRuleFormAsync(new RuleForm
        {
            Name = rule.Name, ServiceId = rule.ServiceId, Metric = rule.Metric, Threshold = rule.Threshold, ForSeconds = rule.ForSeconds, Severity = rule.Severity,
            AutoPublish = rule.AutoPublish, PublicMessage = rule.PublicMessage ?? string.Empty, IsEnabled = rule.IsEnabled,
        });
    }

    private async Task OpenNewRuleAsync()
    {
        editingRuleId = 0;
        ruleOpener = newRuleButton;

        await ShowRuleFormAsync(new RuleForm { Threshold = DefaultThreshold(MonitoringAlertMetric.ErrorRate) });
    }

    /// <summary>The rules the grid asked for, in number order unless a column is sorted.</summary>
    private Task<GridDataProviderResult<MonitoringAlertRule>> ProvideRulesAsync(GridDataProviderRequest<MonitoringAlertRule> request) => view.ApplyAsync(request, State.Rules);

    /// <summary>The enabled rules on a service that post on the status page and could fire during the pending drill.</summary>
    private IReadOnlyList<MonitoringAlertRule> PublishingRules(string serviceId) =>
        State.FindService(serviceId) is { } service && pendingDrill is { } drill
            ? State.Rules.Where(rule => rule.ServiceId == serviceId && rule.IsEnabled && rule.AutoPublish && CouldFire(rule, service, drill.Impact, drill.Seconds)).ToList()
            : Array.Empty<MonitoringAlertRule>();

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the rule count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        gridVersion++;
        Announce(true, "The view is reset: rules in number order, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await MonitoringDisplay.FocusAsync(rulesCountRegion);
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void RuleServerError(Expression<Func<object>> field, string message)
    {
        ruleServerErrors?.Clear();
        ruleServerErrors?.Add(field, message);
        ruleContext.NotifyValidationStateChanged();
    }

    private string ServiceName(string serviceId) => State.FindService(serviceId)?.Name ?? serviceId;

    /// <summary>The message for a refusal every action shares.</summary>
    private static string? SharedRefusal(MonitoringResult result) => result switch
    {
        MonitoringResult.NotAllowed => "Sign in as an admin to change alert rules and run drills. Nothing changed.",
        MonitoringResult.Duplicate => "That already exists, so nothing changed.",
        _ => null,
    };

    private async Task ShowRuleFormAsync(RuleForm form)
    {
        form.Lookup = State.FindService;
        ruleForm = form;
        ruleContext = new EditContext(ruleForm);
        ruleServerErrors = new ValidationMessageStore(ruleContext);
        ruleContext.OnFieldChanged += (_, args) => ruleServerErrors?.Clear(args.FieldIdentifier);
        ruleFormError = null;

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

    /// <summary>
    /// Starts the drill once the form is valid (a failed submit shows the summary, rule WR-002); a drill already running on
    /// the service is the state's answer, shown on the field. A drill that could post on the status page is confirmed first.
    /// </summary>
    private async Task SubmitDrillAsync()
    {
        drillServerErrors?.Clear();

        if (!drillContext.Validate())
            return;

        if (State.FindService(drillForm.ServiceId) is not { } service)
        {
            drillServerErrors?.Add(() => drillForm.ServiceId, "That service no longer exists. Choose another one.");
            drillContext.NotifyValidationStateChanged();

            return;
        }

        if (State.ActiveFault(service.Id) is not null)
        {
            drillServerErrors?.Add(() => drillForm.ServiceId, $"A drill is already running on {service.Name}. Stop it first, or choose another service.");
            drillContext.NotifyValidationStateChanged();

            return;
        }

        var request = new DrillRequest(service.Id, drillForm.Impact, Math.Clamp(drillForm.Seconds, MonitoringState.MinFaultSeconds, MonitoringState.MaxFaultSeconds));
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

    /// <summary>
    /// Saves the rule once the form is valid: the name check finishes first, then every rule runs, and a failed submit shows
    /// the summary (rule WR-002). The state checks again; its answer maps to a field or the form's message, and the dialog
    /// closes once the rule is saved.
    /// </summary>
    private async Task SubmitRuleAsync()
    {
        ruleFormError = null;
        ruleServerErrors?.Clear();
        ruleContext.NotifyValidationStateChanged();

        var nameFree = ruleNameCheck is null || await ruleNameCheck.CheckNowAsync();

        if (!ruleContext.Validate() || !nameFree)
            return;

        if (editingRuleId == 0 && State.Rules.Count >= MonitoringState.MaxRules)
        {
            ruleFormError = $"This tab already has {MonitoringState.MaxRules} rules, the most it keeps. Delete one first.";
            return;
        }

        var name = ruleForm.Name.Trim();
        var message = ruleForm.PublicMessage.Trim();
        var threshold = ruleForm.Threshold!.Value;
        var result = State.SaveRule(editingRuleId, name, ruleForm.ServiceId, ruleForm.Metric, threshold, ruleForm.ForSeconds, ruleForm.Severity,
            ruleForm.AutoPublish, message, ruleForm.IsEnabled, out var saved);
        var serviceName = ServiceName(ruleForm.ServiceId);

        switch (result)
        {
            case MonitoringResult.Ok or MonitoringResult.NoChange:
                Announce(true, result switch
                {
                    MonitoringResult.Ok when editingRuleId == 0 => $"Created rule \"{saved!.Name}\" on {serviceName}." + (saved.IsEnabled ? " It checks every sample from now on." : " It's disabled until you turn it on."),
                    MonitoringResult.Ok => $"Saved rule \"{saved!.Name}\". Incidents it already opened don't change.",
                    _ => $"Nothing changed: \"{name}\" already has these settings.",
                });
                await ruleModal.HideAsync();

                return;

            case MonitoringResult.Duplicate:
                RuleServerError(() => ruleForm.Name, $"Another rule is already called \"{name}\". Rule names are unique, because they become incident titles.");

                return;

            case MonitoringResult.WrongState:
                RuleServerError(() => ruleForm.AutoPublish, $"{serviceName} isn't on the status page, so the rule can't post there. Turn this off, or show the service on the status page first.");

                return;

            case MonitoringResult.NotFound:
                ruleFormError = editingRuleId != 0 && State.Rules.All(rule => rule.Id != editingRuleId)
                    ? "This rule was deleted meanwhile. Cancel and create a new one."
                    : "The service was deleted meanwhile. Choose another service.";

                return;

            case MonitoringResult.Invalid:
                ruleFormError = "The rule couldn't be saved: "
                    + (MonitoringState.RuleProblem(name, ruleForm.Metric, threshold, ruleForm.ForSeconds, ruleForm.Severity, ruleForm.AutoPublish, message)
                       ?? (State.FindService(ruleForm.ServiceId) is { } service ? MonitoringState.NormalRangeProblem(service, ruleForm.Metric, threshold) : null)
                       ?? "this tab's storage is full.");

                return;

            default:
                ruleFormError = SharedRefusal(result) ?? "That didn't work, and nothing was saved.";

                return;
        }
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
        State.FindService(ruleForm.ServiceId) is { } service && ruleForm.Threshold is double threshold && State.ActiveIncident(service.Id) is null && State.ActiveMaintenance(service.Id) is null
        && State.History(service.Id).TakeLast(Math.Max(1, ruleForm.ForSeconds / MonitoringSeedData.TickSeconds)).All(sample => MonitoringDisplay.MetricOf(ruleForm.Metric, sample) > threshold);

    private string ConfirmTitle => confirm switch
    {
        ConfirmKind.DeleteRule => "Delete this alert rule?",
        ConfirmKind.StartDrill => "This drill could post on the status page",
        _ => "Confirm",
    };

    /// <summary>The drill lengths within the state's limits.</summary>
    private static IEnumerable<int> DrillLengths => drillLengthChoices.Where(seconds => seconds is >= MonitoringState.MinFaultSeconds and <= MonitoringState.MaxFaultSeconds);

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

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

    /// <summary>The drill form's fields: the service is required; a drill already running on it is the state's answer.</summary>
    private sealed class DrillForm
    {
        public MonitoringImpact Impact { get; set; } = MonitoringImpact.Errors;

        public int Seconds { get; set; } = 60;

        [Required(ErrorMessage = "Choose the service to inject the failure into.")]
        public string ServiceId { get; set; } = string.Empty;
    }

    /// <summary>A validated drill to start.</summary>
    private sealed record DrillRequest(string ServiceId, MonitoringImpact Impact, int Seconds);

    /// <summary>
    /// The rule form's fields, with the limits of <see cref="MonitoringState.SaveRule" />: a name and a service, the metric's
    /// threshold range, a duration in sample steps, a message when it posts on the status page, and two cross-field rules:
    /// the threshold is above the service's normal level, and only a service on the status page can post there.
    /// </summary>
    private sealed class RuleForm : IValidatableObject
    {
        public bool AutoPublish { get; set; }

        public int ForSeconds { get; set; } = 20;

        public bool IsEnabled { get; set; } = true;

        /// <summary>Finds the chosen service for the cross-field rules; set by the page.</summary>
        public Func<string?, MonitoringService?>? Lookup { get; set; }

        public MonitoringAlertMetric Metric { get; set; } = MonitoringAlertMetric.ErrorRate;

        [Required(ErrorMessage = "Give the rule a name.")]
        [StringLength(MonitoringState.MaxRuleNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(MonitoringState.MaxUpdateLength, ErrorMessage = "Use 1,000 characters or fewer for the message.")]
        public string PublicMessage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose the service this rule watches.")]
        public string ServiceId { get; set; } = string.Empty;

        public MonitoringSeverity Severity { get; set; } = MonitoringSeverity.Major;

        [Required(ErrorMessage = "Enter the threshold the metric must stay above.")]
        public double? Threshold { get; set; }

        /// <summary>Runs once the fields are filled in: the threshold's range and the service's normal level, the duration, and posting.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var (low, high) = MonitoringState.ThresholdRange(Metric);
            var service = Lookup?.Invoke(ServiceId);

            if (Threshold is { } threshold)
            {
                if (!double.IsFinite(threshold) || threshold < low || threshold > high)
                    yield return new ValidationResult(FormattableString.Invariant($"Set a threshold from {low} to {high} {UnitWords(Metric)}."), new[] { nameof(Threshold) });
                else if (service is not null && MonitoringState.NormalRangeProblem(service, Metric, threshold) is { } normal)
                    yield return new ValidationResult(normal, new[] { nameof(Threshold) });
            }

            if (MonitoringState.RuleProblem("x", MonitoringAlertMetric.ErrorRate, 2, ForSeconds, MonitoringSeverity.Major, false, null) is { } duration)
                yield return new ValidationResult(duration, new[] { nameof(ForSeconds) });

            if (AutoPublish && string.IsNullOrWhiteSpace(PublicMessage))
                yield return new ValidationResult("Write the message customers will read, or turn off posting on the status page.", new[] { nameof(PublicMessage) });

            if (AutoPublish && service is { IsPublic: false })
                yield return new ValidationResult($"{service.Name} isn't on the status page, so the rule can't post there. Turn this off, or show the service on the status page first.", new[] { nameof(AutoPublish) });
        }
    }

    /// <summary>One line of the status region, with a link to an incident when it is about one.</summary>
    private sealed record StatusNote(string Text, int? IncidentId, bool IsPublic);

    #endregion
}
