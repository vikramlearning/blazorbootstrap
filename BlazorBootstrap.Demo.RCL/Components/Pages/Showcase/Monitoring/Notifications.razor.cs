using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// The signed-in engineer's notifications (<see cref="MonitoringState.Notifications" />), newest first, filtered by All or
/// Unread and by kind: the inbox behind the header's bell, which reads the same list. Each shows its kind in words with an
/// icon, its title, text, and age, a link to its incident (or its page) when it has one, and Mark read or unread; "Mark all
/// read" clears the unread count. "How you're notified" is an <see cref="EditForm" /> (rule WR-002) that saves the engineer's
/// <see cref="MonitoringState.Preferences" />; someone on call this week or next must keep one channel outside the console.
/// Results are announced in a status message; when a notification leaves the Unread list, focus moves to that message.
/// </summary>
public partial class Notifications : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Set when the notification acted on left the list, so focus moves to the status message after that render.</summary>
    private bool focusStatusAfterRender;

    private string kindFilter = string.Empty;

    /// <summary>The preferences the checkboxes were last loaded from, so unsaved changes survive a state change.</summary>
    private MonitoringNotificationPreferences loaded = new();

    private EditContext prefsContext = new(new PrefsForm());

    private PrefsForm prefsForm = new();

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
        MonitoringNotificationKind.Maintenance => IconName.Tools,
        _ => IconName.CalendarWeek,
    };

    private static string KindLabel(MonitoringNotificationKind kind) => kind switch
    {
        MonitoringNotificationKind.Alert => "Alert",
        MonitoringNotificationKind.Assigned => "Assigned to you",
        MonitoringNotificationKind.Update => "Incident update",
        MonitoringNotificationKind.Maintenance => "Maintenance",
        _ => "On-call",
    };

    private static string NotAllowedText => "Sign in as an engineer to manage your notifications. Nothing changed.";

    /// <summary>Copies the saved preferences into the form, with a new edit context so no old message stays.</summary>
    private void LoadPreferences()
    {
        var saved = State.Preferences;
        loaded = new MonitoringNotificationPreferences { Email = saved.Email, Push = saved.Push, SmsForCritical = saved.SmsForCritical, IncidentUpdates = saved.IncidentUpdates };
        prefsForm = new PrefsForm
        {
            Email = saved.Email, Push = saved.Push, SmsForCritical = saved.SmsForCritical, IncidentUpdates = saved.IncidentUpdates, IsOnCallSoon = IsOnCallSoon(),
        };
        prefsContext = new EditContext(prefsForm);
    }

    /// <summary>Whether the signed-in engineer is primary or secondary on call this week or next (the state's own rule).</summary>
    private bool IsOnCallSoon() =>
        State.Identity.Current is { } account && State.Shifts(0, 2).Any(shift => shift.PrimaryId == account.Id || shift.SecondaryId == account.Id);

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

    /// <summary>Saves once the form is valid (a failed submit shows the summary, rule WR-002); the state checks again and its answer maps to a message.</summary>
    private void SavePreferences()
    {
        prefsForm.IsOnCallSoon = IsOnCallSoon();

        if (!prefsContext.Validate())
            return;

        var value = new MonitoringNotificationPreferences { Email = prefsForm.Email, Push = prefsForm.Push, SmsForCritical = prefsForm.SmsForCritical, IncidentUpdates = prefsForm.IncidentUpdates };
        var result = State.SavePreferences(value);

        if (result == MonitoringResult.Ok)
            LoadPreferences();

        Show(result == MonitoringResult.Ok ? AlertColor.Success : result == MonitoringResult.NoChange ? AlertColor.Info : AlertColor.Warning, result switch
        {
            MonitoringResult.Ok => $"Saved. {ChannelsText(value)}",
            MonitoringResult.NoChange => "These are already your settings. Nothing changed.",
            MonitoringResult.Invalid => "Those settings couldn't be saved. Try again; nothing changed.",
            MonitoringResult.WrongState => "You're on call this week or next: keep email, push, or text messages on, so a page reaches you. Nothing changed.",
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

    private bool IsDirty =>
        prefsForm.Email != loaded.Email || prefsForm.Push != loaded.Push || prefsForm.SmsForCritical != loaded.SmsForCritical || prefsForm.IncidentUpdates != loaded.IncidentUpdates;

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

    #region Nested Types

    /// <summary>"How you're notified", with the rule of <see cref="MonitoringState.SavePreferences" />: on call soon means one channel stays on.</summary>
    private sealed class PrefsForm : IValidatableObject
    {
        public bool Email { get; set; }

        public bool IncidentUpdates { get; set; }

        /// <summary>Whether the engineer is on call this week or next; set by the page, not by a field.</summary>
        public bool IsOnCallSoon { get; set; }

        public bool Push { get; set; }

        public bool SmsForCritical { get; set; }

        /// <summary>The message goes with the first channel, so it is read with the group.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsOnCallSoon && !Email && !Push && !SmsForCritical)
                yield return new ValidationResult("You're on call this week or next: choose email, push, or text messages, so a page reaches you outside the console.", new[] { nameof(Email) });
        }
    }

    #endregion
}
