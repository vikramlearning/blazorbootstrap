namespace BlazorBootstrap;

/// <summary>
/// Action button within a <see cref="Dropdown"/>, which is used to make only <br/>
/// the part defined in this component open up the <see cref="Dropdown"/> rather than the whole <see cref="DropdownToggleButton"/> <br/>
/// For more information, see <seealso href="https://demos.blazorbootstrap.com/dropdown#split-button"/>
/// </summary>
public partial class DropdownActionButton : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (!AdditionalAttributes.TryGetValue("type", out _))
            AdditionalAttributes.Add("type", "button");

        base.OnInitialized();
    }

    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {

        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value!; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Color): Color = (DropdownColor)parameter.Value!; break;
                case nameof(Disabled): Disabled = (bool)parameter.Value!; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(Size): Size = (DropdownSize)parameter.Value!; break;

                case nameof(TabIndex): TabIndex = (int?)parameter.Value!; break;
                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
      
    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; } 

    /// <summary>
    /// Gets or sets the dropdown action button color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownColor.None" />.
    /// </remarks>
    [CascadingParameter(Name = "Color")] public DropdownColor Color { get; set; } = DropdownColor.None;

    /// <summary>
    /// Gets or sets the disabled.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [CascadingParameter(Name = "Disabled")] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the dropdown action button size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="DropdownSize.None" />.
    /// </remarks>
    [CascadingParameter(Name = "Size")] public DropdownSize Size { get; set; } = DropdownSize.None;

    /// <summary>
    /// Gets or sets the dropdown action button tab index.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public int? TabIndex { get; set; }

    #endregion
}
