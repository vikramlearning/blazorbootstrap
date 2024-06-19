namespace BlazorBootstrap;

public partial class GridDetailView<TItem> : BlazorBootstrapComponentBase
{
    private RenderFragment<TItem>? gridDetailViewTemplate;

    protected override async Task OnInitializedAsync()
    {
        Id = IdUtility.GetNextId(); // Required

        Parent.SetGridDetailView(this);

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Specifies the content to be rendered inside the grid column.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment<TItem> ChildContent { get; set; } = default!;

    internal RenderFragment<TItem> GridDetailViewTemplate =>
        gridDetailViewTemplate ??= rowData => builder =>
        {
            builder.AddContent(100, ChildContent, rowData);
        };

    [CascadingParameter] public Grid<TItem> Parent { get; set; } = default!;
}
