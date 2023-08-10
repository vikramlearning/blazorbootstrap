namespace BlazorBootstrap;

public partial class DropdownToggleButton
{
    #region Events

    #endregion

    #region Members

    private ButtonColor color = ButtonColor.None;

    private Size size = Size.None;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Button());
        builder.Append(BootstrapClassProvider.ButtonColor(Color), Color != ButtonColor.None);
        builder.Append(BootstrapClassProvider.ButtonSize(Size), Size != Size.None);
        builder.Append(BootstrapClassProvider.DropdownToggle());

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

        base.OnInitialized();
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="ChildContent"/>.
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
    /// Changes the size of a button.
    /// </summary>
    [Parameter]
    public Size Size
    {
        get => size;
        set
        {
            size = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// If defined, indicates that its element can be focused and can participates in sequential keyboard navigation.
    /// </summary>
    [Parameter] public int? TabIndex { get; set; }

    #endregion
}
