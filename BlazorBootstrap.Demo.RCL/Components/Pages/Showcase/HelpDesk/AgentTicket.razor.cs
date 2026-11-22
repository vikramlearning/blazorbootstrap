using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The agent's ticket page: status and SLA, assign and resolve actions, and tabs for the conversation (with a rich-text
/// reply or internal note, saved replies to insert, editable status, priority, category, and assignee, the customer, and
/// their satisfaction rating), the details with the activity log, and related articles. Replies are rendered through
/// <see cref="ShowcaseHtml.FromEditorHtml" />, so only allow-listed formatting survives. An unknown number renders the
/// site's not-found screen.
/// </summary>
public partial class AgentTicket : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The editor's starting content: one empty paragraph.</summary>
    private const string EmptyReply = "<p><br></p>";

    /// <summary>The longest editor HTML the state accepts for a reply.</summary>
    private const int MaxEditorHtmlLength = 20_000;

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

    /// <summary>The content the editor is built with: empty, or the draft with a saved reply inserted.</summary>
    private string editorValue = EmptyReply;

    /// <summary>Changes after each send or insert, so the editor is rebuilt with <see cref="editorValue" />.</summary>
    private int editorVersion;

    /// <summary>Changes when a field change is refused, so the selects are rebuilt with the ticket's real values.</summary>
    private int fieldsVersion;

    private bool isInternalNote;

    private int? loadedId;

    private string? replyError;

    private int replyErrors;

    private string replyHtml = string.Empty;

    private string replyText = string.Empty;

    private string savedReplyChoice = string.Empty;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

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
        editorValue = EmptyReply;
        replyError = null;
        replyErrors = 0;
        replyHtml = string.Empty;
        replyText = string.Empty;
        savedReplyChoice = string.Empty;
        statusCount = 0;
        statusMessage = null;
        statusText = null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>The text of editor HTML, for the length check before the editor reports its own text.</summary>
    private static string PlainText(string html) =>
        WebUtility.HtmlDecode(TagPattern().Replace(html, " ")).Trim();

    [GeneratedRegex("<[^>]*>", RegexOptions.CultureInvariant)]
    private static partial Regex TagPattern();

    /// <summary>The button disables itself, so focus moves to the status message, which also announces the change.</summary>
    private async Task AssignToMeAsync()
    {
        if (Ticket is not { } ticket || IsMine)
            return;

        if (!State.Assign(ticket, State.AgentName))
            return;

        SetStatus($"Ticket #{ticket.Id} is assigned to you.");
        Toasts.Notify(new ToastMessage(ToastType.Info, IconName.PersonCheck, "Assigned", statusText!));

        await Task.Yield();
        await HelpDeskDisplay.FocusAsync(statusRegion);
    }

    /// <summary>
    /// Inserts the chosen saved reply. The editor reads its value only when it is built, so it is rebuilt (a new key) with
    /// the draft followed by the saved reply; an empty draft is simply replaced, and a draft too long to add to is replaced
    /// and the agent is told so. The Insert button stays in place, so focus stays on it.
    /// </summary>
    private void InsertSavedReply()
    {
        if (!int.TryParse(savedReplyChoice, NumberStyles.None, CultureInfo.InvariantCulture, out var replyId)
            || State.SavedReplies.FirstOrDefault(reply => reply.Id == replyId) is not { } saved)
        {
            SetReplyError("Choose a saved reply to insert.");

            return;
        }

        // The saved reply goes through the allowlist before it reaches the editor.
        var insert = ShowcaseHtml.FromEditorHtml(saved.Html).Value;
        var hasDraft = !string.IsNullOrWhiteSpace(replyText);
        string message;

        if (!hasDraft)
        {
            editorValue = insert;
            message = $"Inserted the saved reply \"{saved.Title}\".";
        }
        else if (ShowcaseHtml.FromEditorHtml(replyHtml).Value is var draft && draft.Length + insert.Length <= MaxEditorHtmlLength)
        {
            // The draft goes through the allowlist too, since the editor renders its value as markup (A05).
            editorValue = draft + insert;
            message = $"Added the saved reply \"{saved.Title}\" after your draft.";
        }
        else
        {
            // Too long to add to: keep the draft rather than lose it.
            message = $"Your draft is too long to add the saved reply \"{saved.Title}\" to. Send or shorten the draft first.";
            SetStatus(message);
            return;
        }

        replyHtml = editorValue;
        replyText = PlainText(editorValue);
        replyError = null;
        replyErrors = 0;
        savedReplyChoice = string.Empty;
        editorVersion++;
        SetStatus(message);
    }

    private void OnAssigneeSelected(ChangeEventArgs args)
    {
        if (Ticket is not { } ticket)
            return;

        var value = args.Value?.ToString();
        var agent = string.IsNullOrEmpty(value) ? null : value;

        if (!State.Assign(ticket, agent))
        {
            RefuseFieldChange("The assignee couldn't be changed.");

            return;
        }

        SetStatus(agent is null ? $"Ticket #{ticket.Id} is unassigned." : $"Ticket #{ticket.Id} is assigned to {agent}.");
    }

    private void OnCategorySelected(ChangeEventArgs args)
    {
        if (Ticket is not { } ticket)
            return;

        var categoryId = args.Value?.ToString() ?? string.Empty;

        if (!State.SetCategory(ticket, categoryId))
        {
            RefuseFieldChange("The category couldn't be changed.");

            return;
        }

        SetStatus($"Category set to {State.FindCategory(categoryId).Name}.");
    }

    private void OnInternalNoteChanged(bool value)
    {
        // The Switch raises ValueChanged on its first render too.
        if (isInternalNote == value)
            return;

        isInternalNote = value;
        replyError = null;
        replyErrors = 0;
    }

    private void OnPrioritySelected(ChangeEventArgs args)
    {
        if (Ticket is not { } ticket)
            return;

        if (!Enum.TryParse<HelpDeskPriority>(args.Value?.ToString(), out var priority) || !Enum.IsDefined(priority) || !State.SetPriority(ticket, priority))
        {
            RefuseFieldChange("The priority couldn't be changed.");

            return;
        }

        SetStatus($"Priority set to {priority}: resolve within {State.TargetHours(priority)} hours ({HelpDeskDisplay.SlaText(State, ticket)}).");
    }

    private void OnReplyChanged(RichTextEditorChange change)
    {
        replyHtml = change.Html;
        replyText = change.Text;

        if (replyError is not null && !string.IsNullOrWhiteSpace(replyText) && replyText.Length <= MaxReplyLength)
        {
            replyError = null;
            replyErrors = 0;
        }
    }

    private void OnSavedReplyChosen(ChangeEventArgs args) => savedReplyChoice = args.Value?.ToString() ?? string.Empty;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnStatusSelected(ChangeEventArgs args)
    {
        if (Ticket is not { } ticket)
            return;

        if (!Enum.TryParse<HelpDeskStatus>(args.Value?.ToString(), out var status) || !Enum.IsDefined(status) || !State.SetStatus(ticket, status))
        {
            RefuseFieldChange("The status couldn't be changed.");

            return;
        }

        SetStatus(status == HelpDeskStatus.Resolved
            ? $"Ticket #{ticket.Id} is resolved ({HelpDeskDisplay.SlaText(State, ticket)})."
            : $"Status set to {status}.");
    }

    /// <summary>A refused change leaves the select showing the new value, so the selects are rebuilt from the ticket.</summary>
    private void RefuseFieldChange(string message)
    {
        fieldsVersion++;
        SetStatus(message);
    }

    /// <summary>Posts the reply or note; the button stays in place, so focus stays on it, and the editor is rebuilt empty.</summary>
    private void Send()
    {
        if (Ticket is not { } ticket)
            return;

        if (string.IsNullOrWhiteSpace(replyText))
        {
            SetReplyError(isInternalNote ? "Write the note before adding it." : "Write a reply before sending it.");

            return;
        }

        if (replyText.Length > MaxReplyLength)
        {
            SetReplyError(FormattableString.Invariant($"Shorten the {(isInternalNote ? "note" : "reply")} to {MaxReplyLength:#,0} characters or fewer (now {replyText.Length:#,0})."));

            return;
        }

        if (!State.AddReply(ticket, replyHtml, isInternalNote))
        {
            SetReplyError(isInternalNote ? "The note couldn't be added. Shorten it and try again." : "The reply couldn't be sent. Shorten it and try again.");

            return;
        }

        SetStatus(isInternalNote ? "Internal note added." : $"Reply sent to {ticket.CustomerName}. The ticket is now {ticket.Status}.");
        Toasts.Notify(new ToastMessage(ToastType.Success, isInternalNote ? IconName.Lock : IconName.Send, isInternalNote ? "Note added" : "Reply sent", statusText!));

        editorValue = EmptyReply;
        replyHtml = string.Empty;
        replyText = string.Empty;
        replyError = null;
        replyErrors = 0;
        editorVersion++;
    }

    /// <summary>Shows a reply error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void SetReplyError(string message)
    {
        replyErrors++;
        replyError = replyErrors == 1 ? message : FormattableString.Invariant($"{message} ({replyErrors})");
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : FormattableString.Invariant($"{message} ({statusCount})");
    }

    /// <summary>Resolves or reopens; the button stays in place, so focus stays on it.</summary>
    private void ToggleResolved()
    {
        if (Ticket is not { } ticket)
            return;

        if (ticket.Status == HelpDeskStatus.Resolved)
        {
            if (!State.Reopen(ticket))
                return;

            SetStatus($"Ticket #{ticket.Id} is open again.");
            Toasts.Notify(new ToastMessage(ToastType.Info, IconName.ArrowCounterclockwise, "Reopened", statusText!));
        }
        else
        {
            if (!State.Resolve(ticket))
                return;

            SetStatus($"Ticket #{ticket.Id} is resolved ({HelpDeskDisplay.SlaText(State, ticket)}).");
            Toasts.Notify(new ToastMessage(ToastType.Success, IconName.CheckCircle, "Resolved", statusText!));
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>The ticket's tier and the rules that escalated it, like "Tier 2, escalated by Urgent not resolved in 1 hour".</summary>
    private string EscalationText
    {
        get
        {
            if (Ticket is not { IsEscalated: true } ticket)
                return "Tier 1 (not escalated)";

            var rules = (ticket.EscalatedByRuleIds ?? new List<int>())
                .Select(id => State.FindEscalationRule(id)?.Name ?? "a deleted rule")
                .ToList();

            return string.Create(CultureInfo.InvariantCulture, $"Tier {ticket.Tier}, escalated by {string.Join(", ", rules)}");
        }
    }

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Queue", Href = HelpDeskAgentLayout.Base },
        new() { Text = Ticket is null ? "Ticket" : $"#{Ticket.Id}", IsCurrentPage = true },
    };

    private HelpDeskCategory Category => State.FindCategory(Ticket?.CategoryId);

    /// <summary>Gets or sets the ticket number from the route.</summary>
    [Parameter] public int Id { get; set; }

    private bool IsMine => Ticket?.Assignee == State.AgentName;

    private IReadOnlyList<HelpDeskArticle> Related => Ticket is null ? Array.Empty<HelpDeskArticle>() : State.RelatedArticles(Ticket.CategoryId);

    private static string SavedRepliesUrl => $"{HelpDeskAgentLayout.Base}/saved-replies";

    [Inject] private HelpDeskState State { get; set; } = default!;

    private HelpDeskTicket? Ticket => State.FindTicket(Id);

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion
}
