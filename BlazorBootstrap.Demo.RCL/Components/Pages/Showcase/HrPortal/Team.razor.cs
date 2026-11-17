using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// My team (managers and HR): the manager's direct reports (<see cref="HrState.DirectReports" />), each with their title,
/// status, approved leave today or in the next 14 days in words, leave requests waiting for a decision, the first limited
/// leave type's days available (vacation), and onboarding progress when they have a plan, linked to their profile; then
/// "Who's away" for the next 4 weeks as a table. The HR admin picks any manager's team with a select kept in
/// <c>?manager=</c> (their own team, if they have one, or the first manager by name, when there is none); an unknown ID
/// shows <see cref="HrNotFound" />. A manager always sees their own team.
/// </summary>
public partial class Team : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>How far ahead the "Who's away" table looks, in days (today included).</summary>
    private const int AwayListDays = 28;

    /// <summary>How far ahead each person's "away" fact looks, in days (today included).</summary>
    private const int SoonDays = 14;

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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

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

    /// <summary>Gets or sets the manager's employee ID from the query (HR admin only).</summary>
    [SupplyParameterFromQuery(Name = "manager")] public int? ManagerQuery { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private HrState State { get; set; } = default!;

    #endregion
}
