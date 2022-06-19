﻿using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap
{
    public partial class Heading
    {
        #region Members

        private HeadingSize headingSize = HeadingSize.H3;

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void BuildClasses(ClassBuilder builder)
        {
            builder.Append(BootstrapClassProvider.HeadingSize(headingSize));

            base.BuildClasses(builder);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the heading tag name.
        /// </summary>
        protected string TagName => $"h{SizeNumber}";

        /// <summary>
        /// Gets the heading size number.
        /// </summary>
        protected string SizeNumber => Size switch
        {
            HeadingSize.H1 => "1",
            HeadingSize.H2 => "2",
            HeadingSize.H3 => "3",
            HeadingSize.H4 => "4",
            HeadingSize.H5 => "5",
            HeadingSize.H6 => "6",
            _ => "3",
        };

        /// <summary>
        /// Gets or sets the heading size.
        /// </summary>
        [Parameter]
        public HeadingSize Size
        {
            get => headingSize;
            set
            {
                headingSize = value;
                DirtyClasses();
            }
        }

        #endregion
    }
}
