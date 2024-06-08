namespace BlazorBootstrap;

public partial class DropdownToggleButton : BlazorBootstrapComponentBase
{
    #region Methods

    protected override void OnInitialized()
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

        base.OnInitialized();
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Button, true),
            (Color.ToDropdownButtonColorClass(), Color != DropdownColor.None),
            (Size.ToDropdownButtonSizeClass(), Size != DropdownSize.None),
            (BootstrapClass.DropdownToggle, true),
            (BootstrapClass.DropdownToggleSplit, Split));

    /// <summary>
    /// If <see langword="true" />, enables the auto close.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [CascadingParameter(Name = "AutoClose")]
    public bool AutoClose { get; set; }

    /// <summary>
    /// Gets or sets the auto close behavior of the dropdown.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownAutoCloseBehavior.Inside" />.
    /// </remarks>
    [CascadingParameter(Name = "AutoCloseBehavior")]
    public DropdownAutoCloseBehavior AutoCloseBehavior { get; set; } = DropdownAutoCloseBehavior.Inside;

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown toggle button color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownColor.None" />.
    /// </remarks>
    [CascadingParameter(Name = "Color")]
    public DropdownColor Color { get; set; } = DropdownColor.None;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [CascadingParameter(Name = "Disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the dropdown action button size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownSize.None" />.
    /// </remarks>
    [CascadingParameter(Name = "Size")]
    public DropdownSize Size { get; set; } = DropdownSize.None;

    [CascadingParameter(Name = "Split")] public bool Split { get; set; }

    /// <summary>
    /// Gets or sets the dropdown toggle button tab index.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public int? TabIndex { get; set; }

    #endregion
}
