using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The client list of Tandem (account managers): every client in a <see cref="Grid{TItem}" /> (name linking to the client,
/// website, industry, owner, primary contact, contact count, open deals and their value, active and total projects, billing
/// currency, contract end, last activity, and client since) with paging, sorting, the column chooser, and CSV export,
/// searched by name, industry, or a contact's name and filtered by owner and industry. The grid's view (page size, sort,
/// columns) is saved for the visit and can be reset. Rows can be selected and handed to another account manager together
/// (<see cref="PbState.BulkChangeClientOwner" />). New client opens the client form. A client deleted on its edit page comes
/// back here with <c>?deleted=1</c>, announced in the always-rendered status region.
/// </summary>
public partial class Clients : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private string? bulkError;

    private int bulkErrors;

    private string bulkOwner = string.Empty;

    private ElementReference? countRegion;

    private bool focusStatus;

    private Grid<ClientRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string industryFilter = string.Empty;

    /// <summary>The owner to show (a person ID), or empty for everyone.</summary>
    private string ownerFilter = string.Empty;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The clients selected in the grid (on its current page).</summary>
    private List<ClientRow> selected = new();

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

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

        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "clients", DefaultPageSize, "name", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        if (Deleted == 1)
            SetStatus("The client was deleted, with its contacts and activity.", false);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>
    /// Hands the selected clients to the chosen account manager in one <see cref="PbState.BulkChangeClientOwner" /> call, then
    /// refreshes the grid and clears the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync()
    {
        if (bulkOwner.Length == 0)
        {
            bulkErrors++;
            const string problem = "Choose the account manager to hand the selected clients to.";
            bulkError = bulkErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkErrors})");

            return;
        }

        bulkError = null;
        bulkErrors = 0;

        var owner = State.FindPerson(bulkOwner)?.Name ?? "the account manager";
        var outcome = State.BulkChangeClientOwner(selected.Select(row => row.Id).ToList(), bulkOwner);
        var message = outcome.Result switch
        {
            PbResult.Ok => $"Handed {PbDisplay.Plural(outcome.Changed, "client", "clients")} to {owner}."
                + (outcome.Unchanged > 0 ? $" {PbDisplay.Plural(outcome.Unchanged, "client was", "clients were")} already {owner}'s, or no longer there." : string.Empty),
            PbResult.NoChange => $"No clients changed: the selected clients are already {owner}'s.",
            PbResult.NotAllowed => "Only account managers can change client owners. Nothing changed.",
            PbResult.NotFound => "That person is no longer an account manager. Nothing changed.",
            _ => "Those changes couldn't be saved. Nothing changed.",
        };

        SetStatus(message, outcome.Result is not (PbResult.Ok or PbResult.NoChange));
        bulkOwner = string.Empty;

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<ClientRow>();
        focusStatus = true;
        StateHasChanged();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = ownerFilter = industryFilter = string.Empty;
        await FiltersChangedAsync();
        await PbDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<ClientRow>();
        bulkError = null;
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    /// <summary>A filter changed: back to the first page of the filtered clients.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Returns the clients that pass every filter, as grid rows, built from one pass over contacts, deals, projects, and activity.</summary>
    private IEnumerable<ClientRow> Matching()
    {
        var term = search.Trim();
        var contactsByClient = State.AllContacts.ToLookup(contact => contact.ClientId);
        var openDeals = State.Deals.Where(DealDisplay.IsOpen).ToLookup(deal => deal.ClientId);
        var projectsByClient = State.Projects.Where(project => project.ClientId is not null).ToLookup(project => project.ClientId!.Value);
        var lastActivity = State.Activities.GroupBy(activity => activity.ClientId).ToDictionary(group => group.Key, group => group.First());
        var names = State.People.ToDictionary(person => person.Id, person => person.Name);

        foreach (var client in State.Clients)
        {
            var clientContacts = contactsByClient[client.Id].ToList();

            if (term.Length > 0 && !(client.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || client.Industry.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || clientContacts.Any(contact => contact.Name.Contains(term, StringComparison.OrdinalIgnoreCase))))
                continue;

            if (ownerFilter.Length > 0 && client.OwnerId != ownerFilter)
                continue;

            if (industryFilter.Length > 0 && !string.Equals(client.Industry, industryFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            var primary = clientContacts.FirstOrDefault(contact => contact.IsPrimary);
            var deals = openDeals[client.Id].ToList();
            var projects = projectsByClient[client.Id].ToList();
            var active = projects.Count(project => project.Status == PbProjectStatus.Active);
            var last = lastActivity.GetValueOrDefault(client.Id);

            yield return new ClientRow(
                client.Id,
                client.Name,
                client.Website,
                client.Industry,
                client.OwnerId,
                names.GetValueOrDefault(client.OwnerId) ?? "Nobody",
                primary?.Name ?? string.Empty,
                primary?.Title ?? string.Empty,
                clientContacts.Count,
                deals.Count,
                deals.Sum(deal => (long)deal.Value),
                projects.Count,
                projects.Count == 0 ? "None" : active == 0 ? PbDisplay.Plural(projects.Count, "project", "projects") : $"{active} active of {projects.Count}",
                client.BillingCurrency,
                client.ContractEnd,
                last?.At,
                last is null ? string.Empty : DealDisplay.KindLabel(last.Kind),
                client.CreatedAt);
        }
    }

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    /// <summary>Filters on every keystroke without changing the box's bound value, so a slower render never overwrites newer typing.</summary>
    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<ClientRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            bulkError = null;
    }

    /// <summary>Another screen or a bulk change changed the state: the grid reads the clients again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of clients the grid asked for, from the filtered list, by name unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ClientRow>> ProvideClientsAsync(GridDataProviderRequest<ClientRow> request) => view.ApplyAsync(request, Shown);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<ClientRow>();
        gridVersion++;
        SetStatus("The view is reset: by name, 25 clients a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await PbDisplay.FocusAsync(countRegion);
    }

    private string RowClass(ClientRow row) => selected.Any(item => item.Id == row.Id) ? "tb-row-selected" : string.Empty;

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether a client was just deleted on its edit page.</summary>
    [SupplyParameterFromQuery(Name = "deleted")] public int? Deleted { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || ownerFilter.Length > 0 || industryFilter.Length > 0;

    private IReadOnlyList<ClientRow> Shown => Matching().ToList();

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One client as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record ClientRow(
        int Id,
        string Name,
        string Website,
        string Industry,
        string OwnerId,
        string Owner,
        string PrimaryContact,
        string PrimaryTitle,
        int Contacts,
        int OpenDeals,
        long OpenDealValue,
        int Projects,
        string ProjectsText,
        string Currency,
        DateTime? ContractEnd,
        DateTime? LastActivity,
        string LastActivityKind,
        DateTime Created)
    {
        /// <summary>Gets the contract's end date in words, or "No contract".</summary>
        public string ContractEndText => ContractEnd is { } end ? PbDisplay.Date(end) : "No contract";

        /// <summary>Gets when the client was added, as a date.</summary>
        public string CreatedText => PbDisplay.Date(Created);

        /// <summary>Gets the last activity's date and kind, as exported.</summary>
        public string LastActivityText => LastActivity is { } at ? $"{PbDisplay.Date(at)} ({LastActivityKind})" : "None";
    }

    #endregion
}
