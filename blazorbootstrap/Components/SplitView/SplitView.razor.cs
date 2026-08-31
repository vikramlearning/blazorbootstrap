namespace BlazorBootstrap;

/// <summary>
/// Provides two resizable panes separated by a draggable divider.
/// </summary>
[AddedVersion("4.0.0")]
public partial class SplitView : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private double currentPrimaryPaneSize = 50;
    private bool hasReceivedParameters;
    private bool isResizing;
    private DotNetObjectReference<SplitView>? objRef;
    private SplitViewColor previousColor;
    private string? previousCustomColor;
    private double previousMinimumPaneSizeParameter;
    private double previousPrimaryPaneSizeParameter;
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
        var normalizedPrimaryPaneSize = NormalizePrimaryPaneSize(PrimaryPaneSize);
        var primaryPaneSizeParameterChanged = !hasReceivedParameters || Math.Abs(previousPrimaryPaneSizeParameter - PrimaryPaneSize) >= 0.01d;
        var minimumPaneSizeParameterChanged = !hasReceivedParameters || Math.Abs(previousMinimumPaneSizeParameter - MinimumPaneSize) >= 0.01d;

        if (primaryPaneSizeParameterChanged)
            currentPrimaryPaneSize = normalizedPrimaryPaneSize;
        else if (minimumPaneSizeParameterChanged)
            currentPrimaryPaneSize = NormalizePrimaryPaneSize(currentPrimaryPaneSize);

        hasReceivedParameters = true;
        previousPrimaryPaneSizeParameter = PrimaryPaneSize;
        previousMinimumPaneSizeParameter = MinimumPaneSize;

        return base.OnParametersSetAsync();
    }

    /// <summary>
    /// Handles the JavaScript notification that divider resizing has ended.
    /// </summary>
    /// <param name="primaryPaneSize">The final primary-pane size as a percentage.</param>
    /// <param name="secondaryPaneSize">The final secondary-pane size as a percentage.</param>
    [AddedVersion("4.0.0")]
    [Description("Handles the notification that divider resizing has ended, updates the current size, and raises OnResizeEnded.")]
    [JSInvokable]
    public async Task OnResizeEndedJS(double primaryPaneSize, double secondaryPaneSize)
    {
        isResizing = false;
        currentPrimaryPaneSize = primaryPaneSize;
        CaptureRenderedState();

        if (OnResizeEnded.HasDelegate)
            await OnResizeEnded.InvokeAsync(new SplitViewResizeEventArgs(primaryPaneSize, secondaryPaneSize, Orientation));
    }

    /// <summary>
    /// Handles the JavaScript notification that divider resizing has started.
    /// </summary>
    /// <param name="primaryPaneSize">The initial primary-pane size as a percentage.</param>
    /// <param name="secondaryPaneSize">The initial secondary-pane size as a percentage.</param>
    [AddedVersion("4.0.0")]
    [Description("Handles the notification that divider resizing has started and raises OnResizeStarted.")]
    [JSInvokable]
    public async Task OnResizeStartedJS(double primaryPaneSize, double secondaryPaneSize)
    {
        isResizing = true;
        currentPrimaryPaneSize = primaryPaneSize;

        if (OnResizeStarted.HasDelegate)
            await OnResizeStarted.InvokeAsync(new SplitViewResizeEventArgs(primaryPaneSize, secondaryPaneSize, Orientation));
    }

    /// <summary>
    /// Handles the JavaScript notification that the divider size has changed.
    /// </summary>
    /// <param name="primaryPaneSize">The current primary-pane size as a percentage.</param>
    /// <param name="secondaryPaneSize">The current secondary-pane size as a percentage.</param>
    [AddedVersion("4.0.0")]
    [Description("Handles a divider-size change, updates the current primary-pane size, and raises the bound size and resize callbacks.")]
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
    /// Gets or sets the Bootstrap contextual color used for the divider.
    /// <para>
    /// Default value is <see cref="SplitViewColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(SplitViewColor.None)]
    [Description("Gets or sets the Bootstrap contextual color used for the divider.")]
    [Parameter]
    public SplitViewColor Color { get; set; }

    /// <summary>
    /// Gets or sets a custom CSS color used for the divider.
    /// <para>
    /// Accepts any valid CSS color expression, including CSS variables.
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets a custom CSS color used for the divider. When specified, it overrides the color supplied by Color.")]
    [Parameter]
    public string? CustomColor { get; set; }

    /// <summary>
    /// Gets or sets whether users can resize the panes.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether users can resize the panes. When true, the divider is displayed but cannot be dragged.")]
    [Parameter]
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed size of each pane as a percentage.
    /// <para>
    /// Default value is <c>0</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(0d)]
    [Description("Gets or sets the minimum allowed size of each pane as a percentage. Values are constrained to the range 0 through 50.")]
    [Parameter]
    public double MinimumPaneSize { get; set; }

    /// <summary>
    /// Occurs after the user releases the divider.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Fires after the user releases the divider and supplies the final size of both panes.")]
    [Parameter]
    public EventCallback<SplitViewResizeEventArgs> OnResizeEnded { get; set; }

    /// <summary>
    /// Occurs whenever dragging changes the divider position.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Fires while the divider is dragged and supplies the current size of both panes.")]
    [Parameter]
    public EventCallback<SplitViewResizeEventArgs> OnResized { get; set; }

    /// <summary>
    /// Occurs when the user starts dragging the divider.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Fires when the user starts dragging the divider and supplies the initial size of both panes.")]
    [Parameter]
    public EventCallback<SplitViewResizeEventArgs> OnResizeStarted { get; set; }

    /// <summary>
    /// Gets or sets whether panes are arranged side by side or stacked.
    /// <para>
    /// Default value is <see cref="SplitViewOrientation.Horizontal" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(SplitViewOrientation.Horizontal)]
    [Description("Gets or sets whether panes are arranged side by side or stacked. The orientation also determines the divider drag direction.")]
    [Parameter]
    public SplitViewOrientation Orientation { get; set; } = SplitViewOrientation.Horizontal;

    /// <summary>
    /// Gets or sets the content rendered in the primary pane.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content rendered in the primary pane, before the divider.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? Pane1 { get; set; }

    /// <summary>
    /// Gets or sets the content rendered in the secondary pane.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content rendered in the secondary pane, after the divider.")]
    [EditorRequired]
    [Parameter]
    public RenderFragment? Pane2 { get; set; }

    /// <summary>
    /// Gets or sets the primary-pane size as a percentage.
    /// <para>
    /// Default value is <c>50</c>.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(50d)]
    [Description("Gets or sets the primary-pane size as a percentage. The value is constrained so both panes satisfy MinimumPaneSize.")]
    [Parameter]
    public double PrimaryPaneSize { get; set; } = 50d;

    /// <summary>
    /// Occurs whenever the user changes the primary-pane size.
    /// </summary>
    [AddedVersion("4.0.0")]
    [Description("Fires whenever dragging changes the primary-pane size and supports two-way binding with PrimaryPaneSize.")]
    [Parameter]
    public EventCallback<double> PrimaryPaneSizeChanged { get; set; }

    private string SeparatorAriaOrientation => Orientation == SplitViewOrientation.Horizontal ? "vertical" : "horizontal";

    protected override string? StyleNames =>
        BuildStyleNames(Style,
                        ($"--bb-split-view-divider-color:{CustomColor}", !string.IsNullOrWhiteSpace(CustomColor)));

    #endregion
}