namespace BlazorBootstrap;

public partial class Preload : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private string? loadingText;

    private bool showBackdrop;

    private string? spinnerColor;

    #endregion

    #region Methods

    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.Modal);
        this.AddClass(BootstrapClassProvider.PageLoadingModal);
        this.AddClass(BootstrapClassProvider.ModalFade);
        this.AddClass(BootstrapClassProvider.Show, showBackdrop);

        base.BuildClasses();
    }

    protected override void BuildStyles()
    {
        this.AddStyle("display:block", showBackdrop);
        this.AddStyle("display:none", !showBackdrop);

        base.BuildStyles();
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            PageLoadingService.OnShow -= OnShow;
            PageLoadingService.OnHide -= OnHide;
        }

        await base.DisposeAsync(disposing);
    }

    protected override void OnInitialized()
    {
        PageLoadingService.OnShow += OnShow;
        PageLoadingService.OnHide += OnHide;
    }

    private void OnHide()
    {
        showBackdrop = false;

        DirtyClasses();
        DirtyStyles();

        StateHasChanged();
    }

    private void OnShow(SpinnerColor spinnerColor, string? loadingText)
    {
        this.spinnerColor = spinnerColor.ToSpinnerColor();
        
        this.showBackdrop = true;

        this.loadingText = loadingText;

        DirtyClasses();
        DirtyStyles();

        StateHasChanged();
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PageLoadingService" /> instance.
    /// </summary>
    [Inject]
    private PreloadService PageLoadingService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the loading text.
    /// </summary>
    [Parameter]
    public string? LoadingText { get; set; }

    #endregion
}
