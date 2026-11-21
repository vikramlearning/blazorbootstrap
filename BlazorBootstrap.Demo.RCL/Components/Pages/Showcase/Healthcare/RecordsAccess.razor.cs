using System.Globalization;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// "Who viewed your record" on the patient's Records page: every look at their chart or registration details by staff
/// (<see cref="HcState.RecordAccessOfMine" />, from the audit log the clinics keep) in a <see cref="Grid{TItem}" /> (when,
/// who, their role, and what they opened), newest first, with paging, sorting, the column chooser, and CSV export. A look
/// has no page of its own, so the rows don't open anything. The grid's view is saved for the visit and can be reset.
/// </summary>
public partial class RecordsAccess : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private Grid<AccessRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "my-record-access", DefaultPageSize, "when", SortDirection.Descending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of looks the grid asked for: newest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<AccessRow>> ProvideAccessAsync(GridDataProviderRequest<AccessRow> request) =>
        view.ApplyAsync(request, State.RecordAccessOfMine().Select(item => new AccessRow(item.At, HcDisplay.When(item.At), item.Actor, item.Role ?? "Staff", item.What)));

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the announcement.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        statusCount++;
        statusMessage = "The view of who viewed your record is reset: newest first, 10 a page, and every column shown."
            + (statusCount == 1 ? null : string.Create(CultureInfo.InvariantCulture, $" ({statusCount})"));
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(statusRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One look as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record AccessRow(DateTime At, string WhenText, string Actor, string Role, string What);

    #endregion
}
