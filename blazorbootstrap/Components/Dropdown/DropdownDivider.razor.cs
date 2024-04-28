namespace BlazorBootstrap;

public partial class DropdownDivider : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    protected override string? ClassNames =>
        new CssClassBuilder(Class)
            .AddClass(BootstrapClass.DropdownDivider)
            .Build();

    #endregion
}
