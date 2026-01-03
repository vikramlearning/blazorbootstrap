namespace BlazorBootstrap;

public partial class GridTemplates : BlazorBootstrapComponentBase
{
    /// <summary>
    /// Specifies the content to be rendered inside the grid templates component.
    /// </summary>
    /// <para>
    /// Default value is null.
    /// </para>
    [Parameter]
    public RenderFragment? ChildContent { get; set; } = default!;
}
