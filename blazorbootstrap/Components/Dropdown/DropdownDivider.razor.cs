namespace BlazorBootstrap;

public partial class DropdownDivider : BlazorBootstrapComponentBase
{
    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.DropdownDivider)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style).Build();

    #endregion
}
