using BlazorBootstrap.Extensions;
using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBootstrap
{
    public partial class Tooltip : BaseComponent
    {
        #region Members

        private DotNetObjectReference<Tooltip> objRef;

        private string placement => TooltipPlacement.ToTooltipPlacementName();

        #endregion Members

        #region Methods

        protected override void OnAfterRender(bool firstRender)
        {
            objRef ??= DotNetObjectReference.Create(this);
            base.OnAfterRender(firstRender);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            ExecuteAfterRender(async () => { await JS.InvokeVoidAsync("window.blazorBootstrap.tooltip.initialize", ElementId); });
        }

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// Specifies the content to be rendered inside this.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Displays informative text when users hover, focus, or tap an element.
        /// </summary>
        [Parameter] public string TooltipTitle { get; set; }

        /// <summary>
        /// Tooltip placement
        /// </summary>
        [Parameter] public BlazorBootstrap.TooltipPlacement TooltipPlacement { get; set; } = TooltipPlacement.Top;

        #endregion Properties
    }
}
