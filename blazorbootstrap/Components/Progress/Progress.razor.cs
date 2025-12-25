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
        BuildClassNames(Class, (BootstrapClass.Progress, true));

    protected override string? StyleNames =>
        BuildStyleNames(Style, ($"height:{height.ToString(CultureInfo.InvariantCulture)}px", height >= 0));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [ParameterTypeName("RenderFragment?")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the height of the Progress. Height is measured in pixels.
    /// <para>
    /// Default value is 16.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(16)]
    [Description("Gets or sets the height of the Progress. Height is measured in pixels.")]
    [Parameter]
    public double Height { get; set; } = 16;

    #endregion
}
