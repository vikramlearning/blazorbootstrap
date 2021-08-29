using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorBootstrap
{
    public abstract class BaseComponent : ComponentBase
    {
        #region Members

        private string customClass;

        private string customStyle;

        #endregion

        #region Constructors

        public BaseComponent()
        {
            ClassBuilder = new(BuildClasses);
            StyleBuilder = new(BuildStyles);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds a list of classnames for this component.
        /// </summary>
        /// <param name="builder">Class builder used to append the classnames.</param>
        protected virtual void BuildClasses(ClassBuilder builder)
        {
            if (Class != null)
                builder.Append(Class);

            // TODO: update this
        }

        /// <summary>
        /// Builds a list of styles for this component.
        /// </summary>
        /// <param name="builder">Style builder used to append the styles.</param>
        protected virtual void BuildStyles(StyleBuilder builder)
        {
            if (Style != null)
                builder.Append(Style);
        }

        /// <summary>
        /// Clears the class-names and mark them to be regenerated.
        /// </summary>
        internal protected virtual void DirtyClasses()
        {
            ClassBuilder.Dirty();
        }

        /// <summary>
        /// Clears the styles-names and mark them to be regenerated.
        /// </summary>
        protected virtual void DirtyStyles()
        {
            StyleBuilder.Dirty();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the class builder.
        /// </summary>
        protected ClassBuilder ClassBuilder { get; private set; }

        /// <summary>
        /// Gets the built class-names based on all the rules set by the component parameters.
        /// </summary>
        public string ClassNames => ClassBuilder.Class;

        /// <summary>
        /// Gets the style mapper.
        /// </summary>
        protected StyleBuilder StyleBuilder { get; private set; }

        /// <summary>
        /// Gets the built styles based on all the rules set by the component parameters.
        /// </summary>
        public string StyleNames => StyleBuilder.Styles;

        /// <summary>
        /// Gets or sets the classname provider.
        /// </summary>
        [Inject]
        protected BootstrapClassProvider BootstrapClassProvider { get; set; }

        /// <summary>
        /// Custom css classname.
        /// </summary>
        [Parameter]
        public string Class
        {
            get => customClass;
            set
            {
                customClass = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Custom html style.
        /// </summary>
        [Parameter]
        public string Style
        {
            get => customStyle;
            set
            {
                customStyle = value;

                DirtyStyles();
            }
        }

        /// <summary>
        /// Captures all the custom attribute that are not part of Blazorise component.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes { get; set; }

        #endregion
    }
}
