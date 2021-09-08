using BlazorBootstrap.States;
using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap
{
    public partial class Alert
    {
        #region Members

        /// <summary>
        /// Holds the state of the <see cref="Alert"/> component.
        /// </summary>
        private AlertState state = new()
        {
            Color = Color.None,
        };

        /// <summary>
        /// Flag that indicates if <see cref="Alert"/> contains the <see cref="AlertMessage"/> component.
        /// </summary>
        private bool hasMessage;

        /// <summary>
        /// Flag that indicates if <see cref="Alert"/> contains the <see cref="AlertDescription"/> component.
        /// </summary>
        private bool hasDescription;

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Alert());
            builder.Append(BootstrapClassProvider.AlertColor(Color), Color != Color.None);
            builder.Append(BootstrapClassProvider.AlertDismisable(), Dismisable);
            builder.Append(BootstrapClassProvider.AlertFade(), Dismisable);
            builder.Append(BootstrapClassProvider.AlertShow(), Dismisable && Visible);
            // TODO: review below
            builder.Append(BootstrapClassProvider.AlertHasMessage(), hasMessage);
            builder.Append(BootstrapClassProvider.AlertHasDescription(), hasDescription);

            base.BuildClasses(builder);
        }

        public void Show() { }

        public void Hide() { }

        public void Toogle() { }

        #endregion

        #region Properties

        /// <summary>
        /// Enables the alert to be closed by placing the padding for close button.
        /// </summary>
        [Parameter]
        public bool Dismisable
        {
            get => state.Dismisable;
            set
            {
                state = state with { Dismisable = value };

                DirtyClasses();
            }
        }

        /// <summary>
        /// Sets the alert visibility.
        /// </summary>
        [Parameter]
        public bool Visible
        {
            get => state.Visible;
            set
            {
                if (value == state.Visible)
                    return;

                state = state with { Visible = value };

                // TODO: enable this
                //HandleVisibilityState(value);
                //RaiseEvents(value);
            }
        }

        /// <summary>
        /// Occurs when the alert visibility state changes.
        /// </summary>
        [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

        /// <summary>
        /// Gets or sets the alert color.
        /// </summary>
        [Parameter]
        public Color Color
        {
            get => state.Color;
            set
            {
                state = state with { Color = value };

                DirtyClasses();
            }
        }

        /// <summary>
        /// Specifies the content to be rendered inside this <see cref="Alert"/>.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion
    }
}
