#region Using Directives
using BlazorBootstrap.Enums;
using BlazorBootstrap.Extensions;
using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
#endregion

namespace BlazorBootstrap.Components
{
    public partial class Button : BaseComponent
    {
        #region Members

        private Color color = Color.None;

        private Size? size;

        private bool outline;

        private bool active;

        private bool block;

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Button());
            builder.Append(BootstrapClassProvider.ButtonColor(Color), Color != Color.None && !Outline);

            // TODO: pending

            base.BuildClasses(builder);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder
                .OpenElement(Type.ToButtonTagName())
                .Type(Type.ToButtonTypeString())
                .Class(ClassNames)
                .Style(StyleNames);

            builder.Attributes(Attributes);
            builder.Content(ChildContent);
            builder.CloseElement();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Defines the button type.
        /// </summary>
        [Parameter] public ButtonType Type { get; set; } = ButtonType.Button;

        /// <summary>
        /// Gets or sets the button color.
        /// </summary>
        [Parameter]
        public Color Color
        {
            get => color;
            set
            {
                color = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Changes the size of a button.
        /// </summary>
        [Parameter]
        public Size? Size
        {
            get => size;
            set
            {
                size = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Makes the button to have the outlines.
        /// </summary>
        [Parameter]
        public bool Outline
        {
            get => outline;
            set
            {
                outline = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// When set to 'true', disables the component's functionality and places it in a disabled state.
        /// </summary>
        //[Parameter]
        //public bool Disabled
        //{
        //    get => disabled || !canExecuteCommand.GetValueOrDefault(true);
        //    set
        //    {
        //        disabled = value;

        //        DirtyClasses();
        //    }
        //}

        /// <summary>
        /// When set to 'true', places the component in the active state with active styling.
        /// </summary>
        [Parameter]
        public bool Active
        {
            get => active;
            set
            {
                active = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Makes the button to span the full width of a parent.
        /// </summary>
        [Parameter]
        public bool Block
        {
            get => block;
            set
            {
                block = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Specifies the content to be rendered inside this <see cref="Button"/>.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion
    }
}
