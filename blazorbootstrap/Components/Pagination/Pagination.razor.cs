namespace BlazorBootstrap;

public partial class Pagination : BaseComponent
{
    #region Fields and Constants

    private Alignment alignment = Alignment.None;

    private PaginationSize size = PaginationSize.None;

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void BuildClasses(CssClassBuilder builder)
    {
        builder.Append(ClassProvider.Pagination());
        builder.Append(ClassProvider.PaginationSize(Size), Size != PaginationSize.None);
        builder.Append(ClassProvider.FlexAlignment(Alignment), Alignment != Alignment.None);
        //builder.Append(ClassProvider.BackgroundColor(BackgroundColor), BackgroundColor != BackgroundColor.None);

        base.BuildClasses(builder);
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

        if (q > 0 && r == 0)
            return ((q - 1) * DisplayPages) + 1;

        if (q > 1 && r < DisplayPages)
            return (q * DisplayPages) + 1;

        return (ActivePageNumber / DisplayPages * DisplayPages) + 1;
    }

    private int GetPageToExclusive() => TotalPages == 0 ? 1 : Math.Min(TotalPages, pageFromInclusive + DisplayPages - 1);

    private int GetPreviousPageNumber()
    {
        var previousPageNUmber = 1;

        if (TotalPages == 1 || ActivePageNumber == 0 || TotalPages == 0)
            previousPageNUmber = 1;
        else if (ActivePageNumber > 1 && TotalPages > 1)
            previousPageNUmber = ActivePageNumber - 1;

        return previousPageNUmber;
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

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Active page number. Starts with 1.
    /// </summary>
    [Parameter]
    public int ActivePageNumber { get; set; } = 1;

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
    /// Gets or sets the maximum page links to be displayed.
    /// </summary>
    [Parameter]
    public int DisplayPages { get; set; } = 5;

    /// <summary>
    /// Gets or sets first link icon.
    /// </summary>
    [Parameter]
    public IconName FirstLinkIcon { get; set; }

    private string firstLinkText => string.IsNullOrWhiteSpace(FirstLinkText) ? "First" : FirstLinkText;

    /// <summary>
    /// Gets or sets first link text. 'FirstLinkText' is ignored if 'FirstLinkIcon' is specified.
    /// </summary>
    [Parameter]
    public string? FirstLinkText { get; set; }

    private int firstPageNumber => 1;

    /// <summary>
    /// Gets or sets last link icon.
    /// </summary>
    [Parameter]
    public IconName LastLinkIcon { get; set; }

    private string lastLinkText => string.IsNullOrWhiteSpace(LastLinkText) ? "Last" : LastLinkText;

    /// <summary>
    /// Gets or sets last link text. 'LastLinkText' is ignored if 'LastLinkIcon' is specified.
    /// </summary>
    [Parameter]
    public string? LastLinkText { get; set; }

    private int lastPageNumber => TotalPages == 0 ? 1 : TotalPages;

    /// <summary>
    /// Gets or sets next link icon.
    /// </summary>
    [Parameter]
    public IconName NextLinkIcon { get; set; }

    private string nextLinkText => string.IsNullOrWhiteSpace(NextLinkText) ? "Next" : NextLinkText;

    /// <summary>
    /// Gets or sets next link text. 'NextLinkText' is ignored if 'NextLinkIcon' is specified.
    /// </summary>
    [Parameter]
    public string? NextLinkText { get; set; }

    private int nextPageNumber => GetNextPageNumber();

    /// <summary>
    /// This event fires immediately when the page number is changed.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    private int pageFromInclusive => GetPageFromInclusive();

    private int pageToExclusive => GetPageToExclusive();

    /// <summary>
    /// Gets or sets previous link icon.
    /// </summary>
    [Parameter]
    public IconName PreviousLinkIcon { get; set; }

    private string previousLinkText => string.IsNullOrWhiteSpace(PreviousLinkText) ? "Previous" : PreviousLinkText;

    /// <summary>
    /// Gets or sets previous link text. 'PreviousLinkText' is ignored if 'PreviousLinkIcon' is specified.
    /// </summary>
    [Parameter]
    public string? PreviousLinkText { get; set; }

    private int previousPageNumber => GetPreviousPageNumber();

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
    /// Total pages of data items.
    /// </summary>
    [Parameter]
    public int TotalPages { get; set; }

    #endregion
}