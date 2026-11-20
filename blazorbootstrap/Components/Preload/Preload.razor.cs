namespace BlazorBootstrap;

public partial class Preload : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isBlockingApplied;

    private string? lastAnnouncedText;

    private string? loadingText;

    private bool showBackdrop;

    private string? spinnerColor;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            PageLoadingService.OnShow -= OnShow;
            PageLoadingService.OnHide -= OnHide;

            // Never leave the page blocked when the overlay goes away while it is shown.
            if (isBlockingApplied)
            {
                isBlockingApplied = false;
                await SafeInvokeVoidAsync("window.blazorBootstrap.preload.hide", Id);
            }
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Call the browser only when the rendered state changes, so Show() followed by Hide() in one handler does nothing.
        if (showBackdrop)
        {
            var announcedText = GetAnnouncedText();

            if (!isBlockingApplied)
            {
                isBlockingApplied = true;
                lastAnnouncedText = announcedText;
                await SafeInvokeVoidAsync("window.blazorBootstrap.preload.show", Id, announcedText);
            }
            else if (announcedText != lastAnnouncedText)
            {
                lastAnnouncedText = announcedText;
                await SafeInvokeVoidAsync("window.blazorBootstrap.preload.update", Id, announcedText);
            }
        }
        else if (isBlockingApplied)
        {
            isBlockingApplied = false;
            await SafeInvokeVoidAsync("window.blazorBootstrap.preload.hide", Id);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        PageLoadingService.OnShow += OnShow;
        PageLoadingService.OnHide += OnHide;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the text announced while the overlay is shown: the visible loading text, otherwise <see cref="VisuallyHiddenText" />.
    /// Returns <see langword="null"/> for custom content, whose own text is announced instead.
    /// </summary>
    private string? GetAnnouncedText()
    {
        if (ChildContent is not null)
            return null;

        if (!string.IsNullOrWhiteSpace(loadingText))
            return loadingText;

        if (!string.IsNullOrWhiteSpace(LoadingText))
            return LoadingText;

        return VisuallyHiddenText;
    }

    // PreloadService events can be raised off the renderer's thread (for example from Task.Run), so render through InvokeAsync;
    // otherwise Hide() fails on the Server host and the page stays blocked.
    private void OnHide() =>
        _ = InvokeAsync(() =>
        {
            showBackdrop = false;

            StateHasChanged();
        });

    private void OnShow(SpinnerColor spinnerColor, string? loadingText) =>
        _ = InvokeAsync(() =>
        {
            this.spinnerColor = spinnerColor.ToSpinnerColorClass();

            showBackdrop = true;

            this.loadingText = loadingText;

            StateHasChanged();
        });

    #endregion

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Modal, true),
            (BootstrapClass.PageLoadingModal, true),
            (BootstrapClass.ModalFade, true),
            (BootstrapClass.Show, showBackdrop));

    protected override string? StyleNames =>
        BuildStyleNames(Style,
            ("display:block", showBackdrop),
            ("display:none", !showBackdrop));

    /// <summary>
    /// Gets or sets the content to be rendered within the component.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content to be rendered within the component.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the loading text.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.4")]
    [DefaultValue(null)]
    [Description("Gets or sets the loading text.")]
    [Parameter]
    public string? LoadingText { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PageLoadingService" /> instance.
    /// </summary>
    [Inject]
    private PreloadService PageLoadingService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the text screen readers announce while the overlay is shown and no loading text is set.
    /// <para>
    /// Default value is 'Loading...'.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("Loading...")]
    [Description("Gets or sets the text screen readers announce while the overlay is shown and no loading text is set.")]
    [Parameter]
    public string VisuallyHiddenText { get; set; } = "Loading...";

    #endregion
}
