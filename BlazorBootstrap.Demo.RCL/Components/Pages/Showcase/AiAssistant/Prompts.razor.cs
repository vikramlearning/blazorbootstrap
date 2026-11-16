using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The prompt library of a member: "My prompts" (<see cref="AiState.MyPrompts" />, the most recently changed first) and
/// "Shared with the workspace" (<see cref="AiState.SharedPrompts" />, the most used first, read-only here: workspace admins
/// manage them in the back office). A labelled filter box (title, text, or tag) and tag toggle buttons narrow both lists.
/// Each prompt shows its title, the first lines of its text, its tags, when it changed and how often it was used, and a
/// "Use in chat" link (the chat page puts it in the composer and counts the use); the member's own prompts also have Edit
/// and Delete, which asks first in a dialog naming the prompt and deletes once the dialog is hidden. The status region
/// reports every outcome and a save from the editor (<see cref="Saved" />).
/// </summary>
public partial class Prompts : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The route of the prompt library.</summary>
    public const string ListUrl = AiLayout.Base + "/prompts";

    /// <summary>The route of the new-prompt editor.</summary>
    public const string NewUrl = AiLayout.Base + "/prompts/new";

    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    /// <summary>Set when Delete was confirmed, so the prompt is deleted once the dialog is hidden.</summary>
    private bool deleteConfirmed;

    private Modal deleteModal = default!;

    /// <summary>The prompt the dialog is about, captured when it opens, so the text stays put while the dialog closes.</summary>
    private AiPrompt? deleting;

    /// <summary>Where focus goes after the next render, if anywhere.</summary>
    private ElementReference? focusNext;

    private bool focusSearch;

    private string search = string.Empty;

    private ElementReference searchInput;

    private string? selectedTag;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusSearch)
        {
            focusSearch = false;
            await AiDisplay.FocusAsync(searchInput);
        }

        if (focusNext is { } target)
        {
            focusNext = null;
            await AiDisplay.FocusAsync(target);
        }
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;

        if (Saved is { } id && State.FindPrompt(id) is { } prompt)
            Announce(true, Added == true
                ? $"\"{prompt.Title}\" is saved to your prompts. Use it in chat whenever you need it."
                : $"Your changes to \"{prompt.Title}\" are saved.");
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the chat route that starts with a saved prompt in the composer.</summary>
    internal static string ChatUrl(int promptId) => string.Create(CultureInfo.InvariantCulture, $"{AiLayout.Base}/chat?prompt={promptId}");

    /// <summary>Returns "1 prompt" or "3 prompts".</summary>
    internal static string CountText(int count, string noun) =>
        count == 1 ? $"1 {noun}" : string.Create(CultureInfo.InvariantCulture, $"{count:#,0} {noun}s");

    /// <summary>Returns how often a prompt was used, in words.</summary>
    internal static string UseText(int count) => count switch
    {
        0 => "not used yet",
        1 => "used once",
        _ => string.Create(CultureInfo.InvariantCulture, $"used {count:#,0} times"),
    };

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>The tags of every prompt shown, alphabetically; the selected tag stays even when no prompt has it now.</summary>
    private List<string> AllTags(IReadOnlyList<AiPrompt> mine, IReadOnlyList<AiPrompt> shared)
    {
        var tags = mine.Concat(shared).SelectMany(prompt => prompt.Tags).ToHashSet(StringComparer.Ordinal);

        if (selectedTag is not null)
            tags.Add(selectedTag);

        return tags.Order(StringComparer.Ordinal).ToList();
    }

    private async Task AskDeleteAsync(AiPrompt prompt)
    {
        deleting = prompt;
        deleteConfirmed = false;
        await deleteModal.ShowAsync();
    }

    private void ClearFilters()
    {
        search = string.Empty;
        selectedTag = null;
        focusSearch = true;
    }

    private async Task ConfirmDeleteAsync()
    {
        deleteConfirmed = true;
        await deleteModal.HideAsync();
    }

    private bool Matches(AiPrompt prompt)
    {
        var term = search.Trim();

        return (selectedTag is null || prompt.Tags.Contains(selectedTag))
            && (term.Length == 0 || prompt.Title.Contains(term, StringComparison.OrdinalIgnoreCase) || prompt.Text.Contains(term, StringComparison.OrdinalIgnoreCase)
                || prompt.Tags.Any(tag => tag.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// The dialog opens from code, so Bootstrap does not return focus. After a deletion, focus goes to the status message
    /// (the prompt's buttons are gone); otherwise back to the Delete button that opened the dialog.
    /// </summary>
    private async Task OnDeleteHiddenAsync()
    {
        var prompt = deleting;
        var confirmed = deleteConfirmed;
        deleting = null;
        deleteConfirmed = false;

        if (prompt is null)
            return;

        if (!confirmed)
        {
            if (deleteButtons.TryGetValue(prompt.Id, out var button))
                await AiDisplay.FocusAsync(button);

            return;
        }

        var result = State.DeletePrompt(prompt.Id);

        switch (result)
        {
            case AiResult.Ok:
                deleteButtons.Remove(prompt.Id);
                Announce(true, $"\"{prompt.Title}\" is deleted from your prompts.");
                break;

            case AiResult.NotFound:
                Announce(false, $"\"{prompt.Title}\" was already deleted, so nothing changed.");
                break;

            case AiResult.NotAllowed:
                Announce(false, "Your membership isn't active any more, so the prompt wasn't deleted.");
                break;

            default:
                Announce(false, $"\"{prompt.Title}\" couldn't be deleted. Try again.");
                break;
        }

        focusNext = statusRegion;
        StateHasChanged();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether the prompt in <see cref="Saved" /> was just created (rather than edited).</summary>
    [SupplyParameterFromQuery(Name = "added")] public bool? Added { get; set; }

    /// <summary>Gets or sets the prompt the editor just saved, so the status region says so.</summary>
    [SupplyParameterFromQuery(Name = "saved")] public int? Saved { get; set; }

    /// <summary>The filter in words, like "the tag "code"" or ""sql" with the tag "code"".</summary>
    private string FilterText
    {
        get
        {
            var term = search.Trim();

            return (term.Length > 0, selectedTag) switch
            {
                (true, { } tag) => $"\"{term}\" with the tag \"{tag}\"",
                (true, null) => $"\"{term}\"",
                (false, { } tag) => $"the tag \"{tag}\"",
                _ => "the filter",
            };
        }
    }

    private bool IsFiltered => search.Trim().Length > 0 || selectedTag is not null;

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
