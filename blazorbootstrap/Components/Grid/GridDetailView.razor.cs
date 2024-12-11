namespace BlazorBootstrap;

public partial class GridDetailView<TItem> : BlazorBootstrapComponentBase
{
    private RenderFragment<TItem>? gridDetailViewTemplate;

    protected override async Task OnInitializedAsync()
    {
        Id = IdUtility.GetNextId(); // Required

        if (Parent is not null)
            Parent.SetGridDetailView(this);

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Specifies the content to be rendered inside the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter] [EditorRequired]
    public RenderFragment<TItem> ChildContent { get; set; } = default!;

    internal RenderFragment<TItem> GetTemplate =>
        gridDetailViewTemplate ??= rowData => builder =>
        {
            builder.AddContent(100, ChildContent, rowData);
        };

    [CascadingParameter(Name = "Parent")]
    public Grid<TItem> Parent { get; set; } = default!;
}
