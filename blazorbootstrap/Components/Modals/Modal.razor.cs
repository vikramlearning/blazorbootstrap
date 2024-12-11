using System.Xml.Linq;

namespace BlazorBootstrap;

/// <summary>
/// Use Bootstrap’s JavaScript modal plugin to add dialogs to your site for lightboxes, user notifications, or completely custom content. <br/>
/// For more information, visit the <see href="https://getbootstrap.com/docs/5.0/components/modal/">Bootstrap Modal</see> documentation.
/// </summary>
public partial class Modal : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private Type? childComponent = default!;

    private ButtonColor footerButtonColor = ButtonColor.Secondary;

    private string footerButtonCssClass = string.Empty;

    private string footerButtonText = string.Empty;

    private bool isVisible;

    private DotNetObjectReference<Modal> objRef = default!;

    private Dictionary<string, object> modalParameters = default!;

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
                    await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.modal.dispose", Id);
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

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.modal.initialize", Id, UseStaticBackdrop, CloseOnEscape, objRef);

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        objRef ??= DotNetObjectReference.Create(this);

        if (ModalService is not null && IsServiceModal)
            ModalService.OnShow += OnShowAsync;

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Invoked when the modal is hidden from the user (will wait for CSS transitions to complete).
    /// </summary> 
    [JSInvokable("bsHiddenModal")]
    public async Task BsHiddenModal()
    {
        await OnHidden.InvokeAsync();

        if (ModalService is not null && IsServiceModal)
            ModalService.OnClose();
    }

    /// <summary>
    /// Invoked immediately when the hide method has been called.
    /// </summary>
    [JSInvokable("bsHideModal")]
    public Task BsHideModal() => OnHiding.InvokeAsync();

    /// <summary>
    /// Invoked when the modal is shown, its backdrop is static and a click outside the modal or an escape key press is performed
    /// with the keyboard option or data-bs-keyboard set to <see langword="false" />.
    /// </summary>
    [JSInvokable("bsHidePreventedModal")]
    public Task BsHidePreventedModal() => OnHidePrevented.InvokeAsync();

    [JSInvokable("bsShowModal")]
    public Task BsShowModal() => OnShowing.InvokeAsync();

    [JSInvokable("bsShownModal")]
    public Task BsShownModal() => OnShown.InvokeAsync();

    /// <summary>
    /// Hides a modal.
    /// </summary>
    public ValueTask HideAsync()
    {
        isVisible = false;
        return JsRuntime.InvokeVoidAsync("window.blazorBootstrap.modal.hide", Id);
    }

    /// <summary>
    /// Opens a modal.
    /// </summary>
    public Task ShowAsync() => ShowAsync(null, null, null, null);

    /// <summary>
    /// Opens a modal.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    public Task ShowAsync<T>(string title, string? message = null, Dictionary<string, object>? parameters = null) => ShowAsync(title, message, typeof(T), parameters);

    private Task OnShowAsync(ModalOption modalOption)
    {
        ArgumentNullException.ThrowIfNull(nameof(modalOption)); 

        ModalType = modalOption.Type;

        Size = modalOption.Size;

        IsVerticallyCentered = modalOption.IsVerticallyCentered;

        showFooterButton = modalOption.ShowFooterButton;

        if (showFooterButton)
        {
            footerButtonColor = modalOption.FooterButtonColor;
            footerButtonCssClass = modalOption.FooterButtonCssClass;
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

        this.modalParameters = parameters!;

        await InvokeAsync(StateHasChanged);

        await JsRuntime.InvokeVoidAsync("window.blazorBootstrap.modal.show", Id);
    }


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case var _ when String.Equals(parameter.Name, nameof(BodyCssClass), StringComparison.OrdinalIgnoreCase): BodyCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(BodyTemplate), StringComparison.OrdinalIgnoreCase): BodyTemplate = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(CloseIconColor), StringComparison.OrdinalIgnoreCase): CloseIconColor = (IconColor)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(CloseOnEscape), StringComparison.OrdinalIgnoreCase): CloseOnEscape = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(DialogCssClass), StringComparison.OrdinalIgnoreCase): DialogCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FooterCssClass), StringComparison.OrdinalIgnoreCase): FooterCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FooterTemplate), StringComparison.OrdinalIgnoreCase): FooterTemplate = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Fullscreen), StringComparison.OrdinalIgnoreCase): Fullscreen = (ModalFullscreen)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(HeaderCssClass), StringComparison.OrdinalIgnoreCase): HeaderCssClass = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(HeaderTemplate), StringComparison.OrdinalIgnoreCase): HeaderTemplate = (RenderFragment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IsScrollable), StringComparison.OrdinalIgnoreCase): IsScrollable = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IsServiceModal), StringComparison.OrdinalIgnoreCase): IsServiceModal = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(IsVerticallyCentered), StringComparison.OrdinalIgnoreCase): IsVerticallyCentered = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Message), StringComparison.OrdinalIgnoreCase): Message = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(ShowCloseButton), StringComparison.OrdinalIgnoreCase): ShowCloseButton = (bool)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Size), StringComparison.OrdinalIgnoreCase): Size = (ModalSize)parameter.Value; break;
                
                case var _ when String.Equals(parameter.Name, nameof(TabIndex), StringComparison.OrdinalIgnoreCase): TabIndex = (int)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Title), StringComparison.OrdinalIgnoreCase): Title = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(UseStaticBackdrop), StringComparison.OrdinalIgnoreCase): UseStaticBackdrop = (bool)parameter.Value; break;
                
                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers
     
    /// <summary>
    /// Gets or sets the body CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string BodyCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the body template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? BodyTemplate { get; set; } 

    /// <summary>
    /// Gets or sets the close icon color.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconColor.None" />.
    /// </remarks>
    [Parameter] public IconColor CloseIconColor { get; set; } = IconColor.None;

    /// <summary>
    /// Indicates whether the modal closes when escape key is pressed.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter] public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Gets or sets the modal dialog (div.modal-dialog) CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string DialogCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the footer CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string FooterCssClass { get; set; } = default!;

    /// <summary>
    /// Gets or sets the footer template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? FooterTemplate { get; set; } 

    /// <summary>
    /// Gets or sets the fullscreen behavior of the modal.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ModalFullscreen.Disabled" />.
    /// </remarks>
    [Parameter] public ModalFullscreen Fullscreen { get; set; } = BlazorBootstrap.ModalFullscreen.Disabled;

    /// <summary>
    /// Gets or sets the header CSS class.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string HeaderCssClass { get; set; } = default!;

    // Modal close "X" button with bootstrap 5.3.3 - https://github.com/vikramlearning/blazorbootstrap/issues/714
    // Review this fix after bootstrap 5.3.4 or 5.4 release. Ref: https://github.com/twbs/bootstrap/issues/39798
    private string HeaderCssClassInternal => $"justify-content-between {EnumExtensions.ModalHeaderColorClassMap[ModalType]}".Trim();

    /// <summary>
    /// Gets or sets the header template.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// If <see langword="true" />, scroll will be enabled on the modal body.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool IsScrollable { get; set; }

    /// <summary>
    /// Indicates whether the modal is related to a modal service or not.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool IsServiceModal { get; set; }

    /// <summary>
    /// If <see langword="true" />, shows the modal vertically in the center.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool IsVerticallyCentered { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter] public string Message { get; set; } = default!; 

    [Inject] private ModalService? ModalService { get; set; }
      
    /// <summary>
    /// Gets or sets the modal type.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ModalType.None" />.
    /// </remarks>
    [Parameter] public ModalType ModalType { get; set; } = ModalType.None;

    /// <summary>
    /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter] public EventCallback OnHidden { get; set; }

    /// <summary>
    /// This event is fired when the modal is shown, its backdrop is static and a click outside the modal or an escape key
    /// press is performed with the keyboard option or data-bs-keyboard set to <see langword="false" />.
    /// </summary>
    [Parameter] public EventCallback OnHidePrevented { get; set; }

    /// <summary>
    /// This event is fired immediately when the hide method has been called.
    /// </summary>
    [Parameter] public EventCallback OnHiding { get; set; }

    /// <summary>
    /// This event fires immediately when the show instance method is called.
    /// </summary>
    [Parameter] public EventCallback OnShowing { get; set; }

    /// <summary>
    /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to
    /// complete).
    /// </summary>
    [Parameter] public EventCallback OnShown { get; set; }

    private string Scrollable => IsScrollable ? "modal-dialog-scrollable" : "";

    /// <summary>
    /// If <see langword="true" />, close button will be visible in the modal header.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="true" />.
    /// </remarks>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the modal size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ModalSize.Regular" />.
    /// </remarks>
    [Parameter] public ModalSize Size { get; set; } = BlazorBootstrap.ModalSize.Regular;

    /// <summary>
    /// Gets or sets the modal tab index.
    /// </summary>
    /// <remarks>
    /// Default value is -1.
    /// </remarks>
    [Parameter] public int TabIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets the modal header title.
    /// </summary>
    [Parameter] public string Title { get; set; } = default!;

    /// <summary>
    /// Indicates whether the modal uses a static backdrop.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false" />.
    /// </remarks>
    [Parameter] public bool UseStaticBackdrop { get; set; }

    private string VerticallyCentered => IsVerticallyCentered ? "modal-dialog-centered" : "";

    /// <summary>
    /// Dependency injected Javascript Runtime
    /// </summary>
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    #endregion
}
