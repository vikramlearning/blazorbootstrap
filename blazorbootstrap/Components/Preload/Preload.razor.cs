using BlazorBootstrap.Extensions;

namespace BlazorBootstrap;

public partial class Preload : BaseComponent
{
    #region Members

    private SpinnerColor color = SpinnerColor.None;

    private string verticallyCentered => IsVerticallyCentered ? "modal-dialog-centered" : "";

    private string spinnerColor => Color.ToSpinnerColor();

    private bool showBackdrop;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Modal());
        builder.Append(BootstrapClassProvider.PageLoadingModal());
        builder.Append(BootstrapClassProvider.ModalFade());
        builder.Append(BootstrapClassProvider.Show(), showBackdrop);

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

    private void OnShow()
    {
        showBackdrop = true;

        this.DirtyClasses();
        this.DirtyStyles();

        StateHasChanged();
    }

    private void OnHide()
    {
        showBackdrop = false;

        this.DirtyClasses();
        this.DirtyStyles();

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
    [Inject] PreloadService PageLoadingService { get; set; }

    /// <summary>
    /// Shows the preload vertically in the center of the page.
    /// </summary>
    [Parameter] public bool IsVerticallyCentered { get; set; } = true;

    /// <summary>
    /// Gets or sets the spinner color.
    /// </summary>
    [Parameter] public SpinnerColor Color
    {
        get => color; set
        {
            color = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Specifies the content to be rendered inside this.
    /// </summary>
    [Parameter] public RenderFragment ChildContent { get; set; }

    #endregion Properties
}
