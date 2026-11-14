using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Ticket page: status and SLA, assign and resolve actions, and tabs for the conversation (with a rich-text reply or
/// internal note), the ticket details, and related articles. Replies are rendered through
/// <see cref="ShowcaseHtml.FromEditorHtml" />, so only allow-listed formatting survives. An unknown number renders the
/// site's not-found screen.
/// </summary>
public partial class TicketDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The editor's starting content: one empty paragraph.</summary>
    private const string EmptyReply = "<p><br></p>";

    /// <summary>The longest reply, in characters of text. The editor's own MaxLength is not enforced (library finding).</summary>
    private const int MaxReplyLength = 4000;

    /// <summary>Only formatting the safe-markup allowlist keeps: no images, tables, colors, or fonts.</summary>
    private static readonly RichTextEditorToolbarItem[] toolbarItems =
    {
        RichTextEditorToolbarItem.Undo,
        RichTextEditorToolbarItem.Redo,
        RichTextEditorToolbarItem.Bold,
        RichTextEditorToolbarItem.Italic,
        RichTextEditorToolbarItem.Underline,
        RichTextEditorToolbarItem.Strikethrough,
        RichTextEditorToolbarItem.UnorderedList,
        RichTextEditorToolbarItem.OrderedList,
        RichTextEditorToolbarItem.Blockquote,
        RichTextEditorToolbarItem.CodeBlock,
        RichTextEditorToolbarItem.Link,
        RichTextEditorToolbarItem.ClearFormatting,
    };

    /// <summary>Changes after each send, so the editor is rebuilt empty.</summary>
    private int editorVersion;

    private bool isInternalNote;

    private int? loadedId;

    private string? replyError;

    private string replyHtml = string.Empty;

    private string replyText = string.Empty;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // A different ticket starts with an empty reply and no messages.
        if (loadedId == Id)
            return;

        loadedId = Id;
        isInternalNote = false;
        replyError = null;
        replyHtml = string.Empty;
        replyText = string.Empty;
        statusMessage = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>The button disables itself, so focus moves to the status message, which also announces the change.</summary>
    private async Task AssignToMeAsync()
    {
        if (Ticket is null || IsMine)
            return;

        State.Assign(Ticket, HelpDeskSeedData.AgentName);
        statusMessage = $"Ticket #{Ticket.Id} is assigned to you.";
        Toasts.Notify(new ToastMessage(ToastType.Info, IconName.PersonCheck, "Assigned", statusMessage));
        StateHasChanged();

        await HelpDeskDisplay.FocusAsync(statusRegion);
    }

    private void OnAssigneeChanged(ChangeEventArgs args)
    {
        if (Ticket is null)
            return;

        var agent = args.Value?.ToString();
        State.Assign(Ticket, string.IsNullOrEmpty(agent) ? null : agent);
        statusMessage = Ticket.Assignee is null ? $"Ticket #{Ticket.Id} is unassigned." : $"Ticket #{Ticket.Id} is assigned to {Ticket.Assignee}.";
    }

    private void OnInternalNoteChanged(bool value)
    {
        isInternalNote = value;
        replyError = null;
    }

    private void OnReplyChanged(RichTextEditorChange change)
    {
        replyHtml = change.Html;
        replyText = change.Text;

        if (replyError is not null && !string.IsNullOrWhiteSpace(replyText) && replyText.Length <= MaxReplyLength)
            replyError = null;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Posts the reply or note; the button stays in place, so focus stays on it, and the editor is rebuilt empty.</summary>
    private void Send()
    {
        if (Ticket is null)
            return;

        if (string.IsNullOrWhiteSpace(replyText))
        {
            replyError = isInternalNote ? "Write the note before adding it." : "Write a reply before sending it.";
            return;
        }

        if (replyText.Length > MaxReplyLength)
        {
            replyError = FormattableString.Invariant($"Shorten the {(isInternalNote ? "note" : "reply")} to {MaxReplyLength:#,0} characters or fewer (now {replyText.Length:#,0}).");
            return;
        }

        State.AddReply(Ticket, replyHtml, isInternalNote);
        statusMessage = isInternalNote ? "Internal note added." : $"Reply sent to {Ticket.CustomerName}. The ticket is now {Ticket.Status}.";
        Toasts.Notify(new ToastMessage(ToastType.Success, isInternalNote ? IconName.Lock : IconName.Send, isInternalNote ? "Note added" : "Reply sent", statusMessage));

        replyHtml = string.Empty;
        replyText = string.Empty;
        editorVersion++;
    }

    /// <summary>Resolves or reopens; the button stays in place, so focus stays on it.</summary>
    private void ToggleResolved()
    {
        if (Ticket is null)
            return;

        if (Ticket.Status == HelpDeskStatus.Resolved)
        {
            State.Reopen(Ticket);
            statusMessage = $"Ticket #{Ticket.Id} is open again.";
            Toasts.Notify(new ToastMessage(ToastType.Info, IconName.ArrowCounterclockwise, "Reopened", statusMessage));
        }
        else
        {
            State.Resolve(Ticket);
            statusMessage = $"Ticket #{Ticket.Id} is resolved ({HelpDeskDisplay.SlaText(Ticket)}).";
            Toasts.Notify(new ToastMessage(ToastType.Success, IconName.CheckCircle, "Resolved", statusMessage));
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Queue", Href = HelpDeskLayout.Base },
        new() { Text = Ticket is null ? "Ticket" : $"#{Ticket.Id}", IsCurrentPage = true },
    };

    private HelpDeskCategory Category => State.FindCategory(Ticket?.CategoryId);

    /// <summary>Gets or sets the ticket number from the route.</summary>
    [Parameter] public int Id { get; set; }

    private bool IsMine => Ticket?.Assignee == HelpDeskSeedData.AgentName;

    private IReadOnlyList<HelpDeskArticle> Related => Ticket is null ? Array.Empty<HelpDeskArticle>() : State.RelatedArticles(Ticket.CategoryId);

    [Inject] private HelpDeskState State { get; set; } = default!;

    private HelpDeskTicket? Ticket => State.FindTicket(Id);

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion
}
