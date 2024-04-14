namespace BlazorBootstrap;

public partial class RibbonItem : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass("bb-ribbon-item");
        this.AddClass(BootstrapClassProvider.Flex);
        this.AddClass(BootstrapClassProvider.FlexColumn);
        this.AddClass(BootstrapClassProvider.AlignItemsCenter);
        this.AddClass("ms-1", IsFirstItem);
        this.AddClass("me-1", IsLastItem);
        this.AddClass("mx-1", !IsFirstItem && !IsLastItem);
        this.AddClass("p-1");

        base.BuildClasses();
    }

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

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside the RibbonItem.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The name of a custom icon to be displayed.
    /// </summary>
    [Parameter] public string? CustomIconName { get; set; }

    /// <summary>
    /// The color of the icon.
    /// </summary>
    [Parameter] public IconColor IconColor { get; set; }

    /// <summary>
    /// CSS class(es) to be applied to the icon element.
    /// </summary>
    [Parameter] public string? IconCssClass { get; set; }

    /// <summary>
    /// The built-in icon to be displayed.
    /// </summary>
    [Parameter] public IconName IconName { get; set; }

    /// <summary>
    /// The size of the icon. Defaults to x3 (extra large).
    /// </summary>
    [Parameter] public IconSize IconSize { get; set; } = IconSize.x3;

    /// <summary>
    /// The height of the image in pixels.
    /// </summary>
    [Parameter]
    public double ImgHeight { get; set; } = 28;

    /// <summary>
    /// The source URL of the image.
    /// </summary>
    [Parameter] public string? ImgSrc { get; set; }

    /// <summary>
    /// The width of the image in pixels.
    /// </summary>
    [Parameter]
    public double ImgWidth { get; set; } = 28;

    /// <summary>
    /// True if this is the first item in the RibbonItemGroup.
    /// </summary>
    [Parameter] public bool IsFirstItem { get; set; } = false;

    /// <summary>
    /// True if this is the last item in the RibbonItemGroup.
    /// </summary>
    [Parameter] public bool IsLastItem { get; set; } = false;

    /// <summary>
    /// The name associated with the RibbonItem.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter(Name = "Ribbon2")]
    internal Ribbon Parent { get; set; } = default!;

    /// <summary>
    /// The text content to be displayed inside the RibbonItem.
    /// </summary>
    [Parameter] public string? Text { get; set; }

    #endregion
}
