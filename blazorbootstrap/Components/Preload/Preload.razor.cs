namespace BlazorBootstrap;

public partial class Preload : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private string? loadingText;

    private bool showBackdrop;

    private string? spinnerColor;
    private string? spinnerType;
    private string? animatedLoaderPrimaryColor;
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

    private void OnShow(SpinnerColor spinnerColor, string? loadingText, string? spinnerType)
    {
        this.spinnerColor = spinnerColor.ToSpinnerColorClass();
        this.animatedLoaderPrimaryColor = spinnerColor.ToHexColorCode();

        showBackdrop = true;

        this.loadingText = loadingText;
        this.spinnerType = spinnerType;
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
    /// Gets or sets the spinner type.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.10.4")]
    [DefaultValue(StringConstants.DefaultBootstrapSpinner)]
    [Description("Gets or sets the spinner type.")]
    [Parameter]
    public string? SpinnerType { get; set; }
    #endregion
}
