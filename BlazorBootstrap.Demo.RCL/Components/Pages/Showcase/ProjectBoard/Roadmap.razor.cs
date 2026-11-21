using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>The kind of a node on the roadmap's mind map.</summary>
public enum RoadmapNodeKind
{
    /// <summary>The root: the agency's roadmap.</summary>
    Root,

    /// <summary>A project.</summary>
    Project,

    /// <summary>A milestone of a project.</summary>
    Milestone,

    /// <summary>An open task that counts toward a milestone.</summary>
    Task,
}

/// <summary>
/// A node of the roadmap's mind map. Nodes are equal when their <see cref="Id" />s are, so the map keeps what is open and
/// selected when the tree is rebuilt after a change.
/// </summary>
/// <param name="Id">A stable ID, like "p-3", "m-3-2", or "t-41".</param>
/// <param name="Label">The label (also what the map's search matches).</param>
/// <param name="Kind">What the node stands for.</param>
/// <param name="Url">The page it links to from the details panel, if any.</param>
/// <param name="Children">The child nodes.</param>
public sealed record RoadmapNode(string Id, string Label, RoadmapNodeKind Kind, string? Url, IReadOnlyList<RoadmapNode> Children)
{
    /// <summary>Gets the milestone, for a milestone or task node.</summary>
    public int? MilestoneId { get; init; }

    /// <summary>Gets the status and progress in words, shown under the label.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>Gets the project, for every node but the root.</summary>
    public int? ProjectId { get; init; }

    /// <summary>Gets the task, for a task node.</summary>
    public int? TaskId { get; init; }

    /// <summary>Returns whether another node has the same ID.</summary>
    public bool Equals(RoadmapNode? other) => other is not null && string.Equals(Id, other.Id, StringComparison.Ordinal);

    /// <summary>Returns the hash of the ID.</summary>
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Id);
}

/// <summary>
/// Roadmap (project managers): a Blazor.Bootstrap <see cref="MindMap{TItem}" /> with the root "Brightline roadmap", each
/// project that isn't completed ("KEY · Name", with its status and tasks done), its milestones (due date, tasks done as
/// "3/5", and Done, Missed, or Upcoming in words), and, when the checkbox is ticked, each milestone's open tasks. Selecting
/// a node shows a details panel with links to the board, the project's editor, and the tasks. Because the map alone is not
/// enough for every reader, the same plan is listed as a timeline: each project from start to due date with its milestones
/// in date order. A planned project created from a won deal shows on both, marked as needing a team. Both open on the
/// signed-in project manager's own projects; "Projects of" shows another project manager's, or everyone's.
/// </summary>
public partial class Roadmap : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The project manager whose projects are shown (a person ID), or empty for everyone's.</summary>
    private string managerFilter = string.Empty;

    private RoadmapNode? selected;

    private bool showTasks;

    private RoadmapNode tree = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        managerFilter = State.CurrentPerson?.Id ?? string.Empty;
        tree = BuildTree();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static bool IsFresh(PbProject project) => project.FromDealId is not null && project.Status == PbProjectStatus.Planned && project.MemberIds.Count == 0;

    private static (PbProject Project, PbMilestone Milestone)? NextMilestone(IReadOnlyList<PbProject> projects, DateTime today) =>
        projects.SelectMany(project => project.Milestones.Select(milestone => (Project: project, Milestone: milestone)))
            .Where(item => item.Milestone.Due >= today)
            .OrderBy(item => item.Milestone.Due)
            .Cast<(PbProject Project, PbMilestone Milestone)?>()
            .FirstOrDefault();

    /// <summary>Builds the mind map's tree from the state.</summary>
    private RoadmapNode BuildTree()
    {
        var today = State.Clock.Today;
        var projects = RoadmapProjects.Select(project =>
        {
            var progress = State.ProjectProgress(project.Id);
            var tasks = State.TasksOf(project.Id);
            var milestones = project.Milestones.OrderBy(item => item.Due).Select(milestone =>
            {
                var (done, total) = State.MilestoneProgress(project.Id, milestone.Id);
                var children = showTasks
                    ? tasks.Where(task => task.MilestoneId == milestone.Id && task.Status != PbTaskStatus.Done).OrderBy(task => task.DueDate ?? DateTime.MaxValue)
                        .Select(task => new RoadmapNode(Invariant($"t-{task.Id}"), $"{State.TaskKey(task)} {task.Title}", RoadmapNodeKind.Task, PbDisplay.TaskUrl(task.Id), Array.Empty<RoadmapNode>())
                        {
                            Note = $"{PbDisplay.StatusLabel(task.Status)} · {PbDisplay.Due(task.DueDate, today)} · {State.FindPerson(task.AssigneeId)?.Name ?? "unassigned"}",
                            ProjectId = project.Id,
                            MilestoneId = milestone.Id,
                            TaskId = task.Id,
                        })
                        .ToList()
                    : new List<RoadmapNode>();

                return new RoadmapNode(Invariant($"m-{project.Id}-{milestone.Id}"), milestone.Title, RoadmapNodeKind.Milestone, ManageProjects.EditUrl(project.Id), children)
                {
                    Note = Invariant($"{PbDisplay.Date(milestone.Due)} · {done}/{total} tasks · {MilestoneState(project, milestone, today)}"),
                    ProjectId = project.Id,
                    MilestoneId = milestone.Id,
                };
            }).ToList();

            return new RoadmapNode(Invariant($"p-{project.Id}"), $"{project.Key} · {project.Name}", RoadmapNodeKind.Project, PbDisplay.ProjectUrl(project.Id), milestones)
            {
                Note = $"{PbDisplay.ProjectStatusLabel(project.Status)} · {ManageProjects.ProgressText(progress.Done, progress.Total)}"
                       + (IsFresh(project) ? " · new from a won deal" : project.Milestones.Count == 0 ? " · no milestones yet" : null),
                ProjectId = project.Id,
            };
        }).ToList();

        return new RoadmapNode("root", $"{PbSeedData.AgencyName.Split(' ')[0]} roadmap", RoadmapNodeKind.Root, null, projects)
        {
            Note = PbDisplay.Plural(projects.Count, "project") + " in progress or planned",
        };
    }

    private static string Invariant(FormattableString value) => FormattableString.Invariant(value);

    /// <summary>Says where a milestone stands, by its tasks and its due date: Done, Missed, Due today, or Upcoming.</summary>
    private string MilestoneState(PbProject project, PbMilestone milestone, DateTime today)
    {
        var (done, total) = State.MilestoneProgress(project.Id, milestone.Id);

        return total > 0 && done == total ? "Done"
            : milestone.Due.Date < today ? (total == 0 ? "Passed, no tasks counted" : "Missed")
            : milestone.Due.Date == today ? "Due today"
            : "Upcoming";
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        tree = BuildTree();
        StateHasChanged();
    });

    /// <summary>Shows one project manager's projects (or everyone's) and builds the map again.</summary>
    private void SetManager(string? personId)
    {
        managerFilter = personId ?? string.Empty;
        selected = null;
        tree = BuildTree();
    }

    private void SetShowTasks(bool value)
    {
        showTasks = value;
        tree = BuildTree();
    }

    /// <summary>A project's timeline: its start, its milestones, and its due date, in date order.</summary>
    private IReadOnlyList<TimelineEntry> TimelineOf(PbProject project, DateTime today)
    {
        var entries = new List<TimelineEntry>
        {
            new("start", project.StartDate, project.StartDate <= today ? "Started" : "Starts", project.StartDate <= today ? "The project began" : "The project begins", "edge"),
        };

        foreach (var milestone in project.Milestones)
        {
            var (done, total) = State.MilestoneProgress(project.Id, milestone.Id);
            var state = MilestoneState(project, milestone, today);
            var tone = state switch { "Done" => "done", "Missed" => "missed", _ => milestone.Due.Date < today ? "past" : "upcoming" };
            entries.Add(new(Invariant($"m-{milestone.Id}"), milestone.Due, milestone.Title, $"Milestone · {state} · {ManageProjects.ProgressText(done, total)}", tone));
        }

        entries.Add(new("due", project.DueDate, "Due", project.DueDate < today ? "The project's due date has passed" : "The project is due", "edge"));

        return entries.OrderBy(entry => entry.Date).ThenBy(entry => entry.Key == "start" ? 0 : entry.Key == "due" ? 2 : 1).ToList();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the projects on the roadmap: every one that isn't completed, of the chosen project manager (or everyone's).</summary>
    private IReadOnlyList<PbProject> RoadmapProjects =>
        State.Projects.Where(project => project.Status != PbProjectStatus.Completed && (managerFilter.Length == 0 || project.OwnerId == managerFilter)).ToList();

    /// <summary>Gets the project of the selected node, if it is still on the roadmap.</summary>
    private PbProject? SelectedProject => selected?.ProjectId is { } id && State.FindProject(id) is { Status: not PbProjectStatus.Completed } project ? project : null;

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One line of a project's timeline.</summary>
    private sealed record TimelineEntry(string Key, DateTime Date, string Title, string Note, string Tone);

    #endregion
}
