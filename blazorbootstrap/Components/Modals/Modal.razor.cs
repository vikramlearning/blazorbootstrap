namespace BlazorBootstrap;

public partial class Modal : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Type? childComponent = default!;

    private IconColor closeIconColor;

    private ButtonColor footerButtonColor = ButtonColor.Secondary;

    private string footerButtonCSSClass = string.Empty;

    private string footerButtonText = string.Empty;

    private ModalFullscreen fullscreen = ModalFullscreen.Disabled;

    private bool isVisible;

    private ModalType modalType = ModalType.Light;

    private DotNetObjectReference<Modal> objRef = default!;

    private Dictionary<string, object> parameters = default!;

    private bool showFooterButton = false;

    private ModalSize size = ModalSize.Regular;

    #endregion

    #region Methods

    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Modal());
        builder.Append(BootstrapClassProvider.ModalFade());

        base.BuildClasses(builder);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (Rendered)
                    await JS.InvokeVoidAsync("window.blazorBootstrap.modal.dispose", ElementId);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();

            if (ModalService is not null && IsServiceModal)
                ModalService.OnShow -= OnShowAsync;
        }

        await base.DisposeAsync(disposing);
    }

    protected override async Task OnInitializedAsync()
    {
        if (ModalService is not null && IsServiceModal)
            ModalService.OnShow += OnShowAsync;

        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        QueueAfterRenderAction(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.modal.initialize", ElementId, UseStaticBackdrop, CloseOnEscape, objRef); });
    }

    [JSInvokable]
    public async Task bsHiddenModal()
    {
        await OnHidden.InvokeAsync();

        if (ModalService is not null && IsServiceModal)
            ModalService.OnClose();
    }

    [JSInvokable]
    public async Task bsHideModal() => await OnHiding.InvokeAsync();

    [JSInvokable]
    public async Task bsHidePreventedModal() => await OnHidePrevented.InvokeAsync();

    [JSInvokable]
    public async Task bsShowModal() => await OnShowing.InvokeAsync();

    [JSInvokable]
    public async Task bsShownModal() => await OnShown.InvokeAsync();

    /// <summary>
    /// Hides a modal.
    /// </summary>
    public async Task HideAsync()
    {
        isVisible = false;
        await JS.InvokeVoidAsync("window.blazorBootstrap.modal.hide", ElementId);
    }

    /// <summary>
    /// Opens a modal.
    /// </summary>
    public async Task ShowAsync() => await ShowAsync(null, null, null, null);

    /// <summary>
    /// Opens a modal.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    public async Task ShowAsync<T>(string title, string? message = null, Dictionary<string, object>? parameters = null) => await ShowAsync(title, message, typeof(T), parameters);

    private Task OnShowAsync(ModalOption modalOption)
    {
        if (modalOption is null)
            throw new ArgumentNullException(nameof(modalOption));

        modalType = modalOption.Type;

        Size = modalOption.Size;

        IsVerticallyCentered = modalOption.IsVerticallyCentered;

        showFooterButton = modalOption.ShowFooterButton;

        if (showFooterButton)
        {
            footerButtonColor = modalOption.FooterButtonColor;
            footerButtonCSSClass = modalOption.FooterButtonCSSClass;
            footerButtonText = modalOption.FooterButtonText;
            FooterCssClass = "border-top-0";
        }

        return ShowAsync(modalOption.Title, modalOption.Message, null, null);
    }

    private async Task ShowAsync(string? title, string? message, Type? type, Dictionary<string, object>? parameters)
    {
        isVisible = true;

        if (!string.IsNullOrWhiteSpace(title))
            Title = title;

        if (!string.IsNullOrWhiteSpace(message))
            Message = message;

        childComponent = type;

        this.parameters = parameters!;

        await InvokeAsync(StateHasChanged);

        await JS.InvokeVoidAsync("window.blazorBootstrap.modal.show", ElementId);
    }

    #endregion

    #region Properties, Indexers

    /// <inheritdoc />
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Additional body CSS class.
    /// </summary>
    [Parameter]
    public string BodyCssClass { get; set; } = default!;

    /// <summary>
    /// Body template.
    /// </summary>
    [Parameter]
    public RenderFragment BodyTemplate { get; set; } = default!;

    /// <summary>
    /// Gets or sets the close icon color.
    /// </summary>
    [Parameter]
    public IconColor CloseIconColor
    {
        get => closeIconColor;
        set
        {
            closeIconColor = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Indicates whether the modal closes when escape key is pressed.
    /// Default value is true.
    /// </summary>
    [Parameter]
    public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Additional CSS class for the dialog (div.modal-dialog element).
    /// </summary>
    [Parameter]
    public string DialogCssClass { get; set; } = default!;

    /// <summary>
    /// Footer css class.
    /// </summary>
    [Parameter]
    public string FooterCssClass { get; set; } = default!;

    /// <summary>
    /// Footer template.
    /// </summary>
    [Parameter]
    public RenderFragment FooterTemplate { get; set; } = default!;

    /// <summary>
    /// Fullscreen behavior of the modal. Default is <see cref="ModalFullscreen.Disabled" />.
    /// </summary>
    [Parameter]
    public ModalFullscreen Fullscreen
    {
        get => fullscreen;
        set
        {
            fullscreen = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Additional header CSS class.
    /// </summary>
    [Parameter]
    public string HeaderCssClass { get; set; } = default!;

    private string headerCssClassInternal => BootstrapClassProvider.ModalHeader(modalType);

    /// <summary>
    /// Header template.
    /// </summary>
    [Parameter]
    public RenderFragment HeaderTemplate { get; set; } = default!;

    /// <summary>
    /// Allows modal body scroll.
    /// </summary>
    [Parameter]
    public bool IsScrollable { get; set; }

    /// <summary>
    /// Indicates whether the modal is related to a modal service or not.
    /// </summary>
    [Parameter]
    public bool IsServiceModal { get; set; } = false;

    /// <summary>
    /// Shows the modal vertically in the center.
    /// </summary>
    [Parameter]
    public bool IsVerticallyCentered { get; set; }

    /// <summary>
    /// Message in modal body.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = default!;

    private string modalFullscreen => BootstrapClassProvider.ToModalFullscreen(Fullscreen)!;

    [Inject] private ModalService ModalService { get; set; } = default!;

    private string modalSize => BootstrapClassProvider.ToModalSize(Size)!;

    /// <summary>
    /// Gets or sets the modal type.
    /// </summary>
    [Parameter]
    public ModalType ModalType
    {
        get => modalType;
        set
        {
            modalType = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter]
    public EventCallback OnHidden { get; set; }

    /// <summary>
    /// This event is fired when the modal is shown, its backdrop is static and a click outside the modal or an escape key
    /// press is performed with the keyboard option or data-bs-keyboard set to false.
    /// </summary>
    [Parameter]
    public EventCallback OnHidePrevented { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [Parameter]
    public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter]
    public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter]
    public EventCallback OnShown { get; set; }

    private string scrollable => IsScrollable ? "modal-dialog-scrollable" : "";

    /// <summary>
    /// Indicates whether the modal shows close button in header.
    /// Default value is true.
    /// </summary>
    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Size of the modal. Default is <see cref="ModalSize.Regular" />.
    /// </summary>
    [Parameter]
    public ModalSize Size
    {
        get => size;
        set
        {
            size = value;
            DirtyClasses();
        }
    }

    /// <summary>
    /// Gets or sets the tab index.
    /// </summary>
    [Parameter]
    public int TabIndex { get; set; } = -1;

    /// <summary>
    /// Title in modal header.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = default!;

    /// <summary>
    /// Indicates whether the modal uses a static backdrop.
    /// Default value is false.
    /// </summary>
    [Parameter]
    public bool UseStaticBackdrop { get; set; } = false;

    private string verticallyCentered => IsVerticallyCentered ? "modal-dialog-centered" : "";

    #endregion
}
