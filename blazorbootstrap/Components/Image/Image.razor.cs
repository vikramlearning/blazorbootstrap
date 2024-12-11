namespace BlazorBootstrap;

/// <summary>
/// Represents a visual image displayed on the web page.
/// </summary>
public partial class Image: BlazorBootstrapComponentBase
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
                case var _ when String.Equals(parameter.Name, nameof(Alt), StringComparison.OrdinalIgnoreCase): Alt = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IsResponsive), StringComparison.OrdinalIgnoreCase): IsResponsive = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IsThumbnail), StringComparison.OrdinalIgnoreCase): IsThumbnail = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Src), StringComparison.OrdinalIgnoreCase): Src = (string)parameter.Value; break;
                

                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion


    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the alternate text for the image.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>
    /// Gets or sets the source of the image.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public string? Src { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the image is responsive.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true"/>.
    /// </remarks>
    [Parameter]
    public bool IsResponsive { get; set; } = true;

    /// <summary>
    /// Makes the image have a rounded 1px border appearance if set to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
    /// </remarks>
    [Parameter]
    public  bool IsThumbnail { get; set; }

    #endregion
}
