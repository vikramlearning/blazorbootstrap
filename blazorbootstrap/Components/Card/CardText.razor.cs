namespace BlazorBootstrap;

/// <summary>
/// This component represents the text of a <see cref="Card"/>. <br/>
/// If no text is required, it can be omitted from the card implementation.
/// </summary>
public partial class CardText : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.CardText, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    #endregion


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
                case nameof(ChildContent):
                    ChildContent = (RenderFragment)parameter.Value;
                    break; 
                default:
                    AdditionalAttributes![parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    } 
}
