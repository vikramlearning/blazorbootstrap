namespace BlazorBootstrap;

public partial class RibbonItemGroup : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            ("bb-ribbon-item-group", true),
            (BootstrapClass.Flex, true),
            (BootstrapClass.FlexColumn, true),
            (BootstrapClass.AlignItemsCenter, true),
            ("my-1", true),
            (BootstrapClass.BorderEnd, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.2.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the text.")]
    [Parameter]
    public string? Text { get; set; }

    #endregion
}
