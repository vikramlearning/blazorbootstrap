namespace BlazorBootstrap;

public partial class Pagination : BlazorBootstrapComponentBase
{
    #region Methods

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
            return (q - 1) * DisplayPages + 1;

        if (q > 1 && r < DisplayPages)
            return q * DisplayPages + 1;

        return ActivePageNumber / DisplayPages * DisplayPages + 1;
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

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Pagination, true),
            (Size.ToPaginationSizeClass(), Size != PaginationSize.None),
            (Alignment.ToPaginationAlignmentClass(), Alignment != Alignment.None));

    /// <summary>
    /// Gets or sets the active page number.
    /// </summary>
    /// <para>
    /// Default value is 1.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(1)]
    [Description("Gets or sets the active page number.")]
    [Parameter]
    public int ActivePageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the pagination alignment.
    /// </summary>
    /// <para>
    /// Default value is <see cref="Alignment.None" />.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(Alignment.None)]
    [Description("Gets or sets the pagination alignment.")]
    [Parameter]
    public Alignment Alignment { get; set; } = Alignment.None;

    /// <summary>
    /// Gets or sets the maximum page links to be displayed.
    /// </summary>
    /// <para>
    /// Default value is 5.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(5)]
    [Description("Gets or sets the maximum page links to be displayed.")]
    [Parameter]
    public int DisplayPages { get; set; } = 5;

    /// <summary>
    /// Gets or sets the first link icon.
    /// </summary>
    /// <para>
    /// Default value is <see cref="IconName.None" />.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the first link icon.")]
    [Parameter]
    public IconName FirstLinkIcon { get; set; } = IconName.None;

    private string firstLinkText => string.IsNullOrWhiteSpace(FirstLinkText) ? "First" : FirstLinkText;

    /// <summary>
    /// Gets or sets the first link text. 'FirstLinkText' is ignored if 'FirstLinkIcon' is specified.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the first link text. <b>FirstLinkText</b> is ignored if <b>FirstLinkIcon</b> is specified.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? FirstLinkText { get; set; }

    private int firstPageNumber => 1;

    /// <summary>
    /// Gets or sets the last link icon.
    /// </summary>
    /// <para>
    /// Default value is <see cref="IconName.None" />.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the last link icon.")]
    [Parameter]
    public IconName LastLinkIcon { get; set; } = IconName.None;

    private string lastLinkText => string.IsNullOrWhiteSpace(LastLinkText) ? "Last" : LastLinkText;

    /// <summary>
    /// Gets or sets the last link text. 'LastLinkText' is ignored if 'LastLinkIcon' is specified.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the last link text. <b>LastLinkText</b> is ignored if <b>LastLinkIcon</b> is specified.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? LastLinkText { get; set; }

    private int lastPageNumber => TotalPages == 0 ? 1 : TotalPages;

    /// <summary>
    /// Gets or sets the next link icon.
    /// </summary>
    /// <para>
    /// Default value is <see cref="IconName.None" />.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the next link icon.")]
    [Parameter]
    public IconName NextLinkIcon { get; set; } = IconName.None;

    private string nextLinkText => string.IsNullOrWhiteSpace(NextLinkText) ? "Next" : NextLinkText;

    /// <summary>
    /// Gets or sets the next link text. 'NextLinkText' is ignored if 'NextLinkIcon' is specified.
    /// </summary>
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the next link text. <b>NextLinkText</b> is ignored if <b>NextLinkIcon</b> is specified.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? NextLinkText { get; set; }

    private int nextPageNumber => GetNextPageNumber();

    /// <summary>
    /// This event fires immediately when the page number is changed.
    /// </summary>
    [AddedVersion("1.0.0")]
    [Description("This event fires immediately when the page number is changed.")]
    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    private int pageFromInclusive => GetPageFromInclusive();

    private int pageToExclusive => GetPageToExclusive();

    /// <summary>
    /// Gets or sets the previous link icon.
    /// </summary>
    /// <para>
    /// Default value is <see cref="IconName.None" />.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(IconName.None)]
    [Description("Gets or sets the previous link icon.")]
    [Parameter]
    public IconName PreviousLinkIcon { get; set; } = IconName.None;

    private string previousLinkText => string.IsNullOrWhiteSpace(PreviousLinkText) ? "Previous" : PreviousLinkText;

    /// <summary>
    /// Gets or sets the previous link text. 'PreviousLinkText' is ignored if 'PreviousLinkIcon' is specified.
    /// </summary>
    /// <para>
    /// Default value is null.
    /// </para>
    [AddedVersion("1.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the previous link text. <b>PreviousLinkText</b> is ignored if <b>PreviousLinkIcon</b> is specified.")]
    [ParameterTypeName("string?")]
    [Parameter]
    public string? PreviousLinkText { get; set; }

    private int previousPageNumber => GetPreviousPageNumber();

    /// <summary>
    /// Gets or sets the pagination size.
    /// <para>
    /// Default value is <see cref="PaginationSize.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(PaginationSize.None)]
    [Description("Gets or sets the pagination size.")]
    [Parameter]
    public PaginationSize Size { get; set; } = PaginationSize.None;

    /// <summary>
    /// Gets or sets the total pages.
    /// <para>
    /// Default value is 0.
    /// </para>
    /// </summary>
    [AddedVersion("1.0.0")]
    [DefaultValue(0)]
    [Description("Gets or sets the total pages.")]
    [Parameter]
    public int TotalPages { get; set; }

    #endregion
}
