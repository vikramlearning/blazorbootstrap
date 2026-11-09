namespace BlazorBootstrap;

/// <summary>
/// A run of text in a <see cref="Navbar" />, vertically centered and colored for the navbar. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/navbar/#text" />
/// </summary>
[AddedVersion("4.1.0")]
public partial class NavbarText : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the text.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the text.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.NavbarText, true));

    #endregion
}
