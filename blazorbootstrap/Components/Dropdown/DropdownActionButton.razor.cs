namespace BlazorBootstrap;

public partial class DropdownActionButton : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        AdditionalAttributes ??= new Dictionary<string, object>();

        if (!AdditionalAttributes.TryGetValue("type", out _))
            AdditionalAttributes.Add("type", "button");

        base.OnInitialized();
    }

    #endregion

    #region Properties, Indexers
    
    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Button, true),
            (Color.ToDropdownButtonColorClass(), Color != DropdownColor.None),
            (Size.ToDropdownButtonSizeClass(), Size != DropdownSize.None));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the dropdown action button color.
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
    /// Default value is <see langword="false" />.
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

    /// <summary>
    /// Gets or sets the dropdown action button tab index.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public int? TabIndex { get; set; }

    #endregion
}
