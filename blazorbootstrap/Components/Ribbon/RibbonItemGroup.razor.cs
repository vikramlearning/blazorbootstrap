namespace BlazorBootstrap;

public partial class RibbonItemGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-ribbon-item-group", true),
            (BootstrapClass.Flex, true),
            (BootstrapClass.FlexColumn, true),
            (BootstrapClass.AlignItemsCenter, true),
            ("my-1", true),
            (BootstrapClass.BorderEnd, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Text { get; set; }

    #endregion
}
