namespace BlazorBootstrap
{
    public partial class Offcanvas : BaseComponent
    {
        #region Members

        private string title = default!;

        private Type? childComponent;

        private Dictionary<string, object> parameters = default!;

        private Placement placement = Placement.End;

        private OffcanvasSize size = OffcanvasSize.Regular;

        private DotNetObjectReference<Offcanvas> objRef = default!;

        #endregion Members

        #region Methods

        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Offcanvas());
            builder.Append(BootstrapClassProvider.Offcanvas(Placement));
            builder.Append(BootstrapClassProvider.ToOffcanvasSize(Size));

            base.BuildClasses(builder);
        }

        protected override async Task OnInitializedAsync()
        {
            this.title = Title;
            objRef ??= DotNetObjectReference.Create(this);
            await base.OnInitializedAsync();

            ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.offcanvas.initialize", ElementId, UseStaticBackdrop, CloseOnEscape, IsScrollable, objRef); });
        }

        /// <summary>
        /// Shows an offcanvas.
        /// </summary>
        public async Task ShowAsync()
        {
            await ShowAsync(title: null, type: null, parameters: null);
        }

        /// <summary>
        /// Opens a offcanvas.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="parameters"></param>
        public async Task ShowAsync<T>(string title, Dictionary<string, object> parameters = null)
        {
            await ShowAsync(title: title, type: typeof(T), parameters: parameters);
        }

        private async Task ShowAsync(string title, Type? type, Dictionary<string, object> parameters)
        {
            if (!string.IsNullOrWhiteSpace(title))
                this.title = title;

            this.childComponent = type;
            this.parameters = parameters;
            await JS.InvokeVoidAsync("window.blazorBootstrap.offcanvas.show", ElementId);
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Hides an offcanvas.
        /// </summary>
        public async Task HideAsync()
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.offcanvas.hide", ElementId);
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                await JS.InvokeVoidAsync("window.blazorBootstrap.offcanvas.dispose", ElementId);
                objRef?.Dispose();
            }

            await base.DisposeAsync(disposing);
        }

        [JSInvokable] public async Task bsShowOffcanvas() => await OnShowing.InvokeAsync();
        [JSInvokable] public async Task bsShownOffcanvas() => await OnShown.InvokeAsync();
        [JSInvokable] public async Task bsHideOffcanvas() => await OnHiding.InvokeAsync();
        [JSInvokable] public async Task bsHiddenOffcanvas() => await OnHidden.InvokeAsync();

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// Additional body CSS class.
        /// </summary>
        [Parameter] public string BodyCssClass { get; set; } = default!;

        /// <summary>
        /// Body content.
        /// </summary>
        [Parameter] public RenderFragment BodyTemplate { get; set; } = default!;

        /// <summary>
        /// Indicates whether the offcanvas closes when escape key is pressed.
        /// Default value is true.
        /// </summary>
        [Parameter] public bool CloseOnEscape { get; set; } = true;

        /// <summary>
        /// Additional footer CSS class.
        /// </summary>
        [Parameter] public string FooterCssClass { get; set; } = default!;

        /// <summary>
        /// Footer content.
        /// </summary>
        [Parameter] public RenderFragment FooterTemplate { get; set; } = default!;

        /// <summary>
        /// Additional header CSS class.
        /// </summary>
        [Parameter] public string HeaderCssClass { get; set; } = default!;

        /// <summary>
        /// Content for the header.
        /// </summary>
        [Parameter] public RenderFragment HeaderTemplate { get; set; } = default!;

        /// <summary>
        /// Indicates whether body (page) scrolling is allowed while offcanvas is open.
        /// Default value is false.
        /// </summary>
        [Parameter] public bool IsScrollable { get; set; }

        /// <summary>
        /// Specifies the placement.
        /// By default, offcanvas is placed on the right of the viewport.
        /// </summary>
        [Parameter]
        public Placement Placement
        {
            get => placement;
            set
            {
                placement = value;
                DirtyClasses();
            }
        }

        /// <summary>
        /// Indicates whether the modal shows close button in header.
        /// Default value is true.
        /// Use <see cref="CloseButtonIcon"/> to change shape of the button.
        /// </summary>
        [Parameter] public bool ShowCloseButton { get; set; } = true;

        /// <summary>
        /// Size of the offcanvas. Default is <see cref="OffcanvasSize.Regular"/>.
        /// </summary>
        [Parameter]
        public OffcanvasSize Size
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
        [Parameter] public int TabIndex { get; set; } = -1;

        /// <summary>
        /// Text for the title in header.
        /// </summary>
        [Parameter] public string Title { get; set; } = default!;

        [Obsolete("Use `UseStaticBackdrop` parameter.")]
        /// <summary>
        /// Indicates whether to apply a backdrop on body while offcanvas is open.
        /// Default value is true.
        /// </summary>
        [Parameter] public bool UseBackdrop { get; set; } = true;

        /// <summary>
        /// When `UseStaticBackdrop` is set to true, the offcanvas will not close when clicking outside of it.
        /// </summary>
        [Parameter] public bool UseStaticBackdrop { get; set; }

        /// <summary>
        /// This event fires immediately when the show instance method is called.
        /// </summary>
        [Parameter] public EventCallback OnShowing { get; set; }

        /// <summary>
        /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete).
        /// </summary>
        [Parameter] public EventCallback OnShown { get; set; }

        /// <summary>
        /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete).
        /// </summary>
        [Parameter] public EventCallback OnHidden { get; set; }

        /// <summary>
        /// This event is fired immediately when the hide method has been called.
        /// </summary>
        [Parameter] public EventCallback OnHiding { get; set; }

        #endregion Properties
    }
}
