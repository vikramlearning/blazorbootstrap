using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// Catalog: search, category, price, rating, and sale filters with sorting and paging. Filters live in
/// <see cref="EcommerceState" />, so they are still set after a visit to a product page. Links with
/// <c>?category=</c> or <c>?sale=1</c> start a fresh filter for that category or for sale items.
/// </summary>
public partial class Products : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int PageSize = 9;

    private List<BreadcrumbItem> breadcrumbItems = new();

    private Collapse filtersCollapse = default!;

    private bool filtersOpen;

    private (string? Category, string? Sale)? loadedQuery;

    private string searchBoxValue = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // Act only on a new query string, so a re-render never resets filters the user set afterwards.
        if (loadedQuery == (Category, SaleQuery))
            return;

        loadedQuery = (Category, SaleQuery);

        if (Category is not null || Sale == 1)
        {
            ResetFilter();

            if (State.FindCategory(Category) is { } category)
                Filter.CategoryIds.Add(category.Id);

            Filter.OnSaleOnly = Sale == 1;
        }

        searchBoxValue = Filter.Search;
        breadcrumbItems = new()
        {
            new() { Text = "Home", Href = EcommerceLayout.Base },
            new() { Text = "Shop", IsCurrentPage = true },
        };
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Apply()
    {
        Filter.Page = 1;
        State.NotifyFilterChanged();
    }

    private void ClearFilters()
    {
        ResetFilter();
        searchBoxValue = string.Empty;
        State.NotifyFilterChanged();
    }

    private void OnCategoriesChanged(IEnumerable<EcommerceCategory> categories)
    {
        Filter.CategoryIds.Clear();

        foreach (var category in categories)
            Filter.CategoryIds.Add(category.Id);

        Apply();
    }

    private void OnMaxPriceChanged(int value)
    {
        Filter.MaxPrice = value;
        Apply();
    }

    private void OnMinRatingChanged(int value)
    {
        Filter.MinRating = value;
        Apply();
    }

    private void OnPageChanged(int page)
    {
        Filter.Page = page;
        State.NotifyFilterChanged();
    }

    private void OnSaleOnlyChanged(bool value)
    {
        Filter.OnSaleOnly = value;
        Apply();
    }

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value;
        SetSearch(value);
    }

    private void OnSearchInput(ChangeEventArgs args) => SetSearch(args.Value?.ToString() ?? string.Empty);

    private void OnSortChanged(EcommerceSort sort)
    {
        Filter.Sort = sort;
        Apply();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void ResetFilter()
    {
        Filter.CategoryIds.Clear();
        Filter.MaxPrice = 400;
        Filter.MinRating = 0;
        Filter.OnSaleOnly = false;
        Filter.Page = 1;
        Filter.Search = string.Empty;
    }

    private void SetSearch(string value)
    {
        if (Filter.Search == value)
            return;

        Filter.Search = value;
        Apply();
    }

    private static string SortLabel(EcommerceSort sort) => sort switch
    {
        EcommerceSort.PriceLowToHigh => "Price, low to high",
        EcommerceSort.PriceHighToLow => "Price, high to low",
        EcommerceSort.TopRated => "Top rated",
        EcommerceSort.Newest => "Newest",
        _ => "Featured",
    };

    private async Task ToggleFiltersAsync() => await filtersCollapse.ToggleAsync();

    #endregion

    #endregion

    #region Properties, Indexers

    private int ActiveFilterCount =>
        Filter.CategoryIds.Count + (Filter.MaxPrice < 400 ? 1 : 0) + (Filter.MinRating > 0 ? 1 : 0) + (Filter.OnSaleOnly ? 1 : 0) + (Filter.Search.Length > 0 ? 1 : 0);

    /// <summary>Gets or sets the category to filter by, from <c>?category=</c>; an unknown value such as <c>all</c> clears every filter.</summary>
    [SupplyParameterFromQuery(Name = "category")] public string? Category { get; set; }

    private EcommerceCatalogFilter Filter => State.Filter;

    private bool HasFilters => ActiveFilterCount > 0;

    private string PageHeading => Filter.OnSaleOnly && Filter.CategoryIds.Count == 0
        ? "Sale"
        : Filter.CategoryIds.Count == 1 && State.FindCategory(Filter.CategoryIds.First()) is { } category
            ? category.Name
            : "Shop all";

    private IReadOnlyList<EcommerceProduct> PageItems
    {
        get
        {
            var matches = State.FilteredProducts();
            Filter.Page = Math.Clamp(Filter.Page, 1, Math.Max(1, (int)Math.Ceiling(matches.Count / (double)PageSize)));

            return matches.Skip((Filter.Page - 1) * PageSize).Take(PageSize).ToList();
        }
    }

    private string ResultsText
    {
        get
        {
            var count = State.FilteredProducts().Count;

            return count switch
            {
                0 => "No products found",
                1 => "1 product",
                _ when count <= PageSize => $"{count} products",
                _ => $"{count} products · page {Filter.Page} of {TotalPages}",
            };
        }
    }

    private int Sale => SaleQuery == "1" ? 1 : 0;

    /// <summary>Gets or sets whether to show sale items only, from <c>?sale=1</c>. A string, so a malformed value can't break query binding.</summary>
    [SupplyParameterFromQuery(Name = "sale")] public string? SaleQuery { get; set; }

    private IEnumerable<EcommerceCategory> SelectedCategories => State.Categories.Where(category => Filter.CategoryIds.Contains(category.Id));

    [Inject] private EcommerceState State { get; set; } = default!;

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(State.FilteredProducts().Count / (double)PageSize));

    #endregion
}
