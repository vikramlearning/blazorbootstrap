﻿using BlazorBootstrap.Utilities;
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

        private void OnToastShownAsync(Guid toastId)
        {
            if (Messages != null && Messages.Any() && Messages.Count >= StackLength)
            {
                Messages.RemoveRange(0, Messages.Count - StackLength);
            }
        }

        private void OnToastHiddenAsync(Guid toastId)
        {
            if (Messages != null && Messages.Any())
            {
                var message = Messages.FirstOrDefault(x => x.Id == toastId);
                if (Messages.Remove(message))
                {
                    // toast message removed successfully.
                }
            }
        }

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// List of all the toasts.
        /// </summary>
        [Parameter]
        public List<ToastMessage> Messages { get; set; }

        /// <summary>
        /// Auto hide the toast. Default is true.
        /// </summary>
        [Parameter] public bool AutoHide { get; set; } = true;

        /// <summary>
        /// Delay hiding the toast (ms). Default is 5000 milli seconds.
        /// </summary>
        [Parameter] public int Delay { get; set; } = 5000;

        /// <summary>
        /// Specifies the toast container maximum capacity. Default is 5.
        /// </summary>
        [Parameter] public int StackLength { get; set; } = 5;

        /// <summary>
        /// Specifies the toasts placement. Default is top right.
        /// </summary>
        [Parameter] public ToastsPlacement Placement { get; set; } = ToastsPlacement.TopRight;

        #endregion Properties
    }
}
