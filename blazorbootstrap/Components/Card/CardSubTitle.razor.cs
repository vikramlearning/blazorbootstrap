namespace BlazorBootstrap;

public partial class CardSubTitle : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.CardSubTitle)
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the heading size.
    /// </summary>
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H6;

    #endregion
}
