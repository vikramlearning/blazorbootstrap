using System;

namespace BlazorBootstrap;

/// <summary>
/// Represents a pagination link within a <see cref="Pagination"/> component.
/// </summary>
public partial class PaginationLink : BlazorBootstrapComponentBase
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
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(LinkAriaLabel), StringComparison.OrdinalIgnoreCase): 
                    LinkAriaLabel = (string)parameter.Value;
                    if (!String.IsNullOrWhiteSpace(LinkAriaLabel))
                        AdditionalAttributes["aria-label"] = LinkAriaLabel; // TODO: this is not working revisit again
                    break;
                case var _ when String.Equals(parameter.Name, nameof(LinkIcon), StringComparison.OrdinalIgnoreCase): LinkIcon = (IconName)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(LinkText), StringComparison.OrdinalIgnoreCase): LinkText = (string)parameter.Value; break;

                case var _ when String.Equals(parameter.Name, nameof(Text), StringComparison.OrdinalIgnoreCase): Text = (string)parameter.Value; break;

                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers 

    /// <summary>
    /// Gets or sets the link aria-label attribute.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? LinkAriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the link icon.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName LinkIcon { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the link text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? LinkText { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? Text { get; set; }

    #endregion
}
