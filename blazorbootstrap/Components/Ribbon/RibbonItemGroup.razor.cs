namespace BlazorBootstrap;

public partial class RibbonItemGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass("bb-ribbon-item-group")
            .AddClass(BootstrapClass.Flex)
            .AddClass(BootstrapClass.FlexColumn)
            .AddClass(BootstrapClass.AlignItemsCenter)
            .AddClass("my-1")
            .AddClass(BootstrapClass.BorderEnd)
            .Build();

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
