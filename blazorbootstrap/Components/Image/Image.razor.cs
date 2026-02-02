namespace BlazorBootstrap;

public partial class Image: BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ImageFluid, IsResponsive),
            (BootstrapClass.ImageThumbnail, IsThumbnail));

    /// <summary>
    /// Gets or sets the alternate text for the image.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the alternate text for the image.")]
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>
    /// Gets or sets the source of the image.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the source of the image.")]
    [Parameter]
    public string? Src { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the image is responsive.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(true)]
    [Description("Gets or sets a value indicating whether the image is responsive.")]
    [Parameter]
    public bool IsResponsive { get; set; } = true;

    /// <summary>
    /// Makes the image have a rounded 1px border appearance if set to <see langword="true"/>.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(false)]
    [Description("Makes the image have a rounded 1px border appearance if set to <b>true</b>.")]
    [Parameter]
    public  bool IsThumbnail { get; set; }

    #endregion
}
