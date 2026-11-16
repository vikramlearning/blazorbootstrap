using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Creates (<c>/prompts/new</c>) or edits (<c>/prompts/{id}/edit</c>) one of the member's own prompts: title, text (with a
/// character count), and comma-separated tags, checked the same way as <see cref="AiState.SavePrompt" /> (lower-case
/// letters, digits, and hyphens; at most <see cref="AiState.MaxTags" /> of <see cref="AiState.MaxTagLength" />). Each
/// problem is shown under its field, tied to it, and the first invalid field takes focus after the render that shows it;
/// every answer of the state is mapped to a field or the status message. A save goes back to the prompt library, whose
/// status region says so. A shared workspace prompt opens read-only, with a link back (admins edit shared prompts in the
/// back office). An unknown ID shows <see cref="AiNotFound" />.
/// </summary>
public partial class PromptEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The longest tags field: room for the most tags of the longest length, with separators.</summary>
    private const int TagsMaxLength = 200;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (the first field with an error).</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The prompt was saved and the library is loading.</summary>
    private bool leaving;

    /// <summary>The route ID the form was loaded for (0 for a new prompt), or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    /// <summary>Whether the form holds the prompt's saved values (it may load later, once the tab's data is restored).</summary>
    private bool loadedPrompt;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string tagsText = string.Empty;

    private string? tagsError;

    private ElementReference? tagsInput;

    private string text = string.Empty;

    private string? textError;

    private ElementReference? textInput;

    private string title = string.Empty;

    private string? titleError;

    private ElementReference? titleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is { } target)
        {
            focusTarget = null;
            await AiDisplay.FocusAsync(target());
        }

        if (focusStatus)
        {
            focusStatus = false;
            await AiDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        var routeId = Id ?? 0;

        if (loadedId == routeId)
            return;

        loadedId = routeId;
        statusMessage = null;
        statusIsError = false;
        Load(Prompt);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>Returns the route of the editor of a prompt.</summary>
    public static string EditUrl(int promptId) => string.Create(CultureInfo.InvariantCulture, $"{AiLayout.Base}/prompts/{promptId}/edit");

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-prompt route has no ID, and the router reuses this page when moving from a
    /// prompt to "New prompt", so the previous prompt's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Internal Methods

    /// <summary>Splits the tags field like the state does: by comma, trimmed, lower case, empty ones dropped, each once.</summary>
    internal static List<string> ParseTags(string value) =>
        value.Split(',').Select(tag => tag.Trim().ToLowerInvariant()).Where(tag => tag.Length > 0).Distinct(StringComparer.Ordinal).ToList();

    /// <summary>Returns what is wrong with the tags in words, or <see langword="null" /> when the state would accept them.</summary>
    internal static string? TagsProblem(IReadOnlyList<string> tags)
    {
        if (tags.Count > AiState.MaxTags)
            return string.Create(CultureInfo.InvariantCulture, $"Use up to {AiState.MaxTags} tags (now {tags.Count}). Remove {tags.Count - AiState.MaxTags}.");

        if (tags.FirstOrDefault(tag => tag.Length > AiState.MaxTagLength) is { } longTag)
            return string.Create(CultureInfo.InvariantCulture, $"Shorten the tag \"{longTag}\" to {AiState.MaxTagLength} characters or fewer (now {longTag.Length}).");

        if (tags.FirstOrDefault(tag => !TagPattern().IsMatch(tag)) is { } badTag)
            return $"The tag \"{badTag}\" can only have letters, digits, and hyphens, and must start with a letter or digit. Use hyphens instead of spaces, like \"code-review\".";

        return null;
    }

    #endregion

    #region Private Methods

    private static List<BreadcrumbItem> BreadcrumbItems(AiPrompt? prompt) => new()
    {
        new() { Text = "Prompt library", Href = Prompts.ListUrl },
        new() { Text = prompt is null ? "New prompt" : prompt.Title, IsCurrentPage = true },
    };

    private static string CounterText(string value, int max)
    {
        var length = value.Trim().Length;

        return length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length:#,0} of {max:#,0} characters: {length - max:#,0} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length:#,0} of {max:#,0} characters");
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex TagPattern();

    private static string? TextProblem(string value, int max, string missing, string what)
    {
        var length = value.Trim().Length;

        return length == 0 ? missing
            : length > max ? string.Create(CultureInfo.InvariantCulture, $"Shorten the {what} to {max:#,0} characters or fewer (now {length:#,0}).")
            : null;
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private bool IsOwn(AiPrompt prompt) => !prompt.IsShared && prompt.OwnerId == State.Identity.Current?.Id;

    /// <summary>Fills the form from one of the member's prompts, or empties it for a new one.</summary>
    private void Load(AiPrompt? prompt)
    {
        var own = prompt is not null && IsOwn(prompt) ? prompt : null;

        title = own?.Title ?? string.Empty;
        text = own?.Text ?? string.Empty;
        tagsText = own is null ? string.Empty : string.Join(", ", own.Tags);
        titleError = textError = tagsError = null;
        loadedPrompt = own is not null;
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        // The tab's data can be restored after the first render: load the prompt then, unless the member already typed.
        if (Id is not null && !loadedPrompt && title.Length == 0 && text.Length == 0 && tagsText.Length == 0 && Prompt is { } prompt)
            Load(prompt);

        StateHasChanged();
    });

    /// <summary>Checks the form, saves it, and maps the state's answer to a field or the status message.</summary>
    private void Save()
    {
        var existing = Prompt;

        if (Id is not null && (existing is null || !IsOwn(existing)))
            return;

        var tags = ParseTags(tagsText);

        titleError = TextProblem(title, AiState.MaxPromptTitleLength, "Give the prompt a title.", "title");
        textError = TextProblem(text, AiState.MaxPromptTextLength, "Write the prompt text: what you'd type in the chat.", "prompt text");
        tagsError = TagsProblem(tags);

        focusTarget = titleError is not null ? () => titleInput
            : textError is not null ? () => textInput
            : tagsError is not null ? () => tagsInput
            : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then save again.");

            return;
        }

        var result = State.SavePrompt(existing?.Id ?? 0, title, text, tags, shared: false, out var saved);

        switch (result)
        {
            case AiResult.Ok when saved is not null:
                leaving = true;
                Navigation.NavigateTo(string.Create(CultureInfo.InvariantCulture, $"{Prompts.ListUrl}?saved={saved.Id}{(existing is null ? "&added=true" : null)}"));

                return;

            case AiResult.NoChange:
                Announce(true, "Nothing changed: the prompt is already saved like this.");
                focusStatus = true;

                return;

            case AiResult.NotFound:
                Announce(false, "This prompt was deleted, so the changes couldn't be saved. Copy your text if you want to keep it, then create a new prompt.");
                focusStatus = true;

                return;

            case AiResult.NotAllowed:
                Announce(false, existing is { IsShared: true }
                    ? "Shared prompts are managed by workspace admins. Nothing was saved."
                    : "Your membership isn't active, so the prompt couldn't be saved.");
                focusStatus = true;

                return;

            case AiResult.Invalid:
                Announce(false, existing is null
                    ? $"The prompt couldn't be saved: this tab holds up to {AiState.MaxPrompts} prompts (yours and shared ones together), or its storage is full. Delete a prompt you don't use, then try again."
                    : "The prompt couldn't be saved: a value is missing or too long, or this tab's storage is full. Nothing was saved.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                return;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the prompt ID from the route, or <see langword="null" /> on the new-prompt route.</summary>
    [Parameter] public int? Id { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>The prompt of the route (own or shared), or <see langword="null" /> for a new prompt or an unknown ID.</summary>
    private AiPrompt? Prompt => Id is { } id ? State.FindPrompt(id) : null;

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
