namespace BlazorBootstrap;

public partial class BlazorBootstrapLayout : BlazorBootstrapLayoutComponentBase
{
    #region Properties, Indexers

    protected string? ClassNames => Class + " bb-page";

    [Parameter] public RenderFragment? ContentSection { get; set; }
    [Parameter] public string? ContentSectionCssClass { get; set; }
    protected string? ContentSectionCssClassNames => ContentSectionCssClass + " p-4";

    [Parameter] public RenderFragment? FooterSection { get; set; }
    [Parameter] public string? FooterSectionCssClass { get; set; } = "bg-body-tertiary";
    protected string? FooterSectionCssClassNames => FooterSectionCssClass + " bb-footer p-4";

    [Parameter] public RenderFragment? HeaderSection { get; set; }
    [Parameter] public string? HeaderSectionCssClass { get; set; } = "d-flex justify-content-end";

    protected string? HeaderSectionCssClassNames => HeaderSectionCssClass + " bb-top-row bb-top-row-sticky px-4"; 

    [Parameter] public bool StickyHeader { get; set; }

    [Parameter] public RenderFragment? SidebarSection { get; set; }

    #endregion
}
