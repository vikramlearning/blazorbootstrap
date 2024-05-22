namespace BlazorBootstrap;

public partial class Placeholder : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.Placeholder)
            .AddClass(Width.ToPlaceholderWidthClass())
            .AddClass(Color.ToPlaceholderColorClass(), Color != PlaceholderColor.None)
            .AddClass(Size.ToPlaceholderSizeClass(), Size != PlaceholderSize.None)
            .Build();

    /// <summary>
    /// Gets or sets the placeholder color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderColor.None" />.
    /// </remarks>
    [Parameter]
    public PlaceholderColor Color { get; set; } = PlaceholderColor.None;

    /// <summary>
    /// Gets or sets the placeholder size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderSize.None" />.
    /// </remarks>
    [Parameter]
    public PlaceholderSize Size { get; set; } = PlaceholderSize.None;

    /// <summary>
    /// Gets or sets the placeholder width.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderWidth.Col1" />.
    /// </remarks>
    [Parameter]
    public PlaceholderWidth Width { get; set; } = PlaceholderWidth.Col1;

    #endregion
}
