using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// One incident in the console: title, service, severity, status, assignee, times, and whether it is on the status page
/// (with a link to its public page). Actions follow the status: Acknowledge (open), Assign to an engineer and Change
/// severity and Resolve (active), Reopen (resolved), each confirmed in a <see cref="Modal" /> that names what changes; a
/// published incident needs a public message to resolve or reopen, prefilled with a draft and emailed to the service's
/// subscribers. Engineers post status page updates (the first one is confirmed, since it makes the incident public) and
/// engineers-only notes. Every form is an <see cref="EditForm" /> with its messages tied to the fields and, on a failed
/// submit, a summary that links to them (rule WR-002). The
/// timeline shows every entry, oldest or newest first, with public ones marked "Published". Resolved incidents have a
/// postmortem to write, save as a draft, publish, or unpublish. A small panel shows the service's latest latency and error
/// rate as of arrival, with a Refresh button (no timer), and redraws on state changes and ticks. Every outcome is
/// announced in a status message, which takes focus when the pressed button is gone. An unknown number shows a
/// not-found panel. <c>?declared=1</c> (from the incidents page) announces a new incident.
/// </summary>
public partial class IncidentDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? addItemButton;

    /// <summary>The engineer the Assign control would assign the incident to.</summary>
    private string assignEngineerId = string.Empty;

    /// <summary>What the dialog is confirming.</summary>
    private DialogKind dialog;

    private EditContext dialogContext = new(new DialogForm());

    private DialogForm dialogForm = new();

    private Modal dialogModal = default!;

    /// <summary>The button that opened the dialog, for focus after Cancel or when it is still there after the action.</summary>
    private Button? dialogOpener;

    private bool editingPostmortem;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (a field with an error, or a new or remaining control).</summary>
    private Func<ElementReference?>? focusTarget;

    private int? loadedId;

    private bool newestFirst;

    private EditContext noteContext = new(new NoteForm());

    private NoteForm noteForm = new();

    /// <summary>
    /// The confirmed action, run once the dialog is hidden (it changes the page under the dialog). It returns whether the
    /// button that opened the dialog is still there to take focus back.
    /// </summary>
    private Func<bool>? pendingAction;

    private Button? pmEditButton;

    private List<ActionItemDraft> pmItems = new();

    private int pmNextKey;

    private Button? pmPublishButton;

    /// <summary>The postmortem waiting for the publish confirmation.</summary>
    private PostmortemRequest? pmRequest;

    private EditContext pmContext = new(new PostmortemForm());

    private PostmortemForm pmForm = new();

    private InputTextArea? pmSummaryInput;

    private Button? pmUnpublishButton;

    private Button? postButton;

    private EditContext postContext = new(new PostForm());

    private PostForm postForm = new();

    private InputTextArea? reasonInput;

    private Button? reopenButton;

    private Button? resolveButton;

    private Button? severityButton;

    private InputSelect<MonitoringSeverity>? severityInput;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
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
        // The live panel shows the clock's latest sample on arrival; Refresh (or a tick from elsewhere) moves it on.
        if (!State.IsPaused)
            State.Tick();

        State.Changed += OnStateChanged;
        State.Ticked += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        statusMessage = null;
        statusIsError = false;
        editingPostmortem = false;
        pendingAction = null;
        pmRequest = null;
        NewPostForm(Incident?.PublicStage is { } stage && stage != MonitoringPublicStage.Resolved ? stage : MonitoringPublicStage.Investigating);
        NewNoteForm();
        assignEngineerId = State.Identity.Current?.Id is { } me && State.FindEngineer(me) is not null ? me : State.Engineers.FirstOrDefault()?.Id ?? string.Empty;

        if (Declared == "1" && Incident is { } incident)
        {
            statusMessage = $"Incident #{incident.Id} declared on {ServiceName(incident)}. The on-call engineers were notified. It stays engineers-only until you post on the status page.";
            focusStatus = true;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes and ticks.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        State.Ticked -= OnStateChanged;
    }

    #endregion

    #region Private Methods

    /// <summary>Says who an update is emailed to.</summary>
    private static string EmailedText(int subscribers) => subscribers switch
    {
        0 => "emailed to 0 subscribers (nobody in this tab follows the service yet)",
        1 => "emailed to 1 subscriber",
        _ => $"emailed to {subscribers} subscribers",
    };

    private static IconName KindIcon(MonitoringUpdateKind kind) => kind switch
    {
        MonitoringUpdateKind.Alert => IconName.BellFill,
        MonitoringUpdateKind.Public => IconName.Broadcast,
        MonitoringUpdateKind.Status => IconName.ArrowRepeat,
        MonitoringUpdateKind.Severity => IconName.Sliders,
        _ => IconName.JournalText,
    };

    private static string LogsUrl(string serviceId) => $"{ConsoleLayout.Base}/logs?service={Uri.EscapeDataString(serviceId)}";

    /// <summary>Returns how the status page shows a service with a published active incident of this severity.</summary>
    private static MonitoringPublicStatus PublicStatusOf(MonitoringSeverity severity) => severity switch
    {
        MonitoringSeverity.Critical => MonitoringPublicStatus.MajorOutage,
        MonitoringSeverity.Major => MonitoringPublicStatus.PartialOutage,
        _ => MonitoringPublicStatus.Degraded,
    };

    /// <summary>The message for a refusal every action shares; the rest are worded per action.</summary>
    private static string? SharedRefusal(MonitoringResult result, int incidentId) => result switch
    {
        MonitoringResult.NotAllowed => "Sign in as an engineer to change incidents. Nothing changed.",
        MonitoringResult.NotFound => $"Incident #{incidentId} no longer exists, so nothing changed.",
        MonitoringResult.Duplicate => "That's already there, so nothing changed.",
        _ => null,
    };

    private static string StageHelp(MonitoringPublicStage stage) => stage switch
    {
        MonitoringPublicStage.Investigating => "We're looking into it.",
        MonitoringPublicStage.Identified => "The cause is known and a fix is underway.",
        MonitoringPublicStage.Monitoring => "A fix is in place and we're watching the results.",
        _ => "It's over. This also resolves the incident.",
    };

    private static string SubscribersText(int subscribers, string serviceName) => subscribers switch
    {
        0 => $"Nobody in this tab follows {serviceName} on the status page yet.",
        1 => $"1 subscriber gets {serviceName} updates by email.",
        _ => $"{subscribers} subscribers get {serviceName} updates by email.",
    };

    /// <summary>The Acknowledge button disappears, so focus moves to the status message, which announces the outcome.</summary>
    private void Acknowledge()
    {
        if (Incident is not { } incident)
            return;

        var result = State.Acknowledge(incident.Id);

        Announce(result == MonitoringResult.Ok, result switch
        {
            MonitoringResult.Ok => $"You acknowledged #{incident.Id}: it's assigned to you.",
            MonitoringResult.NoChange => $"#{incident.Id} was already acknowledged by {incident.Assignee ?? "someone"}, so nothing changed.",
            MonitoringResult.WrongState => $"#{incident.Id} is resolved, so there's nothing to acknowledge.",
            MonitoringResult.Invalid => $"#{incident.Id} couldn't be acknowledged: its timeline or this tab's storage is full. Nothing changed.",
            _ => SharedRefusal(result, incident.Id) ?? "That didn't work, and nothing changed. Try again.",
        });
        focusStatus = true;
    }

    /// <summary>Assigns the incident to the chosen engineer; the control stays, so focus stays on the button.</summary>
    private void Assign()
    {
        if (Incident is not { } incident)
            return;

        var engineer = State.FindEngineer(assignEngineerId);
        var result = engineer is null ? MonitoringResult.NotFound : State.Assign(incident.Id, engineer.Id);

        Announce(result is MonitoringResult.Ok or MonitoringResult.NoChange, result switch
        {
            MonitoringResult.Ok when engineer!.Id == State.Identity.Current?.Id => $"#{incident.Id} is assigned to you. It stays {incident.Status.ToString().ToLowerInvariant()} until someone acknowledges it.",
            MonitoringResult.Ok => $"#{incident.Id} is assigned to {engineer!.Name}" + (MonitoringSeedData.DemoAccounts().Any(account => account.Id == engineer.Id) ? ", who was notified." : "."),
            MonitoringResult.NoChange => $"#{incident.Id} is already assigned to {engineer!.Name}, so nothing changed.",
            MonitoringResult.NotFound => "That engineer isn't on the rotation any more. Nothing changed.",
            MonitoringResult.WrongState => $"#{incident.Id} is resolved, so it can't be assigned. Reopen it first.",
            MonitoringResult.Invalid => $"#{incident.Id} couldn't be assigned: its timeline or this tab's storage is full. Nothing changed.",
            _ => SharedRefusal(result, incident.Id) ?? "That didn't work, and nothing changed. Try again.",
        });
    }

    private void AddActionItem()
    {
        if (pmItems.Count >= MonitoringState.MaxActionItems)
            return;

        var item = new ActionItemDraft(pmNextKey++, string.Empty);
        pmItems.Add(item);
        focusTarget = () => item.Input;
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private bool ApplyPost(MonitoringIncident incident, MonitoringPublicStage stage, string text)
    {
        var wasPublic = incident.IsPublished;
        var subscribers = SubscriberCount(incident);
        var result = State.PostPublicUpdate(incident.Id, stage, text);

        Announce(result == MonitoringResult.Ok, result switch
        {
            MonitoringResult.Ok => $"Posted \"{MonitoringDisplay.StageLabel(stage)}\" on the status page and {EmailedText(subscribers)}."
                + (wasPublic ? string.Empty : $" #{incident.Id} is now public.")
                + (stage == MonitoringPublicStage.Resolved ? " The incident is resolved." : string.Empty),
            MonitoringResult.WrongState => $"#{incident.Id} is resolved, so it takes no more status page updates. Reopen it first. Nothing was posted.",
            MonitoringResult.Invalid => $"The update couldn't be added: #{incident.Id}'s timeline or this tab's storage is full. Nothing was posted.",
            MonitoringResult.NoChange => "Nothing was posted.",
            _ => SharedRefusal(result, incident.Id) ?? "That didn't work, and nothing was posted. Try again.",
        });

        if (result != MonitoringResult.Ok)
            return true;

        NewPostForm(stage == MonitoringPublicStage.Resolved ? MonitoringPublicStage.Investigating : stage);

        // "Resolved" removes the form, and its button with it.
        return stage != MonitoringPublicStage.Resolved;
    }

    private bool ApplyPostmortem(MonitoringIncident incident, PostmortemRequest request)
    {
        var wasPublished = incident.Postmortem?.PublishedAt is not null;
        var result = State.SavePostmortem(incident.Id, request.Summary, request.RootCause, request.ActionItems, request.Publish);

        Announce(result is MonitoringResult.Ok or MonitoringResult.NoChange, result switch
        {
            MonitoringResult.Ok => request.Publish
                ? wasPublished ? $"Postmortem saved. #{incident.Id}'s public incident page shows the changes." : $"Postmortem published on #{incident.Id}'s public incident page."
                : wasPublished ? "Postmortem unpublished. It's an engineers-only draft again." : "Postmortem saved as an engineers-only draft.",
            MonitoringResult.NoChange => "Nothing changed: the postmortem is already saved like this.",
            MonitoringResult.WrongState => incident.Status != MonitoringIncidentStatus.Resolved
                ? $"#{incident.Id} isn't resolved, so its postmortem can't be changed. Nothing was saved."
                : $"#{incident.Id} was never on the status page, so its postmortem can't be published there. Save it as a draft instead.",
            MonitoringResult.Invalid => "The postmortem couldn't be saved: a section is empty or too long, or this tab's storage is full. Nothing was saved.",
            _ => SharedRefusal(result, incident.Id) ?? "That didn't work, and nothing was saved. Try again.",
        });

        if (result is not (MonitoringResult.Ok or MonitoringResult.NoChange))
            return true;

        // The form or the publish button the action came from is gone.
        editingPostmortem = false;

        return false;
    }

    private bool ApplyReopen(MonitoringIncident incident, string reason, string publicMessage)
    {
        var wasPublic = incident.IsPublished;
        var subscribers = SubscriberCount(incident);
        var result = State.Reopen(incident.Id, reason, wasPublic ? publicMessage : null);

        Announce(result == MonitoringResult.Ok, result switch
        {
            MonitoringResult.Ok => $"#{incident.Id} is reopened and assigned to you. The on-call engineers were notified."
                + (wasPublic ? $" The status page says \"Investigating\" again, and the update was {EmailedText(subscribers)}." : string.Empty),
            MonitoringResult.WrongState => $"#{incident.Id} isn't resolved, so there's nothing to reopen.",
            MonitoringResult.Invalid => $"#{incident.Id} couldn't be reopened: write a reason{(wasPublic ? " and a public message" : null)}, or its timeline or this tab's storage is full. Nothing changed.",
            MonitoringResult.NoChange => $"#{incident.Id} is already active, so nothing changed.",
            _ => SharedRefusal(result, incident.Id) ?? "That didn't work, and nothing changed. Try again.",
        });

        if (result == MonitoringResult.Ok)
            NewPostForm(MonitoringPublicStage.Investigating);

        return result != MonitoringResult.Ok;
    }

    private bool ApplyResolve(MonitoringIncident incident, string note, string publicMessage)
    {
        var wasPublic = incident.IsPublished;
        var subscribers = SubscriberCount(incident);
        var serviceName = ServiceName(incident);
        var result = State.Resolve(incident.Id, note, wasPublic ? publicMessage : null);

        Announce(result == MonitoringResult.Ok, result switch
        {
            MonitoringResult.Ok => $"#{incident.Id} is resolved. {serviceName}'s metrics return to normal from the next sample."
                + (wasPublic ? $" \"Resolved\" is on the status page and was {EmailedText(subscribers)}." : string.Empty)
                + " Write the postmortem below while it's fresh.",
            MonitoringResult.NoChange => $"#{incident.Id} was already resolved, so nothing changed.",
            MonitoringResult.Invalid => $"#{incident.Id} couldn't be resolved: {(wasPublic ? "write the public message, or " : null)}its timeline or this tab's storage is full. Nothing changed.",
            MonitoringResult.WrongState => $"#{incident.Id} can't be resolved right now. Nothing changed.",
            _ => SharedRefusal(result, incident.Id) ?? "That didn't work, and nothing changed. Try again.",
        });

        return result != MonitoringResult.Ok;
    }

    private bool ApplySeverity(MonitoringIncident incident, MonitoringSeverity severity, string reason)
    {
        var result = State.SetSeverity(incident.Id, severity, reason);

        Announce(result == MonitoringResult.Ok, result switch
        {
            MonitoringResult.Ok => $"#{incident.Id} is now {severity}."
                + (incident.IsPublished ? $" The status page shows {ServiceName(incident)} as \"{MonitoringDisplay.PublicStatusLabel(PublicStatusOf(severity))}\"." : string.Empty),
            MonitoringResult.NoChange => $"#{incident.Id} is already {severity}, so nothing changed.",
            MonitoringResult.WrongState => $"#{incident.Id} is resolved, so its severity can't change. Reopen it first.",
            MonitoringResult.Invalid => $"The severity couldn't be changed: write a reason, or #{incident.Id}'s timeline or this tab's storage is full. Nothing changed.",
            _ => SharedRefusal(result, incident.Id) ?? "That didn't work, and nothing changed. Try again.",
        });

        return incident.Status != MonitoringIncidentStatus.Resolved;
    }

    private void CancelPostmortemEdit()
    {
        editingPostmortem = false;
        focusTarget = () => pmEditButton?.Element;
    }

    private string DialogConfirmLabel(MonitoringIncident incident) => dialog switch
    {
        DialogKind.Severity => "Change severity",
        DialogKind.Resolve => incident.IsPublished ? "Resolve and publish" : "Resolve",
        DialogKind.Reopen => incident.IsPublished ? "Reopen and publish" : "Reopen",
        DialogKind.FirstPublic => "Publish on the status page",
        DialogKind.PublishPostmortem => incident.Postmortem?.PublishedAt is null ? "Publish postmortem" : "Save and update the public page",
        DialogKind.UnpublishPostmortem => "Unpublish",
        _ => "OK",
    };

    private string DialogTitle(MonitoringIncident incident) => dialog switch
    {
        DialogKind.Severity => $"Change the severity of #{incident.Id}",
        DialogKind.Resolve => $"Resolve #{incident.Id}?",
        DialogKind.Reopen => $"Reopen #{incident.Id}?",
        DialogKind.FirstPublic => $"Make #{incident.Id} public?",
        DialogKind.PublishPostmortem => incident.Postmortem?.PublishedAt is null ? "Publish the postmortem?" : "Update the published postmortem?",
        DialogKind.UnpublishPostmortem => "Unpublish the postmortem?",
        _ => $"Incident #{incident.Id}",
    };

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. A confirmed action runs now that the dialog is gone;
    /// focus then goes back to the button that opened it, or to the status message when that button is gone. Cancel or
    /// Escape returns focus to the opener.
    /// </summary>
    private void OnDialogHidden()
    {
        var action = pendingAction;
        pendingAction = null;
        var opener = dialogOpener;

        if (action is not null && !action())
            focusStatus = true;
        else
            focusTarget = () => opener?.Element;
    }

    private void OnDialogShown() => focusTarget = dialog switch
    {
        DialogKind.Severity => () => severityInput?.Element,
        DialogKind.Resolve or DialogKind.Reopen => () => reasonInput?.Element,
        _ => null,
    };

    /// <summary>A fresh internal note form in a new edit context, so no old message stays.</summary>
    private void NewNoteForm()
    {
        noteForm = new NoteForm();
        noteContext = new EditContext(noteForm);
    }

    /// <summary>A fresh status page update form at a stage, in a new edit context.</summary>
    private void NewPostForm(MonitoringPublicStage stage)
    {
        postForm = new PostForm { Stage = stage };
        postContext = new EditContext(postForm);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDialogAsync(DialogKind kind, Button? opener)
    {
        if (Incident is not { } incident)
            return;

        dialog = kind;
        dialogOpener = opener;
        pendingAction = null;
        var serviceName = ServiceName(incident);

        dialogForm = new DialogForm
        {
            Kind = kind,
            CurrentSeverity = incident.Severity,
            Severity = incident.Severity,
            IsPublished = incident.IsPublished,
            IncidentId = incident.Id,
            PublicText = kind switch
            {
                DialogKind.Resolve when incident.IsPublished =>
                    $"This incident has been resolved and {serviceName} is working normally again. Thank you for your patience, and we're sorry for the disruption.",
                DialogKind.Reopen when incident.IsPublished =>
                    $"The problem affecting {serviceName} has come back, and we are investigating again. We'll post an update as soon as we know more.",
                _ => string.Empty,
            },
        };
        dialogContext = new EditContext(dialogForm);

        await dialogModal.ShowAsync();
    }

    /// <summary>Moves the shown samples up to the clock (there is no timer on this page) and says the new numbers once.</summary>
    private void RefreshLive()
    {
        if (Incident is not { } incident || State.FindService(incident.ServiceId) is not { } service)
            return;

        State.Tick();
        var latest = State.Latest(service.Id);
        Announce(true, $"{service.Name} at {MonitoringDisplay.Time(latest.Time)}: p95 latency {MonitoringDisplay.Latency(latest.LatencyMs)}, error rate {MonitoringDisplay.ErrorRate(latest.ErrorRate)}, {State.Status(service).ToString().ToLowerInvariant()}.");
    }

    private void RemoveActionItem(ActionItemDraft item)
    {
        pmItems.Remove(item);
        focusTarget = () => addItemButton?.Element;
    }

    /// <summary>Checks the postmortem form and asks to confirm publishing it (or saving a published one).</summary>
    private async Task RequestPublishFromFormAsync()
    {
        if (ValidPostmortem(publish: true) is not { } request)
            return;

        pmRequest = request;
        await OpenDialogAsync(DialogKind.PublishPostmortem, pmPublishButton);
    }

    /// <summary>Asks to confirm publishing the saved draft as it is.</summary>
    private async Task RequestPublishFromViewAsync()
    {
        if (Incident?.Postmortem is not { } postmortem)
            return;

        pmRequest = new PostmortemRequest(postmortem.Summary, postmortem.RootCause, postmortem.ActionItems.ToList(), true);
        await OpenDialogAsync(DialogKind.PublishPostmortem, pmPublishButton);
    }

    /// <summary>Saves a draft; Enter in a published postmortem's form asks to confirm instead, since saving it is public.</summary>
    private async Task SavePostmortemDraftAsync()
    {
        if (Incident is not { } incident)
            return;

        if (incident.Postmortem?.PublishedAt is not null)
        {
            await RequestPublishFromFormAsync();
            return;
        }

        if (ValidPostmortem(publish: false) is not { } request)
            return;

        if (!ApplyPostmortem(incident, request))
            focusStatus = true;
    }

    private string ServiceName(MonitoringIncident incident) => State.FindService(incident.ServiceId)?.Name ?? incident.ServiceId;

    private void StartPostmortemEdit()
    {
        var postmortem = Incident?.Postmortem;

        pmForm = new PostmortemForm { Summary = postmortem?.Summary ?? string.Empty, RootCause = postmortem?.RootCause ?? string.Empty };
        pmContext = new EditContext(pmForm);
        pmItems = (postmortem?.ActionItems ?? new List<string>()).Select(text => new ActionItemDraft(pmNextKey++, text)).ToList();
        editingPostmortem = true;
        focusTarget = () => pmSummaryInput?.Element;
    }

    /// <summary>
    /// Checks the dialog's fields (a failed submit shows the summary and moves focus to it, rule WR-002), then closes it;
    /// the action runs once it is hidden.
    /// </summary>
    private async Task SubmitDialogAsync()
    {
        if (pendingAction is not null || Incident is not { } incident)
            return;

        // The incident may have changed since the dialog opened (another tab, a rule).
        dialogForm.CurrentSeverity = incident.Severity;
        dialogForm.IsPublished = incident.IsPublished;

        if ((dialog is DialogKind.Severity or DialogKind.Resolve or DialogKind.Reopen) && !dialogContext.Validate())
            return;

        var reason = dialogForm.Reason.Trim();
        var message = dialogForm.PublicText.Trim();

        switch (dialog)
        {
            case DialogKind.Severity:
                var severity = dialogForm.Severity;
                pendingAction = () => ApplySeverity(incident, severity, reason);
                break;

            case DialogKind.Resolve:
                pendingAction = () => ApplyResolve(incident, reason, message);
                break;

            case DialogKind.Reopen:
                pendingAction = () => ApplyReopen(incident, reason, message);
                break;

            case DialogKind.FirstPublic:
                var stage = postForm.Stage;
                var text = postForm.Text.Trim();
                pendingAction = () => ApplyPost(incident, stage, text);
                break;

            case DialogKind.PublishPostmortem when pmRequest is { } request:
                pendingAction = () => ApplyPostmortem(incident, request);
                break;

            case DialogKind.UnpublishPostmortem when incident.Postmortem is { } postmortem:
                var unpublish = new PostmortemRequest(postmortem.Summary, postmortem.RootCause, postmortem.ActionItems.ToList(), false);
                pendingAction = () => ApplyPostmortem(incident, unpublish);
                break;
        }

        await dialogModal.HideAsync();
    }

    /// <summary>Adds the note once the form is valid (a failed submit shows the summary, rule WR-002); the state checks again.</summary>
    private void SubmitNote()
    {
        if (Incident is not { } incident || !noteContext.Validate())
            return;

        var text = noteForm.Text.Trim();
        var result = State.AddNote(incident.Id, text);

        Announce(result == MonitoringResult.Ok, result switch
        {
            MonitoringResult.Ok => "Note added to the timeline. Only engineers see it.",
            MonitoringResult.Invalid => $"The note couldn't be added: #{incident.Id}'s timeline or this tab's storage is full.",
            MonitoringResult.NoChange or MonitoringResult.WrongState => "The note wasn't added.",
            _ => SharedRefusal(result, incident.Id) ?? "That didn't work, and the note wasn't added. Try again.",
        });

        if (result == MonitoringResult.Ok)
            NewNoteForm();
    }

    /// <summary>
    /// Posts an update once the form is valid (a failed submit shows the summary, rule WR-002); the first one of an
    /// engineers-only incident is confirmed first, since it makes the incident public.
    /// </summary>
    private async Task SubmitPostAsync()
    {
        if (Incident is not { } incident || !postContext.Validate())
            return;

        if (!incident.IsPublished)
        {
            await OpenDialogAsync(DialogKind.FirstPublic, postButton);
            return;
        }

        if (!ApplyPost(incident, postForm.Stage, postForm.Text.Trim()))
            focusStatus = true;
    }

    private int SubscriberCount(MonitoringIncident incident) =>
        State.Subscriptions.Count(subscription => subscription.ServiceIds.Count == 0 || subscription.ServiceIds.Contains(incident.ServiceId));

    /// <summary>Checks the postmortem form; a failed check shows the summary and moves focus to it (rule WR-002).</summary>
    private PostmortemRequest? ValidPostmortem(bool publish) =>
        pmContext.Validate()
            ? new PostmortemRequest(pmForm.Summary.Trim(), pmForm.RootCause.Trim(), pmItems.Select(item => item.Text.Trim()).Where(text => text.Length > 0).ToList(), publish)
            : null;

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Incidents", Href = IncidentsPath },
        new() { Text = $"#{Id}", IsCurrentPage = true },
    };

    /// <summary>Gets or sets <c>1</c> when the incidents page just declared this incident, to announce it.</summary>
    [SupplyParameterFromQuery(Name = "declared")] public string? Declared { get; set; }

    /// <summary>Gets or sets the incident number from the route.</summary>
    [Parameter] public int Id { get; set; }

    private MonitoringIncident? Incident => State.FindIncident(Id);

    private static string IncidentsPath => $"{ConsoleLayout.Base}/incidents";

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>What the dialog confirms.</summary>
    private enum DialogKind
    {
        None,
        Severity,
        Resolve,
        Reopen,
        FirstPublic,
        PublishPostmortem,
        UnpublishPostmortem,
    }

    /// <summary>One action item being edited; the key keeps its input when others are removed.</summary>
    private sealed class ActionItemDraft
    {
        public ActionItemDraft(int key, string text)
        {
            Key = key;
            Text = text;
        }

        public ElementReference? Input { get; set; }

        public int Key { get; }

        public string Text { get; set; }
    }

    /// <summary>
    /// The confirmation dialog's fields, checked for the dialog that is open: a new severity needs a different value and a
    /// reason; reopening needs a reason; resolving or reopening a published incident needs its public message.
    /// </summary>
    private sealed class DialogForm : IValidatableObject
    {
        public MonitoringSeverity CurrentSeverity { get; set; }

        public int IncidentId { get; set; }

        public bool IsPublished { get; set; }

        public DialogKind Kind { get; set; }

        [StringLength(MonitoringState.MaxUpdateLength, ErrorMessage = "Use 1,000 characters or fewer for the message.")]
        public string PublicText { get; set; } = string.Empty;

        [StringLength(MonitoringState.MaxUpdateLength, ErrorMessage = "Use 1,000 characters or fewer.")]
        public string Reason { get; set; } = string.Empty;

        public MonitoringSeverity Severity { get; set; }

        /// <summary>Runs once the lengths are fine: the rules of the open dialog.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Kind == DialogKind.Severity && Severity == CurrentSeverity)
                yield return new ValidationResult($"Choose a different severity: #{IncidentId} is already {CurrentSeverity}.", new[] { nameof(Severity) });

            if (Kind == DialogKind.Severity && string.IsNullOrWhiteSpace(Reason))
                yield return new ValidationResult("Say why the severity changes.", new[] { nameof(Reason) });

            if (Kind == DialogKind.Reopen && string.IsNullOrWhiteSpace(Reason))
                yield return new ValidationResult("Say what came back.", new[] { nameof(Reason) });

            if ((Kind is DialogKind.Resolve or DialogKind.Reopen) && IsPublished && string.IsNullOrWhiteSpace(PublicText))
                yield return new ValidationResult(
                    Kind == DialogKind.Resolve ? "Write the \"Resolved\" message customers will read." : "Write the \"Investigating\" message customers will read.",
                    new[] { nameof(PublicText) });
        }
    }

    /// <summary>The internal note form, with the limit of <see cref="MonitoringState.AddNote" />.</summary>
    private sealed class NoteForm
    {
        [Required(ErrorMessage = "Write the note first.")]
        [StringLength(MonitoringState.MaxUpdateLength, ErrorMessage = "Use 1,000 characters or fewer for the note.")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>The postmortem form's texts, with the limits of <see cref="MonitoringState.SavePostmortem" />.</summary>
    private sealed class PostmortemForm
    {
        [Required(ErrorMessage = "Say why it happened.")]
        [StringLength(MonitoringState.MaxPostmortemLength, ErrorMessage = "Use 2,000 characters or fewer for the root cause.")]
        public string RootCause { get; set; } = string.Empty;

        [Required(ErrorMessage = "Say what happened and how customers were affected.")]
        [StringLength(MonitoringState.MaxPostmortemLength, ErrorMessage = "Use 2,000 characters or fewer for what happened.")]
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>The status page update form, with the limit of <see cref="MonitoringState.PostPublicUpdate" />.</summary>
    private sealed class PostForm
    {
        public MonitoringPublicStage Stage { get; set; } = MonitoringPublicStage.Investigating;

        [Required(ErrorMessage = "Write the update customers will read.")]
        [StringLength(MonitoringState.MaxUpdateLength, ErrorMessage = "Use 1,000 characters or fewer for the update.")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>A validated postmortem to save.</summary>
    private sealed record PostmortemRequest(string Summary, string RootCause, IReadOnlyList<string> ActionItems, bool Publish);

    #endregion
}
