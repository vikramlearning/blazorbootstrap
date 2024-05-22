namespace BlazorBootstrap;

public partial class Icon : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapIconProvider.Icon(), string.IsNullOrWhiteSpace(CustomIconName))
            .AddClass(BootstrapIconProvider.Icon(Name), string.IsNullOrWhiteSpace(CustomIconName))
            .AddClass(CustomIconName!, !string.IsNullOrWhiteSpace(CustomIconName))
            .AddClass(BootstrapIconProvider.IconSize(Size)!, Size != IconSize.None)
            .AddClass(Color.ToIconColorClass(), Color != IconColor.None)
            .Build();

    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconColor.None" />.
    /// </remarks>
    [Parameter]
    public IconColor Color { get; set; } = IconColor.None;

    /// <summary>
    /// Gets or sets the custom icon name.
    /// Specify custom icons of your own, like `fontawesome`. Example: `fas fa-alarm-clock`.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName Name { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the icon size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconSize.None" />.
    /// </remarks>
    [Parameter]
    public IconSize Size { get; set; } = IconSize.None;

    #endregion
}
