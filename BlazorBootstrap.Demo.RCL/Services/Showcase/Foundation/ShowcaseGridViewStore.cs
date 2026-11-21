using System.Text.Json;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// Keeps each Showcase list's saved view (filters, page, page size, hidden and reordered columns, and sort) for the visit,
/// in the tab's session storage, so it survives navigation and a reload like the site's other demo data (rule WR-001).
/// One instance per circuit (Server) or app (WebAssembly).
/// </summary>
public sealed class ShowcaseGridViewStore
{
    #region Fields and Constants

    private const int MaxViews = 200;

    private const string StorageKey = "showcase-grid-views";

    private readonly IJSRuntime js;

    private Dictionary<string, ShowcaseGridViewState>? views;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Creates the store.</summary>
    public ShowcaseGridViewStore(IJSRuntime js) => this.js = js;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Gets the handle of one list's saved view.
    /// </summary>
    /// <param name="siteSlug">The site's slug, for example <c>help-desk</c>.</param>
    /// <param name="viewKey">The list's key within the site, for example <c>ticket-queue</c>.</param>
    /// <param name="defaultPageSize">The list's page size when nothing is saved.</param>
    /// <param name="defaultSortKey">The <c>SortString</c> of the column the list is sorted by when nothing is saved.</param>
    /// <param name="defaultSortDirection">The default sort direction.</param>
    public ShowcaseGridView Get(string siteSlug, string viewKey, int defaultPageSize = 10, string? defaultSortKey = null, SortDirection defaultSortDirection = SortDirection.Ascending) =>
        new(this, $"{siteSlug}/{viewKey}", defaultPageSize, defaultSortKey, defaultSortDirection);

    #endregion

    #region Internal Methods

    internal async Task<ShowcaseGridViewState?> GetAsync(string key)
    {
        await EnsureLoadedAsync();

        return views!.TryGetValue(key, out var state) ? state : null;
    }

    internal ShowcaseGridViewState? GetLoaded(string key) => views is not null && views.TryGetValue(key, out var state) ? state : null;

    internal async Task RemoveAsync(string key)
    {
        await EnsureLoadedAsync();

        if (views!.Remove(key))
            await SaveAsync();
    }

    internal async Task UpdateAsync(string key, Action<ShowcaseGridViewState> update)
    {
        await EnsureLoadedAsync();

        if (!views!.TryGetValue(key, out var state))
        {
            if (views.Count >= MaxViews)
                return;

            views[key] = state = new ShowcaseGridViewState();
        }

        update(state);
        await SaveAsync();
    }

    #endregion

    #region Private Methods

    private async Task EnsureLoadedAsync()
    {
        if (views is not null)
            return;

        views = new Dictionary<string, ShowcaseGridViewState>(StringComparer.Ordinal);

        try
        {
            var json = await js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);

            if (string.IsNullOrWhiteSpace(json))
                return;

            var stored = JsonSerializer.Deserialize<Dictionary<string, ShowcaseGridViewState>>(json);

            if (stored is null)
                return;

            // Keep only well-formed entries; a bad entry is dropped rather than breaking the list.
            foreach (var (key, state) in stored.Take(MaxViews))
                if (!string.IsNullOrWhiteSpace(key) && key.Length <= 200 && state is not null && Enum.IsDefined(state.SortDirection))
                    views[key] = state;
        }
        catch (Exception ex) when (ex is JsonException or JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException)
        {
            // Storage can't be read (prerendering, blocked storage, or bad data): views work in memory for this visit.
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            await js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, JsonSerializer.Serialize(views));
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException)
        {
            // Storage can't be written; the view still holds for this page.
        }
    }

    #endregion

    #endregion
}

