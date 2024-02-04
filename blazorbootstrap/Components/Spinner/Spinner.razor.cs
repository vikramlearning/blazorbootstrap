namespace BlazorBootstrap;

public partial class Spinner : BlazorBootstrapComponentBase
{
    [Parameter] public int Width { get; set; } = 32;

    [Parameter] public int Height { get; set; } = 16;

    [Parameter]
    public SpinnerColor Color { get; set; }

    [Parameter] public string Title { get; set; } = "Loading...";

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.SpinnerDots());
        builder.Append(BootstrapClassProvider.Spinner(Color));
    }

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;
}
