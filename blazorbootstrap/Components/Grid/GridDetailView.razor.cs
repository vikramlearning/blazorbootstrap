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
    /// <para>
    /// Default value is null.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("")]
    [Parameter]
    public RenderFragment<TItem>? ChildContent { get; set; }

    internal RenderFragment<TItem> GetTemplate =>
        gridDetailViewTemplate ??= rowData => builder =>
        {
            builder.AddContent(100, ChildContent, rowData);
        };

    [CascadingParameter(Name = "Parent")]
    public Grid<TItem> Parent { get; set; } = default!;
}
