namespace BlazorBootstrap;

public partial class PlaceholderContainer : BaseComponent
{
    #region Fields and Constants

    private PlaceholderAnimation animation = PlaceholderAnimation.Glow;

    #endregion

    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(ClassProvider.PlaceholderAnimation(Animation));

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the placeholder animation. Default is <see cref="PlaceholderAnimation.Glow" />.
    /// </summary>
    [Parameter]
    public PlaceholderAnimation Animation
    {
        get => animation;
        set
        {
            animation = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}