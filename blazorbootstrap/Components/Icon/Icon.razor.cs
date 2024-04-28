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
    [Parameter]
    public IconColor Color { get; set; } = IconColor.None;

    /// <summary>
    /// Icon name that can be either a string or <see cref="IconName" />.
    /// </summary>
    [Parameter]
    public string? CustomIconName { get; set; }

    /// <summary>
    /// Icon name that can be either a string or <see cref="IconName" />.
    /// </summary>
    [Parameter]
    public IconName Name { get; set; } = IconName.None;

    /// <summary>
    /// Defines the icon size.
    /// </summary>
    [Parameter]
    public IconSize Size { get; set; } = IconSize.None;

    #endregion
}
