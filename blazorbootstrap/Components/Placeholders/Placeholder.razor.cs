using static System.Net.Mime.MediaTypeNames;

namespace BlazorBootstrap;

/// <summary>
/// Use Blazor Bootstrap loading placeholders for your components or pages to indicate something may still be loading.
/// </summary>
public partial class Placeholder : BlazorBootstrapComponentBase
{
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
                case nameof(Class): Class = (string)parameter.Value!; break;
                case nameof(Color): Color = (PlaceholderColor)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(Size): Size = (PlaceholderSize)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value!; break;
                case nameof(Width): Width = (PlaceholderWidth)parameter.Value; break;

                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }
    
    #endregion


    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Placeholder, true),
            (Width.ToPlaceholderWidthClass(), true),
            (Color.ToPlaceholderColorClass(), Color != PlaceholderColor.None),
            (Size.ToPlaceholderSizeClass(), Size != PlaceholderSize.None));

    /// <summary>
    /// Gets or sets the placeholder color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderColor.None" />.
    /// </remarks>
    [Parameter]
    public PlaceholderColor Color { get; set; } = PlaceholderColor.None;

    /// <summary>
    /// Gets or sets the placeholder size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderSize.None" />.
    /// </remarks>
    [Parameter]
    public PlaceholderSize Size { get; set; } = PlaceholderSize.None;

    /// <summary>
    /// Gets or sets the placeholder width.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PlaceholderWidth.Col1" />.
    /// </remarks>
    [Parameter]
    public PlaceholderWidth Width { get; set; } = PlaceholderWidth.Col1;

    #endregion
}
