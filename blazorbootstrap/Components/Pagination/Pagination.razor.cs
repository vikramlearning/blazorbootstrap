using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Pagination : BaseComponent
{
    #region Members

    private int firstPageNumber => 1;

    private int previousPageNumber => GetPreviousPageNumber();

    private int nextPageNumber => GetNextPageNumber();

    private int lastPageNumber => TotalPages == 0 ? 1 : TotalPages;

    private int pageFromInclusive => GetPageFromInclusive();

    private int pageToExclusive => GetPageToExclusive();

    private PaginationSize size = PaginationSize.None;

    private Alignment alignment = Alignment.None;

    private string previousLinkText => string.IsNullOrWhiteSpace(PreviousLinkText) ? "Previous" : PreviousLinkText;

    private string nextLinkText => string.IsNullOrWhiteSpace(NextLinkText) ? "Next" : NextLinkText;

    private string firstLinkText => string.IsNullOrWhiteSpace(FirstLinkText) ? "First" : FirstLinkText;

    private string lastLinkText => string.IsNullOrWhiteSpace(LastLinkText) ? "Last" : LastLinkText;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(BootstrapClassProvider.Pagination());
        builder.Append(BootstrapClassProvider.PaginationSize(Size), Size != PaginationSize.None);
        builder.Append(BootstrapClassProvider.FlexAlignment(Alignment), Alignment != Alignment.None);
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

    private int GetPreviousPageNumber()
    {
        var previousPageNUmber = 1;

        if (TotalPages == 1 || ActivePageNumber == 0 || TotalPages == 0)
            previousPageNUmber = 1;
        else if (ActivePageNumber > 1 && TotalPages > 1)
            previousPageNUmber = ActivePageNumber - 1;

        return previousPageNUmber;
    }

    private int GetNextPageNumber()
    {
        var nextPageNumber = 1;

        if (TotalPages == 1 || ActivePageNumber == 0 || TotalPages == 0)
            nextPageNumber = 1;
        if (ActivePageNumber == TotalPages)
            nextPageNumber = TotalPages;
        else if (TotalPages > 1 && ActivePageNumber >= 1 && ActivePageNumber < TotalPages)
            nextPageNumber = ActivePageNumber + 1;

        return nextPageNumber;
    }

    private int GetPageFromInclusive()
    {
        var q = ActivePageNumber / DisplayPages;
        var r = ActivePageNumber % DisplayPages;

        if (q < 1)
            return 1;
        else if (q > 0 && r == 0)
            return (q - 1) * DisplayPages + 1;
        else if (q > 1 && r < DisplayPages)
            return (q * DisplayPages) + 1;

        return (ActivePageNumber / DisplayPages) * DisplayPages + 1;
    }

    private int GetPageToExclusive()
    {
        if (TotalPages == 0)
            return 1;

        return Math.Min(TotalPages, pageFromInclusive + DisplayPages - 1);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the pagination size.
    /// </summary>
    [Parameter]
    public PaginationSize Size
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
    /// Active page number. Starts with 1.
    /// </summary>
    [Parameter] public int ActivePageNumber { get; set; } = 1;

    /// <summary>
    /// Total pages of data items.
    /// </summary>
    [Parameter] public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets the maximum page links to be displayed.
    /// </summary>
    [Parameter] public int DisplayPages { get; set; } = 5;

    /// <summary>
    /// Gets or sets first link text. 'FirstLinkText' is ignored if 'FirstLinkIcon' is specified.
    /// </summary>
    [Parameter] public string FirstLinkText { get; set; }

    /// <summary>
    /// Gets or sets first link icon.
    /// </summary>
    [Parameter] public IconName FirstLinkIcon { get; set; }

    /// <summary>
    /// Gets or sets previous link text. 'PreviousLinkText' is ignored if 'PreviousLinkIcon' is specified.
    /// </summary>
    [Parameter] public string PreviousLinkText { get; set; }

    /// <summary>
    /// Gets or sets previous link icon.
    /// </summary>
    [Parameter] public IconName PreviousLinkIcon { get; set; }

    /// <summary>
    /// Gets or sets next link text. 'NextLinkText' is ignored if 'NextLinkIcon' is specified.
    /// </summary>
    [Parameter] public string NextLinkText { get; set; }

    /// <summary>
    /// Gets or sets next link icon.
    /// </summary>
    [Parameter] public IconName NextLinkIcon { get; set; }

    /// <summary>
    /// Gets or sets last link text. 'LastLinkText' is ignored if 'LastLinkIcon' is specified.
    /// </summary>
    [Parameter] public string LastLinkText { get; set; }

    /// <summary>
    /// Gets or sets last link icon.
    /// </summary>
    [Parameter] public IconName LastLinkIcon { get; set; }

    /// <summary>
    /// This event fires immediately when the page number is changed.
    /// </summary>
    [Parameter] public EventCallback<int> PageChanged { get; set; }

    #endregion
}

