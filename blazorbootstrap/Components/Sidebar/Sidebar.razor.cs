﻿namespace BlazorBootstrap;

public partial class Sidebar : BaseComponent
{
    #region Events

    #endregion Events

    #region Members

    private bool collapseSidebar = false;

    private bool collapseNavMenu = true;

    private string? navMenuCssClass => GetNavMenuCssClass();

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append("bb-sidebar");
        builder.Append("collapsed", collapseSidebar);
        builder.Append("expanded", !collapseSidebar);

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        Attributes ??= new Dictionary<string, object>();

        await base.OnInitializedAsync();
    }

    private string GetNavMenuCssClass()
    {
        StringBuilder sb = new StringBuilder();

        if (collapseNavMenu)
            sb.Append(" collapse");

        sb.Append(" nav-scrollable custom-scrollbar");

        if (collapseSidebar)
            sb.Append(" custom-scrollbar-hidden");

        return sb.ToString();
    }

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

    /// <summary>
    /// Toggle sidebar.
    /// </summary>
    public void ToggleSidebar()
    {
        collapseSidebar = !collapseSidebar;
        DirtyClasses();
        StateHasChanged();
    }

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public string CustomIconName { get; set; }

    [Parameter] public IconName IconName { get; set; }

    [Parameter] public string ImageSrc { get; set; }

    [Parameter, EditorRequired] public IReadOnlyList<NavItem>? NavItems { get; set; }

    [Parameter, EditorRequired] public string Title { get; set; }

    #endregion Properties
}
