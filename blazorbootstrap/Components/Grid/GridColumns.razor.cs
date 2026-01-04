namespace BlazorBootstrap;

public partial class GridColumns : BlazorBootstrapComponentBase
{
    /// <summary>
    /// Specifies the content to be rendered inside the grid columns component.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("1.0.0")]
    [Description("")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; } = default!;
}
