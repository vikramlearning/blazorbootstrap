using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Layout of the Parley workspace admin back office (D-031): a top bar with the global search, the notifications, "Back to
/// chat", and the account menu; a sidebar with the admin modules the signed-in role may use (<see cref="AiState.RoleAllows" />);
/// and the page behind <see cref="AiMemberGuard" /> for active members (each page adds its own guard for the permission it
/// needs). Workspace roles come from the membership, so a role changed in this tab applies at once.
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

    /// <summary>The admin modules the signed-in role may use.</summary>
    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>();

        if (State.Can(AiPermission.ViewAnalytics))
            items.Add(new NavItem { Id = "overview", Href = Base, IconName = IconName.BarChart, Text = "Usage analytics", Match = NavLinkMatch.All });

        if (State.Can(AiPermission.ManageMembers))
            items.Add(new NavItem { Id = "members", Href = $"{Base}/members", IconName = IconName.People, Text = "Members and roles" });

        if (State.Can(AiPermission.ManageSharedPrompts))
            items.Add(new NavItem { Id = "prompts", Href = $"{Base}/prompts", IconName = IconName.JournalText, Text = "Shared prompts" });

        if (State.Can(AiPermission.ManageSettings))
            items.Add(new NavItem { Id = "settings", Href = $"{Base}/settings", IconName = IconName.Sliders, Text = "Models and policy" });

        if (State.Can(AiPermission.ManageSpend))
            items.Add(new NavItem { Id = "spend", Href = $"{Base}/spend", IconName = IconName.PiggyBank, Text = "Spend" });

        if (State.Can(AiPermission.ReviewSensitiveData))
            items.Add(new NavItem { Id = "sensitive-data", Href = $"{Base}/sensitive-data", IconName = IconName.ShieldExclamation, Text = "Sensitive data review" });

        if (State.Can(AiPermission.ViewAudit))
            items.Add(new NavItem { Id = "audit", Href = $"{Base}/audit", IconName = IconName.ClockHistory, Text = "Audit log" });

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>What the header search finds, in words, for the signed-in role.</summary>
    private string SearchLabel => State.Can(AiPermission.ManageMembers)
        ? "Search members and invitations by name or email, prompts by title, and your conversations by title"
        : "Search your prompts and the shared prompts (drafts included) by title, and your conversations by title";

    private string SearchPlaceholder => State.Can(AiPermission.ManageMembers) ? "Search members, prompts, chats" : "Search prompts and chats";

    [Inject] private AiState State { get; set; } = default!;

    #endregion
}
