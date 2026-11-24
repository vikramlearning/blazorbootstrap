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

    #region Internal Methods

    /// <summary>
    /// Builds the table markup again from the columns' current parameters. Only this component renders, not the table's
    /// <c>ChildContent</c>, so the components that hold the columns get no new parameters and the render cannot come back as
    /// another column change.
    /// </summary>
    internal void Render() => StateHasChanged();

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
