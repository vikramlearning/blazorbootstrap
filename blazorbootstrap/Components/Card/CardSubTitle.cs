using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBootstrap;

/// <summary>
/// This component represents the subtitle of a <see cref="Card"/>. <br/>
/// If no subtitle is required, it can be omitted from the card implementation.
/// </summary>
public sealed class CardSubTitle : BlazorBootstrapComponentBase
{
    #region Properties, Indexers 

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the card subtitle size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="HeadingSize.H6" />.
    /// </remarks>
    [Parameter]
    public HeadingSize Size { get; set; } = HeadingSize.H6;

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
                case nameof(Class): Class = (string)parameter.Value; break;
                case nameof(ChildContent): ChildContent = (RenderFragment)parameter.Value; break;
                case nameof(Id): Id = (string)parameter.Value; break;
                case nameof(Size): Size = (HeadingSize)parameter.Value; break;
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
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
        builder.AddAttribute(2, "class", $"{BootstrapClass.CardSubTitle} {Class}");
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddElementReferenceCapture(4, (value) => Element = value);
        if (ChildContent != null)
        { 
            builder.AddContent(5, ChildContent); 
        } 
        builder.CloseElement();
    }
}
