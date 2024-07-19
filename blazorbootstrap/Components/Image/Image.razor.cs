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
                case nameof(Alt): Alt = (string)parameter.Value!; break;
                case nameof(Class): Class = (string)parameter.Value!; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(IsResponsive): IsResponsive = (bool)parameter.Value!; break;
                case nameof(IsThumbnail): IsThumbnail = (bool)parameter.Value!; break;
                case nameof(Src): Src = (string)parameter.Value!; break;
                case nameof(Style): Style = (string)parameter.Value!; break;

                default: AdditionalAttributes[parameter.Name] = parameter.Value; break;
            }
        }
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion


    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.ImageFluid, IsResponsive),
            (BootstrapClass.ImageThumbnail, IsThumbnail));

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
