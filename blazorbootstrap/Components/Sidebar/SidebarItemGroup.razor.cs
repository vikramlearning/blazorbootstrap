namespace BlazorBootstrap;

public partial class SidebarItemGroup : BaseComponent
{
    #region Events

    private IReadOnlyList<NavItem>? childItems;

    #endregion Events

    #region Members

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();

        childItems = new List<NavItem>
        {
            new NavItem{ Href = "/sidebar", PrefixIconName = IconName.LayoutSidebarInset, Sequence = 1, Text = "Sidebar"},
            new NavItem{ Href = "/form/switch", PrefixIconName = IconName.ToggleOn, Sequence = 2, Text = "Switch"},
            new NavItem{ Href = "/tabs", PrefixIconName = IconName.WindowPlus, Sequence = 3, Text = "Tabs"},
            new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"}
        };

        await base.OnInitializedAsync();
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    #endregion Properties
}
