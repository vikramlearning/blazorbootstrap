namespace BlazorBootstrap;

public partial class BlazorBootstrapLayout : BlazorBootstrapLayoutComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames => BuildClassNames(Class, ("bb-page", true));

    [Parameter] public RenderFragment? ContentSection { get; set; }
    [Parameter] public string? ContentSectionCssClass { get; set; }
    protected string? ContentSectionCssClassNames => BuildClassNames(ContentSectionCssClass, ("p-4", true));

    [Parameter] public RenderFragment? FooterSection { get; set; }
    [Parameter] public string? FooterSectionCssClass { get; set; } = "bg-body-tertiary";
    protected string? FooterSectionCssClassNames => BuildClassNames(FooterSectionCssClass, ("bb-footer p-4", true));

    [Parameter] public RenderFragment? HeaderSection { get; set; }
    [Parameter] public string? HeaderSectionCssClass { get; set; } = "d-flex justify-content-end";

    protected string? HeaderSectionCssClassNames =>
        BuildClassNames(
            HeaderSectionCssClass,
            ("bb-top-row", true),
            ("bb-top-row-sticky", StickyHeader),
            ("px-4", true)
        );

    [Parameter] public bool StickyHeader { get; set; }

    [Parameter] public RenderFragment? SidebarSection { get; set; }

    #endregion
}
