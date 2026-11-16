using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Travel;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Travel;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Travel;

/// <summary>
/// One conversation in the host's inbox: the guest, the listing, and the booking it is about (linked to the reservation,
/// with its phase), the messages oldest first with the author and time on each (the host's own as "You", plain text with
/// line breaks kept), quick replies that add a ready-made message to the box, and a reply box with a character counter.
/// Sending (<see cref="TravelState.ReplyAsHost" />) clears the box, keeps focus in it, and announces "Message sent"
/// politely. Opening the page marks the guest's messages as read (<see cref="TravelState.OpenHostThread" />); a
/// conversation about another host's listing, or an unknown ID, shows a not-found screen. The page re-renders when the
/// state changes.
/// </summary>
public partial class HostThread : ComponentBase, IDisposable
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

        // The Send or quick-reply button took focus when clicked; the host keeps writing in the box.
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

        // Marks the guest's messages as read; null (another host's conversation) renders the not-found screen.
        State.OpenHostThread(ThreadId);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static List<BreadcrumbItem> BreadcrumbItems(TravelThread thread) => new()
    {
        new() { Text = "Inbox", Href = HostInbox.InboxPath },
        new() { Text = thread.GuestName, IsCurrentPage = true },
    };

    private static string FirstName(string name) => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;

    private static string MessageLabel(TravelMessage message) => message.FromHost
        ? $"Your message, {TravelDisplay.Stamp(message.SentAt)}"
        : $"Message from {message.Author}, {TravelDisplay.Stamp(message.SentAt)}";

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // A guest message that arrives while the conversation is open is read right away.
        if (Conversation is { HostUnread: true })
            State.OpenHostThread(ThreadId);

        StateHasChanged();
    });

    /// <summary>Returns the ready-made messages for this conversation, worded for its guest, listing, and booking.</summary>
    private static IReadOnlyList<QuickReply> QuickReplies(TravelThread thread, TravelStay? stay, TravelBooking? booking)
    {
        var guest = FirstName(thread.GuestName);
        var place = stay?.Name ?? "the stay";
        var arrival = booking is null ? "on your check-in day" : $"on {TravelDisplay.Day(booking.CheckIn)}";
        var checkInTime = new TimeOnly(TravelState.CheckInHour, 0).ToString("h tt", CultureInfo.InvariantCulture).ToLowerInvariant();

        return new QuickReply[]
        {
            new("Check-in instructions",
                $"Hi {guest}, check-in at {place} is from {checkInTime} {arrival}. You'll find the address in your trip details. Message me here when you're on your way, and I'll make sure everything is ready."),
            new("Thanks for staying",
                $"Thank you for staying at {place}, {guest}! It was a pleasure to host you. If you have a moment, a review helps other guests. I hope to welcome you back soon."),
            new("Happy to help",
                $"Hi {guest}, thanks for your message. I'm happy to help and will get back to you with the details shortly."),
        };
    }

    private static string ResultMessage(TravelResult result) => result switch
    {
        TravelResult.NotAllowed => "Sign in with the host account to reply to guests.",
        TravelResult.NotFound => "This conversation is no longer in your inbox, so the message wasn't sent.",
        TravelResult.Invalid => $"The message wasn't sent. Write up to {TravelState.MaxMessageLength.ToString("N0", CultureInfo.InvariantCulture)} characters; a conversation holds up to {TravelState.MaxMessagesPerThread} messages.",
        _ => "The message couldn't be sent. Try again.",
    };

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

        var result = State.ReplyAsHost(ThreadId, text);

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

    /// <summary>Puts a ready-made message in an empty box, or adds it after what the host already wrote.</summary>
    private void UseQuickReply(string text)
    {
        var current = replyText.TrimEnd();

        replyText = current.Length == 0 ? text : $"{current}\n\n{text}";
        replyError = replyText.Length > TravelState.MaxMessageLength
            ? $"The message is now longer than {TravelState.MaxMessageLength.ToString("N0", CultureInfo.InvariantCulture)} characters. Shorten it before you send."
            : null;
        pendingFocus = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the conversation when it is about one of the host's listings, without marking it read (rendering must not change state).</summary>
    private TravelThread? Conversation => State.HostThreads().FirstOrDefault(thread => thread.Id == ThreadId);

    private string CountText => FormattableString.Invariant($"{replyText.Length:N0} of {TravelState.MaxMessageLength:N0} characters");

    [Inject] private TravelState State { get; set; } = default!;

    /// <summary>Gets or sets the conversation ID from the route.</summary>
    [Parameter] public int ThreadId { get; set; }

    #endregion

    #region Nested Types

    /// <summary>A ready-made message: the button label and the text it adds.</summary>
    private sealed record QuickReply(string Label, string Text);

    #endregion
}
