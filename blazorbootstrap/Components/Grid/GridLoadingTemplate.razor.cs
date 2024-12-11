namespace BlazorBootstrap;

public partial class GridLoadingTemplate<TItem> : BlazorBootstrapComponentBase
{
    private RenderFragment? template;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        Id = IdUtility.GetNextId(); // Required

        Parent?.SetGridLoadingTemplate(this);

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Specifies the content to be rendered inside the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    internal RenderFragment Template =>
        template ??= builder =>
        {
            builder.AddContent(100, ChildContent);
        };

    [CascadingParameter(Name = "Parent")]
    public Grid<TItem> Parent { get; set; } = default!;
}
