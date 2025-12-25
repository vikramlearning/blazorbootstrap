namespace BlazorBootstrap;

public partial class RibbonItem : BlazorBootstrapComponentBase
{
    #region Methods

    /// <summary>
    /// Triggers the OnRibbonItemClick event of the parent Ribbon component.
    /// </summary>
    private async Task OnRibbonItemClickAsync()
    {
        if (Parent is not null)
            await Parent.OnRibbonItemClick(new RibbonItemEventArgs(Name!));
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-ribbon-item", true),
            (BootstrapClass.Flex, true),
            (BootstrapClass.FlexColumn, true),
            (BootstrapClass.AlignItemsCenter, true),
            ("ms-1", IsFirstItem),
            ("me-1", IsLastItem),
            ("mx-1", !IsFirstItem && !IsLastItem),
            ("p-1", true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the custom icon name.")]
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the icon color.
    /// <para>
    /// Default value is <see cref="IconColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(IconColor.None)]
    [Description("Gets or sets the icon color.")]
    [Parameter]
    public IconColor IconColor { get; set; } = IconColor.None;

    /// <summary>
    /// Gets or sets the icon CSS class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the icon CSS class.")]
    [Parameter]
    public string? IconCssClass { get; set; }

    /// <summary>
    /// Gets or sets the icon name.
    /// <para>
    /// Default value is <see cref="IconName.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the icon name.")]
    [Parameter]
    public IconName IconName { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the icon size.
    /// <para>
    /// Default value is <see cref="IconSize.x3" />.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(IconSize.x3)]
    [Description("Gets or sets the icon size.")]
    [Parameter]
    public IconSize IconSize { get; set; } = IconSize.x3;

    /// <summary>
    /// Gets or sets the image hieght. The height of the image in pixels.
    /// <para>
    /// Default value is 28.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(28)]
    [Description("Gets or sets the image hieght. The height of the image in pixels.")]
    [Parameter]
    public double ImgHeight { get; set; } = 28;

    /// <summary>
    /// Gets or sets the image source.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the image source.")]
    [Parameter]
    public string? ImgSrc { get; set; }

    /// <summary>
    /// Gets or sets the image width. The width of the image in pixels.
    /// <para>
    /// Default value is 28.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(28)]
    [Description("Gets or sets the image width. The width of the image in pixels.")]
    [Parameter]
    public double ImgWidth { get; set; } = 28;

    /// <summary>
    /// Gets or sets the first item in the <see cref="RibbonItemGroup"/>.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the first item in the <b>RibbonItemGroup</b>.")]
    [Parameter]
    public bool IsFirstItem { get; set; }

    /// <summary>
    /// Gets or sets the last item in the <see cref="RibbonItemGroup"/>.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(false)]
    [Description("Gets or sets the last item in the <b>RibbonItemGroup</b>.")]
    [Parameter]
    public bool IsLastItem { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="RibbonItem"/> name.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the <b>RibbonItem</b> name.")]
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter(Name = "Ribbon2")]
    internal Ribbon Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the text to be displayed inside the RibbonItem.
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the text to be displayed inside the RibbonItem.")]
    [Parameter]
    public string? Text { get; set; }

    #endregion
}
