using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The agents' saved replies: each one with a preview rendered through <see cref="ShowcaseHtml.FromEditorHtml" />, a
/// dialog to add or edit one (a title and a rich-text reply), and a confirmed delete. Focus returns to the button that
/// opened a dialog, or to the status message when that button is gone.
/// </summary>
public partial class AgentSavedReplies : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The editor's starting content for a new reply: one empty paragraph.</summary>
    private const string EmptyReply = "<p><br></p>";

    /// <summary>The longest saved reply the state accepts, in characters of editor HTML.</summary>
    private const int MaxHtmlLength = 5000;

    private const int MaxTitleLength = 60;

    /// <summary>The ticket reply's formatting, without Link: its dialog would open inside the editor dialog.</summary>
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
        RichTextEditorToolbarItem.ClearFormatting,
    };

    private Button? addButton;

    private string? bodyError;

    private string bodyHtml = string.Empty;

    private string bodyText = string.Empty;

    private bool deleted;

    private readonly Dictionary<int, Button> deleteButtons = new();

    private HelpDeskSavedReply? deleting;

    private Modal deleteModal = default!;

    private readonly Dictionary<int, Button> editButtons = new();

    /// <summary>The ID of the reply being edited; 0 while adding one.</summary>
    private int editingId;

    private Modal editorModal = default!;

    /// <summary>The content the editor is built with when the dialog opens.</summary>
    private string editorStart = EmptyReply;

    /// <summary>Changes on every open, so the editor is rebuilt with <see cref="editorStart" />.</summary>
    private int editorVersion;

    private string? formError;

    private int formErrors;

    private string? pageError;

    private bool saved;

    private int savedId;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private string title = string.Empty;

    private string? titleError;

    private ElementReference titleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>The text of editor HTML, until the editor reports its own.</summary>
    private static string PlainText(string html) =>
        WebUtility.HtmlDecode(TagPattern().Replace(html, " ")).Trim();

    [GeneratedRegex("<[^>]*>", RegexOptions.CultureInvariant)]
    private static partial Regex TagPattern();

    private static string? TitleProblem(string value) =>
        string.IsNullOrWhiteSpace(value) ? "Give the saved reply a title."
        : value.Trim().Length > MaxTitleLength ? FormattableString.Invariant($"Use {MaxTitleLength} characters or fewer.")
        : null;

    private Task AddAsync() => OpenEditorAsync(null);

    private async Task AskDeleteAsync(HelpDeskSavedReply reply)
    {
        deleting = reply;
        deleted = false;
        pageError = null;

        await deleteModal.ShowAsync();
    }

    private async Task ConfirmDeleteAsync()
    {
        if (deleting is { } reply)
        {
            if (State.DeleteSavedReply(reply.Id))
            {
                deleted = true;
                SetStatus($"The saved reply \"{reply.Title}\" is deleted.");
            }
            else
            {
                pageError = $"The saved reply \"{reply.Title}\" couldn't be deleted; it may be gone already.";
            }
        }

        await deleteModal.HideAsync();
    }

    private Task EditAsync(HelpDeskSavedReply reply) => OpenEditorAsync(reply);

    private void OnBodyChanged(RichTextEditorChange change)
    {
        bodyHtml = change.Html;
        bodyText = change.Text;

        if (bodyError is not null && !string.IsNullOrWhiteSpace(bodyText) && bodyHtml.Length <= MaxHtmlLength)
            bodyError = null;
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: after a delete the card is gone, so focus goes to the status.</summary>
    private async Task OnDeleteHiddenAsync()
    {
        var reply = deleting;
        deleting = null;

        if (deleted)
        {
            deleted = false;
            await HelpDeskDisplay.FocusAsync(statusRegion);
        }
        else if (reply is not null && deleteButtons.TryGetValue(reply.Id, out var button))
        {
            await HelpDeskDisplay.FocusAsync(button.Element);
        }
    }

    /// <summary>
    /// Returns focus after the editor closes: to the saved reply's Edit button after an edit, to the status after adding
    /// one, or to the button that opened the dialog when it was cancelled.
    /// </summary>
    private async Task OnEditorHiddenAsync()
    {
        var id = editingId;
        var wasSaved = saved;
        var editId = wasSaved ? savedId : id;

        editingId = 0;
        saved = false;
        savedId = 0;

        if (wasSaved && id == 0)
            await HelpDeskDisplay.FocusAsync(statusRegion);
        else if (editId > 0 && editButtons.TryGetValue(editId, out var editButton))
            await HelpDeskDisplay.FocusAsync(editButton.Element);
        else if (id == 0)
            await HelpDeskDisplay.FocusAsync(addButton?.Element);
        else
            await HelpDeskDisplay.FocusAsync(statusRegion);
    }

    private Task OnEditorShownAsync() => HelpDeskDisplay.FocusAsync(titleInput);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnTitleInput(ChangeEventArgs args)
    {
        title = args.Value?.ToString() ?? string.Empty;

        if (titleError is not null && TitleProblem(title) is null)
            titleError = null;
    }

    private async Task OpenEditorAsync(HelpDeskSavedReply? reply)
    {
        editingId = reply?.Id ?? 0;
        saved = false;
        savedId = 0;
        title = reply?.Title ?? string.Empty;
        // The editor renders its value as markup, so a stored reply goes through the allowlist first (A05).
        editorStart = reply is null ? EmptyReply : ShowcaseHtml.FromEditorHtml(reply.Html).Value;
        bodyHtml = reply?.Html ?? string.Empty;
        bodyText = reply is null ? string.Empty : PlainText(reply.Html);
        titleError = null;
        bodyError = null;
        formError = null;
        formErrors = 0;
        pageError = null;
        editorVersion++;

        await editorModal.ShowAsync();
    }

    private async Task SaveAsync()
    {
        titleError = TitleProblem(title);
        bodyError = string.IsNullOrWhiteSpace(bodyText)
            ? "Write the reply."
            : bodyHtml.Length > MaxHtmlLength
                ? FormattableString.Invariant($"Shorten the reply: a saved reply holds up to {MaxHtmlLength:#,0} characters, formatting included (now {bodyHtml.Length:#,0}).")
                : null;

        if (titleError is not null)
        {
            await HelpDeskDisplay.FocusAsync(titleInput);

            return;
        }

        if (bodyError is not null)
            return;

        if (State.SaveReply(editingId, title.Trim(), bodyHtml) is not { } result)
        {
            formErrors++;
            var message = editingId == 0
                ? "The saved reply couldn't be added: there's room for up to 100 saved replies. Delete one first."
                : "The saved reply couldn't be saved: it's no longer in the list.";
            formError = formErrors == 1 ? message : FormattableString.Invariant($"{message} ({formErrors})");

            return;
        }

        saved = true;
        savedId = result.Id;
        SetStatus(editingId == 0 ? $"The saved reply \"{result.Title}\" is added." : $"The saved reply \"{result.Title}\" is saved.");

        await editorModal.HideAsync();
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion
}
