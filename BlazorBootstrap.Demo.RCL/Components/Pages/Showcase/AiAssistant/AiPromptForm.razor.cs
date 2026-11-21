using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The prompt form of both editors (rule WR-002) in an <see cref="EditForm" />: title (required, up to 80 characters, unique
/// in its scope — the member's own prompts, or the shared library — checked as you type and again on submit), category
/// (required), text (required, <see cref="AiState.MinPromptTextLength" /> to <see cref="AiState.MaxPromptTextLength" />
/// characters, its <c>{{variables}}</c> well formed and declared), the declared variables (each used in the text: a
/// cross-field rule, see <see cref="AiPromptText" />), tags (lower-case words), and for a shared prompt whether it is
/// published. Messages show after a field is touched or on submit, with a summary that links to each field; every
/// <see cref="AiState.SavePrompt" /> refusal is mapped to its field or the alert. The page decides what follows a save
/// (<see cref="OnSaved" />).
/// </summary>
public partial class AiPromptForm : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private PromptFormModel form = new();

    /// <summary>The prompt the form was filled from (0 for a new one), or <see langword="null" /> before the first fill.</summary>
    private int? loadedId;

    /// <summary>The state's answers that belong to a field (a taken title, a rule the state refused), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private ShowcaseUniqueCheck? titleCheck;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnParametersSet()
    {
        var id = Prompt?.Id ?? 0;

        if (loadedId == id)
            return;

        loadedId = id;
        ClearError();
        Load(Prompt);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to the form's changes.</summary>
    public void Dispose()
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Internal Methods

    /// <summary>Splits the tags field like the state does: by comma, trimmed, lower case, empty ones dropped, each once.</summary>
    internal static List<string> ParseTags(string? value) =>
        (value ?? string.Empty).Split(',').Select(tag => tag.Trim().ToLowerInvariant()).Where(tag => tag.Length > 0).Distinct(StringComparer.Ordinal).ToList();

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

    private static string CounterText(string value) =>
        string.Create(CultureInfo.InvariantCulture, $"{value.Trim().Length:#,0} of {AiState.MaxPromptTextLength:#,0} characters");

    [GeneratedRegex("^[a-z0-9][a-z0-9-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex TagPattern();

    private void ClearError()
    {
        errorMessage = errorText = null;
        errorCount = 0;
    }

    /// <summary>Declares exactly the variables the text uses (a button, so no field handler changes another field).</summary>
    private void FillVariables()
    {
        form.VariablesText = string.Join(", ", AiPromptText.VariablesIn(form.Text));
        editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.VariablesText));
    }

    private Task<bool> IsTitleTakenAsync(string title) => Task.FromResult(State.IsPromptTitleTaken(title, Shared, Prompt?.Id ?? 0));

    /// <summary>Fills the form from a prompt, or with a new prompt's starting values.</summary>
    private void Load(AiPrompt? prompt)
    {
        var next = new PromptFormModel
        {
            Title = prompt?.Title ?? string.Empty,
            Category = prompt?.Category,
            Text = prompt?.Text ?? string.Empty,
            VariablesText = prompt is null ? string.Empty : string.Join(", ", prompt.Variables),
            TagsText = prompt is null ? string.Empty : string.Join(", ", prompt.Tags),
            Publish = prompt?.IsPublished ?? false,
        };

        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>Any change clears the last error and the state's message on that field; no field changes another, so nothing re-enters.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);
        ClearError();
    }

    /// <summary>
    /// Saves the prompt once the form is valid: the title check finishes first, then every rule runs, and a failed submit
    /// shows the summary (rule WR-002). The state checks again; its answer maps to a field or the alert.
    /// </summary>
    private async Task SaveAsync()
    {
        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();

        var titleFree = titleCheck is null || await titleCheck.CheckNowAsync();

        if (!editContext.Validate() || !titleFree || form.Category is not { } category)
            return;

        var input = new AiPromptInput(form.Title, form.Text, category, AiPromptText.ParseVariables(form.VariablesText), ParseTags(form.TagsText), Shared && form.Publish);
        var existing = Prompt;
        var result = State.SavePrompt(existing?.Id ?? 0, input, Shared, out var saved);

        switch (result)
        {
            case AiResult.Ok when saved is not null:
                await OnSaved.InvokeAsync(new AiPromptSaved(saved, existing is null, false));

                return;

            case AiResult.NoChange when saved is not null:
                await OnSaved.InvokeAsync(new AiPromptSaved(saved, false, true));

                return;

            case AiResult.Duplicate:
                ServerError(() => form.Title, TitleTakenMessage(form.Title));

                return;

            case AiResult.NotFound:
                ShowError("This prompt was deleted, so the changes couldn't be saved. Copy your text if you want to keep it.");

                return;

            case AiResult.NotAllowed:
                ShowError(Shared ? "Only prompt editors, Admins, and the Owner change shared prompts. Nothing was saved." : "Your membership isn't active, so the prompt couldn't be saved.");

                return;

            case AiResult.Invalid when existing is null && State.PromptCount >= AiState.MaxPrompts:
                ShowError($"This tab already holds {AiState.MaxPrompts} saved prompts, the most it can. Delete one you don't use, then try again. Nothing was saved.");

                return;

            case AiResult.Invalid when !AiPromptText.IsConsistent(form.Text.Trim(), AiPromptText.ParseVariables(form.VariablesText)):
                ServerError(() => form.VariablesText, AiPromptText.VariablesProblem(form.Text, AiPromptText.ParseVariables(form.VariablesText))
                    ?? AiPromptText.TextProblem(form.Text, AiPromptText.ParseVariables(form.VariablesText)) ?? "The variables don't agree with the text.");

                return;

            case AiResult.Invalid:
                ShowError("The prompt couldn't be saved: a value is out of range, or this tab's storage is full. Nothing was saved.");

                return;

            default:
                ShowError("That didn't work, and nothing was saved. Try again.");

                return;
        }
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    private string TitleTakenMessage(string title) => Shared
        ? $"A shared prompt is already called \"{title.Trim()}\". Choose another title."
        : $"You already have a prompt called \"{title.Trim()}\". Choose another title, or edit that one.";

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets where Cancel goes.</summary>
    [Parameter, EditorRequired] public string CancelUrl { get; set; } = default!;

    /// <summary>Gets or sets the form's name (unique on the page).</summary>
    [Parameter, EditorRequired] public string FormName { get; set; } = default!;

    /// <summary>Gets or sets the prefix of the fields' IDs.</summary>
    [Parameter, EditorRequired] public string IdPrefix { get; set; } = default!;

    /// <summary>Gets or sets what runs after a save (or a save that changed nothing): the page announces it or moves on.</summary>
    [Parameter] public EventCallback<AiPromptSaved> OnSaved { get; set; }

    /// <summary>Gets or sets the prompt being edited, or <see langword="null" /> for a new one.</summary>
    [Parameter] public AiPrompt? Prompt { get; set; }

    /// <summary>Gets or sets whether it is a shared prompt (prompt editors, Admins, and the Owner) rather than the member's own.</summary>
    [Parameter] public bool Shared { get; set; }

    [Inject] private AiState State { get; set; } = default!;

    private string SubmitText => Prompt is null ? Shared ? "Save shared prompt" : "Save prompt" : "Save changes";

    #endregion

    #region Nested Types

    /// <summary>
    /// The prompt's fields, with the limits of <see cref="AiState.SavePrompt" />: a required title, category, and text; the
    /// text's variables against the declared ones (cross-field), and tags of the allowed form.
    /// </summary>
    private sealed class PromptFormModel : IValidatableObject
    {
        [Required(ErrorMessage = "Choose a category.")]
        public AiPromptCategory? Category { get; set; }

        public bool Publish { get; set; }

        public string TagsText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Write the prompt text: what you'd type in the chat.")]
        [StringLength(AiState.MaxPromptTextLength, MinimumLength = AiState.MinPromptTextLength, ErrorMessage = "Use 10 to 4,000 characters for the prompt text.")]
        public string Text { get; set; } = string.Empty;

        [Required(ErrorMessage = "Give the prompt a title.")]
        [StringLength(AiState.MaxPromptTitleLength, ErrorMessage = "Use 80 characters or fewer for the title.")]
        public string Title { get; set; } = string.Empty;

        public string VariablesText { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: the text's variables and the declared ones agree, and the tags fit.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var variables = AiPromptText.ParseVariables(VariablesText);

            if (Text.Trim().Length >= AiState.MinPromptTextLength && AiPromptText.TextProblem(Text, variables) is { } textProblem)
                yield return new ValidationResult(textProblem, new[] { nameof(Text) });

            if (AiPromptText.VariablesProblem(Text, variables) is { } variablesProblem)
                yield return new ValidationResult(variablesProblem, new[] { nameof(VariablesText) });

            if (TagsProblem(ParseTags(TagsText)) is { } tagsProblem)
                yield return new ValidationResult(tagsProblem, new[] { nameof(TagsText) });
        }
    }

    #endregion
}

/// <summary>A prompt the form saved: whether it was new, and whether nothing changed.</summary>
public sealed record AiPromptSaved(AiPrompt Prompt, bool Created, bool Unchanged);
