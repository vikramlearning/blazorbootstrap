using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Workload (project managers): one card per person from <see cref="PbState.Workload" />, the busiest against capacity
/// first, with the hours due this week (by Sunday, or overdue) against their weekly capacity as a bar and in words ("38 of
/// 32 h this week — over capacity by 6 h"), the hours due later, and their open tasks. A person's card expands (a button
/// with <c>aria-expanded</c>) to a table of their open tasks (key, title linking to the task, project, due, estimate) with
/// an assignee select of the task's project members and a Reassign button (<see cref="PbState.Reassign" />); the outcome is
/// announced in an always-rendered status region and focus returns to the person's button. A bar chart compares hours this
/// week with capacity, with a text summary and a data table. A checkbox hides people with no open work.
/// </summary>
public partial class Workload : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The assignee picked in each task's select, by task ID ("" for unassigned), until Reassign is pressed.</summary>
    private readonly Dictionary<int, string> drafts = new();

    /// <summary>The people whose open tasks are shown.</summary>
    private readonly HashSet<string> expanded = new();

    /// <summary>The person whose button takes focus after the next render.</summary>
    private string? focusPersonId;

    private bool focusStatus;

    private bool hideIdle;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    /// <summary>The expand buttons by person ID, for focus after a reassignment.</summary>
    private readonly Dictionary<string, ElementReference> toggleButtons = new();

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusPersonId is { } id)
        {
            focusPersonId = null;

            if (toggleButtons.TryGetValue(id, out var button) && State.Workload().Any(row => row.Person.Id == id && row.Open.Count > 0))
                await PbDisplay.FocusAsync(button);
            else
                focusStatus = true;
        }

        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>The bar labels: first names, or full names when two people share one.</summary>
    private static IReadOnlyList<string> ChartLabels(IReadOnlyList<PbWorkload> rows)
    {
        var first = rows.Select(row => row.Person.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? row.Person.Name).ToList();

        return first.Distinct(StringComparer.OrdinalIgnoreCase).Count() == first.Count ? first : rows.Select(row => row.Person.Name).ToList();
    }

    private static IReadOnlyList<ReportsChartSeries> ChartSeries(IReadOnlyList<PbWorkload> rows) => new[]
    {
        new ReportsChartSeries("Hours this week", rows.Select(row => (double)row.HoursThisWeek).ToList(), 0),
        new ReportsChartSeries("Weekly capacity", rows.Select(row => (double)row.Person.CapacityHours).ToList(), -1),
    };

    private static string Hours(int hours) => string.Create(CultureInfo.InvariantCulture, $"{hours} h");

    private static ProgressColor LoadColor(PbWorkload row) =>
        row.HoursThisWeek > row.Person.CapacityHours ? ProgressColor.Danger
        : row.HoursThisWeek * 10 >= row.Person.CapacityHours * 8 && row.HoursThisWeek > 0 ? ProgressColor.Warning
        : ProgressColor.Primary;

    /// <summary>Describes a person's load this week in words, like "38 of 32 h this week — over capacity by 6 h".</summary>
    private static string LoadText(PbWorkload row)
    {
        var hours = row.HoursThisWeek;
        var capacity = row.Person.CapacityHours;
        var head = string.Create(CultureInfo.InvariantCulture, $"{hours} of {capacity} h this week");

        return hours > capacity ? head + string.Create(CultureInfo.InvariantCulture, $" — over capacity by {hours - capacity} h")
            : hours == capacity ? head + (hours == 0 ? " — free" : " — at capacity")
            : hours * 10 >= capacity * 8 && hours > 0 ? head + string.Create(CultureInfo.InvariantCulture, $" — nearly full, {capacity - hours} h free")
            : head + string.Create(CultureInfo.InvariantCulture, $" — {capacity - hours} h free");
    }

    private static string Summary(IReadOnlyList<PbWorkload> rows)
    {
        var over = rows.Where(row => row.HoursThisWeek > row.Person.CapacityHours).ToList();
        var due = rows.Sum(row => row.HoursThisWeek);
        var capacity = rows.Sum(row => row.Person.CapacityHours);
        var overText = over.Count == 0
            ? "Nobody is over capacity this week."
            : $"{ManageProjects.Plural(over.Count, "person is", "people are")} over capacity this week: "
              + string.Join(", ", over.Select(row => string.Create(CultureInfo.InvariantCulture, $"{row.Person.Name} ({row.HoursThisWeek} of {row.Person.CapacityHours} h)"))) + ".";
        var busiest = rows.MaxBy(row => row.HoursThisWeek + row.HoursLater);

        return overText
            + string.Create(CultureInfo.InvariantCulture, $" In all, {due} h are due this week against {capacity} h of capacity, and {rows.Sum(row => row.HoursLater)} h later.")
            + (busiest is { HoursThisWeek: > 0 } or { HoursLater: > 0 } ? $" {busiest!.Person.Name} has the most open work: {Hours(busiest.HoursThisWeek + busiest.HoursLater)}." : null);
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private string Draft(PbTask task) => drafts.TryGetValue(task.Id, out var draft) ? draft : task.AssigneeId ?? string.Empty;

    /// <summary>The people a project's tasks can be assigned to, by name.</summary>
    private IReadOnlyList<PbPerson> Members(PbProject? project) =>
        project is null ? Array.Empty<PbPerson>() : State.People.Where(person => project.MemberIds.Contains(person.Id)).ToList();

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Assigns a task to the person picked in its select, announces it, and returns focus to the card's button.</summary>
    private void Reassign(int taskId, string fromPersonId)
    {
        focusPersonId = fromPersonId;

        if (State.FindTask(taskId) is not { } task)
        {
            drafts.Remove(taskId);
            Announce(false, "That task was deleted, so nothing changed.");

            return;
        }

        var key = State.TaskKey(task);
        var draft = Draft(task);
        var toId = draft.Length == 0 ? null : draft;
        var from = State.FindPerson(task.AssigneeId);
        var to = State.FindPerson(toId);
        var result = State.Reassign(taskId, toId);
        drafts.Remove(taskId);

        switch (result)
        {
            case PbResult.Ok:
                var toLoad = to is null ? null : State.Workload().FirstOrDefault(row => row.Person.Id == to.Id);
                Announce(true, to is null
                    ? $"{key} “{task.Title}” is now unassigned{(from is null ? null : $" (it was {from.Name}'s)")}. Find it on its project's board."
                    : $"Reassigned {key} “{task.Title}”{(from is null ? null : $" from {from.Name}")} to {to.Name}." + (toLoad is null ? null : $" {to.Name} now has {LoadText(toLoad)}."));

                break;

            case PbResult.NoChange:
                Announce(true, $"{key} is already {(to is null ? "unassigned" : $"assigned to {to.Name}")}, so nothing changed.");

                break;

            case PbResult.WrongState when State.FindProject(task.ProjectId)?.Status == PbProjectStatus.Completed:
                Announce(false, $"{State.FindProject(task.ProjectId)!.Key} is completed, so {key} is read-only. Nothing changed.");

                break;

            case PbResult.WrongState:
                Announce(false, $"{to?.Name ?? "That person"} isn't on the project of {key}, so it can't be assigned to them. Add them to the team in Manage projects first.");

                break;

            case PbResult.NotFound:
                Announce(false, "That task was deleted, so nothing changed.");

                break;

            case PbResult.NotAllowed:
                Announce(false, "Sign in to reassign tasks. Nothing changed.");

                break;

            default:
                Announce(false, $"{key} couldn't be reassigned. Try again.");

                break;
        }
    }

    private void SetDraft(int taskId, string? personId) => drafts[taskId] = personId ?? string.Empty;

    private void Toggle(string personId)
    {
        if (!expanded.Remove(personId))
            expanded.Add(personId);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
