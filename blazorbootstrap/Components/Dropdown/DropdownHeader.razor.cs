namespace BlazorBootstrap;

public partial class DropdownHeader : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.DropdownHeader, true));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    #endregion

    /// <inheritdoc />
    public override Task SetParametersAsync(ParameterView parameters)
    {

        foreach (var parameter in parameters)
        {
            if (parameter.Name == nameof(ChildContent))
            {
                               ChildContent = (RenderFragment)parameter.Value!;
            }
            else
            {
                AdditionalAttributes![parameter.Name] = parameter.Value!;
            }
        }
        return base.SetParametersAsync(ParameterView.Empty);
    }
}
