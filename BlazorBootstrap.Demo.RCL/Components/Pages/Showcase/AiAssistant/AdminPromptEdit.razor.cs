using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Adds (<c>/new</c>) or edits a shared workspace prompt (workspace admins): title and text with counters, and tags separated
/// by commas, checked like <see cref="AiState.SavePrompt" /> (lower-case letters, digits, and hyphens; at most
/// <see cref="AiState.MaxTags" /> of <see cref="AiState.MaxTagLength" /> characters). Each problem is shown under its field,
/// tied to it, and the first invalid field takes focus after the render that shows it; every result of the state is mapped
/// to a field or the status message. Sharing a new prompt moves to its edit route and keeps the message. A personal prompt's
/// ID, or an unknown one, shows <see cref="AiNotFound" />.
/// </summary>
public partial class AdminPromptEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID the router moves to after sharing a new prompt: the form and its status message stay.</summary>
    private int? createdId;

    private bool focusStatus;

    /// <summary>Returns the field to focus after the next render (the first invalid one).</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The route ID the form was loaded for (0 for a new prompt), or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

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

        // Sharing a new prompt moves to its edit route: the form and the status message stay as they are.
        if (createdId is not null && createdId == routeId)
        {
            createdId = null;

            return;
        }

        statusMessage = null;
        statusIsError = false;
        Load(Prompt);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-prompt route has no ID, and the router reuses this page when moving from a
    /// prompt to "New shared prompt", so the previous prompt's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Internal Methods

    /// <summary>Splits comma-separated tags the way the state keeps them: trimmed, lower case, no empty ones, no repeats.</summary>
    internal static IReadOnlyList<string> ParseTags(string value) =>
        value.Split(',').Select(tag => tag.Trim().ToLowerInvariant()).Where(tag => tag.Length > 0).Distinct().ToList();

    #endregion

    #region Private Methods

    private static string CounterText(string value, int max) => string.Create(CultureInfo.InvariantCulture, $"{value.Trim().Length} of {max} characters");

    [GeneratedRegex("^[a-z0-9][a-z0-9-]*$")]
    private static partial Regex TagPattern();

    private static string? TagsProblem(IReadOnlyList<string> tags)
    {
        if (tags.Count > AiState.MaxTags)
            return string.Create(CultureInfo.InvariantCulture, $"Use at most {AiState.MaxTags} tags (now {tags.Count}).");

        var tooLong = tags.Where(tag => tag.Length > AiState.MaxTagLength).ToList();

        if (tooLong.Count > 0)
            return $"Shorten {Quoted(tooLong)} to {AiState.MaxTagLength} characters or fewer.";

        var wrong = tags.Where(tag => !TagPattern().IsMatch(tag)).ToList();

        return wrong.Count > 0
            ? $"Use only lower-case letters, digits, and hyphens, starting with a letter or digit: {Quoted(wrong)} {(wrong.Count == 1 ? "doesn't" : "don't")} fit."
            : null;
    }

    private static string Quoted(IEnumerable<string> values) => string.Join(", ", values.Select(value => $"\"{value}\""));

    private static string? TextProblem(string value, int max, string missing, string what)
    {
        var length = value.Trim().Length;

        return length == 0 ? missing
            : length > max ? string.Create(CultureInfo.InvariantCulture, $"Shorten the {what} to {max} characters or fewer (now {length}).")
            : null;
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Fills the form from a prompt, or empties it for a new one.</summary>
    private void Load(AiPrompt? prompt)
    {
        title = prompt?.Title ?? string.Empty;
        text = prompt?.Text ?? string.Empty;
        tagsText = prompt is null ? string.Empty : string.Join(", ", prompt.Tags);
        titleError = textError = tagsError = null;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Checks the form, saves the shared prompt, and maps the state's answer to a field or the status message.</summary>
    private void Save()
    {
        var existing = Prompt;

        if (Id is not null && existing is null)
            return;

        var tags = ParseTags(tagsText);
        titleError = TextProblem(title, AiState.MaxPromptTitleLength, "Give the prompt a title.", "title");
        textError = TextProblem(text, AiState.MaxPromptTextLength, "Write the prompt text.", "prompt text");
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

        var result = State.SavePrompt(existing?.Id ?? 0, title, text, tags, shared: true, out var saved);

        switch (result)
        {
            case AiResult.Ok when saved is not null:
                Load(saved);

                if (existing is null)
                {
                    Announce(true, $"\"{saved.Title}\" is shared: every member sees it in their prompt library now.");

                    // The edit route renders the same page, so the form and the status message stay.
                    createdId = saved.Id;
                    Navigation.NavigateTo(AdminPrompts.EditUrl(saved.Id), replace: true);

                    return;
                }

                Announce(true, $"\"{saved.Title}\" is saved. Members see the change in their prompt library.");

                return;

            case AiResult.NoChange:
                Announce(true, "Nothing changed: the prompt is already saved like this.");

                return;

            case AiResult.NotFound:
                Announce(false, "This prompt was deleted, or it isn't a shared prompt, so the changes couldn't be saved.");
                focusStatus = true;

                return;

            case AiResult.NotAllowed:
                Announce(false, "Only active workspace admins can change shared prompts. Nothing was saved.");
                focusStatus = true;

                return;

            case AiResult.Invalid:
                Announce(false, existing is null
                    ? $"The prompt couldn't be shared: a value is missing or too long, the tab already holds {AiState.MaxPrompts} saved prompts (personal and shared together), or its storage is full. Nothing was saved."
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

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Shared prompts", Href = AdminPrompts.ListUrl },
        new() { Text = Prompt?.Title ?? "New shared prompt", IsCurrentPage = true },
    };

    /// <summary>Gets or sets the prompt ID from the route, or <see langword="null" /> on the new-prompt route.</summary>
    [Parameter] public int? Id { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets the shared prompt being edited, or <see langword="null" /> (new, or not a shared prompt).</summary>
    private AiPrompt? Prompt => Id is { } id && State.FindPrompt(id) is { IsShared: true } prompt ? prompt : null;

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
