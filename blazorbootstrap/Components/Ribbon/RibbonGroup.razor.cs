namespace BlazorBootstrap;

public partial class RibbonGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass("bb-ribbon-group")
            .AddClass(BootstrapClass.Flex)
            .AddClass(BootstrapClass.FlexRow)
            .AddClass(BootstrapClass.Border)
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered inside this component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
