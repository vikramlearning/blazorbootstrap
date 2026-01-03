namespace BlazorBootstrap;

public partial class GridLoadingTemplate<TItem> : BlazorBootstrapComponentBase
{
    private RenderFragment? template;

    protected override async Task OnInitializedAsync()
    {
        Id = IdUtility.GetNextId(); // Required

        if (Parent is not null)
            Parent.SetGridLoadingTemplate(this);

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Specifies the content to be rendered inside the component.
    /// </summary>
    /// <para>
    /// Default value is null.
    /// </para>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    internal RenderFragment Template =>
        template ??= builder =>
        {
            builder.AddContent(100, ChildContent);
        };

    [CascadingParameter(Name = "Parent")]
    public Grid<TItem> Parent { get; set; } = default!;
}
