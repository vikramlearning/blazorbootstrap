using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBootstrap;

/// <summary>
/// This component represents the title of a <see cref="Card"/>. <br/>
/// If no title is required, it can be omitted from the card implementation.
/// </summary>
public sealed class CardTitle : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the card title size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="HeadingSize.H5" />.
    /// </remarks>
    [Parameter] public HeadingSize Size { get; set; } = HeadingSize.H5;

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
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ChildContent), StringComparison.OrdinalIgnoreCase): ChildContent = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Size), StringComparison.OrdinalIgnoreCase): Size = (HeadingSize)parameter.Value; break;
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
        builder.OpenElement(0, Enum.GetName(Size)!.ToLowerInvariant());
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", $"{BootstrapClass.CardTitle} {Class}");
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddElementReferenceCapture(4, (value) => Element = value);
        if (ChildContent != null)
        {
            builder.AddContent(5, ChildContent);
        }
        builder.CloseElement();
    }
}
