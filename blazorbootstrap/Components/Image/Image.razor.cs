namespace BlazorBootstrap;

public partial class Image: BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ImageFluid, IsResponsive),
            (BootstrapClass.ImageThumbnail, IsThumbnail));

    /// <summary>
    /// Gets or sets the source of the image.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? Src { get; set; }

    /// <summary>
    /// Gets or sets the alternate text for the image.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the image is responsive.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    [Parameter]
    public bool IsResponsive { get; set; } = true;

    /// <summary>
    /// Makes the image have a rounded 1px border appearance if set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
    /// </remarks>
    [Parameter]
    public  bool IsThumbnail { get; set; }

    #endregion
}
