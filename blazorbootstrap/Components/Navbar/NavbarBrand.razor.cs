namespace BlazorBootstrap;

/// <summary>
/// The brand of a <see cref="Navbar" />: a company, product, or project name, an image, or both. <br/>
/// Bootstrap docs: <see href="https://getbootstrap.com/docs/5.3/components/navbar/#brand" />
/// </summary>
[AddedVersion("4.1.0")]
public partial class NavbarBrand : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the brand text, shown after the image when both are set.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the brand text, shown after the image when both are set.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.NavbarBrand, true),
            ("mb-0 h1", Href is null));

    /// <summary>
    /// Gets or sets the link of the brand. When <see langword="null" />, the brand renders as plain text.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the link of the brand. When <b>null</b>, the brand renders as plain text.")]
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the alternative text of the brand image. Set it to the brand name when the image is shown without text.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the alternative text of the brand image. Set it to the brand name when the image is shown without text.")]
    [Parameter]
    public string? ImageAlt { get; set; }

    /// <summary>
    /// Gets or sets the height of the brand image, in pixels.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the height of the brand image, in pixels.")]
    [Parameter]
    public int? ImageHeight { get; set; }

    /// <summary>
    /// Gets or sets the source of the brand image.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the source of the brand image.")]
    [Parameter]
    public string? ImageSrc { get; set; }

    /// <summary>
    /// Gets or sets the width of the brand image, in pixels.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the width of the brand image, in pixels.")]
    [Parameter]
    public int? ImageWidth { get; set; }

    #endregion
}
