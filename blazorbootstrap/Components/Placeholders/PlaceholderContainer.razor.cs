namespace BlazorBootstrap;

public partial class PlaceholderContainer : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class, (Animation.ToPlaceholderAnimationClass(), true));

    /// <summary>
    /// Gets or sets the placeholder animation.
    /// <para>
    /// Default value is <see cref="PlaceholderAnimation.Glow" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(PlaceholderAnimation.Glow)]
    [Description("Gets or sets the placeholder animation.")]
    [Parameter]
    public PlaceholderAnimation Animation { get; set; } = PlaceholderAnimation.Glow;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [EditorRequired]
    [ParameterTypeName("RenderFragment?")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
