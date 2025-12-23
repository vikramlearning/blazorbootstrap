namespace BlazorBootstrap;

public partial class DropdownMenu : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.DropdownMenu, true),
            (Position.ToDropdownMenuPositionClass(), true));

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
    [ParameterTypeName("RenderFragment?")]
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown menu position.
    /// <para>
    /// Default value is <see cref="DropdownMenuPosition.Start" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(DropdownMenuPosition.Start)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [Parameter]
    public DropdownMenuPosition Position { get; set; } = DropdownMenuPosition.Start;

    #endregion
}
