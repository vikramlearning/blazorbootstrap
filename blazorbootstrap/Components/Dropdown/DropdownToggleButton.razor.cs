namespace BlazorBootstrap;

public partial class DropdownToggleButton : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.Button)
        .AddClass(Color.ToButtonColorClass(), Color != ButtonColor.None)
        .AddClass(Size.ToButtonSizeClass(), Size != Size.None)
        .AddClass(BootstrapClass.DropdownToggle)
        .AddClass(BootstrapClass.DropdownToggleSplit, Split)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

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

    /// <summary>
    /// Enables or disables the auto close.
    /// </summary>
    [CascadingParameter(Name = "AutoClose")]
    public bool AutoClose { get; set; }

    /// <summary>
    /// Gets or sets the auto close behavior of the dropdown.
    /// </summary>
    [CascadingParameter(Name = "AutoCloseBehavior")]
    public DropdownAutoCloseBehavior AutoCloseBehavior { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the button color.
    /// </summary>
    [Parameter]
    public ButtonColor Color { get; set; } = ButtonColor.None;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [CascadingParameter(Name = "Disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the size of the <see cref="DropdownToggleButton" />.
    /// </summary>
    [CascadingParameter(Name = "Size")]
    public Size Size { get; set; } = Size.None;

    [CascadingParameter(Name = "Split")]
    public bool Split { get; set; }

    /// <summary>
    /// If defined, indicates that its element can be focused and can participates in sequential keyboard navigation.
    /// </summary>
    [Parameter]
    public int? TabIndex { get; set; }

    #endregion
}
