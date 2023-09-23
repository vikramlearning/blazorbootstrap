namespace BlazorBootstrap;

public partial class DropdownToggleButton
{
    #region Fields and Constants

    private ButtonColor color = ButtonColor.None;

    private bool isSplitButton;

    private Size size = Size.None;
    private bool split;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Button());
        builder.Append(BootstrapClassProvider.ButtonColor(Color), Color != ButtonColor.None);
        builder.Append(BootstrapClassProvider.ButtonSize(Size), Size != Size.None);
        builder.Append(BootstrapClassProvider.DropdownToggle());
        builder.Append(BootstrapClassProvider.DropdownToggleSplit(), Split);

        base.BuildClasses(builder);
    }

    protected override void OnInitialized()
    {
        Attributes ??= new Dictionary<string, object>();

        if (!Attributes.TryGetValue("type", out _))
            Attributes.Add("type", "button");

        if (!Attributes.TryGetValue("data-bs-toggle", out _))
            Attributes.Add("data-bs-toggle", "dropdown");

        if (!Attributes.TryGetValue("aria-expanded", out _))
            Attributes.Add("aria-expanded", "false");

        string? autoClose;

        if (AutoClose && AutoCloseBehavior == DropdownAutoCloseBehavior.Inside)
            autoClose = "inside";
        else if (AutoClose && AutoCloseBehavior == DropdownAutoCloseBehavior.Outside)
            autoClose = "outside";
        else if (AutoClose && AutoCloseBehavior == DropdownAutoCloseBehavior.Both)
            autoClose = "true";
        else
            autoClose = "false";

        if (!Attributes.TryGetValue("data-bs-auto-close", out _))
            Attributes.Add("data-bs-auto-close", autoClose);
        else
            Attributes["data-bs-auto-close"] = autoClose;

        base.OnInitialized();
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

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
    /// Specifies the content to be rendered inside this <see cref="ChildContent" />.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the button color.
    /// </summary>
    [Parameter]
    public ButtonColor Color
    {
        get => color;
        set
        {
            color = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    [CascadingParameter(Name = "Disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the size of the <see cref="DropdownToggleButton" />.
    /// </summary>
    [CascadingParameter(Name = "Size")]
    public Size Size
    {
        get => size;
        set
        {
            size = value;
            DirtyClasses();
        }
    }

    [CascadingParameter(Name = "Split")]
    public bool Split
    {
        get => split;
        set
        {
            split = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// If defined, indicates that its element can be focused and can participates in sequential keyboard navigation.
    /// </summary>
    [Parameter]
    public int? TabIndex { get; set; }

    #endregion
}
