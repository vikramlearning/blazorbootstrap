using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Layout of the Parley workspace admin back office (D-031): a top bar with "Back to chat" and the account menu, a sidebar
/// with the admin modules, and the page behind <see cref="AiMemberGuard" /> for active workspace admins. Workspace roles
/// come from the membership, so an admin promoted in this tab gets here too.
/// </summary>
public partial class AiAdminLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the admin back office.</summary>
    public const string Base = AiLayout.Base + "/admin";

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The admin modules.</summary>
    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "overview", Href = Base, IconName = IconName.BarChart, Text = "Usage analytics", Match = NavLinkMatch.All },
            new() { Id = "members", Href = $"{Base}/members", IconName = IconName.People, Text = "Members and roles" },
            new() { Id = "prompts", Href = $"{Base}/prompts", IconName = IconName.JournalText, Text = "Shared prompts" },
            new() { Id = "settings", Href = $"{Base}/settings", IconName = IconName.Sliders, Text = "Models and policy" },
        };

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
