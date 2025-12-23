namespace BlazorBootstrap;

public partial class DropdownActionButton : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void OnInitialized()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (!AdditionalAttributes.TryGetValue("type", out _))
            AdditionalAttributes.Add("type", "button");

        base.OnInitialized();
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Button, true),
            (Color.ToDropdownButtonColorClass(), Color != DropdownColor.None),
            (Size.ToDropdownButtonSizeClass(), Size != DropdownSize.None));

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
    /// Gets or sets the dropdown action button color.
    /// <para>
    /// Default value is <see cref="DropdownColor.None" />.
    /// </para>
    /// </summary>
    [CascadingParameter(Name = "Color")]
    public DropdownColor Color { get; set; } = DropdownColor.None;

    /// <summary>
    /// Gets or sets the disabled.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [CascadingParameter(Name = "Disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the dropdown action button size.
    /// <para>
    /// Default value is <see cref="DropdownSize.None" />.
    /// </para>
    /// </summary>
    [CascadingParameter(Name = "Size")]
    public DropdownSize Size { get; set; } = DropdownSize.None;

    /// <summary>
    /// Gets or sets the dropdown action button tab index.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the dropdown action button tab index.")]
    [ParameterTypeName("int?")]
    [Parameter]
    public int? TabIndex { get; set; }

    #endregion
}
