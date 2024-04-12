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
        this.AddClass("mt-1 p-1");

        base.BuildClasses();
    }

    protected override void BuildStyles()
    {
        //this.AddStyle("display:block", showBackdrop);

        base.BuildStyles();
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
        }

        await base.DisposeAsync(disposing);
    }

    protected override void OnInitialized()
    {
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
    public IconColor IconColor {  get; set; }

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
    public double ImgWidth { get; set; } = 32;

    /// <summary>
    /// Height in pixels only.
    /// </summary>
    [Parameter]
    public double ImgHeight { get; set; } = 32;

    [Parameter]
    public bool IsFirstItem { get; set; } = false;

    [Parameter]
    public bool IsLastItem { get; set; } = false;

    [Parameter]
    public string? Text {  get; set; }

    #endregion
}
