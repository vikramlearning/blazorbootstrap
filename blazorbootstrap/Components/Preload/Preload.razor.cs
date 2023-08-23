namespace BlazorBootstrap;

public partial class Preload : BaseComponent
{
    #region Members

    private string? spinnerColor;

    private bool showBackdrop;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(ClassProvider.Modal());
        builder.Append(ClassProvider.PageLoadingModal());
        builder.Append(ClassProvider.ModalFade());
        builder.Append(ClassProvider.Show(), showBackdrop);

        base.BuildClasses(builder);
    }

    protected override void BuildStyles(StyleBuilder builder)
    {
        builder.Append("display:block", showBackdrop);
        builder.Append("display:none", !showBackdrop);

        base.BuildStyles(builder);
    }

    protected override void OnInitialized()
    {
        PageLoadingService.OnShow += OnShow;
        PageLoadingService.OnHide += OnHide;
    }

    private void OnShow(SpinnerColor spinnerColor)
    {
        this.spinnerColor = spinnerColor.ToSpinnerColor();

        showBackdrop = true;

        DirtyClasses();
        DirtyStyles();

        StateHasChanged();
    }

    private void OnHide()
    {
        showBackdrop = false;

        DirtyClasses();
        DirtyStyles();

        StateHasChanged();
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

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the <see cref="PageLoadingService" /> instance.
    /// </summary>
    [Inject] private PreloadService PageLoadingService { get; set; } = default!;

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion Properties
}
