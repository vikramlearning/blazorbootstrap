using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// My team (managers and HR): the manager's direct reports (<see cref="HrState.DirectReports" />) as a
/// <see cref="Grid{TItem}" />: name (a row opens the person's profile), title, status, approved leave today or in the next
/// 14 days in words, leave requests waiting for a decision (linking to Leave approvals), the first limited leave type's days
/// available (vacation), and onboarding progress when they have a plan, with paging, sorting, the column chooser, CSV
/// export, and a saved view that can be reset. Then "Who's away" for the next 4 weeks as a plain table (a schedule, not a
/// record list). The HR admin picks any manager's team with a select kept in <c>?manager=</c> (their own team, if they have
/// one, or the first manager by name, when there is none); an unknown ID shows <see cref="HrNotFound" />. A manager always
/// sees their own team.
/// </summary>
public partial class Team : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How far ahead the "Who's away" table looks, in days (today included).</summary>
    private const int AwayListDays = 28;

    private const int DefaultPageSize = 10;

    /// <summary>How far ahead each person's "away" fact looks, in days (today included).</summary>
    private const int SoonDays = 14;

    private Grid<TeamRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private bool focusStatus;

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

        if (focusStatus)
        {
            focusStatus = false;
            await HrDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "team", DefaultPageSize);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>The team's approved leave that overlaps the next <paramref name="days" /> days, soonest first.</summary>
    private IReadOnlyList<(HrEmployee Person, HrLeaveRequest Request)> AwaySoon(IReadOnlyList<HrEmployee> people, DateTime today, int days) =>
        people.SelectMany(person => State.LeaveOf(person.Id)
                .Where(item => item.Status == HrLeaveStatus.Approved && LeaveText.Overlaps(item, today, today.AddDays(days - 1)))
                .Select(item => (person, item)))
            .OrderBy(pair => pair.item.From)
            .ThenBy(pair => pair.person.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    /// <summary>Everyone with direct reports, by name (the HR admin's choices).</summary>
    private IReadOnlyList<HrEmployee> Managers() => State.Directory.Where(person => State.DirectReports(person.Id).Count > 0).ToList();

    /// <summary>A native select, so the team changes only when one is picked; the choice goes into the query.</summary>
    private void OnManagerChanged(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) || id == ManagerQuery)
            return;

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("manager", id), replace: true);
    }

    /// <summary>Leave, people, or onboarding changed (here or elsewhere): the grid reads the team again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of the team the grid asked for, by name unless a column is sorted.</summary>
    private Task<GridDataProviderResult<TeamRow>> ProvideTeamAsync(GridDataProviderRequest<TeamRow> request)
    {
        var today = State.Clock.Today;
        var lead = State.CurrentEmployee is { } me ? TeamLead(me, State.IsHrAdmin ? Managers() : Array.Empty<HrEmployee>()) : null;
        var rows = lead is null ? new List<TeamRow>() : State.DirectReports(lead.Id).Select(person => ToRow(person, today)).ToList();

        return view.ApplyAsync(request, rows);
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the status.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        statusMessage = statusMessage == ResetText ? ResetText + " (again)" : ResetText;
        focusStatus = true;
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>
    /// The manager whose team is shown: yourself (managers); for HR the one in the query, their own team, or the first
    /// manager by name; <see langword="null" /> for an unknown ID in the query.
    /// </summary>
    private HrEmployee? TeamLead(HrEmployee me, IReadOnlyList<HrEmployee> managers)
    {
        if (!State.IsHrAdmin)
            return me;

        if (ManagerQuery is { } id)
            return State.FindEmployee(id) is { Status: not HrEmployeeStatus.Left } chosen ? chosen : null;

        return State.DirectReports(me.Id).Count > 0 ? me : managers.FirstOrDefault() ?? me;
    }

    /// <summary>One direct report as a grid row, with the words the columns show and export.</summary>
    private TeamRow ToRow(HrEmployee person, DateTime today)
    {
        var requests = State.LeaveOf(person.Id);
        var awayNext = requests.Where(item => item.Status == HrLeaveStatus.Approved && item.To >= today && item.From <= today.AddDays(SoonDays - 1)).OrderBy(item => item.From).ToList();
        var away = awayNext.Count == 0
            ? "Not away"
            : string.Join("; ", awayNext.Select(request => $"{Capitalize(LeaveText.AwayText(request, today))} ({State.FindPolicy(request.PolicyId)?.Name ?? "leave"})"));
        var vacation = State.Balances(person.Id).FirstOrDefault(item => !item.Policy.IsUnlimited && item.Policy.IsActive);
        var plan = State.OnboardingOf(person.Id);
        var onboarding = "None";

        if (plan is not null)
        {
            var progress = HrState.Progress(plan);
            var overdue = plan.Tasks.Count(task => task.DoneAt is null && task.Due.Date < today);
            onboarding = string.Create(CultureInfo.InvariantCulture, $"{progress.Done} of {progress.Total} done") + (overdue > 0 ? string.Create(CultureInfo.InvariantCulture, $", {overdue} overdue") : null);
        }

        return new TeamRow(person.Id, person.Name, person.Initials, person.Title, HrDisplay.StatusLabel(person, today), away, awayNext.FirstOrDefault()?.From ?? DateTime.MaxValue,
            awayNext.Any(request => request.From <= today), requests.Count(item => item.Status == HrLeaveStatus.Pending),
            vacation is null ? "No limited leave type" : $"{HrDisplay.Days(vacation.Available)} of {vacation.Allowance}", vacation?.Available ?? -1, onboarding, plan is not null);
    }

    /// <summary>"Away now", "Tomorrow", or "In 5 days".</summary>
    private static string WhenText(HrLeaveRequest request, DateTime today)
    {
        var days = (int)(request.From - today).TotalDays;

        return days switch
        {
            <= 0 => "Away now",
            1 => "Tomorrow",
            _ => $"In {HrDisplay.Days(days)}",
        };
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>Gets or sets the manager's employee ID from the query (HR admin only).</summary>
    [SupplyParameterFromQuery(Name = "manager")] public int? ManagerQuery { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private static string ResetText => "The view is reset: the team by name, 10 a page, and every column shown.";

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One direct report as a grid row: the words each column shows and exports, and the values it sorts by.</summary>
    private sealed record TeamRow(int Id, string Name, string Initials, string Title, string Status, string Away, DateTime AwaySort, bool AwayNow, int Waiting, string Vacation,
        int VacationLeft, string Onboarding, bool HasPlan);

    #endregion
}
