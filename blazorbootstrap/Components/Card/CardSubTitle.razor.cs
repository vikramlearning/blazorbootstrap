namespace BlazorBootstrap;

public partial class CardSubTitle : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.CardSubTitle, true));

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
    /// Gets or sets the card sub title size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="HeadingSize.H6" />.
    /// </remarks>
    [AddedVersion("1.10.0")]
    [DefaultValue(HeadingSize.H6)]
    [Description("Gets or sets the card sub title size.")]
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H6;

    #endregion
}
