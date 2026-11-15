using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Knowledge base: search and a category filter over the seeded articles. <c>?category={id}</c> (the help center's topic
/// tiles) starts with that category selected; an unknown ID shows every category.
/// </summary>
public partial class KnowledgeBase : ComponentBase
{
    #region Fields and Constants

    /// <summary>The <c>?category=</c> value last applied, so the user's own choice stays until the query changes.</summary>
    private string? appliedCategoryQuery;

    private string categoryId = string.Empty;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnParametersSet()
    {
        if (CategoryQuery == appliedCategoryQuery)
            return;

        appliedCategoryQuery = CategoryQuery;
        categoryId = State.Categories.FirstOrDefault(category => category.Id == CategoryQuery)?.Id ?? string.Empty;
    }

    #endregion

    #region Private Methods

    private void OnCategoryChanged(string value) => categoryId = value;

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value;
        search = value;
    }

    private void OnSearchInput(ChangeEventArgs args) => search = args.Value?.ToString() ?? string.Empty;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the category to start with, from <c>?category=</c>.</summary>
    [SupplyParameterFromQuery(Name = "category")] public string? CategoryQuery { get; set; }

    private IReadOnlyList<HelpDeskArticle> Results
    {
        get
        {
            var term = search.Trim();

            return State.Articles
                .Where(article => categoryId.Length == 0 || article.CategoryId == categoryId)
                .Where(article => term.Length == 0
                    || article.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || article.Summary.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion
}
