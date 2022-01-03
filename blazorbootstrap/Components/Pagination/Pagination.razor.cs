using BlazorBootstrap.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap;

public partial class Pagination : BaseComponent
{
    #region Members

    private PaginationSize size = PaginationSize.None;

    private Alignment alignment = Alignment.None;

    private string previousLinkText => string.IsNullOrWhiteSpace(PreviousLinkText) ? "Previous" : PreviousLinkText;

    private string nextLinkText => string.IsNullOrWhiteSpace(NextLinkText) ? "Next" : NextLinkText;

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
            Console.WriteLine($"Before ActivePageNumber: {ActivePageNumber}"); // TODO: remove this
            ActivePageNumber = newPageNumber;
            Console.WriteLine($"After ActivePageNumber: {ActivePageNumber}"); // TODO: remove this
            await PageChanged.InvokeAsync(newPageNumber);
        }
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

