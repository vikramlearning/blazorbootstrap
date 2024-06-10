namespace BlazorBootstrap;

/// <summary>
/// Indicate the loading state of a page with Blazor Bootstrap preload component.
/// </summary>
public partial class Preload : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private string? loadingText;

    private bool showBackdrop;

    private string? spinnerColor;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            PageLoadingService.OnShow -= OnShow;
            PageLoadingService.OnHide -= OnHide;
        }

        await base.DisposeAsyncCore(disposing);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        PageLoadingService.OnShow += OnShow;
        PageLoadingService.OnHide += OnHide;
    }

    private void OnHide()
    {
        showBackdrop = false;

        StateHasChanged();
    }

    private void OnShow(SpinnerColor spinnerColor, string? loadingText)
    {
        this.spinnerColor = spinnerColor.ToSpinnerColorClass();

        showBackdrop = true;

        this.loadingText = loadingText;

        StateHasChanged();
    }

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
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the loading text.
    /// </summary>
    [Parameter]
    public string? LoadingText { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PageLoadingService" /> instance.
    /// </summary>
    [Inject]
    private PreloadService PageLoadingService { get; set; } = default!;

    #endregion
}
