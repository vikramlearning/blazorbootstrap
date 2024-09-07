namespace BlazorBootstrap;

public partial class CarouselCaption : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(
            Class,
            (BootstrapClass.CarouselCaption, true),
            (BootstrapClass.DisplayBlock, true),
            (BootstrapClass.DisplayMediumBlock, true)
        );

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
