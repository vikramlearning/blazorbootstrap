namespace BlazorBootstrap;

public partial class DropdownActionButton : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void OnParametersSet()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (!AdditionalAttributes.TryGetValue("type", out _))
            AdditionalAttributes.Add("type", "button");

        base.OnParametersSet();
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Button, true),
            (EffectiveColor.ToDropdownButtonColorClass(), EffectiveColor != DropdownColor.None),
            (Size.ToDropdownButtonSizeClass(), Size != DropdownSize.None));

    /// <summary>
    /// Gets or sets the color cascaded from the parent <see cref="Dropdown" />.
    /// </summary>
    [CascadingParameter(Name = "Color")]
    private DropdownColor CascadedColor { get; set; } = DropdownColor.None;

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
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown action button color.
    /// When set to a value other than <see cref="DropdownColor.None" />, it overrides the color of the parent <see cref="Dropdown" />.
    /// <para>
    /// Default value is <see cref="DropdownColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(DropdownColor.None)]
    [Description("Gets or sets the dropdown action button color. When set to a value other than <b>None</b>, it overrides the color of the parent <b>Dropdown</b>.")]
    [Parameter]
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
    /// Gets the color to render: <see cref="Color" /> when it is not <see cref="DropdownColor.None" />; otherwise, the color cascaded from the parent <see cref="Dropdown" />.
    /// </summary>
    private DropdownColor EffectiveColor => Color != DropdownColor.None ? Color : CascadedColor;

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
    [Parameter]
    public int? TabIndex { get; set; }

    #endregion
}
