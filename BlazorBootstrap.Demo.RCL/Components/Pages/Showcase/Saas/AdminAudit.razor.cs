using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The operator's audit log from <see cref="SaasState.AuditLog" />: what Lumora's operators did and what automatic billing
/// did, newest first, filtered by action and actor and searched by target and detail, 25 entries per page. A target with a
/// link opens the tenant, invoice, plan, or lead screen it refers to.
/// </summary>
public partial class AdminAudit : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int PageSize = 25;

    private string actionFilter = string.Empty;

    private string actorFilter = string.Empty;

    private Button? nextButton;

    private int pageNumber = 1;

    private Button? previousButton;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private TextInput? searchInput;

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

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static int TotalPages(int count) => Math.Max(1, (int)Math.Ceiling(count / (double)PageSize));

    /// <summary>
    /// The actor choices: the operator accounts by name, anyone else found in the log (an operator account no longer in the
    /// tab), and automatic billing last.
    /// </summary>
    private IReadOnlyList<Actor> Actors(IReadOnlyList<SaasAuditEntry> log)
    {
        var actors = State.Identity.Accounts
            .Where(account => account.RoleId == SaasSeedData.OperatorRole)
            .Select(account => new Actor(account.Id, account.Name))
            .ToList();

        foreach (var entry in log)
        {
            if (entry.ActorId != SaasState.SystemActorId && actors.All(actor => actor.Id != entry.ActorId))
                actors.Add(new Actor(entry.ActorId, entry.ActorName));
        }

        return actors
            .OrderBy(actor => actor.Name, StringComparer.OrdinalIgnoreCase)
            .Append(new Actor(SaasState.SystemActorId, "Automatic billing"))
            .ToList();
    }

    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = actionFilter = actorFilter = string.Empty;
        pageNumber = 1;
        StateHasChanged();

        // The button is gone once nothing is filtered, so focus moves to the search box.
        await Task.Yield();
        await SaasDisplay.FocusAsync(searchInput?.Element);
    }

    private IReadOnlyList<SaasAuditEntry> FilteredRows(IReadOnlyList<SaasAuditEntry> log)
    {
        IEnumerable<SaasAuditEntry> rows = log;
        var term = search.Trim();

        if (actionFilter.Length > 0)
            rows = rows.Where(entry => entry.Action == actionFilter);

        if (actorFilter.Length > 0)
            rows = rows.Where(entry => entry.ActorId == actorFilter);

        if (term.Length > 0)
            rows = rows.Where(entry => entry.Target.Contains(term, StringComparison.OrdinalIgnoreCase) || entry.Detail.Contains(term, StringComparison.OrdinalIgnoreCase));

        return rows.ToList();
    }

    /// <summary>
    /// Moves to a page. The button just pressed is disabled on the first and last page, so focus moves to the other one
    /// instead of being lost.
    /// </summary>
    private async Task GoToPageAsync(int target, int totalPages)
    {
        target = Math.Clamp(target, 1, totalPages);

        if (target == Math.Clamp(pageNumber, 1, totalPages))
            return;

        pageNumber = target;
        StateHasChanged();

        if (totalPages < 2)
            return;

        await Task.Yield();

        if (target == totalPages)
            await SaasDisplay.FocusAsync(previousButton?.Element);
        else if (target == 1)
            await SaasDisplay.FocusAsync(nextButton?.Element);
    }

    private void OnActionChanged(ChangeEventArgs args)
    {
        actionFilter = args.Value?.ToString() ?? string.Empty;
        pageNumber = 1;
    }

    private void OnActorChanged(ChangeEventArgs args)
    {
        actorFilter = args.Value?.ToString() ?? string.Empty;
        pageNumber = 1;
    }

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value;

        if (search == value)
            return;

        search = value;
        pageNumber = 1;
    }

    /// <summary>
    /// Filters on every keystroke without changing the box's bound value. On Blazor Server a render answering an earlier
    /// keystroke would otherwise write that older text back into the box and drop what was typed since.
    /// </summary>
    private void OnSearchInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (search == value)
            return;

        search = value;
        pageNumber = 1;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private static string SummaryText(int matches, int total, int page, int shown)
    {
        if (total == 0)
            return "The audit log is empty.";

        if (matches == 0)
            return $"No entries match these filters (0 of {Count(total)}).";

        var from = ((page - 1) * PageSize) + 1;
        var range = $"{Count(from)}–{Count(from + shown - 1)}";

        return matches == total
            ? $"Showing {range} of {Count(total)} entries, newest first."
            : $"{Count(matches)} of {Count(total)} entries match · showing {range}, newest first.";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private bool HasFilters => search.Trim().Length > 0 || actionFilter.Length > 0 || actorFilter.Length > 0;

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One choice of the actor filter.</summary>
    private sealed record Actor(string Id, string Name);

    #endregion
}
