using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Tabs : BaseComponent
{
    #region Members

    private List<Tab> tabs = new List<Tab>();

    #endregion

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Nav);
        builder.Append(BootstrapClassProvider.NavTabs);

        base.BuildClasses(builder);
    }

    internal void AddTab(Tab tab)
    {
        tabs.Add(tab);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; }

    #endregion
}
