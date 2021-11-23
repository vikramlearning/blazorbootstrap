using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public partial class Modal : BaseComponent
    {
        #region Members

        private DotNetObjectReference<Modal> objRef;

        #endregion Members

        #region Methods

        protected override void OnAfterRender(bool firstRender)
        {
            objRef ??= DotNetObjectReference.Create(this);
            base.OnAfterRender(firstRender);
        }

        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Modal());
            builder.Append(BootstrapClassProvider.ModalFade());

            if (IsScrollable)
            {

            }

            base.BuildClasses(builder);
        }

        protected override void BuildStyles(StyleBuilder builder)
        {
            base.BuildStyles(builder);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        public async Task ShowAsync()
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.modal.show", ElementId, objRef);
        }

        public async Task HideAsync()
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.modal.hide", ElementId);
        }

        [JSInvokable] public async Task bsShowModal() => await Showing.InvokeAsync();
        [JSInvokable] public async Task bsShownModal() => await Shown.InvokeAsync();
        [JSInvokable] public async Task bsHideModal() => await Hiding.InvokeAsync();
        [JSInvokable] public async Task bsHiddenModal() => await Hidden.InvokeAsync();
        [JSInvokable] public async Task bsHidePreventedModal() => await HidePrevented.InvokeAsync();

        public void Dispose()
        {
            objRef?.Dispose();
        }

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// Allows modal body scroll.
        /// </summary>
        [Parameter] public bool IsScrollable { get; set; }

        /// <summary>
        /// Shows the modal vertically in the center.
        /// </summary>
        [Parameter] public bool IsVerticallyCentered { get; set; }

        /// <summary>
        /// This event fires immediately when the show instance method is called.
        /// </summary>
        [Parameter] public EventCallback Showing { get; set; }

        /// <summary>
        /// This event is fired when an offcanvas element has been made visible to the user (will wait for CSS transitions to complete).
        /// </summary>
        [Parameter] public EventCallback Shown { get; set; }

        /// <summary>
        /// This event is fired immediately when the hide method has been called.
        /// </summary>
        [Parameter] public EventCallback Hiding { get; set; }

        /// <summary>
        /// This event is fired when an offcanvas element has been hidden from the user (will wait for CSS transitions to complete).
        /// </summary>
        [Parameter] public EventCallback Hidden { get; set; }

        /// <summary>
        /// This event is fired when the modal is shown, its backdrop is static and a click outside the modal or an escape key press is performed with the keyboard option or data-bs-keyboard set to false.
        /// </summary>
        [Parameter] public EventCallback HidePrevented { get; set; }

        /// <summary>
        /// Specifies the content to be rendered inside this.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion Properties
    }
}
