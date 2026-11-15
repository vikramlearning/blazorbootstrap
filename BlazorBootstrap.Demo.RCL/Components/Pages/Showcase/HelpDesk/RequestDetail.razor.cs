using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// One of the signed-in customer's requests: the thread oldest first without internal notes (agent replies through
/// <see cref="ShowcaseHtml.FromEditorHtml" />, the customer's own text encoded), a side panel with the status, priority,
/// category, and agent, a plain-text reply (which reopens a pending or solved request), "Mark as solved" with a
/// confirmation, and a Good or Bad rating with an optional comment once it is solved. Opening the page marks an agent's
/// reply as read (<see cref="HelpDeskState.FindOwnRequest" />). Someone else's request, or an unknown number, shows the
/// not-found screen. <c>?received=1</c> (after submitting) announces that the request was received.
/// </summary>
public partial class RequestDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The longest rating comment <see cref="HelpDeskState.Rate" /> accepts.</summary>
    private const int MaxRatingCommentLength = 500;

    /// <summary>The most messages a thread holds (the state refuses more).</summary>
    private const int MaxMessages = 200;

    private Button? changeRatingButton;

    private bool editingRating;

    private int? loadedId;

    private PendingFocus pendingFocus;

    private HelpDeskSatisfaction? rating;

    private string ratingComment = string.Empty;

    private string? ratingError;

    private ElementReference ratingBad;

    private ElementReference ratingGood;

    private bool receivedAnnounced;

    private string? replyError;

    private string replyText = string.Empty;

    private Button? solveButton;

    private Modal solveModal = default!;

    private bool solveDone;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Set after the page is on screen, so the status region already exists and the message is announced.
        if (Received == 1 && !receivedAnnounced && Ticket is { Status: HelpDeskStatus.New } ticket)
        {
            receivedAnnounced = true;
            SetStatus($"Request #{ticket.Id} received. It's in the support team's queue; their replies show up here and under My requests.");
            StateHasChanged();
        }

        if (pendingFocus == PendingFocus.None)
            return;

        var target = pendingFocus;
        pendingFocus = PendingFocus.None;

        await HelpDeskDisplay.FocusAsync(target switch
        {
            PendingFocus.ChangeRating => changeRatingButton?.Element,
            PendingFocus.Rating => rating == HelpDeskSatisfaction.Bad ? ratingBad : ratingGood,
            _ => statusRegion,
        });
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Another request starts with an empty reply, no rating draft, and no messages.
        if (loadedId != Id)
        {
            loadedId = Id;
            editingRating = false;
            pendingFocus = PendingFocus.None;
            rating = null;
            ratingComment = string.Empty;
            ratingError = null;
            receivedAnnounced = false;
            replyError = null;
            replyText = string.Empty;
            statusCount = 0;
            statusMessage = null;
            statusText = null;
        }

        // Marks an agent's reply as read; null (someone else's request) renders the not-found screen.
        State.FindOwnRequest(Id);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string? AgentFirstName(HelpDeskTicket ticket) => ticket.Assignee?.Split(' ')[0];

    private static string MessageLabel(HelpDeskMessage message) => message.IsAgent
        ? $"Reply from {message.Author}, {HelpDeskDisplay.Stamp(message.PostedAt)}"
        : $"Your message, {HelpDeskDisplay.Stamp(message.PostedAt)}";

    private static string ReplyHint(HelpDeskStatus status) => status switch
    {
        HelpDeskStatus.New => "The team hasn't answered yet. Add anything that helps them.",
        HelpDeskStatus.Open => "An agent is working on it. Add details at any time.",
        HelpDeskStatus.Pending => "The support team is waiting for your answer. Replying puts the request back in their queue.",
        _ => "This request is solved. Replying reopens it.",
    };

    /// <summary>The thread oldest first, without internal notes: customers never see them.</summary>
    private static IEnumerable<HelpDeskMessage> VisibleMessages(HelpDeskTicket ticket) =>
        ticket.Messages.Where(message => !message.IsInternalNote).OrderBy(message => message.PostedAt);

    private async Task AskSolveAsync()
    {
        solveDone = false;

        await solveModal.ShowAsync();
    }

    /// <summary>"Keep my rating" goes away with the form, so focus returns to "Change your rating".</summary>
    private void CancelChangeRating()
    {
        editingRating = false;
        ratingError = null;
        pendingFocus = PendingFocus.ChangeRating;
    }

    private async Task ConfirmSolveAsync()
    {
        solveDone = true;
        SetStatus(State.MarkSolved(Id)
            ? $"Request #{Id} is marked as solved. You can rate how the team did below."
            : $"Request #{Id} couldn't be marked as solved. It may be solved already.");

        await solveModal.HideAsync();
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // A reply that arrives while the request is open is read right away.
        if (Ticket is { CustomerUnread: true })
            State.FindOwnRequest(Id);

        StateHasChanged();
    });

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: after solving, the button is gone, so focus goes to the status.</summary>
    private async Task OnSolveHiddenAsync()
    {
        if (solveDone)
        {
            solveDone = false;
            await HelpDeskDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await HelpDeskDisplay.FocusAsync(solveButton?.Element);
        }
    }

    /// <summary>The send button stays in place, so focus stays on it; the status announces the result.</summary>
    private void SendReply()
    {
        replyError = null;

        if (Ticket is not { } ticket)
            return;

        var text = replyText.Trim();

        if (text.Length == 0)
        {
            replyError = "Write a message before you send it.";
            return;
        }

        if (text.Length > HelpDeskState.MaxCustomerMessageLength)
        {
            replyError = "Use 4,000 characters or fewer.";
            return;
        }

        var reopens = ticket.Status is HelpDeskStatus.Pending or HelpDeskStatus.Resolved;

        if (!State.ReplyAsCustomer(Id, text))
        {
            replyError = "The reply couldn't be sent. This conversation may be full; submit a new request to keep going.";
            return;
        }

        replyText = string.Empty;
        editingRating = false;
        SetStatus(reopens
            ? "Your reply reopened the request. It's back in the support team's queue."
            : "Reply sent. The support team sees it with your request.");
    }

    private void SetRating(HelpDeskSatisfaction value)
    {
        rating = value;
        ratingError = null;
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>Opens the rating form with the current rating; the button goes away, so focus moves to the chosen rating.</summary>
    private void StartChangeRating()
    {
        if (Ticket is not { } ticket)
            return;

        editingRating = true;
        rating = ticket.Satisfaction;
        ratingComment = ticket.SatisfactionComment ?? string.Empty;
        ratingError = null;
        pendingFocus = PendingFocus.Rating;
    }

    /// <summary>The form goes away after a rating, so focus moves to the status, which also announces it.</summary>
    private void SubmitRating()
    {
        ratingError = null;

        if (rating is not { } value)
        {
            ratingError = "Choose Good or Bad first.";
            return;
        }

        if (ratingComment.Trim().Length > MaxRatingCommentLength)
        {
            ratingError = "Use 500 characters or fewer for the comment.";
            return;
        }

        if (!State.Rate(Id, value, ratingComment))
        {
            ratingError = "The rating couldn't be saved: the request is no longer solved.";
            return;
        }

        editingRating = false;
        pendingFocus = PendingFocus.Status;
        SetStatus(value == HelpDeskSatisfaction.Good
            ? "Thanks for your rating. Glad the team could help."
            : "Thanks for your rating. The team will look at what could have gone better.");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "My requests", Href = $"{HelpDeskLayout.Base}/requests" },
        new() { Text = $"Request #{Id}", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the request number from the route.</summary>
    [Parameter] public int Id { get; set; }

    /// <summary>Gets or sets <c>?received=1</c>, set after the request was submitted.</summary>
    [SupplyParameterFromQuery(Name = "received")] public int? Received { get; set; }

    [Inject] private HelpDeskState State { get; set; } = default!;

    /// <summary>Gets the request when it is the signed-in customer's own, without marking it read (rendering must not change state).</summary>
    private HelpDeskTicket? Ticket => State.MyRequests().FirstOrDefault(ticket => ticket.Id == Id);

    #endregion

    #region Nested Types

    private enum PendingFocus
    {
        None,
        Status,
        Rating,
        ChangeRating,
    }

    #endregion
}
