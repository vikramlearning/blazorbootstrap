using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The signed-in customer's notifications, newest first: the inbox behind the header's bell, which reads the same list
/// (<see cref="SaasState.GetNotificationsForCurrentAccount" />), so the unread count agrees. Each shows its icon, title,
/// text, time, a link when the notice has a same-site one, a "New" badge, and "Mark read" while unread. "Mark all as read"
/// marks them with <see cref="SaasState.MarkNoticesRead" />; a button that goes away moves focus to the status message.
/// With no notifications, the page says so.
/// </summary>
public partial class AppNotifications : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Notice links must stay inside Lumora.</summary>
    private const string SitePrefix = SaasLayout.Base + "/";

    private bool focusStatus;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await SaasDisplay.FocusAsync(statusRegion);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>
    /// Returns a notice's link when it is a path inside Lumora (starting with <c>/showcase/saas/</c>), or <see langword="null" />,
    /// so a stored notice can never send someone to another site.
    /// </summary>
    public static string? SafeLink(string? link) =>
        link is { Length: > 0 and <= 200 } && link.StartsWith(SitePrefix, StringComparison.Ordinal) && !link.Contains("//", StringComparison.Ordinal)
        && !link.Contains('\\') && !link.Any(char.IsWhiteSpace)
            ? link
            : null;

    #endregion

    #region Private Methods

    private static string CountText(int total, int unread)
    {
        var all = total == 1 ? "1 notification" : $"{total.ToString(CultureInfo.InvariantCulture)} notifications";

        return unread == 0 ? $"{all}, all read." : $"{all}, {unread.ToString(CultureInfo.InvariantCulture)} unread.";
    }

    /// <summary>Marks one notice read (the bell's count follows); its button goes away, so focus moves to the status message.</summary>
    private void MarkRead(SaasNotice notice)
    {
        State.MarkNotificationRead(notice.Id);
        SetStatus($"\"{notice.Title}\" is marked as read.");
        focusStatus = true;
    }

    private void MarkAllRead()
    {
        State.MarkNoticesRead();
        SetStatus("All notifications are marked as read.");
        focusStatus = true;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private SaasState State { get; set; } = default!;

    #endregion
}
