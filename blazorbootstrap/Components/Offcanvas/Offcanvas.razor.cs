using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public partial class Offcanvas : IObserver<ElementReference>, IDisposable
    {
        [Inject] OffcanvasService OffcanvasService { get; set; }
        [Inject] IJSRuntime JS { get; set; }

        public ElementReference ElementRef { get; set; }
        [Parameter] public string ElementId { get; set; }
        [Parameter] public Placement Placement { get; set; } = Placement.Right;
        [Parameter] public EventCallback OnOffcanvasShow { get; set; }
        [Parameter] public EventCallback OnOffcanvasShown { get; set; }
        [Parameter] public EventCallback OnOffcanvasHide { get; set; }
        [Parameter] public EventCallback OnOffcanvasHidden { get; set; }

        private DotNetObjectReference<Offcanvas> objRef;

        private string offcanvasPlacement = "offcanvas-end"; // default

        protected override void OnInitialized()
        {
            OffcanvasService.OnShow += Show;
            OffcanvasService.OnHide += Hide;

            base.OnInitialized();
        }

        private void Show(string elementId)
        {
            offcanvasPlacement = Placement switch
            {
                Placement.Right => "offcanvas-end",
                Placement.Top => "offcanvas-top",
                Placement.Bottom => "offcanvas-bottom",
                _ => "offcanvas-start",
            };

            Task.Run(async () =>
            {
                objRef = objRef ?? DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("blazorBootstrap.offcanvas.show", elementId, objRef);
            });
        }

        private void Hide(string elementId)
        {
            Task.Run(async () => { await JS.InvokeVoidAsync("blazorBootstrap.offcanvas.hide", elementId); });
        }

        [JSInvokable] public async Task bsShowOffcanvas() => await OnOffcanvasShow.InvokeAsync();
        [JSInvokable] public async Task bsShownOffcanvas() => await OnOffcanvasShown.InvokeAsync();
        [JSInvokable] public async Task bsHideOffcanvas() => await OnOffcanvasHide.InvokeAsync();
        [JSInvokable] public async Task bsHiddenOffcanvas() => await OnOffcanvasHidden.InvokeAsync();

        #region Implementations

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ElementReference value)
        {
            throw new NotImplementedException();
        }

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
