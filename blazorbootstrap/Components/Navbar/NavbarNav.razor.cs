namespace BlazorBootstrap;

/// <summary>
/// The navigation list of a <see cref="Navbar" />, holding <see cref="NavbarItem" /> and <see cref="NavbarDropdown" /> components. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/navbar/#nav" />
/// </summary>
[AddedVersion("4.1.0")]
public partial class NavbarNav : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the items of the navigation: <see cref="NavbarItem" /> and <see cref="NavbarDropdown" /> components.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the items of the navigation: <b>NavbarItem</b> and <b>NavbarDropdown</b> components.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.NavbarNav, true),
            (BootstrapClass.NavbarNavScroll, Scrollable));

    /// <summary>
    /// Gets or sets whether the collapsed navigation scrolls within <see cref="ScrollHeight" /> instead of growing to fit its items.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the collapsed navigation scrolls within <b>ScrollHeight</b> instead of growing to fit its items.")]
    [Parameter]
    public bool Scrollable { get; set; }

    /// <summary>
    /// Gets or sets the maximum height of the scrolling navigation when <see cref="Scrollable" /> is <see langword="true" />, as a CSS length.
    /// <para>
    /// Default value is "75vh".
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("75vh")]
    [Description("Gets or sets the maximum height of the scrolling navigation when <b>Scrollable</b> is <b>true</b>, as a CSS length.")]
    [Parameter]
    public string ScrollHeight { get; set; } = "75vh";

    protected override string? StyleNames =>
        BuildStyleNames(Style,
            ($"--bs-scroll-height: {ScrollHeight}", Scrollable && !string.IsNullOrWhiteSpace(ScrollHeight)));

    #endregion
}
