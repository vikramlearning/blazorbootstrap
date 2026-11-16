using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// One of the signed-in guest's conversations with a host: the host, the stay, and the booking it is about (with its
/// phase), the messages oldest first with the author and time on each (plain text, line breaks kept), and a reply box
/// with a character counter. Sending clears the box, keeps focus in it, and announces "Message sent" politely. Opening
/// the page marks the host's messages as read (<see cref="TravelState.OpenThread" />); someone else's conversation, or an
/// unknown ID, shows a not-found screen. The page re-renders when the state changes.
/// </summary>
public partial class MessageThread : ComponentBase, IDisposable
{
    #region Fields and Constants

    private int? loadedId;

    private bool pendingFocus;

    private ElementReference? replyBox;

    private string? replyError;

    private string replyText = string.Empty;

    private int statusCount;

    private string? statusMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!pendingFocus)
            return;

        pendingFocus = false;

        // The Send button took focus when clicked; the guest keeps writing in the box.
        await TravelDisplay.FocusAsync(replyBox);
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Another conversation starts with an empty reply and no messages.
        if (loadedId != ThreadId)
        {
            loadedId = ThreadId;
            pendingFocus = false;
            replyError = null;
            replyText = string.Empty;
            statusCount = 0;
            statusMessage = null;
        }

        // Marks the host's messages as read; null (someone else's conversation) renders the not-found screen.
        State.OpenThread(ThreadId);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static List<BreadcrumbItem> BreadcrumbItems(TravelHost? host) => new()
    {
        new() { Text = "Messages", Href = MessagesPath },
        new() { Text = host?.Name ?? "Conversation", IsCurrentPage = true },
    };

    private static string MessageLabel(TravelMessage message) => message.FromHost
        ? $"Message from {message.Author}, {TravelDisplay.Stamp(message.SentAt)}"
        : $"Your message, {TravelDisplay.Stamp(message.SentAt)}";

    private static string ResultMessage(TravelResult result) => result switch
    {
        TravelResult.NotAllowed => "Sign in with a guest account to send messages.",
        TravelResult.NotFound => "This conversation is no longer in your account, so the message wasn't sent.",
        TravelResult.Invalid => $"The message wasn't sent. Write up to {TravelState.MaxMessageLength.ToString("N0", CultureInfo.InvariantCulture)} characters; a conversation holds up to {TravelState.MaxMessagesPerThread} messages.",
        _ => "The message couldn't be sent. Try again.",
    };

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // A host message that arrives while the conversation is open is read right away.
        if (Conversation is { GuestUnread: true })
            State.OpenThread(ThreadId);

        StateHasChanged();
    });

    private void Send()
    {
        replyError = null;

        var text = replyText.Trim();

        if (text.Length == 0)
        {
            replyError = "Write a message before you send it.";
            pendingFocus = true;
            return;
        }

        if (text.Length > TravelState.MaxMessageLength)
        {
            replyError = $"Use {TravelState.MaxMessageLength.ToString("N0", CultureInfo.InvariantCulture)} characters or fewer.";
            pendingFocus = true;
            return;
        }

        var result = State.SendMessage(ThreadId, text);

        if (result != TravelResult.Ok)
        {
            replyError = ResultMessage(result);
            return;
        }

        replyText = string.Empty;
        pendingFocus = true;

        // The same text again gets a counter, so a screen reader announces each send.
        statusCount++;
        statusMessage = statusCount == 1 ? "Message sent." : $"Message sent ({statusCount}).";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the conversation when it is the signed-in guest's own, without marking it read (rendering must not change state).</summary>
    private TravelThread? Conversation => State.MyThreads().FirstOrDefault(thread => thread.Id == ThreadId);

    private string CountText => FormattableString.Invariant($"{replyText.Length:N0} of {TravelState.MaxMessageLength:N0} characters");

    private static string MessagesPath => $"{TravelLayout.Base}/messages";

    [Inject] private TravelState State { get; set; } = default!;

    /// <summary>Gets or sets the conversation ID from the route.</summary>
    [Parameter] public int ThreadId { get; set; }

    #endregion
}
