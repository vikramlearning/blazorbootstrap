namespace BlazorBootstrap;

public partial class DropdownToggleButton : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void OnParametersSet()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (!AdditionalAttributes.TryGetValue("type", out _))
            AdditionalAttributes.Add("type", "button");

        if (!AdditionalAttributes.TryGetValue("data-bs-toggle", out _))
            AdditionalAttributes.Add("data-bs-toggle", "dropdown");

        if (!AdditionalAttributes.TryGetValue("aria-expanded", out _))
            AdditionalAttributes.Add("aria-expanded", "false");

        string? autoClose;

        if (AutoClose && AutoCloseBehavior == DropdownAutoCloseBehavior.Inside)
            autoClose = "inside";
        else if (AutoClose && AutoCloseBehavior == DropdownAutoCloseBehavior.Outside)
            autoClose = "outside";
        else if (AutoClose && AutoCloseBehavior == DropdownAutoCloseBehavior.Both)
            autoClose = "true";
        else
            autoClose = "false";

        if (!AdditionalAttributes.TryGetValue("data-bs-auto-close", out _))
            AdditionalAttributes.Add("data-bs-auto-close", autoClose);
        else
            AdditionalAttributes["data-bs-auto-close"] = autoClose;

        base.OnParametersSet();
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Button, true),
            (EffectiveColor.ToDropdownButtonColorClass(), EffectiveColor != DropdownColor.None),
            (Size.ToDropdownButtonSizeClass(), Size != DropdownSize.None),
            (BootstrapClass.DropdownToggle, true),
            (BootstrapClass.DropdownToggleSplit, Split));

    /// <summary>
    /// If <see langword="true" />, enables the auto close.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [CascadingParameter(Name = "AutoClose")]
    public bool AutoClose { get; set; }

    /// <summary>
    /// Gets or sets the auto close behavior of the dropdown.
    /// <para>
    /// Default value is <see cref="DropdownAutoCloseBehavior.Inside" />.
    /// </para>
    /// </summary>
    [CascadingParameter(Name = "AutoCloseBehavior")]
    public DropdownAutoCloseBehavior AutoCloseBehavior { get; set; } = DropdownAutoCloseBehavior.Inside;

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
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown toggle button color.
    /// When set to a value other than <see cref="DropdownColor.None" />, it overrides the color of the parent <see cref="Dropdown" />.
    /// <para>
    /// Default value is <see cref="DropdownColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(DropdownColor.None)]
    [Description("Gets or sets the dropdown toggle button color. When set to a value other than <b>None</b>, it overrides the color of the parent <b>Dropdown</b>.")]
    [Parameter]
    public DropdownColor Color { get; set; } = DropdownColor.None;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    [CascadingParameter(Name = "Disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets the color to render: <see cref="Color" /> when it is not <see cref="DropdownColor.None" />; otherwise, the color cascaded from the parent <see cref="Dropdown" />.
    /// </summary>
    private DropdownColor EffectiveColor => Color != DropdownColor.None ? Color : CascadedColor;

    /// <summary>
    /// Gets or sets the dropdown action button size.
    /// </summary>
    /// <para>
    /// Default value is <see cref="DropdownSize.None" />.
    /// </para>
    [CascadingParameter(Name = "Size")]
    public DropdownSize Size { get; set; } = DropdownSize.None;

    [CascadingParameter(Name = "Split")] public bool Split { get; set; }

    /// <summary>
    /// Gets or sets the dropdown toggle button tab index.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("1.10.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the dropdown toggle button tab index.")]
    [Parameter]
    public int? TabIndex { get; set; }

    #endregion
}
