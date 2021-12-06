using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public partial class Toasts : BaseComponent, IDisposable
    {
        #region Members

        //private DotNetObjectReference<Toasts> objRef;

        //private List<Toast> ToastContainer;

        #endregion Members

        #region Methods

        protected override void OnAfterRender(bool firstRender)
        {
            //objRef ??= DotNetObjectReference.Create(this);
            base.OnAfterRender(firstRender);
        }

        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.ToastContainer());
            builder.Append(BootstrapClassProvider.PositionAbsolute());
            builder.Append(BootstrapClassProvider.ToToastsPlacement(Placement));

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

        private void OnToastHiddenAsync(Guid toastId)
        {
            if(Messages!= null && Messages.Any())
            {
                var message = Messages.FirstOrDefault(x => x.Id == toastId);
                Messages.Remove(message);
            }
        }

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        [Parameter] public List<ToastMessage> Messages { get; set; }

        [Parameter] public ToastsPlacement Placement { get; set; } = ToastsPlacement.TopRight;

        #endregion Properties
    }
}
