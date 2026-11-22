using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Maintenance windows (admins): every window in a <see cref="Grid{TItem}" /> (title and description, status in words,
/// services, site-time start and end, duration, who scheduled it, and where it stands on the status page) with a status
/// filter, paging, sorting, the column chooser, CSV export, and a saved view. A scheduled window can be edited (the whole
/// row fills the form) or cancelled; one in progress can be ended now. Both are confirmed in a <see cref="Modal" /> with
/// an "Email subscribers" option that says how many follow its services. The "Schedule maintenance" form is an
/// <see cref="EditForm" /> (rule WR-002): a title, a description, the services as checkboxes (at least one), start and end
/// as <c>datetime-local</c> in site time (with the site's current time and quick presets), and the email option. Its rules
/// are the state's: the start not passed and within 90 days, the end after the start and at most 24 hours later, and no
/// overlap with another window on the same service. Saving is confirmed too, since it is announced on the status page and
/// may be emailed. Actions run once the dialog is hidden, and every outcome is announced in a status message. The page
/// re-renders every 30 seconds, so windows move from Scheduled to In progress to Completed as the clock runs.
/// </summary>
public partial class AdminMaintenance : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many days before it starts the status page announces a window (as <see cref="MonitoringState.PublicMaintenance" /> does).</summary>
    private const int AnnounceDays = 14;

    private const int DefaultPageSize = 10;

    /// <summary>How far ahead a window may start.</summary>
    private const int MaxDaysAhead = 90;

    /// <summary>The key of the form's submit button among the dialog openers.</summary>
    private const string SubmitKey = "submit";

    /// <summary>How often the page re-renders, so statuses follow the clock (no samples are taken).</summary>
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(30);

    private ElementReference? countRegion;

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

    private EditContext editContext = new(new MaintenanceForm());

    /// <summary>The key of the Edit button that filled the form, so Stop editing can return focus to it.</summary>
    private string? editOpenerKey;

    /// <summary>The window being edited; <see langword="null" /> while scheduling a new one.</summary>
    private int? editingId;

    /// <summary>The edited window's title as saved, for the form's heading.</summary>
    private string editingTitle = string.Empty;

    /// <summary>Returns the element to focus after the next render (a field, the status message, or a button).</summary>
    private Func<ElementReference?>? focusTarget;

    private MaintenanceForm form = new();

    private ElementReference? formHeading;

    private Grid<WindowRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The Edit, Cancel, End now, and submit buttons, by key.</summary>
    private readonly Dictionary<string, ElementReference> openers = new();

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The confirmed action, run once the dialog is hidden (it changes the page under the dialog).</summary>
    private Func<Task>? pendingAction;

    /// <summary>The checked form values waiting for the save confirmation.</summary>
    private Draft? pendingDraft;

    private string? presetStatus;

    /// <summary>The state's answers that belong to a field (an overlapping window, a deleted service), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    /// <summary>The grid's status filter: empty for every window, a status name, or <c>past</c>.</summary>
    private string statusFilter = string.Empty;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

    private MonitoringLiveTimer? timer;

    private InputText? titleInput;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // The saved view lives in the tab's session storage, so it loads once the page is interactive; then the grid shows.
        if (firstRender && !disposed)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
            timer = new MonitoringLiveTimer(RefreshInterval, OnRefreshAsync);
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await MonitoringDisplay.FocusAsync(target());
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "maintenance", DefaultPageSize, "start", SortDirection.Descending);
        NewForm(new MaintenanceForm());
        State.Changed += OnStateChanged;
    }

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

    /// <summary>Formats a window's end: the time alone on the start's day, else the date too.</summary>
    private static string EndText(DateTime start, DateTime end) =>
        end.Date == start.Date ? end.ToString("HH:mm", CultureInfo.InvariantCulture) : MonitoringDisplay.Stamp(end);

    /// <summary>Formats a time for a <c>datetime-local</c> input or a <c>time</c> element (site time, invariant culture).</summary>
    private static string InputValue(DateTime time) => time.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

    private static string RangeText(DateTime start, DateTime end) => $"{MonitoringDisplay.Stamp(start)} to {EndText(start, end)}";

    private static string Subscribers(int count) => count == 1 ? "1 subscriber" : $"{count.ToString(CultureInfo.InvariantCulture)} subscribers";

    /// <summary>Drops the seconds, so a preset fills a <c>datetime-local</c> input exactly.</summary>
    private static DateTime ToMinute(DateTime time) => new(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, time.Kind);

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

        form.Start = start;
        form.End = end;
        editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.Start));
        editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.End));
        presetStatus = $"Start set to {MonitoringDisplay.Stamp(start)}, end to {EndText(start, end)}.";
    }

    /// <summary>Saves the confirmed form: schedules a new window, or changes the edited one.</summary>
    private async Task ApplySave(Draft draft, int? windowId)
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

            case MonitoringResult.Duplicate:
                var overlap = State.OverlappingMaintenance(draft.ServiceIds, draft.Start, draft.End, windowId);
                ServerError(FieldIdentifier.Create(() => form.Start), overlap is null
                    ? "Another maintenance window on these services covers that time. Choose another time."
                    : $"“{overlap.Title}” already covers {ServiceNames(overlap.ServiceIds.Where(draft.ServiceIds.Contains))} from {RangeText(overlap.Start, overlap.End)}. Choose another time, or edit that window.");
                statusMessage = $"Nothing was {verb}: it overlaps another window. Check the start and end.";
                statusIsError = true;

                return;

            case MonitoringResult.WrongState when saved is not null:
                var status = saved.StatusAt(now);
                SetStatus(status switch
                {
                    MonitoringMaintenanceStatus.InProgress => $"“{saved.Title}” has already started, so it can't be edited. End it early if you need to; nothing changed.",
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
                form.ServiceIds = ChosenIds.ToList();
                ServerError(FieldIdentifier.Create(() => form.ServiceIds), "One of the chosen services was deleted. Check the services again.");
                statusMessage = $"One of the chosen services no longer exists, so nothing was {verb}. Check the services and try again.";
                statusIsError = true;

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
                form.Now = State.Clock.Now;

                if (!await editContext.ValidateAsync())
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
                pendingAction = () => { FinishWindow(window, notifyChoice, cancel: true); return Task.CompletedTask; };

                break;

            case DialogKind.End when dialogWindow is { } window:
                pendingAction = () => { FinishWindow(window, notifyChoice, cancel: false); return Task.CompletedTask; };

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

    /// <summary>Says how many windows there are and how many are coming up.</summary>
    private string CountText(DateTime now)
    {
        var windows = State.MaintenanceWindows;
        var running = windows.Count(window => window.StatusAt(now) == MonitoringMaintenanceStatus.InProgress);
        var scheduled = windows.Count(window => window.StatusAt(now) == MonitoringMaintenanceStatus.Scheduled);

        return string.Create(CultureInfo.InvariantCulture, $"{windows.Count} windows: {running} in progress, {scheduled} scheduled");
    }

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

    /// <summary>Fills the form with a scheduled window, to change it, and moves focus to its title.</summary>
    private void Edit(MonitoringMaintenance window)
    {
        editingId = window.Id;
        editingTitle = window.Title;
        editOpenerKey = $"edit-{window.Id}";
        NewForm(new MaintenanceForm
        {
            Title = window.Title, Description = window.Description, ServiceIds = window.ServiceIds.ToList(), Start = window.Start, End = window.End, Notify = true,
        });
        presetStatus = null;
        focusTarget = () => titleInput?.Element;
    }

    private static string EmailPlan(int followers) =>
        followers == 0 ? "Nobody follows these services yet, so no email is sent." : $"It's emailed to {Subscribers(followers)}.";

    private static string EmailResult(bool notify, int followers) =>
        !notify ? "No email was sent."
        : followers == 0 ? "Nobody follows its services yet, so no email was sent."
        : $"{Subscribers(followers)} emailed.";

    /// <summary>A filter changed: back to the first page of the matching windows.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

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

    /// <summary>Puts a form in a new edit context, so no old message stays; the overlap rule asks the state.</summary>
    private void NewForm(MaintenanceForm value)
    {
        value.Now = State.Clock.Now;
        value.Overlap = (ids, start, end) => State.OverlappingMaintenance(ids, start, end, editingId);
        value.Names = ServiceNames;
        form = value;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += (_, args) =>
        {
            serverErrors?.Clear(args.FieldIdentifier);

            // An overlap message sits on the start; changing the end or the services can clear it too.
            serverErrors?.Clear(FieldIdentifier.Create(() => form.Start));
        };
    }

    private string NotifyLabel(int followers, bool editing) =>
        (editing ? "Email subscribers about the change" : "Email subscribers")
        + (followers == 0 ? " (nobody follows these services yet)" : $" ({Subscribers(followers)} follow these services)");

    private Task OnDialogShownAsync() => MonitoringDisplay.FocusAsync(dialogDismiss);

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. A confirmed action runs now that the dialog is gone,
    /// and focus moves to its outcome; Cancel or Escape returns focus to the button that opened the dialog.
    /// </summary>
    private async Task OnDialogHidden()
    {
        var action = pendingAction;
        var openerKey = dialogOpenerKey;
        pendingAction = null;
        pendingDraft = null;
        dialogWindow = null;
        dialogOpenerKey = null;

        if (action is not null)
            await action();
        else if (openerKey is not null && openers.TryGetValue(openerKey, out var opener))
            focusTarget = () => opener;
    }

    private Task OnRefreshAsync() => disposed ? Task.CompletedTask : InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>A start without an end gets the usual hour, which the admin can change.</summary>
    private void OnStartChanged()
    {
        if (form.End is null && form.Start is { } start)
        {
            form.End = start.AddHours(1);
            editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.End));
        }
    }

    /// <summary>A window or service changed (here or elsewhere): the grid reads the windows again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenDialogAsync(DialogKind kind, string openerKey)
    {
        dialog = kind;
        dialogOpenerKey = openerKey;
        pendingAction = null;

        await dialogModal.ShowAsync();
    }

    /// <summary>The page of windows the grid asked for, the latest start first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<WindowRow>> ProvideWindowsAsync(GridDataProviderRequest<WindowRow> request)
    {
        var now = State.Clock.Now;
        var rows = State.MaintenanceWindows
            .Select(window => new WindowRow(window, window.StatusAt(now), ServiceNames(window.ServiceIds), StatusPageText(window, window.StatusAt(now), now) ?? "Not shown (over)"))
            .Where(row => statusFilter.Length == 0
                || (statusFilter == "past" ? row.StatusValue is MonitoringMaintenanceStatus.Completed or MonitoringMaintenanceStatus.Cancelled : row.StatusValue.ToString() == statusFilter))
            .ToList();

        return view.ApplyAsync(request, rows);
    }

    /// <summary>The names of the services that are on the status page (the ones customers see).</summary>
    private IReadOnlyList<string> PublicNames(IEnumerable<string> serviceIds) =>
        serviceIds.Select(id => State.FindService(id)).Where(service => service is { IsPublic: true }).Select(service => service!.Name).ToList();

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Empties the form back to a new window.</summary>
    private void ResetForm()
    {
        editingId = null;
        editingTitle = string.Empty;
        editOpenerKey = null;
        NewForm(new MaintenanceForm());
        presetStatus = null;
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        statusMessage = "The view is reset: the latest start first, 10 windows a page, and every column shown.";
        statusIsError = false;
        StateHasChanged();

        await Task.Yield();
        await MonitoringDisplay.FocusAsync(countRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(FieldIdentifier field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(field, message);
        editContext.NotifyValidationStateChanged();
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

        focusTarget = () => titleInput?.Element;
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

    /// <summary>
    /// Checks the form (a failed submit shows the summary and moves focus to it, rule WR-002) against the site's clock, and
    /// asks to confirm the save.
    /// </summary>
    private async Task SubmitFormAsync()
    {
        serverErrors?.Clear();
        form.Now = State.Clock.Now;
        form.ServiceIds = ChosenIds.ToList();

        if (!await editContext.ValidateAsync() || form.Start is not { } start || form.End is not { } end)
            return;

        pendingDraft = new Draft(form.Title.Trim(), form.Description.Trim(), form.ServiceIds, start, end, form.Notify);
        dialogWindow = null;

        await OpenDialogAsync(DialogKind.Save, SubmitKey);
    }

    private void ToggleService(string serviceId, ChangeEventArgs e)
    {
        var ids = form.ServiceIds.ToList();

        if (e.Value is true || (e.Value is string text && bool.TryParse(text, out var isOn) && isOn))
        {
            if (!ids.Contains(serviceId))
                ids.Add(serviceId);
        }
        else
        {
            ids.Remove(serviceId);
        }

        form.ServiceIds = ids;
        editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.ServiceIds));
    }

    /// <summary>Summarizes the window the start and end make, or nothing while they don't make one.</summary>
    private string? WindowSummary() =>
        form.Start is { } start && form.End is { } end && end > start
            ? $"{RangeText(start, end)} · {MonitoringDisplay.Duration(end - start)}"
            : null;

    /// <summary>Explains a refused cancel or end: the window started, ended, or was cancelled meanwhile.</summary>
    private static string WrongStateText(string windowTitle, MonitoringMaintenanceStatus? status, bool cancel) => status switch
    {
        MonitoringMaintenanceStatus.InProgress when cancel => $"“{windowTitle}” has already started, so it can't be cancelled. End it now instead; nothing changed.",
        MonitoringMaintenanceStatus.Scheduled => $"“{windowTitle}” hasn't started yet, so it can't be ended. Cancel it instead; nothing changed.",
        MonitoringMaintenanceStatus.Completed => $"“{windowTitle}” has already ended. Nothing changed.",
        MonitoringMaintenanceStatus.Cancelled => $"“{windowTitle}” was already cancelled. Nothing changed.",
        _ => $"“{windowTitle}” can't be {(cancel ? "cancelled" : "ended")} right now. Nothing changed.",
    };

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the chosen services that still exist, in the services' order.</summary>
    private IReadOnlyList<string> ChosenIds => State.Services.Where(service => form.ServiceIds.Contains(service.Id)).Select(service => service.Id).ToList();

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

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

    /// <summary>
    /// The maintenance form's fields, with the limits of <see cref="MonitoringState.ScheduleMaintenance" />: a title and a
    /// description, at least one service, and the cross-field time rules (the start not passed and within 90 days, the end
    /// after the start and at most 24 hours later, and no overlap with another window on the same service).
    /// </summary>
    private sealed class MaintenanceForm : IValidatableObject
    {
        [Required(ErrorMessage = "Say what customers should expect during the window.")]
        [StringLength(MonitoringState.MaxMaintenanceDescriptionLength, ErrorMessage = "Use 1,000 characters or fewer for the description.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the end date and time.")]
        public DateTime? End { get; set; }

        /// <summary>Names services for a message; set by the page.</summary>
        public Func<IEnumerable<string>, string>? Names { get; set; }

        public bool Notify { get; set; } = true;

        /// <summary>The site's time when the form is checked; set by the page.</summary>
        public DateTime Now { get; set; }

        /// <summary>Finds a window that would overlap this one; set by the page.</summary>
        public Func<IReadOnlyList<string>, DateTime, DateTime, MonitoringMaintenance?>? Overlap { get; set; }

        [MinLength(1, ErrorMessage = "Choose at least one service.")]
        public List<string> ServiceIds { get; set; } = new();

        [Required(ErrorMessage = "Enter the start date and time.")]
        public DateTime? Start { get; set; }

        [Required(ErrorMessage = "Enter a title, for example “Orders database upgrade”.")]
        [StringLength(MonitoringState.MaxMaintenanceTitleLength, ErrorMessage = "Use 120 characters or fewer for the title.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: the start and end against the clock and each other, then the overlap.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Start is not { } start || End is not { } end)
                yield break;

            var timesValid = true;

            if (start < Now.AddMinutes(-1))
            {
                timesValid = false;
                yield return new ValidationResult($"The start has passed. Choose {MonitoringDisplay.Stamp(Now)} or later, or use “Now, for 1 hour”.", new[] { nameof(Start) });
            }
            else if (start > Now.AddDays(MaxDaysAhead))
            {
                timesValid = false;
                yield return new ValidationResult($"Start within {MaxDaysAhead} days, by {MonitoringDisplay.Stamp(Now.AddDays(MaxDaysAhead))}.", new[] { nameof(Start) });
            }

            if (end <= start)
            {
                timesValid = false;
                yield return new ValidationResult($"End after the start ({MonitoringDisplay.Stamp(start)}).", new[] { nameof(End) });
            }
            else if (end - start > MonitoringState.MaxMaintenanceLength)
            {
                timesValid = false;
                yield return new ValidationResult($"Keep a window to {MaxHours} hours or less: end by {MonitoringDisplay.Stamp(start.Add(MonitoringState.MaxMaintenanceLength))}.", new[] { nameof(End) });
            }

            if (timesValid && Overlap?.Invoke(ServiceIds, start, end) is { } other)
                yield return new ValidationResult(
                    $"“{other.Title}” already covers {Names?.Invoke(other.ServiceIds.Where(ServiceIds.Contains)) ?? "these services"} from {RangeText(other.Start, other.End)}. Choose another time, or edit that window.",
                    new[] { nameof(Start) });
        }
    }

    /// <summary>One row of the list: a window, its status now, and its texts, with properties the CSV export can read.</summary>
    private sealed record WindowRow(MonitoringMaintenance Window, MonitoringMaintenanceStatus StatusValue, string Services, string StatusPage)
    {
        public string Duration => MonitoringDisplay.Duration(Window.End - Window.Start);

        public DateTime End => Window.End;

        public string ScheduledBy => Window.CreatedBy;

        public DateTime Start => Window.Start;

        public string Status => MonitoringDisplay.MaintenanceStatusLabel(StatusValue);

        public string Title => Window.Title;
    }

    #endregion
}
