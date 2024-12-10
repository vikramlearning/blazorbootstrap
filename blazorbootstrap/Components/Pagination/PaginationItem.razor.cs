namespace BlazorBootstrap;

/// <summary>
/// Represents a pagination item within a <see cref="Pagination"/> component.
/// </summary>
public partial class PaginationItem : BlazorBootstrapComponentBase
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
                case nameof(Active): 
                    Active = (bool)parameter.Value;
                    if (Active)
                        AdditionalAttributes["aria-current"] = "page";
                    break;
                case nameof(AriaLabel): AriaLabel = (string)parameter.Value; break;
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Disabled): Disabled = (bool)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(LinkIcon): LinkIcon = (IconName)parameter.Value; break;
                case nameof(LinkText): LinkText = (string)parameter.Value; break;

                case nameof(Text): Text = (string)parameter.Value; break;

                default: AdditionalAttributes![parameter.Name] = parameter.Value; break;
            }
        }
         
        
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the pagination item active state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the pagination item aria-label attribute.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the pagination item disable state.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the link icon.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter] public IconName LinkIcon { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the link text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? LinkText { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? Text { get; set; }

    #endregion
}
