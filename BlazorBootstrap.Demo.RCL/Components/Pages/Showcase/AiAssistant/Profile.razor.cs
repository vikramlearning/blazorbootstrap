using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The member's profile (the account menu links here): the shared name, email, and password editor
/// (<c>ShowcaseAccountProfile</c>); the chat preferences (<see cref="AiState.Preferences" />: the default
/// model for new chats, from the models the workspace enables or the workspace default, and whether Enter sends), saved
/// with <see cref="AiState.SavePreferences" /> and every answer said in the status region; a note in words when the saved
/// model was turned off by an admin; and the workspace membership (workspace, role in words, joined date, quota, and for
/// admins a link to the back office).
/// </summary>
public partial class Profile : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool enterToSend = true;

    private bool focusModel;

    private bool focusStatus;

    /// <summary>The chosen model ID, or an empty string for the workspace default.</summary>
    private string modelChoice = string.Empty;

    private ElementReference? modelSelect;

    /// <summary>The account the form was filled for, so it fills again when the tab's data is restored or the account changes.</summary>
    private string? loadedAccountId;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusModel)
        {
            focusModel = false;
            await AiDisplay.FocusAsync(modelSelect);
        }

        if (focusStatus)
        {
            focusStatus = false;
            await AiDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        LoadIfNeeded();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Fills the form from the saved preferences once per account; a saved model the admin turned off shows as the workspace default.</summary>
    private void LoadIfNeeded()
    {
        var accountId = State.Identity.Current?.Id;

        if (accountId is null || accountId == loadedAccountId)
            return;

        loadedAccountId = accountId;
        var saved = State.Preferences;
        modelChoice = saved.ModelId is { } id && State.Policy.EnabledModelIds.Contains(id) ? id : string.Empty;
        enterToSend = saved.EnterToSend;
    }

    private string ModelName(string? modelId) =>
        modelId is null ? $"the workspace default ({State.FindModel(State.Policy.DefaultModelId)?.Name ?? State.Policy.DefaultModelId})" : State.FindModel(modelId)?.Name ?? modelId;

    private void OnStateChanged() => InvokeAsync(() =>
    {
        LoadIfNeeded();
        StateHasChanged();
    });

    private void SavePreferences()
    {
        var modelId = modelChoice.Length == 0 ? null : modelChoice;
        var result = State.SavePreferences(modelId, enterToSend);
        var keys = enterToSend ? "Enter sends a message" : "Ctrl+Enter sends a message";

        switch (result)
        {
            case AiResult.Ok:
                Announce(true, $"Preferences saved. New chats start with {ModelName(modelId)}, and {keys}.");
                break;

            case AiResult.NoChange:
                Announce(true, "Nothing changed: these are already your preferences.");
                break;

            case AiResult.WrongState:
                Announce(false, $"{ModelName(modelId)} isn't offered any more: a workspace admin turned it off. Choose another model, then save again.");
                modelChoice = string.Empty;
                focusModel = true;

                return;

            case AiResult.NotAllowed:
                Announce(false, "Your membership isn't active, so your preferences weren't saved.");
                break;

            default:
                Announce(false, "That didn't work, and your preferences weren't saved. Try again.");
                break;
        }

        focusStatus = result is not (AiResult.Ok or AiResult.NoChange);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private AiState State { get; set; } = default!;

    /// <summary>The model the member saved as their default when a workspace admin has turned it off since, or <see langword="null" />.</summary>
    private AiModel? TurnedOffPreference =>
        State.Preferences.ModelId is { } id && !State.Policy.EnabledModelIds.Contains(id) ? State.FindModel(id) : null;

    #endregion
}
