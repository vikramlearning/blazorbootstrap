using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Operations audit log: every recorded customer, operations, and system action, newest first, in a plain table (caption
/// and header scopes) with the time, actor, role in words, action, subject, and details. Filtered by role and action and
/// searched by actor, subject, and details, with a live count; shown 50 at a time. The log is read-only: entries can't be
/// changed or deleted.
/// </summary>
public partial class OpsAudit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How many more rows "Show more" adds.</summary>
    private const int PageSize = 50;

    /// <summary>The roles an entry can be recorded under, as stored.</summary>
    private static readonly string[] Roles = { "Customer", "Operations", "System" };

    private string actionFilter = string.Empty;

    private ElementReference firstNewCell;

    /// <summary>The index of the first row the last "Show more" added, until focus has moved to it; -1 otherwise.</summary>
    private int firstNewIndex = -1;

    private bool focusFirstNew;

    private bool focusSearch;

    private string roleFilter = string.Empty;

    private string search = string.Empty;

    private TextInput? searchInput;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private int shownCount = PageSize;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusFirstNew)
        {
            focusFirstNew = false;
            await FintechDisplay.FocusAsync(firstNewCell);
        }

        // "Clear filters" leaves the page once used, so focus goes to the search box rather than being lost.
        if (focusSearch)
        {
            focusSearch = false;
            await FintechDisplay.FocusAsync(searchInput?.Element);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the role an audit entry was recorded under, in words.</summary>
    internal static string RoleText(string role) => role switch
    {
        "Customer" => "Customer",
        "Operations" => "Operations",
        "System" => "System",
        _ => string.IsNullOrWhiteSpace(role) ? "Unknown" : role,
    };

    #endregion

    #region Private Methods

    private void ClearFilters()
    {
        actionFilter = string.Empty;
        roleFilter = string.Empty;
        search = string.Empty;
        searchBoxValue = string.Empty;
        focusSearch = true;
        ResetPaging();
    }

    private IEnumerable<FintechAuditEntry> Filtered()
    {
        var term = search.Trim();

        return State.AuditLog.Where(entry =>
            (roleFilter.Length == 0 || entry.Role == roleFilter)
            && (actionFilter.Length == 0 || entry.Action == actionFilter)
            && (term.Length == 0
                || entry.Actor.Contains(term, StringComparison.OrdinalIgnoreCase)
                || entry.Subject.Contains(term, StringComparison.OrdinalIgnoreCase)
                || entry.Details.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    private void OnActionChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        actionFilter = Actions.Contains(value) ? value : string.Empty;
        ResetPaging();
    }

    private void OnRoleChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        roleFilter = Roles.Contains(value) ? value : string.Empty;
        ResetPaging();
    }

    private void OnSearchChanged(string value)
    {
        searchBoxValue = value ?? string.Empty;
        OnSearchValue(searchBoxValue);
    }

    private void OnSearchInput(ChangeEventArgs args) => OnSearchValue(args.Value?.ToString() ?? string.Empty);

    private void OnSearchValue(string value)
    {
        if (search == value)
            return;

        search = value;
        ResetPaging();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>A changed filter starts the list from the top again.</summary>
    private void ResetPaging()
    {
        shownCount = PageSize;
        firstNewIndex = -1;
    }

    private void ShowMore()
    {
        firstNewIndex = shownCount;
        shownCount += PageSize;
        focusFirstNew = true;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the distinct actions in the log, in alphabetical order, for the action filter.</summary>
    private IReadOnlyList<string> Actions => State.AuditLog
        .Select(entry => entry.Action)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(action => action, StringComparer.OrdinalIgnoreCase)
        .ToList();

    /// <summary>Gets the table caption: what the rows are, with the filters applied.</summary>
    private string CaptionText
    {
        get
        {
            var parts = new List<string> { "Audit log entries", roleFilter.Length == 0 ? "all roles" : RoleText(roleFilter) };

            if (actionFilter.Length > 0)
                parts.Add(actionFilter);

            if (search.Trim().Length > 0)
                parts.Add($"matching \"{search.Trim()}\"");

            return string.Join(", ", parts) + ", newest first";
        }
    }

    private int FilteredCount => Filtered().Count();

    private bool HasFilters => actionFilter.Length > 0 || roleFilter.Length > 0 || search.Trim().Length > 0;

    private string MatchText => FilteredCount switch
    {
        0 => "No entries found",
        1 => "1 entry found",
        var count => string.Create(CultureInfo.InvariantCulture, $"{count} entries found"),
    };

    private IReadOnlyList<FintechAuditEntry> Shown => Filtered().Take(shownCount).ToList();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
