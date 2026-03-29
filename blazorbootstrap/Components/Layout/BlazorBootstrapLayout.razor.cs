namespace BlazorBootstrap;

public partial class BlazorBootstrapLayout : BlazorBootstrapLayoutComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames => BuildClassNames(Class, ("bb-page", true));

    /// <summary>
    /// Gets or sets the content section.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content section.")]
    [Parameter] 
    public RenderFragment? ContentSection { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for content section.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS class for content section.")]
    [Parameter] 
    public string? ContentSectionCssClass { get; set; }
    
    protected string? ContentSectionCssClassNames => BuildClassNames(ContentSectionCssClass, ("p-4", true));

    /// <summary>
    /// Gets or sets the footer section.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the footer section.")]
    [Parameter] 
    public RenderFragment? FooterSection { get; set; }

    /// <summary>
    /// Gets or sets the CSS class applied to the footer section of the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.2.0")]
    [DefaultValue("bg-body-tertiary")]
    [Description("Gets or sets the CSS class applied to the footer section of the component.")]
    [Parameter] 
    public string FooterSectionCssClass { get; set; } = "bg-body-tertiary";
    
    protected string? FooterSectionCssClassNames => BuildClassNames(FooterSectionCssClass, ("bb-footer p-4", true));

    /// <summary>
    /// Gets or sets the header section.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the header section.")]
    [Parameter] 
    public RenderFragment? HeaderSection { get; set; }

    /// <summary>
    /// Gets or sets the CSS class applied to the header section of the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.2.0")]
    [DefaultValue("d-flex justify-content-end")]
    [Description("Gets or sets the CSS class applied to the header section of the component.")]
    [Parameter] 
    public string HeaderSectionCssClass { get; set; } = "d-flex justify-content-end";

    protected string? HeaderSectionCssClassNames =>
        BuildClassNames(
            HeaderSectionCssClass,
            ("bb-top-row", true),
            ("bb-top-row-sticky", StickyHeader),
            ("px-4", true)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the header section is sticky.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.2.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the header section is sticky.")]
    [Parameter] 
    public bool StickyHeader { get; set; }

    /// <summary>
    /// Gets or sets the sidebar section.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the sidebar section.")]
    [Parameter] 
    public RenderFragment? SidebarSection { get; set; }

    #endregion
}
