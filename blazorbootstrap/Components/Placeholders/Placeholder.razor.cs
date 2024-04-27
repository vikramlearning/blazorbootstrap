namespace BlazorBootstrap;

public partial class Placeholder : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.Placeholder)
        .AddClass(Width.ToPlaceholderWidthClass())
        .AddClass(Color.ToPlaceholderColorClass(), Color != PlaceholderColor.None)
        .AddClass(Size.ToPlaceholderSizeClass(), Size != PlaceholderSize.None)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the placeholder color. Default is <see cref="PlaceholderColor.None" />.
    /// </summary>
    [Parameter]
    public PlaceholderColor Color { get; set; } = PlaceholderColor.None;

    /// <summary>
    /// Gets or sets the placeholder size. Default is <see cref="PlaceholderSize.None" />.
    /// </summary>
    [Parameter]
    public PlaceholderSize Size { get; set; } = PlaceholderSize.None;

    /// <summary>
    /// Gets or sets the placeholder width. Default is <see cref="PlaceholderWidth.Col1" />.
    /// </summary>
    [Parameter]
    public PlaceholderWidth Width { get; set; } = PlaceholderWidth.Col1;

    #endregion
}
