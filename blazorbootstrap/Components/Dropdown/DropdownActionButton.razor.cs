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
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.Button)
            .AddClass(Color.ToButtonColorClass(), Color != ButtonColor.None)
            .AddClass(Size.ToButtonSizeClass(), Size != Size.None)
            .Build();

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
    /// Gets or sets the size of the <see cref="DropdownActionButton" />.
    /// </summary>
    [CascadingParameter(Name = "Size")]
    public Size Size { get; set; } = Size.None;

    /// <summary>
    /// If defined, indicates that its element can be focused and can participates in sequential keyboard navigation.
    /// </summary>
    [Parameter]
    public int? TabIndex { get; set; }

    #endregion
}
