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

    #endregion
}
