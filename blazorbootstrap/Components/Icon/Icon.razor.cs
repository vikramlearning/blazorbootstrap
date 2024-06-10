namespace BlazorBootstrap;

/// <summary>
/// Blazor Bootstrap icon component will display an icon from any icon font. <br/>
/// Use the parameter <see cref="Name"/> to specify the Bootstrap icon name. The values from <see cref="IconName"/>
/// are derived from the <see href="https://icons.getbootstrap.com/">official Bootstrap icons set.</see> <br/>
/// Alternatively, one may set the <see cref="CustomIconName"/> parameter to specify custom icons of your own, like the ones from `fontawesome`. <br/>
/// </summary>
public partial class Icon : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapIconUtility.Icon(), string.IsNullOrWhiteSpace(CustomIconName)),
            (BootstrapIconUtility.Icon(Name), string.IsNullOrWhiteSpace(CustomIconName)),
            (CustomIconName!, !string.IsNullOrWhiteSpace(CustomIconName)),
            (BootstrapIconUtility.IconSize(Size)!, Size != IconSize.None),
            (Color.ToIconColorClass(), Color != IconColor.None));

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
    /// Default value is <see langword="null" />.
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
