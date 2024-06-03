namespace BlazorBootstrap;

/// <summary>
/// This component represents the footer of a <see cref="Card"/>. <br/>
/// If no footer is required, it can be omitted from the card implementation.
/// </summary>
public partial class CardFooter : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.CardFooter)
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    #endregion
}
