using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public partial class Offcanvas : IDisposable
    {
        [Inject] OffcanvasService OffcanvasService { get; set; }
        [Inject] IJSRuntime JS { get; set; }

        public ElementReference ElementRef { get; set; }
        [Parameter] public string ElementId { get; set; }
        [Parameter] public Placement Placement { get; set; } = Placement.Right;

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

        private DotNetObjectReference<Offcanvas> objRef;

        private string offcanvasPlacement = "offcanvas-end"; // default
        private bool isShown = false; // default

        protected override void OnInitialized()
        {
            OffcanvasService.OnShow += Show;
            OffcanvasService.OnHide += Hide;

            base.OnInitialized();
        }

        private void Show(string elementId)
        {
            isShown = true;

            offcanvasPlacement = Placement switch
            {
                Placement.Right => "offcanvas-end",
                Placement.Top => "offcanvas-top",
                Placement.Bottom => "offcanvas-bottom",
                _ => "offcanvas-start",
            };

            Task.Run(async () =>
            {
                objRef ??= DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("blazorBootstrap.offcanvas.show", elementId, objRef);
            });
        }

        private void Hide(string elementId)
        {
            Task.Run(async () =>
            {
                await JS.InvokeVoidAsync("blazorBootstrap.offcanvas.hide", elementId);
                //isShown = false;
            });
        }

        [JSInvokable] public async Task bsShowOffcanvas() => await Showing.InvokeAsync();
        [JSInvokable] public async Task bsShownOffcanvas() => await Shown.InvokeAsync();
        [JSInvokable] public async Task bsHideOffcanvas() => await Hiding.InvokeAsync();
        [JSInvokable] public async Task bsHiddenOffcanvas() => await Hidden.InvokeAsync();

        #region Implementations

        public void Dispose()
        {
            objRef?.Dispose();
        }

        #endregion
    }

    public enum Placement
    {
        Left = 1,
        Right,
        Top,
        Bottom,
    }
}
