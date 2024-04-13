namespace BlazorBootstrap;

public partial class RibbonItemGroup : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass("bb-ribbon-item-group");
        this.AddClass(BootstrapClassProvider.Flex);
        this.AddClass(BootstrapClassProvider.FlexColumn);
        this.AddClass(BootstrapClassProvider.AlignItemsCenter);
        this.AddClass("my-1");
        this.AddClass(BootstrapClassProvider.BorderEnd);

        base.BuildClasses();
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            // TODO: update
        }

        await base.DisposeAsync(disposing);
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
    public string? Text { get; set; }

    #endregion
}
