using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The shared prompt library of the Parley workspace admin back office: every shared prompt (title with a short excerpt,
/// tags, author, uses, last update), the most used first, with Edit (<see cref="AdminPromptEdit" />) and Delete. Delete
/// opens a <see cref="Modal" /> saying members will no longer see the prompt; it is deleted once the dialog is hidden, and
/// focus moves to the status message (the row is gone) or back to the Delete button when it failed. The routes of the
/// editor are shared from here.
/// </summary>
public partial class AdminPrompts : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The Delete buttons by prompt ID, for focus after the dialog.</summary>
    private readonly Dictionary<int, Button> deleteButtons = new();

    private Modal deleteModal = default!;

    /// <summary>The prompt the dialog is about.</summary>
    private int dialogPromptId;

    /// <summary>The prompt whose Delete button takes focus after the next render.</summary>
    private int? focusButtonId;

    private bool focusStatus;

    /// <summary>Whether Delete was confirmed; it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusButtonId is { } id)
        {
            focusButtonId = null;

            if (State.FindPrompt(id) is { IsShared: true } && deleteButtons.TryGetValue(id, out var button))
                await AiDisplay.FocusAsync(button.Element);
            else
                focusStatus = true;
        }

        if (focusStatus)
        {
            focusStatus = false;
            await AiDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the route of a shared prompt's editor.</summary>
    internal static string EditUrl(int promptId) => string.Create(CultureInfo.InvariantCulture, $"{ListUrl}/{promptId}/edit");

    #endregion

    #region Private Methods

    /// <summary>The first line of a prompt, cut near 90 characters.</summary>
    private static string Excerpt(string text)
    {
        var line = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;

        return line.Length <= 90 ? line : line[..90].TrimEnd() + "…";
    }

    private async Task ConfirmDeleteAsync()
    {
        if (pendingDelete)
            return;

        pendingDelete = true;
        await deleteModal.HideAsync();
    }

    /// <summary>Deletes the prompt now that the dialog is hidden, and says what happened.</summary>
    private void OnDeleteHidden()
    {
        var id = dialogPromptId;

        if (!pendingDelete)
        {
            focusButtonId = id;

            return;
        }

        pendingDelete = false;
        var prompt = State.FindPrompt(id);
        var title = prompt?.Title ?? "The prompt";
        var result = prompt is { IsShared: true } ? State.DeletePrompt(id) : AiResult.NotFound;

        statusIsError = result != AiResult.Ok;
        statusMessage = result switch
        {
            AiResult.Ok => $"\"{title}\" is deleted. Members no longer see it in their prompt library.",
            AiResult.NotFound => "That prompt was already deleted, so nothing changed.",
            AiResult.NotAllowed => "Only active workspace admins can delete shared prompts. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        };

        if (result == AiResult.Ok)
        {
            deleteButtons.Remove(id);
            focusStatus = true;
        }
        else
        {
            focusButtonId = id;
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteAsync(int promptId)
    {
        dialogPromptId = promptId;
        pendingDelete = false;

        await deleteModal.ShowAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private AiPrompt? DialogPrompt => State.FindPrompt(dialogPromptId) is { IsShared: true } prompt ? prompt : null;

    /// <summary>Gets the route of the shared prompt list.</summary>
    internal static string ListUrl => $"{AiAdminLayout.Base}/prompts";

    /// <summary>Gets the route of the new shared prompt editor.</summary>
    internal static string NewUrl => $"{ListUrl}/new";

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
