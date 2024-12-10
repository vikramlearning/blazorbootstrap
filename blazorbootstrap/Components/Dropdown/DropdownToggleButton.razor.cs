namespace BlazorBootstrap;

public partial class DropdownToggleButton : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    public override Task SetParametersAsync(ParameterView parameters)
    {


        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(AutoClose): AutoClose = (bool)parameter.Value!; break;
                case nameof(AutoCloseBehavior): AutoCloseBehavior = (DropdownAutoCloseBehavior)parameter.Value!; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value!; break;
                case nameof(Class): Class = (string)parameter.Value!; break;
                case nameof(Color): Color = (DropdownColor)parameter.Value!; break;
                case nameof(Disabled): Disabled = (bool)parameter.Value!; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(Size): Size = (DropdownSize)parameter.Value!; break;
                case nameof(Split): Split = (bool)parameter.Value!; break;
                case nameof(TabIndex): TabIndex = (int?)parameter.Value!; break;
                default: AdditionalAttributes![parameter.Name] = parameter.Value!; break;
            }
        }

        if (!AdditionalAttributes!.TryGetValue("type", out _))
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

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// If <see langword="true" />, enables the auto close.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
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
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

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
    /// If true, the css class 'dropdown-toggle-split' will be added to the button.
    /// </summary>
    [CascadingParameter(Name = "Split")] 
    public bool Split { get; set; }

    /// <summary>
    /// Gets or sets the dropdown toggle button tab index.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public int? TabIndex { get; set; }

    #endregion
}
