namespace BlazorBootstrap;

public partial class GridTemplates : BlazorBootstrapComponentBase
{
    /// <summary>
    /// Specifies the content to be rendered inside the grid templates component.
    /// </summary>
    /// <para>
    /// Default value is null.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
