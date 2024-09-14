namespace BlazorBootstrap;

public partial class Markdown : BlazorBootstrapComponentBase
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
