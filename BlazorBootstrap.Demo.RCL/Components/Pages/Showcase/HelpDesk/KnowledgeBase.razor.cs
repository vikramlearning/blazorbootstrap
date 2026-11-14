using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// Knowledge base: search and a category filter over the seeded articles.
/// </summary>
public partial class KnowledgeBase : ComponentBase
{
    #region Fields and Constants

    private string categoryId = string.Empty;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    #endregion

    #region Methods

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
