using BlazorBootstrap.Utilities;
using BlazorBootstrap.Extensions;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorBootstrap
{
    public partial class Breadcrumb : BaseComponent
    {
        #region Members

        #endregion Members

        #region Methods

        #endregion Methods

        #region Properties

        /// <inheritdoc/>
        protected override bool ShouldAutoGenerateId => true;

        /// <summary>
        /// List of all the items.
        /// </summary>
        [Parameter] public List<BreadcrumbItem> Items { get; set; }

        #endregion Properties
    }
}
