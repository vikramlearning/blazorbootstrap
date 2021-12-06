using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public partial class Toast : BaseComponent, IDisposable
    {
        #region Members

        private DotNetObjectReference<Toast> objRef;

        #endregion Members

        #region Methods

        protected override void OnAfterRender(bool firstRender)
        {
            objRef ??= DotNetObjectReference.Create(this);
            base.OnAfterRender(firstRender);
        }

        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Toast());
            builder.Append(BootstrapClassProvider.Fade());
            builder.Append(BootstrapClassProvider.Show());

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

        /// <summary>
        /// Reveals an element’s toast.
        /// </summary>
        public async Task ShowAsync()
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.show", ElementId, objRef);
        }

        /// <summary>
        /// Hides an element’s toast.
        /// </summary>
        public async Task HideAsync()
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.toasts.hide", ElementId);
        }

        [JSInvokable] public async Task bsShowToast() => await Showing.InvokeAsync();
        [JSInvokable] public async Task bsShownToast() => await Shown.InvokeAsync();
        [JSInvokable] public async Task bsHideToast() => await Hiding.InvokeAsync();
        [JSInvokable] public async Task bsHiddenToast() => await Hidden.InvokeAsync(this.ToastMessage.Id);

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        [Parameter] public ToastMessage ToastMessage { get; set; }

        /// <summary>
        /// This event fires immediately when the show instance method is called.
        /// </summary>
        [Parameter] public EventCallback Showing { get; set; }

        /// <summary>
        /// This event is fired when the toast has been made visible to the user.
        /// </summary>
        [Parameter] public EventCallback Shown { get; set; }

        /// <summary>
        /// This event is fired immediately when the hide instance method has been called.
        /// </summary>
        [Parameter] public EventCallback Hiding { get; set; }

        /// <summary>
        /// This event is fired when the toast has finished being hidden from the user.
        /// </summary>
        [Parameter] public EventCallback<Guid> Hidden { get; set; }

        #endregion Properties
    }
}
