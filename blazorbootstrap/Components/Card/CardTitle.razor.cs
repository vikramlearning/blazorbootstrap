namespace BlazorBootstrap;

public partial class CardTitle : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.CardTitle)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the heading size.
    /// </summary>
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H5;

    #endregion
}
