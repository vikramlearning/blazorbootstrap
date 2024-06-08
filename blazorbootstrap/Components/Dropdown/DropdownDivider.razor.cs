namespace BlazorBootstrap;

public partial class DropdownDivider : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class, (BootstrapClass.DropdownDivider, true));

    #endregion
}
