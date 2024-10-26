namespace BlazorBootstrap;

public partial class BlazorBootstrapLayout : BlazorBootstrapLayoutComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames => BuildClassNames(Class, ("bb-page", true));

    [Parameter] public RenderFragment? ContentSection { get; set; }
    [Parameter] public string? ContentSectionCssClass { get; set; }
    protected string? ContentSectionCssClassNames => BuildClassNames(ContentSectionCssClass, ("px-4 py-2", true));

    [Parameter] public RenderFragment? FooterSection { get; set; }
    [Parameter] public string? FooterSectionCssClass { get; set; }
    protected string? FooterSectionCssClassNames => BuildClassNames(ContentSectionCssClass, ("bb-footer p-4 mt-4", true));


    [Parameter] public RenderFragment? HeaderSection { get; set; }

    [Parameter] public string? HeaderSectionCssClass { get; set; }

    protected string? HeaderSectionCssClassNames => BuildClassNames(HeaderSectionCssClass, ("bb-top-row px-4", true));

    [Parameter] public RenderFragment? SidebarSection { get; set; }

    #endregion
}
