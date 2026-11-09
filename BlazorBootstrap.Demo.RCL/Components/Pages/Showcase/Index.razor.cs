using BlazorBootstrap.Demo.RCL.Models.Showcase;
using BlazorBootstrap.Demo.RCL.Services.Showcase;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase;

/// <summary>
/// The Showcase gallery: every sample website with its screenshot, components, and links.
/// </summary>
public partial class Index : ComponentBase
{
    #region Fields and Constants

    private const string AllCategories = "All";

    private readonly IReadOnlyList<string> categories =
        new[] { AllCategories }.Concat(ShowcaseCatalog.Sites.Select(site => site.Category).Distinct()).ToList();

    private string selectedCategory = AllCategories;

    #endregion

    #region Methods

    #region Private Methods

    private void SelectCategory(string category) => selectedCategory = category;

    #endregion

    #endregion

    #region Properties, Indexers

    private IEnumerable<ShowcaseSite> VisibleSites => selectedCategory == AllCategories
        ? ShowcaseCatalog.Sites
        : ShowcaseCatalog.Sites.Where(site => site.Category == selectedCategory);

    #endregion
}
