using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBootstrap;

/// <summary>
/// Renders its content in a later step of the same render pass than the component that declares it. <see cref="Table{TItem}" />
/// places it after its columns, so the table markup is built after every column has received its current parameters.
/// </summary>
internal sealed class TableDefer : ComponentBase
{
    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, ChildContent);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content to render.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    #endregion
}
