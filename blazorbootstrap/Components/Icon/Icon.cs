using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBootstrap;

/// <summary>
/// Blazor Bootstrap icon component will display an icon from any icon font. <br/>
/// Use the parameter <see cref="Name"/> to specify the Bootstrap icon name. The values from <see cref="IconName"/>
/// are derived from the <see href="https://icons.getbootstrap.com/">official Bootstrap icons set.</see> <br/>
/// Alternatively, one may set the <see cref="CustomIconName"/> parameter to specify custom icons of your own, like the ones from `fontawesome`. <br/>
/// </summary>
public sealed class Icon : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
      
    /// <summary>
    /// Gets or sets the icon color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconColor.None" />.
    /// </remarks>
    [Parameter] public IconColor Color { get; set; } = IconColor.None;

    /// <summary>
    /// Gets or sets the custom icon name.
    /// Specify custom icons of your own, like `fontawesome`. Example: `fas fa-alarm-clock`.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string? CustomIconName { get; set; }

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter] public IconName Name { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the icon size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconSize.None" />.
    /// </remarks>
    [Parameter] public IconSize Size { get; set; } = IconSize.None;

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
                case "class": Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Color), StringComparison.OrdinalIgnoreCase): Color = (IconColor)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(CustomIconName), StringComparison.OrdinalIgnoreCase): CustomIconName = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Name), StringComparison.OrdinalIgnoreCase): Name = (IconName)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Size), StringComparison.OrdinalIgnoreCase): Size = (IconSize)parameter.Value; break;
                
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var cssClasses = $"{Class} {BootstrapIconUtility.IconSizeClassMap[Size]} {EnumExtensions.IconColorClassMap[Color]}";
        if (String.IsNullOrEmpty(CustomIconName))
        {
            cssClasses += $" {BootstrapIconUtility.IconPrefix} {BootstrapIconUtility.Icon(Name)}"; 
        }
        else
        {
            cssClasses += $" {CustomIconName}";
        }
        builder.OpenElement(0, "i");
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", cssClasses);
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        builder.CloseElement();
    }
}
