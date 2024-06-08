namespace BlazorBootstrap;

public partial class CardTitle : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.CardTitle, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the card title size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="HeadingSize.H5" />.
    /// </remarks>
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H5;

    #endregion
}
