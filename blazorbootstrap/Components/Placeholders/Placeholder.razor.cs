namespace BlazorBootstrap;

public partial class Placeholder : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Placeholder, true),
            (Width.ToPlaceholderWidthClass(), true),
            (Color.ToPlaceholderColorClass(), Color != PlaceholderColor.None),
            (Size.ToPlaceholderSizeClass(), Size != PlaceholderSize.None));

    /// <summary>
    /// Gets or sets the placeholder color.
    /// <para>
    /// Default value is <see cref="PlaceholderColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(PlaceholderColor.None)]
    [Description("Gets or sets the placeholder color.")]
    [Parameter]
    public PlaceholderColor Color { get; set; } = PlaceholderColor.None;

    /// <summary>
    /// Gets or sets the placeholder size.
    /// <para>
    /// Default value is <see cref="PlaceholderSize.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(PlaceholderSize.None)]
    [Description("Gets or sets the placeholder size.")]
    [Parameter]
    public PlaceholderSize Size { get; set; } = PlaceholderSize.None;

    /// <summary>
    /// Gets or sets the placeholder width.
    /// <para>
    /// Default value is <see cref="PlaceholderWidth.Col1" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(PlaceholderWidth.Col1)]
    [Description("Gets or sets the placeholder width.")]
    [Parameter]
    public PlaceholderWidth Width { get; set; } = PlaceholderWidth.Col1;

    #endregion
}
