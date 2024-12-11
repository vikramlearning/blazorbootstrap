namespace BlazorBootstrap;

/// <summary>
/// Bootstrap's cards provide a flexible and extensible content container with multiple variants and options. <br/>
/// This component is based on the <see href="https://getbootstrap.com/docs/5.0/components/card/">Bootstrap Card</see> component.
/// </summary>
public partial class Card : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
      
    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; } 

    /// <summary>
    /// Gets or sets the card color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CardColor.None" />.
    /// </remarks>
    [Parameter]
    public CardColor Color { get; set; } = CardColor.None;

    /// <summary>
    /// Gets or sets the text alignment of the card.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.None" />.
    /// </remarks>
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.None;

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
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Color): Color = (CardColor)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(TextAlignment): TextAlignment = (Alignment)parameter.Value; break;
                
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion
}
