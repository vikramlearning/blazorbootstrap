namespace BlazorBootstrap;

public partial class RibbonItem : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    #endregion

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

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            // TOOD: update
        }

        await base.DisposeAsync(disposing);
    }

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
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? CustomIconName { get; set; }

    [Parameter]
    public IconColor IconColor { get; set; }

    [Parameter]
    public string? IconCssClass { get; set; }

    [Parameter]
    public IconName IconName { get; set; }

    [Parameter]
    public IconSize IconSize { get; set; } = IconSize.x3;

    [Parameter]
    public string? ImgSrc { get; set; }

    /// <summary>
    /// Width in pixels only.
    /// </summary>
    [Parameter]
    public double ImgWidth { get; set; } = 28;

    /// <summary>
    /// Height in pixels only.
    /// </summary>
    [Parameter]
    public double ImgHeight { get; set; } = 28;

    [Parameter]
    public bool IsFirstItem { get; set; } = false;

    [Parameter]
    public bool IsLastItem { get; set; } = false;

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    [CascadingParameter(Name = "Ribbon2")]
    internal Ribbon Parent { get; set; } = default!;

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Text { get; set; }

    #endregion
}
