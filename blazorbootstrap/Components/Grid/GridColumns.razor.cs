namespace BlazorBootstrap;

public partial class GridColumns : BlazorBootstrapComponentBase
{
    /// <summary>
    /// Specifies the content to be rendered inside the grid columns component.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [Parameter]
    public RenderFragment? ChildContent { get; set; } = default!;
}
