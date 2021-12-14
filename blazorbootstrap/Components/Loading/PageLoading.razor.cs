using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap
{
    public partial class PageLoading : BaseComponent
    {
        #region Members

        [Inject] PageLoadingService PageLoadingService { get; set; }

        private bool showBackdrop;

        #endregion Members

        #region Methods

        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Modal());
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
            PageLoadingService.OnShow += OnShow;
            PageLoadingService.OnHide += OnHide;
        }

        private void OnShow()
        {
            showBackdrop = true;

            this.DirtyClasses();
            this.DirtyStyles();

            StateHasChanged();
        }

        private void OnHide()
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
                PageLoadingService.OnShow -= OnShow;
                PageLoadingService.OnHide -= OnHide;
            }

            base.Dispose(disposing);
        }

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// Shows the modal vertically in the center.
        /// </summary>
        [Parameter] public bool IsVerticallyCentered { get; set; }

        /// <summary>
        /// Specifies the content to be rendered inside this.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion Properties
    }
}
