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
                    await SafeInvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
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
            await SafeInvokeVoidAsync("window.blazorBootstrap.tooltip.initialize", Element);

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

                await SafeInvokeVoidAsync("window.blazorBootstrap.tooltip.dispose", Element);
                await SafeInvokeVoidAsync("window.blazorBootstrap.tooltip.update", Element);
            }
    }

    public async Task ShowAsync()
    {
        await SafeInvokeVoidAsync("window.blazorBootstrap.tooltip.show", Element);
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the tooltip color.
    /// <para>
    /// Default value is <see cref="TooltipColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.0")]
    [DefaultValue(TooltipColor.None)]
    [Description("Gets or sets the tooltip color.")]
    [Parameter]
    public TooltipColor Color { get; set; } = TooltipColor.None;

    private string colorClass => Color.ToTooltipColorClass()!;

    /// <summary>
    /// Gets or sets a value indicating whether to display the content as HTML instead of text.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("2.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether to display the content as HTML instead of text.")]
    [Parameter]
    public bool IsHtml { get; set; }

    private string placement => Placement.ToTooltipPlacementName();

    /// <summary>
    /// Gets or sets the tooltip placement.
    /// <para>
    /// Default value is <see cref="TooltipPlacement.Top" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(TooltipPlacement.Top)]
    [Description("Gets or sets the tooltip placement.")]
    [Parameter]
    public TooltipPlacement Placement { get; set; } = TooltipPlacement.Top;

    /// <summary>
    /// Displays informative text when users hover, focus, or tap an element.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Displays informative text when users hover, focus, or tap an element.")]
    [EditorRequired]
    [Parameter]
    public string? Title { get; set; }

    #endregion
}
