namespace BlazorBootstrap;

public partial class DropdownHeader : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.DropdownHeader, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
