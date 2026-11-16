using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The help center home: a "How can we help?" search over the articles' titles, summaries, and bodies with the results on
/// the page, topic tiles that open the knowledge base filtered by category, the most viewed articles, and a "Still need
/// help?" panel that fits the visitor: submit a request and My requests for a customer, sign-in first for a visitor, and
/// the agent desk for an agent.
/// </summary>
public partial class HelpCenter : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int MaxSearchLength = 100;

    private const int PopularCount = 5;

    private ElementReference resultsCount;

    private string search = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string SignInUrl(string returnPath, bool register) =>
        $"{HelpDeskLayout.SignInPath}?{(register ? "mode=register&" : string.Empty)}returnUrl={Uri.EscapeDataString(returnPath)}";

    private void OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        search = value.Length > MaxSearchLength ? value[..MaxSearchLength] : value;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Results already follow the typing; Enter or "Search" moves focus to the result count, which leads the list.</summary>
    private async Task SearchAsync()
    {
        if (Term.Length > 0)
            await HelpDeskDisplay.FocusAsync(resultsCount);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private IEnumerable<HelpDeskArticle> Popular => State.Articles.OrderByDescending(article => article.Views).Take(PopularCount);

    private string RegisterForRequestPath => SignInUrl(RequestNewPath, register: true);

    private string RequestNewPath => $"{HelpDeskLayout.Base}/requests/new";

    /// <summary>Gets the articles whose title, summary, or body contains every word of the search; title matches first, then by views.</summary>
    private IReadOnlyList<HelpDeskArticle> Results
    {
        get
        {
            var words = Term.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
                return Array.Empty<HelpDeskArticle>();

            return State.Articles
                .Where(article => words.All(word => article.Title.Contains(word, StringComparison.OrdinalIgnoreCase)
                    || article.Summary.Contains(word, StringComparison.OrdinalIgnoreCase)
                    || article.PlainText.Contains(word, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(article => words.Count(word => article.Title.Contains(word, StringComparison.OrdinalIgnoreCase)))
                .ThenByDescending(article => article.Views)
                .ToList();
        }
    }

    private string ResultsText => Term.Length == 0
        ? string.Empty
        : Results.Count switch
        {
            0 => $"No articles match “{Term}”",
            1 => $"1 article matches “{Term}”",
            var count => $"{count} articles match “{Term}”",
        };

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    private string SignInForRequestPath => SignInUrl(RequestNewPath, register: false);

    [Inject] private HelpDeskState State { get; set; } = default!;

    private string Term => search.Trim();

    #endregion
}
