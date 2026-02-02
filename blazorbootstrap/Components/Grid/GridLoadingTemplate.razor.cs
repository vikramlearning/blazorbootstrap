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
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Specifies the content to be rendered inside the component.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    internal RenderFragment Template =>
        template ??= builder =>
        {
            builder.AddContent(100, ChildContent);
        };

    [CascadingParameter(Name = "Parent")]
    public Grid<TItem> Parent { get; set; } = default!;
}
