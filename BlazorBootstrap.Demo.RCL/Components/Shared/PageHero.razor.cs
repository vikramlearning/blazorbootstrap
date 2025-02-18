namespace BlazorBootstrap.Demo.RCL;

public partial class PageHero : BlazorBootstrapComponentBase
{
    [Parameter]
    public string? Heading { get; set; }

    [Parameter]
    public RenderFragment? LeadSection { get; set; }
}
