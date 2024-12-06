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
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value!; break;
                case nameof(LinkAriaLabel): 
                    LinkAriaLabel = (string)parameter.Value;
                    if (!String.IsNullOrWhiteSpace(LinkAriaLabel))
                        AdditionalAttributes["aria-label"] = LinkAriaLabel; // TODO: this is not working revisit again
                    break;
                case nameof(LinkIcon): LinkIcon = (IconName)parameter.Value; break;
                case nameof(LinkText): LinkText = (string)parameter.Value; break;
                case nameof(Style): Style = (string)parameter.Value; break;
                case nameof(Text): Text = (string)parameter.Value; break;

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
        BuildClassNames(Class, (BootstrapClass.PaginationLink, true));

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
