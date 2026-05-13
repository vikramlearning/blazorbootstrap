namespace BlazorBootstrap;

public partial class SplitView : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private double currentPrimaryPaneSize = 50;
    private bool isResizing;
    private DotNetObjectReference<SplitView>? objRef;
    private SplitViewColor previousColor;
    private string? previousCustomColor;
    private double previousMinimumPaneSize;
    private SplitViewOrientation previousOrientation;
    private double previousPrimaryPaneSize;
    private bool previousIsDisabled;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (IsRenderComplete && Id is not null)
                    await SafeInvokeVoidAsync("window.blazorBootstrap.splitView.dispose", Id);
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
            await SafeInvokeVoidAsync("window.blazorBootstrap.splitView.initialize", Id!, Orientation.ToString(), currentPrimaryPaneSize, EffectiveMinimumPaneSize,
                                      IsDisabled, objRef!);

            CaptureRenderedState();
        }
        else if (!isResizing && ParametersChanged())
        {
            await SafeInvokeVoidAsync("window.blazorBootstrap.splitView.update", Id!, Orientation.ToString(), currentPrimaryPaneSize, EffectiveMinimumPaneSize,
                                      IsDisabled);

            CaptureRenderedState();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        return base.OnInitializedAsync();
    }

    protected override Task OnParametersSetAsync()
    {
        currentPrimaryPaneSize = NormalizePrimaryPaneSize(PrimaryPaneSize);

        return base.OnParametersSetAsync();
    }

    [JSInvokable]
    public async Task OnResizeEndedJS(double primaryPaneSize, double secondaryPaneSize)
    {
        isResizing = false;
        currentPrimaryPaneSize = primaryPaneSize;
        CaptureRenderedState();

        if (OnResizeEnded.HasDelegate)
            await OnResizeEnded.InvokeAsync(new SplitViewResizeEventArgs(primaryPaneSize, secondaryPaneSize, Orientation));
    }

    [JSInvokable]
    public async Task OnResizeStartedJS(double primaryPaneSize, double secondaryPaneSize)
    {
        isResizing = true;
        currentPrimaryPaneSize = primaryPaneSize;

        if (OnResizeStarted.HasDelegate)
            await OnResizeStarted.InvokeAsync(new SplitViewResizeEventArgs(primaryPaneSize, secondaryPaneSize, Orientation));
    }

    [JSInvokable]
    public async Task OnResizedJS(double primaryPaneSize, double secondaryPaneSize)
    {
        if (Math.Abs(currentPrimaryPaneSize - primaryPaneSize) < 0.01d && !PrimaryPaneSizeChanged.HasDelegate && !OnResized.HasDelegate)
            return;

        currentPrimaryPaneSize = primaryPaneSize;

        if (PrimaryPaneSizeChanged.HasDelegate)
            await PrimaryPaneSizeChanged.InvokeAsync(primaryPaneSize);

        if (OnResized.HasDelegate)
            await OnResized.InvokeAsync(new SplitViewResizeEventArgs(primaryPaneSize, secondaryPaneSize, Orientation));
    }

    private void CaptureRenderedState()
    {
        previousPrimaryPaneSize = currentPrimaryPaneSize;
        previousMinimumPaneSize = EffectiveMinimumPaneSize;
        previousOrientation = Orientation;
        previousIsDisabled = IsDisabled;
        previousColor = Color;
        previousCustomColor = CustomColor;
    }

    private double NormalizePrimaryPaneSize(double primaryPaneSize)
    {
        var minimumPaneSize = EffectiveMinimumPaneSize;
        var maximumPaneSize = 100d - minimumPaneSize;

        return Math.Clamp(primaryPaneSize, minimumPaneSize, maximumPaneSize);
    }

    private bool ParametersChanged() =>
        previousPrimaryPaneSize != currentPrimaryPaneSize ||
        previousMinimumPaneSize != EffectiveMinimumPaneSize ||
        previousOrientation != Orientation ||
        previousIsDisabled != IsDisabled ||
        previousColor != Color ||
        previousCustomColor != CustomColor;

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
                        ("bb-split-view", true),
                        ("bb-split-view-horizontal", Orientation == SplitViewOrientation.Horizontal),
                        ("bb-split-view-vertical", Orientation == SplitViewOrientation.Vertical),
                        (Color.ToSplitViewColorClass(), Color != SplitViewColor.None),
                        ("bb-split-view-disabled", IsDisabled));

    private double EffectiveMinimumPaneSize => Math.Clamp(MinimumPaneSize, 0d, 50d);

    /// <summary>
    /// Gets or sets the divider color.
    /// <para>
    /// Default value is <see cref="SplitViewColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(SplitViewColor.None)]
    [Description("Gets or sets the divider color.")]
    [Parameter]
    public SplitViewColor Color { get; set; }

    /// <summary>
    /// Gets or sets a custom divider color.
    /// <para>
    /// Accepts any valid CSS color expression, including CSS variables.
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a custom divider color.")]
    [Parameter]
    public string? CustomColor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether user interaction is disabled.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether user interaction is disabled.")]
    [Parameter]
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Gets or sets the minimum pane size as a percentage.
    /// <para>
    /// Default value is <c>0</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(0d)]
    [Description("Gets or sets the minimum pane size as a percentage.")]
    [Parameter]
    public double MinimumPaneSize { get; set; }

    /// <summary>
    /// Fired when resizing ends.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Fired when resizing ends.")]
    [Parameter]
    public EventCallback<SplitViewResizeEventArgs> OnResizeEnded { get; set; }

    /// <summary>
    /// Fired while the divider is being dragged.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Fired while the divider is being dragged.")]
    [Parameter]
    public EventCallback<SplitViewResizeEventArgs> OnResized { get; set; }

    /// <summary>
    /// Fired when resizing starts.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Fired when resizing starts.")]
    [Parameter]
    public EventCallback<SplitViewResizeEventArgs> OnResizeStarted { get; set; }

    /// <summary>
    /// Gets or sets the SplitView orientation.
    /// <para>
    /// Default value is <see cref="SplitViewOrientation.Horizontal" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(SplitViewOrientation.Horizontal)]
    [Description("Gets or sets the SplitView orientation.")]
    [Parameter]
    public SplitViewOrientation Orientation { get; set; } = SplitViewOrientation.Horizontal;

    /// <summary>
    /// Gets or sets the first pane content.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the first pane content.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? Pane1 { get; set; }

    /// <summary>
    /// Gets or sets the second pane content.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the second pane content.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? Pane2 { get; set; }

    /// <summary>
    /// Gets or sets the primary pane size as a percentage.
    /// <para>
    /// Default value is <c>50</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(50d)]
    [Description("Gets or sets the primary pane size as a percentage.")]
    [Parameter]
    public double PrimaryPaneSize { get; set; } = 50d;

    /// <summary>
    /// Fired when the primary pane size changes.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Fired when the primary pane size changes.")]
    [Parameter]
    public EventCallback<double> PrimaryPaneSizeChanged { get; set; }

    private string SeparatorAriaOrientation => Orientation == SplitViewOrientation.Horizontal ? "vertical" : "horizontal";

    protected override string? StyleNames =>
        BuildStyleNames(Style,
                        ($"--bb-split-view-divider-color:{CustomColor}", !string.IsNullOrWhiteSpace(CustomColor)));

    #endregion
}