namespace BlazorBootstrap;

public partial class GridTemplates : BlazorBootstrapComponentBase
{
    /// <summary>
    /// Specifies the content to be rendered inside the grid templates component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Specifies the content to be rendered inside the grid templates component.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
