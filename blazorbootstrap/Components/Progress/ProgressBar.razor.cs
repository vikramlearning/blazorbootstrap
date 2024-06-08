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
    /// <returns></returns>
    public double GetWidth() => width;

    /// <summary>
    /// Increase the progress bar width.
    /// </summary>
    /// <param name="width"></param>
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
    public void SetColor(ProgressColor color) => Color = color;

    /// <summary>
    /// Set the progress bar label.
    /// </summary>
    /// <param name="text"></param>
    public void SetLabel(string text) => Label = text;

    /// <summary>
    /// Set the progress bar width.
    /// </summary>
    /// <param name="width"></param>
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
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ProgressColor.None" />.
    /// </remarks>
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
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string Label { get; set; } = default!;

    /// <summary>
    /// Gets or sets the progress bar type.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ProgressType.Default" />.
    /// </remarks>
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
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
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
