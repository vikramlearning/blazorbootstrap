namespace BlazorBootstrap;

public partial class Icon : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapIconUtility.Icon(), string.IsNullOrWhiteSpace(CustomIconName)),
            (BootstrapIconUtility.Icon(Name), string.IsNullOrWhiteSpace(CustomIconName)),
            (CustomIconName!, !string.IsNullOrWhiteSpace(CustomIconName)),
            (BootstrapIconUtility.IconSize(Size)!, Size != IconSize.None),
            (Color.ToIconColorClass(), Color != IconColor.None));

    /// <summary>
    /// Gets or sets the icon color.
    /// <para>
    /// Default value is <see cref="IconColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.9.0")]
    [DefaultValue(IconColor.None)]
    [Description("Gets or sets the icon color.")]
    [Parameter]
    public IconColor Color { get; set; } = IconColor.None;

    /// <summary>
    /// Gets or sets the custom icon name.
    /// Specify custom icons of your own, like `fontawesome`. Example: `fas fa-alarm-clock`.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the custom icon name. Specify custom icons of your own, like `fontawesome`. Example: `fas fa-alarm-clock`.")]
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the icon name.
    /// <para>
    /// Default value is <see cref="IconName.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the icon name.")]
    [Parameter]
    public IconName Name { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the icon size.
    /// <para>
    /// Default value is <see cref="IconSize.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(IconSize.None)]
    [Description("Gets or sets the icon size.")]
    [Parameter]
    public IconSize Size { get; set; } = IconSize.None;

    #endregion
}
