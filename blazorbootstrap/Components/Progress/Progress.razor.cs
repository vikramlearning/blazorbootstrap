namespace BlazorBootstrap;

public partial class Progress : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private double height = 0;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.Progress);

        base.BuildClasses();
    }

    protected override void BuildStyles()
    {
        this.AddClass($"height:{height}px", height >= 0);

        base.BuildStyles();
    }

    protected override void OnInitialized()
    {
        height = Height;

        base.OnInitialized();
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Progress" />.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the height of the Progress. Height is measured in pixels, and the default height is 16 pixels.
    /// </summary>
    [Parameter]
    public double Height { get; set; } = 16;

    #endregion
}
