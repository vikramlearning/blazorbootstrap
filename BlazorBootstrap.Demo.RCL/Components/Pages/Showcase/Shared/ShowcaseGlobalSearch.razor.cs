using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The header's global search for a site that implements <see cref="IShowcaseEnterpriseState" />. It searches as the
/// person types, lists the matching records as links, announces how many matched, and opens the first match on Enter.
/// Escape clears it, and it closes when the page changes.
/// </summary>
public partial class ShowcaseGlobalSearch : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int MaxTermLength = 100;

    private readonly string inputId = $"sc-search-{Guid.NewGuid():N}";

    private IReadOnlyList<ShowcaseSearchResult> results = Array.Empty<ShowcaseSearchResult>();

    private readonly string statusId = $"sc-search-status-{Guid.NewGuid():N}";

    private string term = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => Navigation.LocationChanged += OnLocationChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening for page changes.</summary>
    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;

    #endregion

    #region Private Methods

    private void Close()
    {
        term = string.Empty;
        results = Array.Empty<ShowcaseSearchResult>();
    }

    private void OnInput(ChangeEventArgs args)
    {
        term = (args.Value?.ToString() ?? string.Empty);

        if (term.Length > MaxTermLength)
            term = term[..MaxTermLength];

        results = string.IsNullOrWhiteSpace(term) || Enterprise is null
                      ? Array.Empty<ShowcaseSearchResult>()
                      : Enterprise.Search(term.Trim(), MaxResults);
    }

    private Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            Close();
        }
        else if (args.Key == "Enter" && results.Count > 0)
        {
            var url = results[0].Url;
            Close();
            Navigation.NavigateTo(url);
        }

        return Task.CompletedTask;
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs args)
    {
        if (term.Length == 0)
            return;

        Close();
        _ = InvokeAsync(StateHasChanged);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets extra CSS classes.</summary>
    [Parameter] public string? Class { get; set; }

    private IShowcaseEnterpriseState? Enterprise => Session.State as IShowcaseEnterpriseState;

    private bool IsPanelOpen => !string.IsNullOrWhiteSpace(term);

    /// <summary>Gets or sets the search box's accessible name.</summary>
    [Parameter] public string Label { get; set; } = "Search";

    /// <summary>Gets or sets how many matches to list.</summary>
    [Parameter] public int MaxResults { get; set; } = 8;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    /// <summary>Gets or sets the text shown when nothing matches.</summary>
    [Parameter] public string NoMatchText { get; set; } = "No matches";

    /// <summary>Gets or sets the search box placeholder, for example "Search tickets, people…".</summary>
    [Parameter] public string Placeholder { get; set; } = "Search";

    /// <summary>Gets or sets the accessible name of the results list.</summary>
    [Parameter] public string ResultsLabel { get; set; } = "Search results";

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    private string StatusText =>
        !IsPanelOpen ? string.Empty
        : results.Count == 0 ? NoMatchText
        : results.Count == 1 ? "1 match. Press Enter to open it."
        : $"{results.Count} matches. Press Enter to open the first.";

    #endregion
}
