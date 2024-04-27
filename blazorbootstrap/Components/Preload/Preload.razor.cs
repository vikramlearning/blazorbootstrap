namespace BlazorBootstrap;

public partial class Preload : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private string? loadingText;

    private bool showBackdrop;

    private string? spinnerColor;

    #endregion

    #region Methods

    protected string? ClassNames => new CssClassBuilder(Class)
        .AddClass(BootstrapClass.Modal)
        .AddClass(BootstrapClass.PageLoadingModal)
        .AddClass(BootstrapClass.ModalFade)
        .AddClass(BootstrapClass.Show, showBackdrop)
        .Build();

    protected string? StyleNames => new CssStyleBuilder(Style)
        .AddStyle("display:block", showBackdrop)
        .AddStyle("display:none", !showBackdrop)
        .Build();

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
        
        this.showBackdrop = true;

        this.loadingText = loadingText;

        StateHasChanged();
    }

    #endregion

    #region Properties, Indexers

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
