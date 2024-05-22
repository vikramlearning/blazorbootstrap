namespace BlazorBootstrap;

public partial class PlaceholderContainer : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(Animation.ToPlaceholderAnimationClass())
            .Build();

    /// <summary>
    /// Gets or sets the placeholder animation.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderAnimation.Glow" />.
    /// </remarks>
    [Parameter]
    public PlaceholderAnimation Animation { get; set; } = PlaceholderAnimation.Glow;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
