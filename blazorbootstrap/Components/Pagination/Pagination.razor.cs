using System.Reflection.Metadata.Ecma335;

namespace BlazorBootstrap;

/// <summary>
/// Use Blazor Bootstrap pagination component to indicate a series of related content exists across multiple pages. <br/>
/// For more information, visit the <see href="https://getbootstrap.com/docs/5.0/components/pagination/">Bootstrap Pagination</see> documentation.
/// </summary>
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

        if (r == 0)
            return (q - 1) * DisplayPages + 1;

        if (q > 1 && r < DisplayPages)
            return q * DisplayPages + 1;

        return ActivePageNumber / DisplayPages * DisplayPages + 1;
    }

    private int GetPageToExclusive() => TotalPages == 0 ? 1 : Math.Min(TotalPages, PageFromInclusive + DisplayPages - 1);

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


    /// <summary>
    /// Parameters are loaded manually for sake of performance.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/performance#implement-setparametersasync-manually"/>
    /// </summary> 
    public override Task SetParametersAsync(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case var _ when String.Equals(parameter.Name, nameof(ActivePageNumber), StringComparison.OrdinalIgnoreCase): ActivePageNumber = (int)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Alignment), StringComparison.OrdinalIgnoreCase): Alignment = (Alignment)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Class), StringComparison.OrdinalIgnoreCase): Class = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(DisplayPages), StringComparison.OrdinalIgnoreCase): DisplayPages = (int)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FirstLinkIcon), StringComparison.OrdinalIgnoreCase): FirstLinkIcon = (IconName)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(FirstLinkText), StringComparison.OrdinalIgnoreCase): FirstLinkText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Id), StringComparison.OrdinalIgnoreCase): Id = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(LastLinkIcon), StringComparison.OrdinalIgnoreCase): LastLinkIcon = (IconName)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(LastLinkText), StringComparison.OrdinalIgnoreCase): LastLinkText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(NextLinkIcon), StringComparison.OrdinalIgnoreCase): NextLinkIcon = (IconName)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(NextLinkText), StringComparison.OrdinalIgnoreCase): NextLinkText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(PageChanged), StringComparison.OrdinalIgnoreCase): PageChanged = (EventCallback<int>)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(PreviousLinkIcon), StringComparison.OrdinalIgnoreCase): PreviousLinkIcon = (IconName)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(PreviousLinkText), StringComparison.OrdinalIgnoreCase): PreviousLinkText = (string)parameter.Value; break;
                case var _ when String.Equals(parameter.Name, nameof(Size), StringComparison.OrdinalIgnoreCase): Size = (PaginationSize)parameter.Value; break;
                
                case var _ when String.Equals(parameter.Name, nameof(TotalPages), StringComparison.OrdinalIgnoreCase): TotalPages = (int)parameter.Value; break;

                default:
                    AdditionalAttributes[parameter.Name] = parameter.Value;
                    break;
            }
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    #endregion

    #region Properties, Indexers 
    
    /// <summary>
    /// Gets or sets the active page number.
    /// </summary>
    /// <remarks>
    /// Default value is 1.
    /// </remarks>
    [Parameter]
    public int ActivePageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the pagination alignment.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="Alignment.None" />.
    /// </remarks>
    [Parameter]
    public Alignment Alignment { get; set; } = Alignment.None;

    /// <summary>
    /// Gets or sets the maximum page links to be displayed.
    /// </summary>
    /// <remarks>
    /// Default value is 5.
    /// </remarks>
    [Parameter]
    public int DisplayPages { get; set; } = 5;

    /// <summary>
    /// Gets or sets the first link icon.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName FirstLinkIcon { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the first link text. 'FirstLinkText' is ignored if 'FirstLinkIcon' is specified.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? FirstLinkText { get; set; }

    /// <summary>
    /// Gets or sets the last link icon.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName LastLinkIcon { get; set; } = IconName.None;

    /// <summary>
    /// Gets or sets the last link text. 'LastLinkText' is ignored if 'LastLinkIcon' is specified.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? LastLinkText { get; set; }

    private int LastPageNumber => TotalPages == 0 ? 1 : TotalPages;

    /// <summary>
    /// Gets or sets the next link icon.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName NextLinkIcon { get; set; } = IconName.None;
    
    /// <summary>
    /// Gets or sets the next link text. 'NextLinkText' is ignored if 'NextLinkIcon' is specified.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? NextLinkText { get; set; }

    private int NextPageNumber => GetNextPageNumber();

    /// <summary>
    /// This event fires immediately when the page number is changed.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    private int PageFromInclusive => GetPageFromInclusive();

    private int PageToExclusive => GetPageToExclusive();

    /// <summary>
    /// Gets or sets the previous link icon.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconName.None" />.
    /// </remarks>
    [Parameter]
    public IconName PreviousLinkIcon { get; set; } = IconName.None;
    
    /// <summary>
    /// Gets or sets the previous link text. 'PreviousLinkText' is ignored if 'PreviousLinkIcon' is specified.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="null" />.
    /// </remarks>
    [Parameter]
    public string? PreviousLinkText { get; set; }

    private int PreviousPageNumber => GetPreviousPageNumber();

    /// <summary>
    /// Gets or sets the pagination size.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="PaginationSize.None" />.
    /// </remarks>
    [Parameter]
    public PaginationSize Size { get; set; } = PaginationSize.None;

    /// <summary>
    /// Gets or sets the total pages.
    /// </summary>
    /// <remarks>
    /// Default value is 0.
    /// </remarks>
    [Parameter]
    public int TotalPages { get; set; }

    #endregion
}
