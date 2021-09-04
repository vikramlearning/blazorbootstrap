using BlazorBootstrap.Enums;
using BlazorBootstrap.Extensions;
using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BlazorBootstrap
{
    public partial class Button : BaseComponent
    {
        #region Members

        private Color color = Color.None;

        private Size size = Size.None;

        private bool outline;

        private bool disabled;

        private bool active;

        private bool block;

        private bool loading;

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.Button());
            builder.Append(BootstrapClassProvider.ButtonColor(Color), Color != Color.None && !Outline);
            builder.Append(BootstrapClassProvider.ButtonOutline(Color), Color != Color.None && Outline);
            builder.Append(BootstrapClassProvider.ButtonSize(Size), Size != Enums.Size.None);
            builder.Append(BootstrapClassProvider.ButtonDisabled(), disabled);
            builder.Append(BootstrapClassProvider.ButtonActive(), active);
            builder.Append(BootstrapClassProvider.ButtonBlock(), Block);
            builder.Append(BootstrapClassProvider.ButtonLoading(), Loading && LoadingTemplate != null);

            base.BuildClasses(builder);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder
                .OpenElement(Type.ToButtonTagName())
                .Type(Type.ToButtonTypeString())
                .Class(ClassNames)
                .Style(StyleNames)
                .Disabled(Disabled)
                .AriaPressed(Active)
                .TabIndex(TabIndex);
                //.DataBootstrap("toggle", "button");

            if(Type == ButtonType.Link)
            {
                builder.Role("button")
                    .Href(To)
                    .Target(Target);

                if (Disabled)
                {
                    builder
                        .TabIndex(-1)
                        .AriaDisabled("true");
                }
            }

            builder.Attributes(Attributes);
            
            if (Loading && LoadingTemplate != null)
                builder.Content(LoadingTemplate);
            else
                builder.Content(ChildContent);

            builder.CloseElement();

            base.BuildRenderTree(builder);
        }

        protected override void OnInitialized()
        {
            LoadingTemplate ??= ProvideDefaultLoadingTemplate();

            base.OnInitialized();
        }

        //protected virtual RenderFragment ProvideDefaultLoadingTemplate() => null;

        protected virtual RenderFragment ProvideDefaultLoadingTemplate() => builder =>
        {
            builder.MarkupContent($"<span class=\"spinner-border spinner-border-sm\" role=\"status\" aria-hidden=\"true\"></span> {LoadingText}");
        };

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
        public Size Size
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
        [Parameter]
        public bool Disabled
        {
            get => disabled;
            set
            {
                disabled = value;
                DirtyClasses();
            }
        }

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
        /// Shows the loading spinner or a <see cref="LoadingTemplate"/>.
        /// </summary>
        [Parameter]
        public bool Loading
        {
            get => loading;
            set
            {
                loading = value;
                DirtyClasses();
            }
        }

        /// <summary>
        /// Gets or sets the loadgin text.
        /// <see cref="LoadingTemplate"/> takes precedence.
        /// </summary>
        [Parameter] public string LoadingText { get; set; } = "Loading...";

        /// <summary>
        /// Gets or sets the component loading template.
        /// </summary>
        [Parameter] public RenderFragment LoadingTemplate { get; set; }

        /// <summary>
        /// Denotes the target route of the <see cref="ButtonType.Link"/> button.
        /// </summary>
        [Parameter] public string To { get; set; }

        /// <summary>
        /// The target attribute specifies where to open the linked document for a <see cref="ButtonType.Link"/>.
        /// </summary>
        [Parameter] public Target Target { get; set; } = Target.None;

        /// <summary>
        /// If defined, indicates that its element can be focused and can participates in sequential keyboard navigation.
        /// </summary>
        [Parameter] public int? TabIndex { get; set; }

        /// <summary>
        /// Specifies the content to be rendered inside this <see cref="Button"/>.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion

        // TODO:
        // - Disable text wrapping: https://getbootstrap.com/docs/5.1/components/buttons/#disable-text-wrapping
        // - Toogle states: https://getbootstrap.com/docs/5.1/components/buttons/#toggle-states
    }
}
