namespace BlazorBootstrap;

public partial class Progress
{
    #region Members

    private double height = 0;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        height = Height;
        base.OnInitialized();
    }

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Progress());
        base.BuildClasses(builder);
    }

    protected override void BuildStyles(StyleBuilder builder)
    {
        builder.Append($"height:{height}px", height >= 0);
        base.BuildStyles(builder);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="Progress"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the height of the Progress. Height is measured in pixels, and the default height is 16 pixels.
    /// </summary>
    [Parameter] public double Height { get; set; } = 16;

    #endregion
}
