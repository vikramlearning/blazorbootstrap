﻿using System.Xml.Linq;

namespace BlazorBootstrap;

/// <summary>
/// Represents a small set of <see cref="RibbonItem"/>s within a <see cref="RibbonTab"/> component.
/// </summary>
public partial class RibbonItemGroup : BlazorBootstrapComponentBase
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
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                
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
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? Text { get; set; }

    #endregion
}
