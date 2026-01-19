namespace BlazorBootstrap;

public partial class CardTitle : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.CardTitle, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the card title size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="HeadingSize.H5" />.
    /// </remarks>
    [AddedVersion("1.10.0")]
    [DefaultValue(HeadingSize.H5)]
    [Description("Gets or sets the card title size.")]
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H5;

    #endregion
}
