namespace BlazorBootstrap;

public partial class Spinner : BlazorBootstrapComponentBase
{
    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.SpinnerDots());
        builder.Append(BootstrapClassProvider.Spinner(Color));
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    [Parameter] public SpinnerColor Color { get; set; }

    [Parameter] public int Height { get; set; } = 16;

    [Parameter] public string Title { get; set; } = "Loading...";

    [Parameter] public int Width { get; set; } = 32;

    #endregion
}
