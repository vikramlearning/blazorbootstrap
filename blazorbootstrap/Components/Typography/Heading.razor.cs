﻿namespace BlazorBootstrap;

public partial class Heading : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private HeadingSize headingSize = HeadingSize.H3;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses()
    {
        this.AddClass(BootstrapClassProvider.HeadingSize(headingSize));

        base.BuildClasses();
    }

    #endregion

    #region Properties, Indexers

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

    /// <summary>
    /// Gets the heading size number.
    /// </summary>
    protected string SizeNumber =>
        Size switch
        {
            HeadingSize.H1 => "1",
            HeadingSize.H2 => "2",
            HeadingSize.H3 => "3",
            HeadingSize.H4 => "4",
            HeadingSize.H5 => "5",
            HeadingSize.H6 => "6",
            _ => "3"
        };

    /// <summary>
    /// Gets the heading tag name.
    /// </summary>
    protected string TagName => $"h{SizeNumber}";

    #endregion
}
