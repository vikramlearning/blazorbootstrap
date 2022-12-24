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
            new NavItem{ Href = "/breadcrumb", PrefixIconName = IconName.SegmentedNav, Sequence = 1, Text = "Breadcrumb"},
            new NavItem{ Href = "/sidebar", PrefixIconName = IconName.LayoutSidebarInset, Sequence = 2, Text = "Sidebar"},
            new NavItem{ Href = "/tabs", PrefixIconName = IconName.WindowPlus, Sequence = 3, Text = "Tabs"},
            new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"},
            //new NavItem{ Href = "/toasts", PrefixIconName = IconName.ExclamationTriangleFill, Sequence = 4, Text = "Toasts"}
        };

        await base.OnInitializedAsync();
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public bool NavigationExpanded { get; set; }

    #endregion Properties
}
