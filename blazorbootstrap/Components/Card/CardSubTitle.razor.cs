namespace BlazorBootstrap;

/// <summary>
/// This component represents the subtitle of a <see cref="Card"/>. <br/>
/// If no subtitle is required, it can be omitted from the card implementation.
/// </summary>
public partial class CardSubTitle : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.CardSubTitle, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the card sub title size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="HeadingSize.H6" />.
    /// </remarks>
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H6;

    #endregion
}
