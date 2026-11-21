using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Creates (<c>/prompts/new</c>) or edits (<c>/prompts/{id}/edit</c>) one of the member's own prompts with
/// <see cref="AiPromptForm" />: title unique among their own prompts, category, text and its variables, and tags. A save
/// goes back to the prompt library, whose status region says so (<c>?saved={id}&amp;added=1</c>). A shared prompt opens
/// read-only, with its variables and a link to the back office editor for those who manage shared prompts. An unknown ID,
/// or a draft shared prompt for a member, shows <see cref="AiNotFound" />.
/// </summary>
public partial class PromptEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool focusStatus;

    /// <summary>The prompt was saved and the library is loading.</summary>
    private bool leaving;

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

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

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>Returns the route of the editor of a prompt.</summary>
    public static string EditUrl(int promptId) => AiSeedData.PromptPath(promptId);

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

    #region Private Methods

    private static List<BreadcrumbItem> BreadcrumbItems(AiPrompt? prompt) => new()
    {
        new() { Text = "Prompt library", Href = Prompts.ListUrl },
        new() { Text = prompt is null ? "New prompt" : prompt.Title, IsCurrentPage = true },
    };

    private bool IsOwn(AiPrompt prompt) => !prompt.IsShared && prompt.OwnerId == State.Identity.Current?.Id;

    /// <summary>A save goes back to the library, which says so; a save that changed nothing is said here.</summary>
    private Task OnSavedAsync(AiPromptSaved outcome)
    {
        if (outcome.Unchanged)
        {
            const string message = "Nothing changed: the prompt is already saved like this.";
            statusCount = statusMessage?.StartsWith(message, StringComparison.Ordinal) == true ? statusCount + 1 : 1;
            statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
            focusStatus = true;

            return Task.CompletedTask;
        }

        leaving = true;
        Navigation.NavigateTo(string.Create(CultureInfo.InvariantCulture, $"{Prompts.ListUrl}?saved={outcome.Prompt.Id}{(outcome.Created ? "&added=1" : null)}"));

        return Task.CompletedTask;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the prompt ID from the route, or <see langword="null" /> on the new-prompt route.</summary>
    [Parameter] public int? Id { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>The prompt of the route (own or a shared one the member may see), or <see langword="null" /> for a new prompt or an unknown ID.</summary>
    private AiPrompt? Prompt => Id is { } id ? State.FindPrompt(id) : null;

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
