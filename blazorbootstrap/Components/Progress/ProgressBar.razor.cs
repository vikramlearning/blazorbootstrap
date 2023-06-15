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

    protected override void OnInitialized()
    {
        this.color = Color;
        this.label = Label;
        this.type = Type;
        this.width = Width;
        base.OnInitialized();
    }

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

    protected override void OnParametersSet()
    {
        var stateChanged = false;

        if (this.color != Color)
        {
            this.color = Color;
            stateChanged = true;
        }

        if (this.type != Type)
        {
            this.type = Type;
            stateChanged = true;
        }

        if (this.width != Width)
        {
            if (Width < 0 || Width > 100)
                throw new ArgumentOutOfRangeException("Width");

            this.width = Width;
            stateChanged = true;
        }

        if (stateChanged)
        {
            DirtyClasses();
            DirtyStyles();
            StateHasChanged();
        }

        base.OnParametersSet();
    }

    /// <summary>
    /// Decrease the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    public void DecreaseWidth(double width)
    {
        if (width < 0 || width > 100 || this.width - width < 0)
            return;

        this.width -= width;
        DirtyStyles();
        StateHasChanged();
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
        if (width < 0 || width > 100 || this.width + width > 100)
            return;

        this.width += width;
        DirtyStyles();
        StateHasChanged();
    }

    /// <summary>
    /// Set the progress bar color.
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(ProgressColor color)
    {
        this.color = color;
        DirtyClasses();
        StateHasChanged();
    }

    /// <summary>
    /// Set the progress bar label.
    /// </summary>
    /// <param name="text"></param>
    public void SetLabel(string text)
    {
        this.label = text;
    }

    /// <summary>
    /// Set the progress bar width.
    /// </summary>
    /// <param name="barPercentage"></param>
    public void SetWidth(double barPercentage)
    {
        if (width < 0 || width > 100)
            return;

        this.width = barPercentage;
        DirtyStyles();
        StateHasChanged();
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the progress color.
    /// </summary>
    [Parameter] public ProgressColor Color { get; set; }

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
    [Parameter] public ProgressType Type { get; set; }

    /// <summary>
    /// Get or sets the progress bar width.
    /// </summary>
    [Parameter] public double Width { get; set; }

    #endregion
}
