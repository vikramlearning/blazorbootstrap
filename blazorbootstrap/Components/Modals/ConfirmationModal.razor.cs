using BlazorBootstrap.Utilities;
using BlazorBootstrap.Extensions;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap
{
    public partial class ConfirmationModal : BaseComponent
    {
        #region Members

        private string verticallyCentered => IsVerticallyCentered ? "modal-dialog-centered" : "";
        private string yesButtonColor => YesButtonColor.ToButtonClass();
        private string noButtonColor => NoButtonColor.ToButtonClass();

        private bool showBackdrop;

        #endregion Members

        #region Methods

        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Modal());
            builder.Append(BootstrapClassProvider.ConfirmationModal());
            builder.Append(BootstrapClassProvider.ModalFade());
            builder.Append(BootstrapClassProvider.Show(), showBackdrop);

            base.BuildClasses(builder);
        }

        protected override void BuildStyles(StyleBuilder builder)
        {
            builder.Append("display:block", showBackdrop);
            builder.Append("display:none", !showBackdrop);

            base.BuildStyles(builder);
        }

        protected override void OnInitialized()
        {
        }

        public void Show()
        {
            showBackdrop = true;

            this.DirtyClasses();
            this.DirtyStyles();

            StateHasChanged();
        }

        public void Hide()
        {
            showBackdrop = false;

            this.DirtyClasses();
            this.DirtyStyles();

            StateHasChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: update this
            }

            base.Dispose(disposing);
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
        /// Shows the preload vertically in the center of the page.
        /// </summary>
        [Parameter] public bool IsVerticallyCentered { get; set; } = true;

        [Parameter] public string Title { get; set; }
        [Parameter] public string TitleBackgroundColor { get; set; }
        [Parameter] public bool Dismissable { get; set; } = true;

        [Parameter] public string Message1 { get; set; }
        [Parameter] public string Message2 { get; set; }

        [Parameter] public string YesButtonText { get; set; } = "Yes";
        [Parameter] public ButtonColor YesButtonColor { get; set; } = ButtonColor.Primary;
        [Parameter] public string OnYes { get; set; }

        [Parameter] public string NoButtonText { get; set; } = "No";
        [Parameter] public ButtonColor NoButtonColor { get; set; } = ButtonColor.Secondary;
        [Parameter] public string OnNo { get; set; }

        #endregion Properties
    }
}
