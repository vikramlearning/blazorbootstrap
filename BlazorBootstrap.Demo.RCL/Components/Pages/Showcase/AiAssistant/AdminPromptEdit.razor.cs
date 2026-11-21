using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Adds (<c>/new</c>) or edits a shared workspace prompt (prompt editors, Admins, and the Owner) with
/// <see cref="AiPromptForm" />: title unique among the shared prompts, category, text and its variables, tags, and whether
/// it is published. A save is announced in the status region, which takes focus; saving a new prompt moves to its edit
/// route and keeps the message. A personal prompt's ID, or an unknown one, shows <see cref="AiNotFound" />.
/// </summary>
public partial class AdminPromptEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The ID the router moves to after saving a new prompt: the status message stays.</summary>
    private int? createdId;

    private bool focusStatus;

    /// <summary>The route ID the page was shown for (0 for a new prompt), or <see langword="null" /> before the first render.</summary>
    private int? loadedId;

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

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

    protected override void OnParametersSet()
    {
        var routeId = Id ?? 0;

        if (loadedId == routeId)
            return;

        loadedId = routeId;

        // Saving a new prompt moves to its edit route: the status message stays as it is.
        if (createdId is not null && createdId == routeId)
        {
            createdId = null;

            return;
        }

        statusMessage = statusText = null;
        statusCount = 0;
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

    #region Private Methods

    private void Announce(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
        focusStatus = true;
    }

    /// <summary>Says what the save did; a new prompt moves to its edit route, where the message stays.</summary>
    private Task OnSavedAsync(AiPromptSaved outcome)
    {
        var prompt = outcome.Prompt;

        if (outcome.Unchanged)
        {
            Announce("Nothing changed: the prompt is already saved like this.");

            return Task.CompletedTask;
        }

        Announce(prompt.IsPublished
            ? outcome.Created ? $"\"{prompt.Title}\" is published: every member sees it in their prompt library now, and hears about it." : $"\"{prompt.Title}\" is saved. Members see the change in their prompt library."
            : $"\"{prompt.Title}\" is saved as a draft. Only prompt editors, Admins, and the Owner see it until you publish it.");

        if (outcome.Created)
        {
            // The edit route renders the same page, so the status message stays.
            createdId = prompt.Id;
            Navigation.NavigateTo(AdminPrompts.EditUrl(prompt.Id), replace: true);
        }

        return Task.CompletedTask;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

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

    /// <summary>Gets the shared prompt being edited, drafts included, or <see langword="null" /> (new, or not a shared prompt).</summary>
    private AiPrompt? Prompt => Id is { } id && State.FindPrompt(id) is { IsShared: true } prompt ? prompt : null;

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
