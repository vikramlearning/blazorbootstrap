namespace BlazorBootstrap;

public partial class Progress : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private double height = 0;

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        height = Height;

        base.OnInitialized();
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.Progress)
            .Build();

    protected override string? StyleNames =>
        new CssStyleBuilder(Style)
            .AddStyle($"height:{height.ToString(CultureInfo.InvariantCulture)}px", height >= 0)
            .Build();

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the height of the Progress. Height is measured in pixels.
    /// </summary>
    /// <remarks>
    /// Default value is 16.
    /// </remarks>
    [Parameter]
    public double Height { get; set; } = 16;

    #endregion
}
