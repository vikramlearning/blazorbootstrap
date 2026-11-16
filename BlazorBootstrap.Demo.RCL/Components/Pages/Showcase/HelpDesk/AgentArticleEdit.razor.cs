using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Writes a new knowledge base article or edits one: title, category, summary, and a rich-text body, with a preview
/// rendered exactly as the help center renders it (<see cref="ShowcaseHtml.FromEditorHtml" />, A05). A new or draft
/// article can be saved as a draft or published; a published one updated or unpublished; an existing one deleted after
/// a confirmation. Every <see cref="HelpDeskAdminResult" /> maps to a message. An unknown ID shows the not-found screen.
/// </summary>
public partial class AgentArticleEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The editor's starting content for a new article: one empty paragraph.</summary>
    private const string EmptyBody = "<p><br></p>";

    /// <summary>The ticket reply's formatting, with section headings (the help center keeps h2 and h3) and Link.</summary>
    private static readonly RichTextEditorToolbarItem[] toolbarItems =
    {
        RichTextEditorToolbarItem.Undo,
        RichTextEditorToolbarItem.Redo,
        RichTextEditorToolbarItem.Bold,
        RichTextEditorToolbarItem.Italic,
        RichTextEditorToolbarItem.Underline,
        RichTextEditorToolbarItem.Strikethrough,
        RichTextEditorToolbarItem.Heading2,
        RichTextEditorToolbarItem.Heading3,
        RichTextEditorToolbarItem.UnorderedList,
        RichTextEditorToolbarItem.OrderedList,
        RichTextEditorToolbarItem.Blockquote,
        RichTextEditorToolbarItem.CodeBlock,
        RichTextEditorToolbarItem.Link,
        RichTextEditorToolbarItem.ClearFormatting,
    };

    private string? bodyError;

    /// <summary>The body as the editor reports it (or as stored, until it is edited); saved as written, shown through the allowlist.</summary>
    private string bodyHtml = string.Empty;

    private string bodyText = string.Empty;

    private string? categoryError;

    private string categoryId = string.Empty;

    private ElementReference categorySelect;

    /// <summary>Set by "Delete article"; the delete itself runs once the dialog is hidden, so the page never leaves under an open dialog.</summary>
    private bool confirmDelete;

    /// <summary>The ID a new article got on its first save: its edit route keeps the form as it is instead of reloading it.</summary>
    private int? createdId;

    private Button? deleteButton;

    private Modal deleteModal = default!;

    /// <summary>The content the editor is built with when an article is loaded.</summary>
    private string editorStart = EmptyBody;

    /// <summary>Changes on every load, so the editor is rebuilt with <see cref="editorStart" />.</summary>
    private int editorVersion;

    private int formErrors;

    private string? formError;

    /// <summary>Set after a delete, while the list loads.</summary>
    private bool leaving;

    /// <summary>The route ID the form was filled from (0 for a new article); <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    private string summary = string.Empty;

    private string? summaryError;

    private ElementReference summaryInput;

    private string title = string.Empty;

    private string? titleError;

    private ElementReference titleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        var routeId = Id ?? 0;

        if (loadedId == routeId)
            return;

        loadedId = routeId;

        // The first save of a new article moves to its edit route: the form, editor, and status message stay as they are.
        if (createdId == routeId)
        {
            createdId = null;

            return;
        }

        Load(Item);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-article route has no ID, and the router reuses this page when moving from an
    /// article to "New article", so the previous article's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Private Methods

    private static string CounterText(string value, int max)
    {
        var length = value.Trim().Length;

        return length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters: {length - max} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters");
    }

    /// <summary>The text of editor HTML, until the editor reports its own.</summary>
    private static string PlainText(string html) =>
        WebUtility.HtmlDecode(TagPattern().Replace(html, " ")).Replace(' ', ' ').Trim();

    private static string? SummaryProblem(string value) =>
        string.IsNullOrWhiteSpace(value) ? "Write a summary: one sentence on what the article helps with."
        : value.Trim().Length > HelpDeskState.MaxArticleSummaryLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the summary to {HelpDeskState.MaxArticleSummaryLength} characters or fewer (now {value.Trim().Length}).")
        : null;

    [GeneratedRegex("<[^>]*>", RegexOptions.CultureInvariant)]
    private static partial Regex TagPattern();

    private static string? TitleProblem(string value) =>
        string.IsNullOrWhiteSpace(value) ? "Give the article a title."
        : value.Trim().Length > HelpDeskState.MaxArticleTitleLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the title to {HelpDeskState.MaxArticleTitleLength} characters or fewer (now {value.Trim().Length}).")
        : null;

    private async Task AskDeleteAsync()
    {
        confirmDelete = false;
        formError = null;

        await deleteModal.ShowAsync();
    }

    private string? BodyProblem() =>
        string.IsNullOrWhiteSpace(bodyText) ? "Write the article body."
        : bodyHtml.Length > HelpDeskState.MaxArticleHtmlLength
            ? string.Create(CultureInfo.InvariantCulture, $"Shorten the body: an article holds up to {HelpDeskState.MaxArticleHtmlLength:#,0} characters, formatting included (now {bodyHtml.Length:#,0}).")
        : null;

    private async Task ConfirmDeleteAsync()
    {
        confirmDelete = true;

        await deleteModal.HideAsync();
    }

    /// <summary>Says what a delete takes with it: the help center page (when published) and the article's votes.</summary>
    private string DeleteConsequence(HelpDeskArticle? article)
    {
        if (article is null)
            return string.Empty;

        var (helpful, notHelpful) = State.Votes(article.Id);
        var place = article.IsPublished
            ? "It's removed from the knowledge base and leaves the help center, so links to it stop working."
            : "The draft is removed from the knowledge base.";
        var votes = helpful + notHelpful == 0
            ? "Its \"Was this helpful?\" votes go with it (none so far)."
            : string.Create(CultureInfo.InvariantCulture, $"Its \"Was this helpful?\" votes are lost too: {helpful} yes, {notHelpful} no.");

        return $"{place} {votes} This can't be undone.";
    }

    /// <summary>Fills the form from an article, or empties it for a new one.</summary>
    private void Load(HelpDeskArticle? article)
    {
        title = article?.Title ?? string.Empty;
        summary = article?.Summary ?? string.Empty;
        categoryId = article?.CategoryId ?? State.Categories.FirstOrDefault()?.Id ?? string.Empty;
        // The editor renders its value as markup, so a stored body goes through the allowlist first (A05).
        editorStart = article is null ? EmptyBody : ShowcaseHtml.FromEditorHtml(article.Html, keepHeadings: true).Value;
        bodyHtml = article?.Html ?? string.Empty;
        bodyText = article is null ? string.Empty : PlainText(article.Html);
        titleError = null;
        summaryError = null;
        categoryError = null;
        bodyError = null;
        formError = null;
        formErrors = 0;
        statusMessage = null;
        statusText = null;
        statusCount = 0;
        editorVersion++;
    }

    private void OnBodyChanged(RichTextEditorChange change)
    {
        bodyHtml = change.Html;
        bodyText = change.Text;

        if (bodyError is not null && BodyProblem() is null)
            bodyError = null;
    }

    private void OnCategoryChanged(ChangeEventArgs args)
    {
        categoryId = args.Value?.ToString() ?? string.Empty;
        categoryError = null;
    }

    /// <summary>
    /// Runs the confirmed delete once the dialog is hidden, then goes back to the list; a cancelled or failed delete returns
    /// focus to the Delete button or the error.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        if (!confirmDelete)
        {
            await HelpDeskDisplay.FocusAsync(deleteButton?.Element);

            return;
        }

        confirmDelete = false;

        if (Item is not { } article)
        {
            ShowFormError("The article couldn't be deleted: it's no longer in the knowledge base.");
            await HelpDeskDisplay.FocusAsync(statusRegion);

            return;
        }

        var result = State.DeleteArticle(article.Id);

        if (result == HelpDeskAdminResult.Ok)
        {
            leaving = true;
            Toasts.Notify(new ToastMessage(ToastType.Success, IconName.Trash3, "Article deleted", $"\"{article.Title}\" is removed from the knowledge base."));
            Navigation.NavigateTo(ListUrl);

            return;
        }

        ShowFormError(result switch
        {
            HelpDeskAdminResult.NotFound => "The article couldn't be deleted: it's no longer in the knowledge base.",
            HelpDeskAdminResult.NotAllowed => "Only agents and admins can delete articles. Sign in with a staff account.",
            _ => "The article couldn't be deleted. Try again.",
        });
        await HelpDeskDisplay.FocusAsync(deleteButton?.Element);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnSummaryInput(ChangeEventArgs args)
    {
        summary = args.Value?.ToString() ?? string.Empty;

        if (summaryError is not null && SummaryProblem(summary) is null)
            summaryError = null;
    }

    private void OnTitleInput(ChangeEventArgs args)
    {
        title = args.Value?.ToString() ?? string.Empty;

        if (titleError is not null && TitleProblem(title) is null)
            titleError = null;
    }

    /// <summary>
    /// Saves the article, as a draft or published. A first save moves to the article's edit route; when the buttons change
    /// (published ↔ draft, or new → saved), focus goes to the status message, because the clicked button is gone.
    /// </summary>
    private async Task SaveAsync(bool publish)
    {
        formError = null;
        titleError = TitleProblem(title);
        categoryError = State.Categories.Any(category => category.Id == categoryId) ? null : "Choose a category.";
        summaryError = SummaryProblem(summary);
        bodyError = BodyProblem();

        if (titleError is not null)
        {
            await HelpDeskDisplay.FocusAsync(titleInput);

            return;
        }

        if (categoryError is not null)
        {
            await HelpDeskDisplay.FocusAsync(categorySelect);

            return;
        }

        if (summaryError is not null)
        {
            await HelpDeskDisplay.FocusAsync(summaryInput);

            return;
        }

        if (bodyError is not null)
            return;

        var before = Item;
        var result = State.SaveArticle(before?.Id ?? 0, title, categoryId, summary, bodyHtml, publish, out var saved);

        if (result != HelpDeskAdminResult.Ok || saved is null)
        {
            await ShowSaveProblemAsync(result, before);

            return;
        }

        title = saved.Title;
        summary = saved.Summary;
        formErrors = 0;
        SetStatus(before is null
            ? publish ? $"The article \"{saved.Title}\" is published. Customers can find it in the help center." : $"The draft \"{saved.Title}\" is saved. Only agents see it."
            : publish && before.IsPublished ? $"The article \"{saved.Title}\" is updated in the help center."
            : publish ? $"The article \"{saved.Title}\" is published. Customers can find it in the help center."
            : before.IsPublished ? $"The article \"{saved.Title}\" is unpublished and saved as a draft. It's no longer in the help center."
            : $"The draft \"{saved.Title}\" is saved.");

        if (before is null)
        {
            // The edit route renders the same page, so the form, editor, and status stay; the router then focuses the heading.
            createdId = saved.Id;
            Navigation.NavigateTo(HelpDeskDisplay.ArticleEditUrl(saved.Id), replace: true);

            return;
        }

        if (before.IsPublished != saved.IsPublished)
            await HelpDeskDisplay.FocusAsync(statusRegion);
    }

    /// <summary>Enter in a text field saves without changing whether the article is published.</summary>
    private Task SaveKeepingStatusAsync() => SaveAsync(Item?.IsPublished ?? false);

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Shows a form-level error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void ShowFormError(string message)
    {
        formErrors++;
        formError = formErrors == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrors})");
    }

    /// <summary>Maps a refused save to the field it concerns, or to a form-level message.</summary>
    private async Task ShowSaveProblemAsync(HelpDeskAdminResult result, HelpDeskArticle? before)
    {
        switch (result)
        {
            case HelpDeskAdminResult.Duplicate:
                titleError = "Another article already has this title. Use a different one, so customers can tell them apart.";
                await HelpDeskDisplay.FocusAsync(titleInput);
                break;

            case HelpDeskAdminResult.Invalid when State.Categories.All(category => category.Id != categoryId):
                categoryError = "The chosen category no longer exists. Choose another one.";
                await HelpDeskDisplay.FocusAsync(categorySelect);
                break;

            case HelpDeskAdminResult.Invalid:
                ShowFormError("The article couldn't be saved: check the title, summary, and body for missing or too-long text.");
                break;

            case HelpDeskAdminResult.NotFound:
                ShowFormError($"The article \"{before?.Title}\" couldn't be saved: it's no longer in the knowledge base. Copy your text, then start a new article.");
                break;

            case HelpDeskAdminResult.NotAllowed:
                ShowFormError("Only agents and admins can write articles. Sign in with a staff account.");
                break;

            case HelpDeskAdminResult.LimitReached:
                ShowFormError(FormattableString.Invariant($"The article couldn't be added: the knowledge base holds up to {HelpDeskState.MaxArticles} articles. Delete one you no longer need first."));
                break;

            case HelpDeskAdminResult.NoChange:
                SetStatus("Nothing changed, so there was nothing to save.");
                break;

            default:
                ShowFormError("The article couldn't be saved. Try again.");
                break;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Knowledge base", Href = ListUrl },
        new() { Text = Item?.Title ?? "New article", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the article ID from the route; <see langword="null" /> on the new-article route.</summary>
    [Parameter] public int? Id { get; set; }

    /// <summary>The article being edited (drafts included), or <see langword="null" /> for a new or unknown one.</summary>
    private HelpDeskArticle? Item => Id is int id ? State.FindArticleForEdit(id) : null;

    private static string ListUrl => $"{HelpDeskAgentLayout.Base}/kb";

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HelpDeskState State { get; set; } = default!;

    private bool SummaryOver => summary.Trim().Length > HelpDeskState.MaxArticleSummaryLength;

    private bool TitleOver => title.Trim().Length > HelpDeskState.MaxArticleTitleLength;

    [Inject] private ToastService Toasts { get; set; } = default!;

    #endregion
}
