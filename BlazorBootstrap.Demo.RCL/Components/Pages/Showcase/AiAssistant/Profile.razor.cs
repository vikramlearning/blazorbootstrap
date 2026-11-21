using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The member's profile (the account menu links here): the shared name, email, and password editor
/// (<c>ShowcaseAccountProfile</c>, which validates them); the chat preferences in an <see cref="EditForm" /> (rule WR-002:
/// the default model for new chats must be one the workspace enables, or the workspace default, a rule against the model
/// policy; and whether Enter sends), saved with <see cref="AiState.SavePreferences" /> and every answer said in the status
/// region or the alert; a note in words when the saved model was turned off by an admin; and the workspace membership
/// (workspace, role and what it may do, joined date, quota, and for the roles with an admin page a link to it).
/// </summary>
public partial class Profile : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext;

    private int errorCount;

    private string? errorMessage;

    private PreferencesForm form = new();

    private bool focusStatus;

    /// <summary>The account the form was filled for, so it fills again when the tab's data is restored or the account changes.</summary>
    private string? loadedAccountId;

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with an empty form.</summary>
    public Profile() => editContext = new EditContext(form);

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
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

    private void Announce(string message)
    {
        statusCount = statusMessage?.StartsWith(message, StringComparison.Ordinal) == true ? statusCount + 1 : 1;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
        errorMessage = null;
        focusStatus = true;
    }

    /// <summary>Fills the form from the saved preferences once per account; a saved model the admin turned off shows as the workspace default.</summary>
    private void LoadIfNeeded()
    {
        var accountId = State.Identity.Current?.Id;

        if (accountId is null || accountId == loadedAccountId)
            return;

        loadedAccountId = accountId;
        var saved = State.Preferences;
        form = new PreferencesForm
        {
            ModelId = saved.ModelId is { } id && State.Policy.EnabledModelIds.Contains(id) ? id : string.Empty,
            EnterToSend = saved.EnterToSend,
            EnabledModelIds = State.Policy.EnabledModelIds.ToList(),
        };
        editContext = new EditContext(form);
    }

    private string ModelName(string? modelId) =>
        modelId is null ? $"the workspace default ({State.FindModel(State.Policy.DefaultModelId)?.Name ?? State.Policy.DefaultModelId})" : State.FindModel(modelId)?.Name ?? modelId;

    private void OnStateChanged() => InvokeAsync(() =>
    {
        LoadIfNeeded();
        StateHasChanged();
    });

    /// <summary>Saves the preferences once the form is valid (the model must be one the workspace enables now); the state checks again.</summary>
    private void SavePreferences()
    {
        form.EnabledModelIds = State.Policy.EnabledModelIds.ToList();

        if (!editContext.Validate())
            return;

        var modelId = form.ModelId.Length == 0 ? null : form.ModelId;
        var result = State.SavePreferences(modelId, form.EnterToSend);
        var keys = form.EnterToSend ? "Enter sends a message" : "Ctrl+Enter sends a message";

        switch (result)
        {
            case AiResult.Ok:
                Announce($"Preferences saved. New chats start with {ModelName(modelId)}, and {keys}.");
                break;

            case AiResult.NoChange:
                Announce("Nothing changed: these are already your preferences.");
                break;

            case AiResult.WrongState:
                ShowError($"{ModelName(modelId)} isn't offered any more: an Admin turned it off. Choose another model, then save again.");
                break;

            case AiResult.NotAllowed:
                ShowError("Your membership isn't active, so your preferences weren't saved.");
                break;

            default:
                ShowError("That didn't work, and your preferences weren't saved. Try again.");
                break;
        }
    }

    private void ShowError(string message)
    {
        errorCount = errorMessage?.StartsWith(message, StringComparison.Ordinal) == true ? errorCount + 1 : 1;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private AiState State { get; set; } = default!;

    /// <summary>The model the member saved as their default when an admin has turned it off since, or <see langword="null" />.</summary>
    private AiModel? TurnedOffPreference =>
        State.Preferences.ModelId is { } id && !State.Policy.EnabledModelIds.Contains(id) ? State.FindModel(id) : null;

    #endregion

    #region Nested Types

    /// <summary>The chat preferences: a model the workspace enables (a rule against the model policy, set by the page), or none for the workspace default.</summary>
    private sealed class PreferencesForm : IValidatableObject
    {
        /// <summary>The models the workspace enables now, set by the page before it validates.</summary>
        public List<string> EnabledModelIds { get; set; } = new();

        public bool EnterToSend { get; set; } = true;

        /// <summary>The chosen model, or an empty string for the workspace default.</summary>
        public string ModelId { get; set; } = string.Empty;

        /// <summary>Runs on submit: the chosen model must still be one the workspace enables.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ModelId.Length > 0 && !EnabledModelIds.Contains(ModelId))
                yield return new ValidationResult("That model isn't offered any more: an Admin turned it off. Choose one of the models listed, or the workspace default.", new[] { nameof(ModelId) });
        }
    }

    #endregion
}
