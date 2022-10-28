namespace BlazorBootstrap;

public partial class ProgressBar
{
    #region Members

    private ProgressColor color = ProgressColor.None;

    private string label;

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
        builder.Append(BootstrapClassProvider.ProgressBarStriped(), (type == ProgressType.Striped || type == ProgressType.StripedAndAnimated ));
        builder.Append(BootstrapClassProvider.ProgressBarAnimated(), type == ProgressType.StripedAndAnimated);
        builder.Append(BootstrapClassProvider.ProgressBackgroundColor(this.color), this.color != ProgressColor.None);
        base.BuildClasses(builder);
    }

    protected override void BuildStyles(StyleBuilder builder)
    {
        builder.Append($"width:{width}%", width >= 0 && width <= 100);
        base.BuildStyles(builder);
    }

    public double GetWidth() => this.width;

    public void SetWidth(double barPercentage)
    {
        if (width < 0 || width > 100)
            return;

        this.width = barPercentage;
        DirtyStyles();
        StateHasChanged();
    }

    public void IncreaseWidth(double width)
    {
        if (width < 0 || width > 100 || this.width + width > 100)
            return;

        this.width += width;
        DirtyStyles();
        StateHasChanged();
    }

    public void DecreaseProgressBar(double width)
    {
        if (width < 0 || width > 100 || this.width - width < 0)
            return;

        this.width -= width;
        DirtyStyles();
        StateHasChanged();
    }

    public void SetLabel(string text)
    {
        this.label = text;
    }

    public void SetColor(ProgressColor color)
    {
        this.color = color;
        DirtyClasses();
        StateHasChanged();
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public ProgressColor Color { get; set; }

    [Parameter] public string Label { get; set; }

    [Parameter] public ProgressType Type { get; set; }

    [Parameter] public double Width { get; set; }

    #endregion
}
