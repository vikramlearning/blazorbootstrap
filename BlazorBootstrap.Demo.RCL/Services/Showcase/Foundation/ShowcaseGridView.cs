namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// One Showcase list's saved view, connected to its <see cref="Grid{TItem}" />:
/// <list type="bullet">
/// <item><see cref="GetSettingsAsync" /> is the grid's <c>SettingsProvider</c>, and <see cref="SaveSettingsAsync" /> its <c>GridSettingsChanged</c>.</item>
/// <item>The grid's <c>DataProvider</c> returns <see cref="ApplyAsync{TItem}" />, which also remembers the sort.</item>
/// <item>Each sortable column sets <c>SortString</c> to a stable key, <c>IsDefaultSortColumn</c> to <see cref="IsSortColumn" />,
/// and <c>SortDirection</c> to <see cref="DirectionFor" />, so the saved sort is applied when the list opens.</item>
/// </list>
/// The grid's settings don't include the sort, so the sort is restored through the columns' own default-sort parameters.
/// </summary>
public sealed class ShowcaseGridView
{
    #region Fields and Constants

    /// <summary>Every page size a Showcase list offers; a stored size outside them came from edited storage.</summary>
    private static readonly HashSet<int> pageSizes = new() { 5, 10, 25, 50, 100 };

    private readonly int defaultPageSize;

    private readonly SortDirection defaultSortDirection;

    private readonly string? defaultSortKey;

    private readonly string key;

    private readonly ShowcaseGridViewStore store;

    #endregion

    #region Constructors and Finalizers

    internal ShowcaseGridView(ShowcaseGridViewStore store, string key, int defaultPageSize, string? defaultSortKey, SortDirection defaultSortDirection)
    {
        this.store = store;
        this.key = key;
        this.defaultPageSize = defaultPageSize;
        this.defaultSortKey = defaultSortKey;
        this.defaultSortDirection = defaultSortDirection;
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Returns the page of rows the grid asked for, and remembers the sort it asked for. An export request (page 0, size 0)
    /// isn't treated as a sort change.
    /// </summary>
    public async Task<GridDataProviderResult<TItem>> ApplyAsync<TItem>(GridDataProviderRequest<TItem> request, IEnumerable<TItem> data)
    {
        var isExport = request.PageNumber == 0 && request.PageSize == 0;
        var sort = request.Sorting?.FirstOrDefault(item => item.SortDirection != SortDirection.None);

        if (!isExport)
        {
            var sortKey = sort?.SortString;
            var direction = sort?.SortDirection ?? SortDirection.None;
            var saved = store.GetLoaded(key);

            // Sorting by the list's default is the same as having no saved sort.
            if (sortKey == defaultSortKey && direction == defaultSortDirection)
                sortKey = null;

            if (saved?.SortKey != sortKey || (sortKey is not null && saved!.SortDirection != direction))
                await store.UpdateAsync(key, state =>
                {
                    state.SortKey = sortKey;
                    state.SortDirection = sortKey is null ? SortDirection.None : direction;
                });
        }

        return request.ApplyTo(data);
    }

    /// <summary>Gets the sort direction a column starts with: the saved one for the saved sort column, the default for the default column, otherwise none.</summary>
    public SortDirection DirectionFor(string sortKey)
    {
        var saved = store.GetLoaded(key);

        if (saved?.SortKey is { } savedKey)
            return savedKey == sortKey ? saved.SortDirection : SortDirection.None;

        return sortKey == defaultSortKey ? defaultSortDirection : SortDirection.None;
    }

    /// <summary>Gets the saved grid settings, or the list's defaults (first page, its page size) when nothing is saved.</summary>
    public async Task<GridSettings> GetSettingsAsync()
    {
        var saved = (await store.GetAsync(key))?.Settings;

        // A page size no Showcase list offers, or a page before the first, can only come from edited storage: the list's
        // defaults are used for them. A hidden-column set that would hide every column is ignored by the grid itself.
        var pageSize = saved is not null && pageSizes.Contains(saved.PageSize) ? saved.PageSize : defaultPageSize;
        var pageNumber = saved is not null && saved.PageSize == pageSize && saved.PageNumber > 0 ? saved.PageNumber : 1;

        return new GridSettings { PageNumber = pageNumber, PageSize = pageSize, Filters = saved?.Filters, ColumnOrder = saved?.ColumnOrder, HiddenColumns = saved?.HiddenColumns };
    }

    /// <summary>Gets whether the column with this <c>SortString</c> is the one the list opens sorted by.</summary>
    public bool IsSortColumn(string sortKey) => DirectionFor(sortKey) != SortDirection.None;

    /// <summary>
    /// Loads the saved view before the list renders, so <see cref="IsSortColumn" /> and <see cref="DirectionFor" /> see it.
    /// Call it from the page's <c>OnInitializedAsync</c> and render the grid after it.
    /// </summary>
    public async Task LoadAsync() => await store.GetAsync(key);

    /// <summary>Forgets the saved view; the page then re-renders the list to show its defaults.</summary>
    public Task ResetAsync() => store.RemoveAsync(key);

    /// <summary>Saves the grid's settings.</summary>
    public Task SaveSettingsAsync(GridSettings settings) => store.UpdateAsync(key, state => state.Settings = settings);

    #endregion

    #endregion
}
