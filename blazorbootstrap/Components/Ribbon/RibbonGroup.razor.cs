namespace BlazorBootstrap;

public partial class RibbonGroup : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass("bb-ribbon-group");
        this.AddClass(BootstrapClassProvider.Flex);
        this.AddClass(BootstrapClassProvider.FlexRow);
        this.AddClass(BootstrapClassProvider.Border);

        base.BuildClasses();
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the content to be rendered inside this component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
