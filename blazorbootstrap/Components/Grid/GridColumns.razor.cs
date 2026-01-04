namespace BlazorBootstrap;

public partial class GridColumns : BlazorBootstrapComponentBase
{
    /// <summary>
    /// Specifies the content to be rendered inside the grid columns component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("Specifies the content to be rendered inside the grid columns component.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
