
namespace BlazorBootstrap;

/// <summary>
/// The Blazor Bootstrap Badge component shows the small count and labels. <br/>
/// See <see href="https://getbootstrap.com/docs/5.0/components/badge/">Bootstrap Badge</see> for more information.
/// </summary>
public partial class Badge : BlazorBootstrapComponentBase
{
    #region Properties, Indexers 

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the badge color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="BadgeColor.Secondary" />.
    /// </remarks>
    [Parameter] public BadgeColor Color { get; set; } = BadgeColor.Secondary;

    /// <summary>
    /// Gets or sets the badge indicator.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="BadgeIndicatorType.None" />.
    /// </remarks>
    [Parameter] public BadgeIndicatorType IndicatorType { get; set; } = BadgeIndicatorType.None;

    /// <summary>
    /// Gets or sets the badge placement.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="BadgePlacement.None" />.
    /// </remarks>
    [Parameter] public BadgePlacement Placement { get; set; } = BadgePlacement.None;

    /// <summary>
    /// Gets or sets the badge position.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Position.None" />.
    /// </remarks>
    [Parameter] public Position Position { get; set; } = Position.None;

    /// <summary>
    /// Gets or sets the visually hidden text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string VisuallyHiddenText { get; set; } = default!;

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
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Color), StringComparison.OrdinalIgnoreCase): Color = (BadgeColor)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IndicatorType), StringComparison.OrdinalIgnoreCase): IndicatorType = (BadgeIndicatorType)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Placement), StringComparison.OrdinalIgnoreCase): Placement = (BadgePlacement)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Position), StringComparison.OrdinalIgnoreCase): Position = (Position)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(VisuallyHiddenText), StringComparison.OrdinalIgnoreCase): VisuallyHiddenText = (string)parameter.Value; break;
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion
}
