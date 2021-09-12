using System.Threading.Tasks;
using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBootstrap
{
    public partial class Icon : BaseComponent
    {
        #region Members

        private IconName name;

        private string customName;

        private IconSize size = IconSize.None;

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapIconProvider.Icon(), string.IsNullOrWhiteSpace(CustomIconName));
            builder.Append(BootstrapIconProvider.Icon(Name), string.IsNullOrWhiteSpace(CustomIconName));
            builder.Append(customName, !string.IsNullOrWhiteSpace(CustomIconName));
            builder.Append(BootstrapIconProvider.IconSize(Size), Size != IconSize.None);

            base.BuildClasses(builder);
        }

        #endregion

        #region Properties

        /// <summary>
        /// An icon provider that is responsible to give the icon a class-name.
        /// </summary>
        [Inject] protected BootstrapIconProvider BootstrapIconProvider { get; set; }

        /// <summary>
        /// Icon name that can be either a string or <see cref="IconName"/>.
        /// </summary>
        [Parameter]
        public IconName Name
        {
            get => name;
            set
            {
                name = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Icon name that can be either a string or <see cref="IconName"/>.
        /// </summary>
        [Parameter]
        public string CustomIconName
        {
            get => customName;
            set
            {
                customName = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Defines the icon size.
        /// </summary>
        [Parameter]
        public IconSize Size
        {
            get => size;
            set
            {
                size = value;

                DirtyClasses();
            }
        }

        #endregion
    }
}
