using BlazorBootstrap.Utilities;
using BlazorBootstrap.Extensions;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public partial class ConfirmDialog : BaseComponent
    {
        #region Members

        private string scrollable => IsScrollable ? "modal-dialog-scrollable" : "";
        private string titleBackgroundColor => TitleBackgroundColor.ToBackgroundAndTextClass();
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

        /// <summary>
        /// Shows confirm dialog.
        /// </summary>
        public void Show()
        {
            showBackdrop = true;

            this.DirtyClasses();
            this.DirtyStyles();

            StateHasChanged();
        }

        /// <summary>
        /// Hides confirm dialog.
        /// </summary>
        public void Hide()
        {
            showBackdrop = false;

            this.DirtyClasses();
            this.DirtyStyles();

            StateHasChanged();
        }

        private async Task YesAsync()
        {
            Hide();
            await OnYes.InvokeAsync();
        }

        private async Task NoAsync()
        {
            Hide();
            await OnNo.InvokeAsync();
        }

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// Allows confirm dialog body to be scrollable.
        /// </summary>
        [Parameter] public bool IsScrollable { get; set; }

        /// <summary>
        /// Shows the confirm dialog vertically in the center of the page.
        /// </summary>
        [Parameter] public bool IsVerticallyCentered { get; set; }

        /// <summary>
        /// Gets or sets the title of the confirm dialog.
        /// </summary>
        [Parameter] public string Title { get; set; }

        /// <summary>
        /// Gets or sets the background color of the confirm dialog title. <see cref="BackgroundColor"/>
        /// </summary>
        [Parameter] public BackgroundColor TitleBackgroundColor { get; set; } = BackgroundColor.None;

        /// <summary>
        /// Adds a dismissable close button to the confirm dialog.
        /// </summary>
        [Parameter] public bool Dismissable { get; set; } = true;

        /// <summary>
        /// Gets or sets the Message1 of the confirmation dialog.
        /// </summary>
        [Parameter] public string Message1 { get; set; }

        /// <summary>
        /// Gets or sets the Message2 of the confirmation dialog. This is optional.
        /// </summary>
        [Parameter] public string Message2 { get; set; }

        /// <summary>
        /// Gets or sets the 'Yes' button text.
        /// </summary>
        [Parameter] public string YesButtonText { get; set; } = "Yes";

        /// <summary>
        /// Gets or sets the 'Yes' button color. <see cref="ButtonColor"/>
        /// </summary>
        [Parameter] public ButtonColor YesButtonColor { get; set; } = ButtonColor.Primary;

        /// <summary>
        /// Triggers once the user confirms 'Yes'.
        /// </summary>
        [Parameter] public EventCallback OnYes { get; set; }

        /// <summary>
        /// Gets or sets the 'No' button text.
        /// </summary>
        [Parameter] public string NoButtonText { get; set; } = "No";

        /// <summary>
        /// Gets or sets the 'No' button color. <see cref="ButtonColor"/>
        /// </summary>
        [Parameter] public ButtonColor NoButtonColor { get; set; } = ButtonColor.Secondary;

        /// <summary>
        /// Triggers once the user confirms 'No'.
        /// </summary>
        [Parameter] public EventCallback OnNo { get; set; }

        #endregion Properties
    }
}
