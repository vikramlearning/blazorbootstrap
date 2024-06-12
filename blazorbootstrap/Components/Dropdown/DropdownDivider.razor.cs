namespace BlazorBootstrap;

/// <summary>
/// Renders a divider within a <see cref="Dropdown"/> component, to separate different sections.
/// </summary> 
public partial class DropdownDivider : BlazorBootstrapComponentBase
{
    #region Properties, Indexers
    
    /// <inheritdoc />
    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.DropdownDivider, true));

    /// <inheritdoc />
    public override Task SetParametersAsync(ParameterView parameters)
    {

        foreach (var parameter in parameters)
        {
            AdditionalAttributes![parameter.Name] = parameter.Value!;
        }
        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion
}
