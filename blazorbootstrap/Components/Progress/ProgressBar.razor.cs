namespace BlazorBootstrap;

public partial class ProgressBar
{
    #region Fields and Constants

    private ProgressColor color = ProgressColor.None;
    private ProgressType type = ProgressType.Default;

    private double width = 0;

    #endregion

    #region Methods

    /// <summary>
    /// Decrease the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    [AddedVersion("1.0.0")]
    [Description("Decrease the progress bar width.")]
    public void DecreaseWidth(double width)
    {
        if (width is < 0 or > 100)
            return;

        if (Width - width < 0)
            Width = 0;
        else
            Width -= width;
    }

    /// <summary>
    /// Get the progress bar width.
    /// </summary>
    /// <returns>double</returns>
    [AddedVersion("1.0.0")]
    [Description("Get the progress bar width.")]
    public double GetWidth() => width;

    /// <summary>
    /// Increase the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    [AddedVersion("1.0.0")]
    [Description("Increase the progress bar width.")]
    public void IncreaseWidth(double width)
    {
        if (width is < 0 or > 100)
            return;

        if (Width + width > 100)
            Width = 100;
        else
            Width += width;
    }

    /// <summary>
    /// Set the progress bar color.
    /// </summary>
    /// <param name="color"></param>
    [AddedVersion("1.0.0")]
    [Description("Set the progress bar color.")]
    public void SetColor(ProgressColor color) => Color = color;

    /// <summary>
    /// Set the progress bar label.
    /// </summary>
    /// <param name="text"></param>
    [AddedVersion("1.0.0")]
    [Description("Set the progress bar label.")]
    public void SetLabel(string text) => Label = text;

    /// <summary>
    /// Set the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    [AddedVersion("1.0.0")]
    [Description("Set the progress bar width.")]
    public void SetWidth(double width)
    {
        if (width is < 0 or > 100)
            return;

        Width = width;
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ProgressBar, true),
            (BootstrapClass.ProgressBarStriped, type is ProgressType.Striped or ProgressType.StripedAndAnimated),
            (BootstrapClass.ProgressBarAnimated, type == ProgressType.StripedAndAnimated),
            (color.ToProgressColorClass(), color != ProgressColor.None));

    protected override string? StyleNames =>
        BuildStyleNames(Style,
            // FIX: Toast progressbar not showing: https://github.com/vikramlearning/blazorbootstrap/issues/155
            ($"width:{width.ToString(CultureInfo.InvariantCulture)}%", width is >= 0 and <= 100));

    /// <summary>
    /// Gets or sets the progress color.
    /// <para>
    /// Default value is <see cref="ProgressColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(ProgressColor.None)]
    [Description("Gets or sets the progress color.")]
    [Parameter]
    public ProgressColor Color
    {
        get => color;
        set
        {
            color = value;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Gets or sets the progress bar label.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the progress bar label.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the progress bar type.
    /// <para>
    /// Default value is <see cref="ProgressType.Default" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(ProgressType.Default)]
    [Description("Gets or sets the progress bar type.")]
    [Parameter]
    public ProgressType Type
    {
        get => type;
        set
        {
            type = value;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Get or sets the progress bar width.
    /// <para>
    /// Default value is 0.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(0)]
    [Description("Get or sets the progress bar width.")]
    [Parameter]
    public double Width
    {
        get => width;
        set
        {
            width = value;
            StateHasChanged();
        }
    }

    #endregion
}
