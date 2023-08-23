namespace BlazorBootstrap;

public partial class DropdownDivider
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(ClassProvider.DropdownDivider());

        base.BuildClasses(builder);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    #endregion
}