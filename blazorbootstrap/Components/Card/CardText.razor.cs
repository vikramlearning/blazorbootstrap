namespace BlazorBootstrap;

/// <summary>
/// This component represents the text of a <see cref="Card"/>. <br/>
/// If no text is required, it can be omitted from the card implementation.
/// </summary>
public partial class CardText : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.CardText, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    #endregion
}
