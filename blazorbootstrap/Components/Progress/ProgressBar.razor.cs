namespace BlazorBootstrap;

/// <summary>
/// Represents a colored bar within a <see cref="Progress"/>. A <see cref="Progress"/> may contain multiple <see cref="ProgressBar"/> components.
/// </summary>
public partial class ProgressBar
{
    #region Methods

    /// <summary>
    /// Decrease the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    public void DecreaseWidth(double width)
    {
        if (width is < 0 or > 100)
            return;

        if (Width - width < 0)
            Width = 0;
        else
            Width -= width;
    }

    /// <summary>
    /// Increase the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    public void IncreaseWidth(double width)
    { 
        Width = Math.Clamp(Width + width, 0, 100);
    }

    /// <summary>
    /// Set the progress bar color.
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(ProgressColor color) => Color = color;

    /// <summary>
    /// Set the progress bar label.
    /// </summary>
    /// <param name="text"></param>
    public void SetLabel(string text) => Label = text;

    /// <summary>
    /// Set the progress bar width.
    /// </summary>
    /// <param name="width"></param>
    public void SetWidth(double width)
    { 
        Width = Math.Clamp(width, 0, 100);
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
                case var _ when String.Equals(parameter.Name, nameof(Color), StringComparison.OrdinalIgnoreCase): Color = (ProgressColor)parameter.Value; break;

                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                
                case var _ when String.Equals(parameter.Name, nameof(Label), StringComparison.OrdinalIgnoreCase): Label = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Type), StringComparison.OrdinalIgnoreCase): Type = (ProgressType)parameter.Value; break;
                case "style": Style = (string)parameter.Value; break;

                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /*
     * StateHasChanged() needed to be invoked in .NET 6 to re-render a component when a property got altered.
     * In .NET 7 and later, this is no longer necessary, and having a set/get body is considered bad practice.
     * Hence, the following code are 3 simple properties in .NET 7 and later,
     * but for .NET 6, they are implemented with a set/get body and a private variable for the sake of StateHasChanged().
     */


#if NET6_0
    private ProgressColor color = ProgressColor.None;

    /// <summary>
    /// Gets or sets the progress color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ProgressColor.None" />.
    /// </remarks>
    [Parameter]
    public ProgressColor Color
    {
        get => color;
        set
        {
            color = value;
            StateHasChanged();
        }
    }
#else
    /// <summary>
    /// Gets or sets the progress color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ProgressColor.None" />.
    /// </remarks>
    [Parameter] public ProgressColor Color { get; set; } 
#endif


    /// <summary>
    /// Gets or sets the progress bar label.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string Label { get; set; } = default!;


#if NET6_0
    private ProgressType type = ProgressType.Default;
    /// <summary>
    /// Gets or sets the progress bar type.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ProgressType.Default" />.
    /// </remarks>
    [Parameter]
    public ProgressType Type
    {
        get => type;
        set
        {
            type = value;
            StateHasChanged();
        }
    }
#else
    /// <summary>
    /// Gets or sets the progress bar type.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ProgressType.Default" />.
    /// </remarks>
    [Parameter] public ProgressType Type { get; set; } 
    
#endif

#if NET6_0
    private double width = 0;

    /// <summary>
    /// Get or sets the progress bar width.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [Parameter]
    public double Width
    {
        get => width;
        set
        {
            width = value;
            StateHasChanged();
        }
    }
#else
    /// <summary>
    /// Get or sets the progress bar width.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [Parameter] public double Width { get; set; }
    
#endif

    private string Style { get; set; } = "";
    #endregion
}
