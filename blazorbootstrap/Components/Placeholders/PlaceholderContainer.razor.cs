namespace BlazorBootstrap;

public partial class PlaceholderContainer : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private PlaceholderAnimation animation = PlaceholderAnimation.Glow;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.PlaceholderAnimation(Animation));

        base.BuildClasses();
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
