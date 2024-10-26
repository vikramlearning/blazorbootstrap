namespace BlazorBootstrap;

public partial class BlazorBootstrapLayout : LayoutComponentBase
{
    [Parameter]
    public RenderFragment? SidebarSection { get; set; }

    [Parameter]
    public RenderFragment? HeaderSection { get; set; }

    [Parameter]
    public RenderFragment? ContentSection { get; set; }

    [Parameter]
    public RenderFragment? FooterSection { get; set; }
}
