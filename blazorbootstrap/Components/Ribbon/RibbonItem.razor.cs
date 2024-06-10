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
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the custom icon name.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconColor.None" />.
    /// </remarks>
    [Parameter]
    public IconColor IconColor { get; set; } = IconColor.None;

    /// <summary>
    /// Gets or sets the icon CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? IconCssClass { get; set; }

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName IconName { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the icon size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="BlazorBootstrap.IconSize.x3" />.
    /// </remarks>
    [Parameter]
    public IconSize IconSize { get; set; } = IconSize.x3;

    /// <summary>
    /// Gets or sets the image hieght. The height of the image in pixels.
    /// </summary>
    /// <remarks>
    /// Default value is 28.
    /// </remarks>
    [Parameter]
    public double ImgHeight { get; set; } = 28;

    /// <summary>
    /// Gets or sets the image source.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? ImgSrc { get; set; }

    /// <summary>
    /// Gets or sets the image width. The width of the image in pixels.
    /// </summary>
    /// <remarks>
    /// Default value is 28.
    /// </remarks>
    [Parameter]
    public double ImgWidth { get; set; } = 28;

    /// <summary>
    /// Gets or sets the first item in the RibbonItemGroup.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool IsFirstItem { get; set; } = false;

    /// <summary>
    /// Gets or sets the last item in the RibbonItemGroup.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter]
    public bool IsLastItem { get; set; } = false;

    /// <summary>
    /// Gets or sets the RibbonItem name.
    /// </summary>
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
    [Parameter]
    public string? Text { get; set; }

    #endregion
}
