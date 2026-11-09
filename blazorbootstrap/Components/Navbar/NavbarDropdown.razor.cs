namespace BlazorBootstrap;

/// <summary>
/// A dropdown menu in a <see cref="NavbarNav" />, holding <see cref="DropdownItem" />, <see cref="DropdownHeader" />, and <see cref="DropdownDivider" /> components. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/navbar/#nav" />
/// </summary>
[AddedVersion("4.1.0")]
public partial class NavbarDropdown : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing && IsRenderComplete)
            await SafeInvokeVoidAsync("window.blazorBootstrap.dropdown.dispose", Id);

        await base.DisposeAsyncCore(disposing);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the menu items: <see cref="DropdownItem" />, <see cref="DropdownHeader" />, and <see cref="DropdownDivider" /> components.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the menu items: <b>DropdownItem</b>, <b>DropdownHeader</b>, and <b>DropdownDivider</b> components.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.NavItem, true),
            (BootstrapClass.Dropdown, true));

    /// <summary>
    /// Gets or sets whether the menu is aligned to the end of the toggle instead of the start, when the navbar is expanded.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the menu is aligned to the end of the toggle instead of the start, when the navbar is expanded.")]
    [Parameter]
    public bool MenuEnd { get; set; }

    private string menuClassNames =>
        BuildClassNames(
            (BootstrapClass.DropdownMenu, true),
            ("dropdown-menu-end", MenuEnd));

    /// <summary>
    /// Gets or sets the text of the dropdown toggle.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the text of the dropdown toggle.")]
    [EditorRequired]
    [Parameter]
    public string? Text { get; set; }

    #endregion
}
