namespace BlazorBootstrap;

public partial class ProgressBar
{
    #region Members

    private ProgressColor color = ProgressColor.None;

    private string label = default!;

    private ProgressType type = ProgressType.Default;

    private double width = 0;

    #endregion

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.ProgressBar());
        builder.Append(BootstrapClassProvider.ProgressBarStriped(), (type == ProgressType.Striped || type == ProgressType.StripedAndAnimated));
        builder.Append(BootstrapClassProvider.ProgressBarAnimated(), type == ProgressType.StripedAndAnimated);
        builder.Append(BootstrapClassProvider.ProgressBackgroundColor(this.color), this.color != ProgressColor.None);
        base.BuildClasses(builder);
    }

    protected override void BuildStyles(StyleBuilder builder)
    {
        // FIX: Toast progressbar not showing: https://github.com/vikramlearning/blazorbootstrap/issues/155
        builder.Append($"width:{width.ToString(CultureInfo.InvariantCulture)}%", width >= 0 && width <= 100);
        base.BuildStyles(builder);
    }

    /// <summary>
    /// Decrease the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    public void DecreaseWidth(double width)
    {
        if (width < 0 || width > 100)
            return;
        else if (this.Width - width < 0)
            this.Width = 0;
        else
            this.Width -= width;
    }

    /// <summary>
    /// Get the progress bar width.
    /// </summary>
    /// <returns></returns>
    public double GetWidth() => this.width;

    /// <summary>
    /// Increase the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    public void IncreaseWidth(double width)
    {
        if (width < 0 || width > 100)
            return;
        else if (this.Width + width > 100)
            this.Width = 100;
        else
            this.Width += width;
    }

    /// <summary>
    /// Set the progress bar color.
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(ProgressColor color)
    {
        this.Color = color;
    }

    /// <summary>
    /// Set the progress bar label.
    /// </summary>
    /// <param name="text"></param>
    public void SetLabel(string text)
    {
        this.Label = text;
    }

    /// <summary>
    /// Set the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    public void SetWidth(double width)
    {
        if (width < 0 || width > 100)
            return;

        this.Width = width;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the progress color.
    /// </summary>
    [Parameter]
    public ProgressColor Color
    {
        get => color;
        set
        {
            color = value;
            DirtyClasses();
            StateHasChanged();
        }
    }

    /// <summary>
    /// Gets or sets the progress bar label.
    /// </summary>
    [Parameter]
    public string Label
    {
        get => label;
        set => label = value;
    }

    /// <summary>
    /// Gets or sets the progress bar type.
    /// </summary>
    [Parameter]
    public ProgressType Type
    {
        get => type;
        set
        {
            type = value;
            DirtyClasses();
            StateHasChanged();
        }
    }

    /// <summary>
    /// Get or sets the progress bar width.
    /// </summary>
    [Parameter]
    public double Width
    {
        get => width;
        set
        {
            width = value;
            DirtyStyles();
            StateHasChanged();
        }
    }

    #endregion
}
