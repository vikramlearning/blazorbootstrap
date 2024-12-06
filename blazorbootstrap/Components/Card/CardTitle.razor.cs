namespace BlazorBootstrap;

/// <summary>
/// This component represents the title of a <see cref="Card"/>. <br/>
/// If no title is required, it can be omitted from the card implementation.
/// </summary>
public partial class CardTitle : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.CardTitle, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the card title size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="HeadingSize.H5" />.
    /// </remarks>
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H5;

    #endregion

    #region Methods
    
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
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
                case nameof(Size): Size = (HeadingSize)parameter.Value; break;
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }
    #endregion
}
