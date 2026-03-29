namespace BlazorBootstrap;

public partial class Modal : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Type? childComponent = default!;

    private ButtonColor footerButtonColor = ButtonColor.Secondary;

    private string footerButtonCSSClass = string.Empty;

    private string footerButtonText = string.Empty;

    private bool isVisible;

    private DotNetObjectReference<Modal> objRef = default!;

    private Dictionary<string, object> parameters = default!;

    private bool showFooterButton = false;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (IsRenderComplete)
                    await SafeInvokeVoidAsync("window.blazorBootstrap.modal.dispose", Id);
            }
            catch (JSDisconnectedException)
            {
                // do nothing
            }

            objRef?.Dispose();

            if (ModalService is not null && IsServiceModal)
                ModalService.OnShow -= OnShowAsync;
        }

        await base.DisposeAsyncCore(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await SafeInvokeVoidAsync("window.blazorBootstrap.modal.initialize", Id, UseStaticBackdrop, CloseOnEscape, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        if (ModalService is not null && IsServiceModal)
            ModalService.OnShow += OnShowAsync;

        await base.OnInitializedAsync();
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
    [AddedVersion("1.0.0")]
    [Description("Hides a modal.")]
    public async Task HideAsync()
    {
        isVisible = false;
        await SafeInvokeVoidAsync("window.blazorBootstrap.modal.hide", Id);
    }

    /// <summary>
    /// Opens a modal.
    /// </summary>
    [AddedVersion("1.4.1")]
    [Description("Opens a modal.")]
    public async Task ShowAsync() => await ShowAsync(null, null, null, null);

    /// <summary>
    /// Opens a modal. T is component.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    [AddedVersion("1.0.0")]
    [Description("Opens a modal. T is component.")]
    public async Task ShowAsync<T>(string title, string? message = null, Dictionary<string, object>? parameters = null) => await ShowAsync(title, message, typeof(T), parameters);

    private Task OnShowAsync(ModalOption modalOption)
    {
        if (modalOption is null)
            throw new ArgumentNullException(nameof(modalOption));

        ModalType = modalOption.Type;

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

        await SafeInvokeVoidAsync("window.blazorBootstrap.modal.show", Id);
    }

    #endregion

    #region Properties, Indexers

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Modal, true),
            (BootstrapClass.ModalFade, true));

    /// <summary>
    /// Gets or sets the body CSS class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the body CSS class.")]
    [Parameter]
    public string? BodyCssClass { get; set; }

    /// <summary>
    /// Gets or sets the body template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the body template.")]
    [Parameter]
    public RenderFragment? BodyTemplate { get; set; }

    /// <summary>
    /// Gets or sets the close icon color.
    /// <para>
    /// Default value is <see cref="IconColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(IconColor.None)]
    [Description("Gets or sets the close icon color.")]
    [Parameter]
    public IconColor CloseIconColor { get; set; } = IconColor.None;

    /// <summary>
    /// Indicates whether the modal closes when escape key is pressed.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(true)]
    [Description("Indicates whether the modal closes when escape key is pressed.")]
    [Parameter]
    public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Gets or sets the modal dialog (div.modal-dialog) CSS class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the modal dialog (div.modal-dialog) CSS class.")]
    [Parameter]
    public string? DialogCssClass { get; set; }

    /// <summary>
    /// Gets or sets the footer CSS class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the footer CSS class.")]
    [Parameter]
    public string? FooterCssClass { get; set; }

    /// <summary>
    /// Gets or sets the footer template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the footer template.")]
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the fullscreen behavior of the modal.
    /// <para>
    /// Default value is <see cref="ModalFullscreen.Disabled" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(ModalFullscreen.Disabled)]
    [Description("Gets or sets the fullscreen behavior of the modal.")]
    [Parameter]
    public ModalFullscreen Fullscreen { get; set; } = ModalFullscreen.Disabled;

    /// <summary>
    /// Gets or sets the header CSS class.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the header CSS class.")]
    [Parameter]
    public string? HeaderCssClass { get; set; }

    // Modal close "X" button with bootstrap 5.3.3 - https://github.com/vikramlearning/blazorbootstrap/issues/714
    // Review this fix after bootstrap 5.3.4 or 5.4 release. Ref: https://github.com/twbs/bootstrap/issues/39798
    private string headerCssClassInternal => $"justify-content-between {ModalType.ToModalHeaderColorClass()}".Trim();

    /// <summary>
    /// Gets or sets the header template.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the header template.")]
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// If <see langword="true" />, scroll will be enabled on the modal body.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("If <b>true</b>, scroll will be enabled on the modal body.")]
    [Parameter]
    public bool IsScrollable { get; set; }

    /// <summary>
    /// Indicates whether the modal is related to a modal service or not.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.9.4")]
    [DefaultValue(false)]
    [Description("Indicates whether the modal is related to a modal service or not.")]
    [Parameter]
    public bool IsServiceModal { get; set; }

    /// <summary>
    /// If <see langword="true" />, shows the modal vertically in the center.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("If <b>true</b>, shows the modal vertically in the center.")]
    [Parameter]
    public bool IsVerticallyCentered { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the message.")]
    [Parameter]
    public string? Message { get; set; }

    private string modalFullscreen => Fullscreen.ToModalFullscreenClass();

    [Inject] 
    private ModalService ModalService { get; set; } = default!;

    private string modalSize => Size.ToModalSizeClass();

    /// <summary>
    /// Gets or sets the modal type.
    /// <para>
    /// Default value is <see cref="ModalType.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.9.5")]
    [DefaultValue(ModalType.None)]
    [Description("Gets or sets the modal type.")]
    [Parameter]
    public ModalType ModalType { get; set; } = ModalType.None;

    /// <summary>
    /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete).")]
    [Parameter]
    public EventCallback OnHidden { get; set; }

    /// <summary>
    /// This event is fired when the modal is shown, its backdrop is static and a click outside the modal or an escape key
    /// press is performed with the keyboard option or data-bs-keyboard set to <see langword="false"/>.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event is fired when the modal is shown, its backdrop is static and a click outside the modal or an escape key press is performed with the keyboard option or data-bs-keyboard set to <b>false</b>.")]
    [Parameter]
    public EventCallback OnHidePrevented { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event is fired immediately when the hide method has been called.")]
    [Parameter]
    public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event fires immediately when the show instance method is called.")]
    [Parameter]
    public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete).
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete).")]
    [Parameter]
    public EventCallback OnShown { get; set; }

    private string scrollable => IsScrollable ? "modal-dialog-scrollable" : "";

    /// <summary>
    /// If <see langword="true" />, close button will be visible in the modal header.
    /// <para>
    /// Default value is <see langword="true"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(true)]
    [Description("If <b>true</b>, close button will be visible in the modal header.")]
    [Parameter]
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the modal size.
    /// <para>
    /// Default value is <see cref="ModalSize.Regular" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(ModalSize.Regular)]
    [Description("Gets or sets the modal size.")]
    [Parameter]
    public ModalSize Size { get; set; } = ModalSize.Regular;

    /// <summary>
    /// Gets or sets the modal tab index.
    /// <para>
    /// Default value is -1.
    /// </para>
    /// </summary>
    [AddedVersion("1.6.0")]
    [DefaultValue(-1)]
    [Description("Gets or sets the modal tab index.")]
    [Parameter]
    public int TabIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets the modal header title.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the modal header title.")]
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Indicates whether the modal uses a static backdrop.
    /// <para>
    /// Default value is <see langword="false"/>.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(false)]
    [Description("Indicates whether the modal uses a static backdrop.")]
    [Parameter]
    public bool UseStaticBackdrop { get; set; }

    private string verticallyCentered => IsVerticallyCentered ? "modal-dialog-centered" : "";

    #endregion
}
