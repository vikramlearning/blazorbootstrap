﻿using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBootstrap
{
    public partial class Offcanvas : BaseComponent, IDisposable
    {
        #region Members

        private DotNetObjectReference<Offcanvas> objRef;

        #endregion Members

        #region Methods

        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Offcanvas());
            builder.Append(BootstrapClassProvider.Offcanvas(Placement));

            base.BuildClasses(builder);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            objRef ??= DotNetObjectReference.Create(this);
            base.OnAfterRender(firstRender);
        }

        /// <summary>
        /// Shows an offcanvas.
        /// </summary>
        public async Task ShowAsync()
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.offcanvas.show", ElementId, objRef);
        }

        /// <summary>
        /// Hides an offcanvas.
        /// </summary>
        public async Task HideAsync()
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.offcanvas.hide", ElementId);
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
        /// Specifies the placement.
        /// By default, offcanvas is placed on the right of the viewport.
        /// </summary>
        [Parameter] public Placement Placement { get; set; } = Placement.End;

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
        /// Specifies the content to be rendered inside this.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion Properties
    }
}
