using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Pagination : BaseComponent
{
    #region Members

    private Size? size;

    private Alignment alignment = Alignment.None;

    private bool showFirstLast => ShowFirstLast();

    private string previousLinkText => string.IsNullOrWhiteSpace(PreviousLinkText) ? "Previous" : PreviousLinkText;

    private string nextLinkText => string.IsNullOrWhiteSpace(NextLinkText) ? "Next" : NextLinkText;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Pagination());
        //builder.Append(BootstrapClassProvider.FlexAlignment(Alignment), Alignment != Alignment.None);
        //builder.Append(BootstrapClassProvider.BackgroundColor(BackgroundColor), BackgroundColor != BackgroundColor.None);

        base.BuildClasses(builder);
    }

    /// <summary>
    /// Changes current page number and fires event.
    /// </summary>
    private async Task SetPageNumberTo(int newPageNumber)
    {
        if (ActivePageNumber != newPageNumber)
        {
            ActivePageNumber = newPageNumber;
            await PageChanged.InvokeAsync(newPageNumber);
        }
    }

    private bool ShowFirstLast()
    {
        return true;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the pagination size.
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
    /// Gets or sets the pagination alignment.
    /// </summary>
    [Parameter]
    public Alignment Alignment
    {
        get => alignment;
        set
        {
            alignment = value;

            DirtyClasses();
        }
    }

    /// <summary>
    /// Active page number. Zero based.
    /// Displayed numbers start with 1.
    /// </summary>
    [Parameter] public int ActivePageNumber { get; set; }

    /// <summary>
    /// Page size.
    /// </summary>
    [Parameter] public int PageSize { get; set; }

    /// <summary>
    /// Total pages of data items.
    /// </summary>
    [Parameter] public int TotalPages { get; set; }

    [Parameter] public string PreviousLinkText { get; set; }

    [Parameter] public IconName PreviousLinkIcon { get; set; }

    [Parameter] public string NextLinkText { get; set; }

    [Parameter] public IconName NextLinkIcon { get; set; }

    /// <summary>
    /// Event raised where page number is changed.
    /// </summary>
    [Parameter] public EventCallback<int> PageChanged { get; set; }

    #endregion
}

