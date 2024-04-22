namespace BlazorBootstrap;

public partial class RibbonItemGroup : BlazorBootstrapComponentBase
{
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

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the text content of the component.
    /// </summary>
    [Parameter] public string? Text { get; set; }

    #endregion
}
