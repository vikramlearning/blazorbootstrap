using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Maintenance windows (admins): the windows in progress, scheduled, and past (completed and cancelled), each with its
/// services, site-time range and duration, status in words, who scheduled it, its description, and where it stands on the
/// status page. A scheduled window can be edited (in the same form) or cancelled; one in progress can be ended now. Both
/// are confirmed in a <see cref="Modal" /> with an "Email subscribers" option that says how many follow its services. The
/// "Schedule maintenance" form has a title, a description with a counter, the services as checkboxes, start and end as
/// <c>datetime-local</c> in site time (with the site's current time and quick presets), and the email option; errors are
/// tied to their fields, and focus moves to the first one. Saving is confirmed too, since it is announced on the status
/// page and may be emailed. Actions run once the dialog is hidden, and every outcome is announced in a status message.
/// The page re-renders every 30 seconds, so windows move from Scheduled to In progress to Completed as the clock runs.
/// </summary>
public partial class AdminMaintenance : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many days before it starts the status page announces a window (as <see cref="MonitoringState.PublicMaintenance" /> does).</summary>
    private const int AnnounceDays = 14;

    /// <summary>How far ahead a window may start.</summary>
    private const int MaxDaysAhead = 90;

    /// <summary>The key of the form's submit button among the dialog openers.</summary>
    private const string SubmitKey = "submit";

    /// <summary>How often the page re-renders, so statuses follow the clock (no samples are taken).</summary>
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(30);

    private readonly HashSet<string> chosen = new();

    private string description = string.Empty;

    private string? descriptionError;

    private ElementReference? descriptionInput;

    /// <summary>What the dialog is confirming.</summary>
    private DialogKind dialog;

    private ElementReference? dialogDismiss;

    private Modal dialogModal = default!;

    /// <summary>Whether the cancel or end confirmation emails subscribers.</summary>
    private bool dialogNotify = true;

    /// <summary>The key of the button that opened the dialog, so Cancel can return focus to it.</summary>
    private string? dialogOpenerKey;

    /// <summary>The window the cancel or end confirmation is about.</summary>
    private MonitoringMaintenance? dialogWindow;

    private bool disposed;

    /// <summary>The key of the Edit button that filled the form, so Stop editing can return focus to it.</summary>
    private string? editOpenerKey;

    /// <summary>The window being edited; <see langword="null" /> while scheduling a new one.</summary>
    private int? editingId;

    /// <summary>The edited window's title as saved, for the form's heading.</summary>
    private string editingTitle = string.Empty;

    private string endText = string.Empty;

    private string? endError;

    private ElementReference? endInput;

    /// <summary>Returns the element to focus after the next render (a field with an error, the status message, or a button).</summary>
    private Func<ElementReference?>? focusTarget;

    private ElementReference? formHeading;

    private bool notify = true;

    /// <summary>The Edit, Cancel, End now, and submit buttons, by key.</summary>
    private readonly Dictionary<string, ElementReference> openers = new();

    /// <summary>The confirmed action, run once the dialog is hidden (it changes the page under the dialog).</summary>
    private Action? pendingAction;

    /// <summary>The checked form values waiting for the save confirmation.</summary>
    private Draft? pendingDraft;

    private string? presetStatus;

    private readonly Dictionary<string, ElementReference> serviceInputs = new();

    private string? servicesError;

    private string startText = string.Empty;

    private string? startError;

    private ElementReference? startInput;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

    private MonitoringLiveTimer? timer;

    private string title = string.Empty;

    private string? titleError;

    private ElementReference? titleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !disposed)
            timer = new MonitoringLiveTimer(RefreshInterval, OnRefreshAsync);

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await MonitoringDisplay.FocusAsync(target());
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops the refresh timer and stops listening to state changes.</summary>
    public void Dispose()
    {
        disposed = true;
        timer?.Dispose();
        State.Changed -= OnStateChanged;
    }

    #endregion

    #region Private Methods

    private static string? AriaInvalid(string? error) => error is null ? null : "true";

    /// <summary>Formats a window's end: the time alone on the start's day, else the date too.</summary>
    private static string EndText(DateTime start, DateTime end) =>
        end.Date == start.Date ? end.ToString("HH:mm", CultureInfo.InvariantCulture) : MonitoringDisplay.Stamp(end);

    /// <summary>Formats a time for a <c>datetime-local</c> input or a <c>time</c> element (site time, invariant culture).</summary>
    private static string InputValue(DateTime time) => time.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

    private static string? Invalid(string? error) => error is null ? null : "is-invalid";

    private static string RangeText(DateTime start, DateTime end) => $"{MonitoringDisplay.Stamp(start)} to {EndText(start, end)}";

    private static string Subscribers(int count) => count == 1 ? "1 subscriber" : $"{count.ToString(CultureInfo.InvariantCulture)} subscribers";

    /// <summary>Drops the seconds, so a preset fills a <c>datetime-local</c> input exactly.</summary>
    private static DateTime ToMinute(DateTime time) => new(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, time.Kind);

    /// <summary>Reads a <c>datetime-local</c> value (with or without seconds) as site time.</summary>
    private static DateTime? TryParseInput(string? text) =>
        DateTime.TryParseExact(text?.Trim(), new[] { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

    /// <summary>Says when the status page shows a window that is being saved.</summary>
    private static string AnnouncementText(IReadOnlyList<string> publicNames, DateTime start, DateTime now)
    {
        if (publicNames.Count == 0)
            return "None of these services are on the status page, so customers don't see it.";

        var names = string.Join(", ", publicNames);

        if (start <= now)
            return $"It starts now: the status page shows {names} as “Under maintenance”.";

        return start <= now.AddDays(AnnounceDays)
            ? $"The status page announces it at once, for {names}."
            : $"The status page announces it from {MonitoringDisplay.Stamp(start.AddDays(-AnnounceDays))} ({AnnounceDays} days before), for {names}.";
    }

    /// <summary>Applies one of the quick times and says what it set.</summary>
    private void ApplyPreset(Preset preset)
    {
        var now = ToMinute(State.Clock.Now);
        var nextNight = now.Date.AddHours(2);

        if (nextNight <= now)
            nextNight = nextNight.AddDays(1);

        var (start, end) = preset switch
        {
            Preset.Now => (now, now.AddHours(1)),
            Preset.InAnHour => (now.AddHours(1), now.AddHours(2)),
            _ => (nextNight, nextNight.AddHours(2)),
        };

        startText = InputValue(start);
        endText = InputValue(end);
        startError = endError = null;
        presetStatus = $"Start set to {MonitoringDisplay.Stamp(start)}, end to {EndText(start, end)}.";
    }

    /// <summary>Saves the confirmed form: schedules a new window, or changes the edited one.</summary>
    private void ApplySave(Draft draft, int? windowId)
    {
        var followers = Followers(draft.ServiceIds);
        var names = ServiceNames(draft.ServiceIds);
        MonitoringResult result;
        MonitoringMaintenance? saved = null;

        if (windowId is { } id)
        {
            result = State.UpdateMaintenance(id, draft.Title, draft.Description, draft.ServiceIds, draft.Start, draft.End, draft.Notify);
            saved = State.MaintenanceWindows.FirstOrDefault(window => window.Id == id);
        }
        else
        {
            result = State.ScheduleMaintenance(draft.Title, draft.Description, draft.ServiceIds, draft.Start, draft.End, draft.Notify, out saved);
        }

        var verb = windowId is null ? "scheduled" : "saved";
        var now = State.Clock.Now;

        switch (result)
        {
            case MonitoringResult.Ok when saved is not null:
                var announced = StatusPageText(saved, saved.StatusAt(now), now) ?? string.Empty;
                SetStatus($"“{saved.Title}” is {(windowId is null ? "scheduled" : "updated")}: {RangeText(saved.Start, saved.End)} on {names}. {announced}. {EmailResult(draft.Notify, followers)}", false);
                ResetForm();

                return;

            case MonitoringResult.NoChange:
                SetStatus($"You didn't change anything in “{editingTitle}”, so nothing was saved or emailed.", false);

                return;

            case MonitoringResult.WrongState when saved is not null:
                var status = saved.StatusAt(now);
                SetStatus(status switch
                {
                    MonitoringMaintenanceStatus.InProgress => $"“{saved.Title}” has already started, so it can't be edited. End it early under In progress if you need to; nothing changed.",
                    MonitoringMaintenanceStatus.Completed => $"“{saved.Title}” has already ended, so it can't be edited. Nothing changed.",
                    _ => $"“{saved.Title}” was cancelled, so it can't be edited. Nothing changed.",
                }, true);
                ResetForm();

                return;

            case MonitoringResult.NotFound when windowId is not null && saved is null:
            case MonitoringResult.WrongState:
                SetStatus($"“{editingTitle}” no longer exists (its services may have been deleted). Nothing changed.", true);
                ResetForm();

                return;

            case MonitoringResult.NotFound:
                chosen.IntersectWith(State.Services.Select(service => service.Id));
                servicesError = "One of the chosen services was deleted. Check the services again.";
                SetStatus($"One of the chosen services no longer exists, so nothing was {verb}. Check the services and try again.", true);
                focusTarget = () => FirstServiceInput;

                return;

            case MonitoringResult.NotAllowed:
                SetStatus($"Only admins can schedule or change maintenance. Sign in again as an admin; nothing was {verb}.", true);

                return;

            case MonitoringResult.Invalid:
                if (windowId is null && State.MaintenanceWindows.Count >= MonitoringState.MaxMaintenanceWindows)
                {
                    SetStatus($"This tab already holds {MonitoringState.MaxMaintenanceWindows} maintenance windows, the most it keeps, so nothing was scheduled.", true);

                    return;
                }

                // The clock moved on while the dialog was open (a start that just passed), or the tab is out of room.
                if (Validate() is null)
                {
                    statusMessage = $"Nothing was {verb}: check the fields marked below.";
                    statusIsError = true;

                    return;
                }

                SetStatus($"The window is too large to keep in this tab. Shorten the description and try again; nothing was {verb}.", true);

                return;

            default:
                SetStatus($"That didn't work, and nothing was {verb}. Try again.", true);

                return;
        }
    }

    /// <summary>Asks to cancel a scheduled window or end one in progress.</summary>
    private Task AskAsync(DialogKind kind, MonitoringMaintenance window, string openerKey)
    {
        dialogWindow = window;
        dialogNotify = true;
        pendingDraft = null;

        return OpenDialogAsync(kind, openerKey);
    }

    /// <summary>Records the confirmed action and closes the dialog; the action runs once it is hidden.</summary>
    private async Task ConfirmAsync()
    {
        if (pendingAction is not null)
            return;

        var notifyChoice = dialogNotify;

        switch (dialog)
        {
            case DialogKind.Save when pendingDraft is { } draft:
                var windowId = editingId;
                pendingAction = () => ApplySave(draft, windowId);

                break;

            case DialogKind.Cancel when dialogWindow is { } window:
                pendingAction = () => FinishWindow(window, notifyChoice, cancel: true);

                break;

            case DialogKind.End when dialogWindow is { } window:
                pendingAction = () => FinishWindow(window, notifyChoice, cancel: false);

                break;

            default:
                return;
        }

        await dialogModal.HideAsync();
    }

    private string ConfirmText() => dialog switch
    {
        DialogKind.Save => editingId is null ? "Schedule maintenance" : "Save changes",
        DialogKind.Cancel => "Cancel the maintenance",
        _ => "End maintenance now",
    };

    private string DialogNotifyLabel(int count) =>
        (dialog == DialogKind.Cancel ? "Email subscribers that it's cancelled" : "Email subscribers that it's complete")
        + (count == 0 ? " (nobody follows its services yet)" : $" ({Subscribers(count)} follow its services)");

    private string DialogTitle() => dialog switch
    {
        DialogKind.Save when pendingDraft is { } draft => editingId is null ? $"Schedule “{draft.Title}”?" : $"Save the changes to “{draft.Title}”?",
        DialogKind.Cancel when dialogWindow is { } window => $"Cancel “{window.Title}”?",
        DialogKind.End when dialogWindow is { } window => $"End “{window.Title}” now?",
        _ => "Maintenance",
    };

    private string DismissText() => dialog switch
    {
        DialogKind.Cancel => "Keep it scheduled",
        DialogKind.End => "Keep it running",
        _ => "Go back",
    };

    /// <summary>Fills the form with a scheduled window, to change it.</summary>
    private void Edit(MonitoringMaintenance window)
    {
        ClearErrors();
        editingId = window.Id;
        editingTitle = window.Title;
        editOpenerKey = $"edit-{window.Id}";
        title = window.Title;
        description = window.Description;
        chosen.Clear();
        chosen.UnionWith(window.ServiceIds);
        startText = InputValue(window.Start);
        endText = InputValue(window.End);
        notify = true;
        presetStatus = null;
        focusTarget = () => titleInput;
    }

    private static string EmailPlan(int followers) =>
        followers == 0 ? "Nobody follows these services yet, so no email is sent." : $"It's emailed to {Subscribers(followers)}.";

    private static string EmailResult(bool notify, int followers) =>
        !notify ? "No email was sent."
        : followers == 0 ? "Nobody follows its services yet, so no email was sent."
        : $"{Subscribers(followers)} emailed.";

    /// <summary>Cancels a scheduled window, or ends one in progress, and says what happened.</summary>
    private void FinishWindow(MonitoringMaintenance window, bool notifyChoice, bool cancel)
    {
        var followers = Followers(window.ServiceIds);
        var result = cancel ? State.CancelMaintenance(window.Id, notifyChoice) : State.EndMaintenanceNow(window.Id, notifyChoice);
        var current = State.MaintenanceWindows.FirstOrDefault(item => item.Id == window.Id);

        if (result == MonitoringResult.Ok && cancel && editingId == window.Id)
            ResetForm();

        var message = result switch
        {
            MonitoringResult.Ok when cancel => $"“{window.Title}” is cancelled and off the status page. {EmailResult(notifyChoice, followers)}",
            MonitoringResult.Ok => $"“{window.Title}” ended at {MonitoringDisplay.Stamp(State.Clock.Now)}. Its services are back to their usual status, and their alert rules fire again. {EmailResult(notifyChoice, followers)}",
            MonitoringResult.NotFound => $"“{window.Title}” no longer exists (its services may have been deleted). Nothing changed.",
            MonitoringResult.WrongState => WrongStateText(window.Title, current?.StatusAt(State.Clock.Now), cancel),
            MonitoringResult.NotAllowed => "Only admins can cancel or end maintenance. Sign in again as an admin; nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        };

        SetStatus(message, result != MonitoringResult.Ok);
    }

    /// <summary>Counts the subscriptions that would get an email about these services: every component, or any of them.</summary>
    private int Followers(IReadOnlyCollection<string> serviceIds) =>
        State.Subscriptions.Count(subscription => subscription.ServiceIds.Count == 0 || subscription.ServiceIds.Any(serviceIds.Contains));

    private string NotifyLabel(int followers, bool editing) =>
        (editing ? "Email subscribers about the change" : "Email subscribers")
        + (followers == 0 ? " (nobody follows these services yet)" : $" ({Subscribers(followers)} follow these services)");

    private void ClearErrors() => titleError = descriptionError = servicesError = startError = endError = null;

    private Task OnDialogShownAsync() => MonitoringDisplay.FocusAsync(dialogDismiss);

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. A confirmed action runs now that the dialog is gone,
    /// and focus moves to its outcome; Cancel or Escape returns focus to the button that opened the dialog.
    /// </summary>
    private void OnDialogHidden()
    {
        var action = pendingAction;
        var openerKey = dialogOpenerKey;
        pendingAction = null;
        pendingDraft = null;
        dialogWindow = null;
        dialogOpenerKey = null;

        if (action is not null)
            action();
        else if (openerKey is not null && openers.TryGetValue(openerKey, out var opener))
            focusTarget = () => opener;
    }

    // The datetime-local inputs bind by hand: @bind would treat them as DateTime, and an empty or partial value as an error.
    private void OnEndChanged(ChangeEventArgs e) => endText = e.Value?.ToString() ?? string.Empty;

    private void OnStartChanged(ChangeEventArgs e)
    {
        startText = e.Value?.ToString() ?? string.Empty;

        // A start without an end gets the usual hour, which the admin can change.
        if (endText.Length == 0 && TryParseInput(startText) is { } start)
            endText = InputValue(start.AddHours(1));
    }

    private Task OnRefreshAsync() => disposed ? Task.CompletedTask : InvokeAsync(StateHasChanged);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDialogAsync(DialogKind kind, string openerKey)
    {
        dialog = kind;
        dialogOpenerKey = openerKey;
        pendingAction = null;

        await dialogModal.ShowAsync();
    }

    /// <summary>The names of the services that are on the status page (the ones customers see).</summary>
    private IReadOnlyList<string> PublicNames(IEnumerable<string> serviceIds) =>
        serviceIds.Select(id => State.FindService(id)).Where(service => service is { IsPublic: true }).Select(service => service!.Name).ToList();

    /// <summary>Empties the form back to a new window.</summary>
    private void ResetForm()
    {
        ClearErrors();
        editingId = null;
        editingTitle = string.Empty;
        editOpenerKey = null;
        title = description = startText = endText = string.Empty;
        chosen.Clear();
        notify = true;
        presetStatus = null;
    }

    /// <summary>The page's three lists: in progress (soonest end first), scheduled (soonest first), and past (latest first).</summary>
    private IReadOnlyList<WindowSection> Sections(DateTime now)
    {
        var windows = State.MaintenanceWindows;

        return new List<WindowSection>
        {
            new("mt-running", "In progress", "No maintenance is running.",
                windows.Where(window => window.StatusAt(now) == MonitoringMaintenanceStatus.InProgress).OrderBy(window => window.End).ToList()),
            new("mt-scheduled", "Scheduled", "Nothing is scheduled. Use the form to plan a window.",
                windows.Where(window => window.StatusAt(now) == MonitoringMaintenanceStatus.Scheduled).OrderBy(window => window.Start).ToList()),
            new("mt-past", "Past", "No windows have finished or been cancelled yet.",
                windows.Where(window => window.StatusAt(now) is MonitoringMaintenanceStatus.Completed or MonitoringMaintenanceStatus.Cancelled).ToList()),
        };
    }

    private string ServiceNames(IEnumerable<string> serviceIds) => string.Join(", ", serviceIds.Select(id => State.FindService(id)?.Name ?? id));

    private void SetStatus(string message, bool isError)
    {
        statusMessage = message;
        statusIsError = isError;
        focusTarget = () => statusRegion;
    }

    /// <summary>Starts a new window: leaves editing, and moves focus to the title.</summary>
    private void StartNew()
    {
        if (editingId is not null)
            ResetForm();

        focusTarget = () => titleInput;
    }

    /// <summary>Says how a window shows on the status page now, or <see langword="null" /> for a past one.</summary>
    private string? StatusPageText(MonitoringMaintenance window, MonitoringMaintenanceStatus status, DateTime now)
    {
        if (status is MonitoringMaintenanceStatus.Completed or MonitoringMaintenanceStatus.Cancelled)
            return null;

        var names = PublicNames(window.ServiceIds);

        if (names.Count == 0)
            return "Not shown: none of its services are on the status page";

        if (status == MonitoringMaintenanceStatus.InProgress)
            return $"Shows {string.Join(", ", names)} as “Under maintenance”";

        return window.Start <= now.AddDays(AnnounceDays)
            ? "Announced now"
            : $"Announced from {MonitoringDisplay.Stamp(window.Start.AddDays(-AnnounceDays))}";
    }

    /// <summary>Leaves editing without saving, and returns focus to the Edit button (or the form's heading when it is gone).</summary>
    private void StopEditing()
    {
        var key = editOpenerKey;
        ResetForm();
        focusTarget = key is not null && openers.TryGetValue(key, out var opener) ? () => opener : () => formHeading;
    }

    /// <summary>Checks the form and asks to confirm the save.</summary>
    private async Task SubmitFormAsync()
    {
        if (Validate() is not { } draft)
            return;

        pendingDraft = draft;
        dialogWindow = null;

        await OpenDialogAsync(DialogKind.Save, SubmitKey);
    }

    private void ToggleService(string serviceId, ChangeEventArgs e)
    {
        if (e.Value is true)
            chosen.Add(serviceId);
        else
            chosen.Remove(serviceId);

        if (chosen.Count > 0)
            servicesError = null;
    }

    /// <summary>
    /// Checks the form as <see cref="MonitoringState.ScheduleMaintenance" /> does, with a message on each field; focus goes to
    /// the first invalid one after the render that shows it. Returns the values, or <see langword="null" /> when one is wrong.
    /// </summary>
    private Draft? Validate()
    {
        var now = State.Clock.Now;
        var trimmedTitle = title.Trim();
        var trimmedDescription = description.Trim();
        var serviceIds = ChosenIds;
        var start = TryParseInput(startText);
        var end = TryParseInput(endText);

        titleError = trimmedTitle.Length == 0 ? "Enter a title, for example “Orders database upgrade”."
            : trimmedTitle.Length > MonitoringState.MaxMaintenanceTitleLength ? $"Keep the title to {MonitoringState.MaxMaintenanceTitleLength} characters or fewer."
            : null;

        descriptionError = trimmedDescription.Length == 0 ? "Say what customers should expect during the window."
            : trimmedDescription.Length > MonitoringState.MaxMaintenanceDescriptionLength ? $"Keep the description to {MonitoringState.MaxMaintenanceDescriptionLength} characters or fewer."
            : null;

        servicesError = serviceIds.Count == 0 ? "Choose at least one service." : null;

        startError = start is not { } startValue ? "Enter the start date and time."
            : startValue < now.AddMinutes(-1) ? $"The start has passed. Choose {MonitoringDisplay.Stamp(now)} or later, or use “Now, for 1 hour”."
            : startValue > now.AddDays(MaxDaysAhead) ? $"Start within {MaxDaysAhead} days, by {MonitoringDisplay.Stamp(now.AddDays(MaxDaysAhead))}."
            : null;

        endError = end is not { } endValue ? "Enter the end date and time."
            : start is { } from && endValue <= from ? $"End after the start ({MonitoringDisplay.Stamp(from)})."
            : start is { } begin && endValue - begin > MonitoringState.MaxMaintenanceLength ? $"Keep a window to {MaxHours} hours or less: end by {MonitoringDisplay.Stamp(begin.Add(MonitoringState.MaxMaintenanceLength))}."
            : null;

        focusTarget = titleError is not null ? () => titleInput
            : descriptionError is not null ? () => descriptionInput
            : servicesError is not null ? () => FirstServiceInput
            : startError is not null ? () => startInput
            : endError is not null ? () => endInput
            : null;

        return focusTarget is null && start is { } validStart && end is { } validEnd
            ? new Draft(trimmedTitle, trimmedDescription, serviceIds, validStart, validEnd, notify)
            : null;
    }

    /// <summary>Summarizes the window the start and end make, or nothing while they don't make one.</summary>
    private string? WindowSummary() =>
        TryParseInput(startText) is { } start && TryParseInput(endText) is { } end && end > start
            ? $"{RangeText(start, end)} · {MonitoringDisplay.Duration(end - start)}"
            : null;

    /// <summary>Explains a refused cancel or end: the window started, ended, or was cancelled meanwhile.</summary>
    private static string WrongStateText(string windowTitle, MonitoringMaintenanceStatus? status, bool cancel) => status switch
    {
        MonitoringMaintenanceStatus.InProgress when cancel => $"“{windowTitle}” has already started, so it can't be cancelled. End it now under In progress instead; nothing changed.",
        MonitoringMaintenanceStatus.Scheduled => $"“{windowTitle}” hasn't started yet, so it can't be ended. Cancel it instead; nothing changed.",
        MonitoringMaintenanceStatus.Completed => $"“{windowTitle}” has already ended. Nothing changed.",
        MonitoringMaintenanceStatus.Cancelled => $"“{windowTitle}” was already cancelled. Nothing changed.",
        _ => $"“{windowTitle}” can't be {(cancel ? "cancelled" : "ended")} right now. Nothing changed.",
    };

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the chosen services that still exist, in the services' order.</summary>
    private IReadOnlyList<string> ChosenIds => State.Services.Where(service => chosen.Contains(service.Id)).Select(service => service.Id).ToList();

    /// <summary>Gets the first service checkbox, for focus when no service is chosen.</summary>
    private ElementReference? FirstServiceInput =>
        State.Services.Count > 0 && serviceInputs.TryGetValue(State.Services[0].Id, out var input) ? input : null;

    /// <summary>Gets the longest window in hours.</summary>
    private static int MaxHours => (int)MonitoringState.MaxMaintenanceLength.TotalHours;

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>What the dialog confirms.</summary>
    private enum DialogKind
    {
        /// <summary>Scheduling a new window, or saving changes to one.</summary>
        Save,

        /// <summary>Cancelling a scheduled window.</summary>
        Cancel,

        /// <summary>Ending a window in progress now.</summary>
        End,
    }

    /// <summary>The quick times.</summary>
    private enum Preset
    {
        /// <summary>From now, for an hour.</summary>
        Now,

        /// <summary>In an hour, for an hour.</summary>
        InAnHour,

        /// <summary>The next 02:00, for two hours.</summary>
        Night,
    }

    /// <summary>The checked values of the form.</summary>
    private sealed record Draft(string Title, string Description, IReadOnlyList<string> ServiceIds, DateTime Start, DateTime End, bool Notify);

    /// <summary>One of the page's lists of windows.</summary>
    private sealed record WindowSection(string Id, string Title, string EmptyText, IReadOnlyList<MonitoringMaintenance> Windows);

    #endregion
}
