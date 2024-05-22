namespace BlazorBootstrap;

public partial class Tooltip : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private TooltipColor color = default!;
    private bool isFirstRenderComplete = false;
    private DotNetObjectReference<Tooltip> objRef = default!;
    private string? title;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (IsRenderComplete)
                    await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.initialize", Element);

            isFirstRenderComplete = true;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        title = Title;

        color = Color;

        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (isFirstRenderComplete)
            if (title != Title || color != Color)
            {
                title = Title;
                color = Color;

                await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
                await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.update", Element);
            }
    }

    public async Task ShowAsync()
    {
        await JSRuntime.InvokeVoidAsync("window.blazorBootstrap.tooltip.show", Element);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tooltip color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="TooltipColor.None" />.
    /// </remarks>
    [Parameter]
    public TooltipColor Color { get; set; } = TooltipColor.None;

    private string colorClass => Color.ToTooltipColorClass()!;

    /// <summary>
    /// Gets or sets a value indicating whether to display the content as HTML instead of text.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool IsHtml { get; set; }

    private string placement => Placement.ToTooltipPlacementName();

    /// <summary>
    /// Gets or sets the tooltip placement.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="TooltipPlacement.Top" />.
    /// </remarks>
    [Parameter]
    public TooltipPlacement Placement { get; set; } = TooltipPlacement.Top;

    /// <summary>
    /// Displays informative text when users hover, focus, or tap an element.
    /// </summary>
    /// <remarks>
    /// Default value is null.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = default!;

    #endregion
}
