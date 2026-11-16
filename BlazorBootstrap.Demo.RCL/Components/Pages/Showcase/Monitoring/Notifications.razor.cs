using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The signed-in engineer's notifications (<see cref="MonitoringState.Notifications" />), newest first, filtered by All or
/// Unread and by kind. Each shows its kind in words with an icon, its title, text, and age, a link to its incident when it
/// has one, and Mark read or unread; "Mark all read" clears the unread count. "How you're notified" saves the engineer's
/// <see cref="MonitoringState.Preferences" /> with native checkboxes. Results are announced in a status message; when a
/// notification leaves the Unread list, focus moves to that message.
/// </summary>
public partial class Notifications : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Set when the notification acted on left the list, so focus moves to the status message after that render.</summary>
    private bool focusStatusAfterRender;

    private string kindFilter = string.Empty;

    /// <summary>The preferences the checkboxes were last loaded from, so unsaved changes survive a state change.</summary>
    private MonitoringNotificationPreferences loaded = new();

    private bool prefEmail;

    private bool prefPush;

    private bool prefSms;

    private bool prefUpdates;

    private AlertColor statusColor = AlertColor.Success;

    private string? statusMessage;

    private ElementReference statusRegion;

    private bool unreadOnly;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatusAfterRender)
        {
            focusStatusAfterRender = false;
            await MonitoringDisplay.FocusAsync(statusRegion);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        LoadPreferences();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static IconName KindIcon(MonitoringNotificationKind kind) => kind switch
    {
        MonitoringNotificationKind.Alert => IconName.ExclamationOctagon,
        MonitoringNotificationKind.Assigned => IconName.PersonCheck,
        MonitoringNotificationKind.Update => IconName.ChatLeftText,
        _ => IconName.CalendarWeek,
    };

    private static string KindLabel(MonitoringNotificationKind kind) => kind switch
    {
        MonitoringNotificationKind.Alert => "Alert",
        MonitoringNotificationKind.Assigned => "Assigned to you",
        MonitoringNotificationKind.Update => "Incident update",
        _ => "On-call",
    };

    private static string NotAllowedText => "Sign in as an engineer to manage your notifications. Nothing changed.";

    /// <summary>Copies the saved preferences into the checkboxes.</summary>
    private void LoadPreferences()
    {
        var saved = State.Preferences;
        loaded = new MonitoringNotificationPreferences { Email = saved.Email, Push = saved.Push, SmsForCritical = saved.SmsForCritical, IncidentUpdates = saved.IncidentUpdates };
        prefEmail = saved.Email;
        prefPush = saved.Push;
        prefSms = saved.SmsForCritical;
        prefUpdates = saved.IncidentUpdates;
    }

    private void MarkAllRead()
    {
        var unread = State.UnreadCount;
        var result = State.MarkAllRead();

        Show(result == MonitoringResult.Ok ? AlertColor.Success : AlertColor.Info, result switch
        {
            MonitoringResult.Ok => unread == 1 ? "Marked 1 notification read." : $"Marked all {unread} notifications read.",
            MonitoringResult.NoChange => "Everything is already read. Nothing changed.",
            MonitoringResult.NotAllowed => NotAllowedText,
            _ => "That didn't work, and nothing changed. Try again.",
        });
    }

    private void OnKindChanged(ChangeEventArgs args) => kindFilter = args.Value?.ToString() ?? string.Empty;

    /// <summary>Reloads the checkboxes after a state change (a restored snapshot or another sign-in) unless they hold unsaved changes.</summary>
    private void OnStateChanged() => InvokeAsync(() =>
    {
        if (!IsDirty)
            LoadPreferences();

        StateHasChanged();
    });

    private void SavePreferences()
    {
        var value = new MonitoringNotificationPreferences { Email = prefEmail, Push = prefPush, SmsForCritical = prefSms, IncidentUpdates = prefUpdates };
        var result = State.SavePreferences(value);

        if (result == MonitoringResult.Ok)
            LoadPreferences();

        Show(result == MonitoringResult.Ok ? AlertColor.Success : result == MonitoringResult.NoChange ? AlertColor.Info : AlertColor.Warning, result switch
        {
            MonitoringResult.Ok => $"Saved. {ChannelsText(value)}",
            MonitoringResult.NoChange => "These are already your settings. Nothing changed.",
            MonitoringResult.Invalid => "Those settings couldn't be saved. Try again; nothing changed.",
            MonitoringResult.NotAllowed => NotAllowedText,
            _ => "That didn't work, and nothing changed. Try again.",
        });
    }

    /// <summary>Says in words where alerts now reach the engineer.</summary>
    private static string ChannelsText(MonitoringNotificationPreferences value)
    {
        var channels = new List<string>();

        if (value.Email)
            channels.Add("email");

        if (value.Push)
            channels.Add("push notification");

        if (value.SmsForCritical)
            channels.Add("text message for critical alerts");

        var reach = channels.Count == 0
            ? "Alerts appear only here in the console."
            : $"Alerts also reach you by {string.Join(channels.Count == 2 ? " and " : ", ", channels)}.";

        return reach + (value.IncidentUpdates ? " You're told about updates to incidents you're working on." : " You're not told about updates to your incidents, only alerts.");
    }

    private void SetUnreadOnly(bool value) => unreadOnly = value;

    private void Show(AlertColor color, string message)
    {
        statusColor = color;
        statusMessage = message;
    }

    private void ToggleRead(MonitoringNotification item)
    {
        var target = !item.IsRead;
        var result = State.MarkRead(item.Id, target);

        // In the Unread list a notification marked read disappears with its button; focus goes to the result instead.
        if (result == MonitoringResult.Ok && unreadOnly && target)
            focusStatusAfterRender = true;

        Show(result == MonitoringResult.Ok ? AlertColor.Success : AlertColor.Info, result switch
        {
            MonitoringResult.Ok => $"Marked {(target ? "read" : "unread")}: {item.Title}.",
            MonitoringResult.NoChange => $"It's already {(target ? "read" : "unread")}. Nothing changed.",
            MonitoringResult.NotFound => "That notification is gone (the oldest are removed as new ones arrive). Nothing changed.",
            MonitoringResult.NotAllowed => NotAllowedText,
            _ => "That didn't work, and nothing changed. Try again.",
        });
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string CountText
    {
        get
        {
            var count = Shown.Count;

            return count == 1 ? "1 notification" : $"{count} notifications";
        }
    }

    private bool IsDirty => prefEmail != loaded.Email || prefPush != loaded.Push || prefSms != loaded.SmsForCritical || prefUpdates != loaded.IncidentUpdates;

    /// <summary>Gets the notifications matching the filters, newest first.</summary>
    private List<MonitoringNotification> Shown =>
        State.Notifications.Where(item => (!unreadOnly || !item.IsRead) && (kindFilter.Length == 0 || item.Kind.ToString() == kindFilter)).ToList();

    [Inject] private MonitoringState State { get; set; } = default!;

    private string UnreadText => State.UnreadCount switch
    {
        0 => "You're all caught up.",
        1 => "1 unread.",
        var count => $"{count} unread.",
    };

    #endregion
}
