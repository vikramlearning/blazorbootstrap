namespace BlazorBootstrap;

public partial class RibbonGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-ribbon-group", true),
            (BootstrapClass.Flex, true),
            (BootstrapClass.FlexRow, true),
            (BootstrapClass.Border, true));

    /// <summary>
    /// Gets or sets the content to be rendered inside this component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
