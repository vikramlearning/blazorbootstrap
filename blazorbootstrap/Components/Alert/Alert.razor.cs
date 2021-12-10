using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public partial class Alert
    {
        #region Members

        private DotNetObjectReference<Alert> objRef;

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Alert());
            builder.Append(BootstrapClassProvider.AlertColor(Color), Color != AlertColor.None);
            builder.Append(BootstrapClassProvider.AlertDismisable(), Dismisable);

            base.BuildClasses(builder);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.alert.initialize", ElementId); });
        }

        protected override void OnAfterRender(bool firstRender)
        {
            objRef ??= DotNetObjectReference.Create(this);
            base.OnAfterRender(firstRender);
        }

        /// <summary>
        /// Closes an alert by removing it from the DOM.
        /// </summary>
        public async Task CloseAsync()
        {
            await JS.InvokeVoidAsync("window.blazorBootstrap.alert.close", ElementId);
        }

        [JSInvokable] public async Task bsCloseAlert() => await OnClose.InvokeAsync();
        [JSInvokable] public async Task bsClosedAlert() => await OnClosed.InvokeAsync();

        #endregion

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// Gets or sets the alert color.
        /// </summary>
        [Parameter]
        public AlertColor Color { get; set; } = AlertColor.None;

        /// <summary>
        /// Enables the alert to be closed by placing the padding for close button.
        /// </summary>
        [Parameter] public bool Dismisable { get; set; }

        /// <summary>
        /// Fires immediately when the close instance method is called.
        /// </summary>
        [Parameter] public EventCallback OnClose { get; set; }

        /// <summary>
        /// Fired when the alert has been closed and CSS transitions have completed.
        /// </summary>
        [Parameter] public EventCallback OnClosed { get; set; }

        /// <summary>
        /// Specifies the content to be rendered inside this <see cref="Alert"/>.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion

        // TODO: review below
        // https://getbootstrap.com/docs/5.1/components/alerts/#live-example
        // https://getbootstrap.com/docs/5.1/components/alerts/#additional-content
    }
}
