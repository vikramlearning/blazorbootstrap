using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorBootstrap
{
    public partial class Toasts : BaseComponent, IDisposable
    {
        #region Members

        #endregion Members

        #region Methods

        protected override void OnAfterRender(bool firstRender)
        {
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
            if (Messages != null && Messages.Any())
            {
                var message = Messages.FirstOrDefault(x => x.Id == toastId);
                if (Messages.Remove(message))
                {
                    // message removed.
                }
            }
        }

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// Holds all the toasts.
        /// </summary>
        [Parameter] public List<ToastMessage> Messages { get; set; }

        [Parameter] public ToastsPlacement Placement { get; set; } = ToastsPlacement.TopRight;

        #endregion Properties
    }
}
