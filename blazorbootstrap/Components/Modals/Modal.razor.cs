﻿namespace BlazorBootstrap;

public partial class Modal : BaseComponent
{
    #region Members

    private bool isVisible;

    private string title = default!;

    private string message = default!;

    private Type? childComponent = default!;

    private Dictionary<string, object> parameters = default!;

    private ModalSize size = ModalSize.Regular;

    private ModalFullscreen fullscreen = ModalFullscreen.Disabled;

    private string scrollable => IsScrollable ? "modal-dialog-scrollable" : "";

    private string verticallyCentered => IsVerticallyCentered ? "modal-dialog-centered" : "";

    private string modalSize => BootstrapClassProvider.ToModalSize(Size);

    private string modalFullscreen => BootstrapClassProvider.ToModalFullscreen(Fullscreen);

    private bool showFooterButton = false;

    private string footerButtonText = string.Empty;

    private ButtonColor footerButtonColor = ButtonColor.Secondary;

    private string footerButtonCSSClass = string.Empty;

    private DotNetObjectReference<Modal> objRef = default!;

    #endregion Members

    #region Methods

    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Modal());
        builder.Append(BootstrapClassProvider.ModalFade());

        base.BuildClasses(builder);
    }

    protected override async Task OnInitializedAsync()
    {
        if (ModalService is not null)
            ModalService.OnShow += OnShowAsync;

        objRef ??= DotNetObjectReference.Create(this);
        await base.OnInitializedAsync();

        ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.modal.initialize", ElementId, UseStaticBackdrop, CloseOnEscape, objRef); });
    }

    #region Modal Service Events

    private Task OnShowAsync(ModalOption modalOption)
    {
        if (modalOption.Type == ModalType.Success)
            HeaderCssClass = "text-bg-success border-bottom border-success";
        else if (modalOption.Type == ModalType.Danger)
            HeaderCssClass = "text-bg-danger border-bottom border-danger";
        else if (modalOption.Type == ModalType.Warning)
            HeaderCssClass = "text-bg-warning border-bottom border-warning";
        else if (modalOption.Type == ModalType.Info)
            HeaderCssClass = "text-bg-info border-bottom border-info";
        else
            HeaderCssClass = "";

        showFooterButton = modalOption.ShowFooterButton;
        if (showFooterButton)
        {
            footerButtonText = modalOption.FooterButtonText;
            FooterCssClass = "border-top-0";
        }

        return ShowAsync(title: modalOption.Title, message: modalOption.Message, type: null, parameters: null);
    }

    #endregion

    /// <summary>
    /// Opens a modal.
    /// </summary>
    public async Task ShowAsync()
    {
        await ShowAsync(title: null, message: null, type: null, parameters: null);
    }

    /// <summary>
    /// Opens a modal.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    public async Task ShowAsync<T>(string title, string message = null, Dictionary<string, object> parameters = null)
    {
        await ShowAsync(title: title, message: message, type: typeof(T), parameters: parameters);
    }

    private async Task ShowAsync(string title, string message, Type? type, Dictionary<string, object> parameters)
    {
        this.isVisible = true;

        if (!string.IsNullOrWhiteSpace(title))
            this.Title = title;

        if (!string.IsNullOrWhiteSpace(message))
            this.Message = message;

        this.childComponent = type;
        this.parameters = parameters;

        await JS.InvokeVoidAsync("window.blazorBootstrap.modal.show", ElementId);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Hides a modal.
    /// </summary>
    public async Task HideAsync()
    {
        this.isVisible = false;
        await JS.InvokeVoidAsync("window.blazorBootstrap.modal.hide", ElementId);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.modal.dispose", ElementId);
            objRef?.Dispose();
        }

        await base.DisposeAsync(disposing);
    }

    [JSInvokable] public async Task bsShowModal() => await OnShowing.InvokeAsync();
    [JSInvokable] public async Task bsShownModal() => await OnShown.InvokeAsync();
    [JSInvokable] public async Task bsHideModal() => await OnHiding.InvokeAsync();
    [JSInvokable] public async Task bsHiddenModal() => await OnHidden.InvokeAsync();
    [JSInvokable] public async Task bsHidePreventedModal() => await OnHidePrevented.InvokeAsync();

    #endregion Methods

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Title in modal header.
    /// </summary>
    [Parameter]
    public string Title
    {
        get => title;
        set => title = value;
    }

    /// <summary>
    /// Header template.
    /// </summary>
    [Parameter] public RenderFragment HeaderTemplate { get; set; } = default!;

    /// <summary>
    /// Message in modal body.
    /// </summary>
    [Parameter]
    public string Message
    {
        get => message;
        set => message = value;
    }

    /// <summary>
    /// Body template.
    /// </summary>
    [Parameter] public RenderFragment BodyTemplate { get; set; } = default!;

    /// <summary>
    /// Footer template.
    /// </summary>
    [Parameter] public RenderFragment FooterTemplate { get; set; } = default!;

    /// <summary>
    /// Size of the modal. Default is <see cref="ModalSize.Regular"/>.
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
    /// Fullscreen behavior of the modal. Default is <see cref="ModalFullscreen.Disabled"/>.
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
    /// Indicates whether the modal shows close button in header.
    /// Default value is true.
    /// </summary>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Indicates whether the modal closes when escape key is pressed.
    /// Default value is true.
    /// </summary>
    [Parameter] public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Indicates whether the modal uses a static backdrop.
    /// Default value is false.
    /// </summary>
    [Parameter] public bool UseStaticBackdrop { get; set; } = false;

    /// <summary>
    /// Allows modal body scroll.
    /// </summary>
    [Parameter] public bool IsScrollable { get; set; }

    /// <summary>
    /// Shows the modal vertically in the center.
    /// </summary>
    [Parameter] public bool IsVerticallyCentered { get; set; }

    /// <summary>
    /// Additional CSS class for the dialog (div.modal-dialog element).
    /// </summary>
    [Parameter] public string DialogCssClass { get; set; } = default!;

    /// <summary>
    /// Additional header CSS class.
    /// </summary>
    [Parameter] public string HeaderCssClass { get; set; } = default!;

    /// <summary>
    /// Additional body CSS class.
    /// </summary>
    [Parameter] public string BodyCssClass { get; set; } = default!;

    /// <summary>
    /// Footer css class.
    /// </summary>
    [Parameter] public string FooterCssClass { get; set; } = default!;

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter] public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter] public EventCallback OnShown { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [Parameter] public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete).
    /// </summary>
    [Parameter] public EventCallback OnHidden { get; set; }

    /// <summary>
    /// This event is fired when the modal is shown, its backdrop is static and a click outside the modal or an escape key press is performed with the keyboard option or data-bs-keyboard set to false.
    /// </summary>
    [Parameter] public EventCallback OnHidePrevented { get; set; }

    /// <summary>
    /// Gets or sets the tab index.
    /// </summary>
    [Parameter] public int TabIndex { get; set; } = -1;

    #endregion Properties

    #region Services

    [Inject] public ModalService ModalService { get; set; } = default!;

    #endregion
}
