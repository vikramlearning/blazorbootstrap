namespace BlazorBootstrap;

public partial class PlaceholderContainer : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(Animation.ToPlaceholderAnimationClass())
            .Build();

    /// <summary>
    /// Gets or sets the placeholder animation. Default is <see cref="PlaceholderAnimation.Glow" />.
    /// </summary>
    [Parameter]
    public PlaceholderAnimation Animation { get; set; } = PlaceholderAnimation.Glow;

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
