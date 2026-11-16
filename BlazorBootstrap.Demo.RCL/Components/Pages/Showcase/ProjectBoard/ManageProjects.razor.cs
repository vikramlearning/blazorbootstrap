using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Manage projects (project managers): a table of every project (key, name linking to its editor, client, status in words,
/// start and due dates, team size, and tasks done as "x of y") with a filter by status and its counts, and Open board, Edit,
/// and Delete in each row. A callout at the top lists the projects that came from a won deal and are still planned with
/// nobody on the team ("New from a won deal"), each with a "Plan it" link to its editor, and their rows are marked too.
/// Delete opens a <see cref="Modal" /> that names the project and how many tasks go with it (the deal it came from stays
/// won); the deletion runs once the dialog is hidden, the outcome is announced in an always-rendered status region, and
/// focus returns to the Delete button, or to the status message when the row is gone.
/// </summary>
public partial class ManageProjects : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The route of this page.</summary>
    internal const string ListUrl = PbLayout.Base + "/manage/projects";

    /// <summary>The route of the new-project editor.</summary>
    internal const string NewUrl = ListUrl + "/new";

    /// <summary>The Delete buttons by project ID, for focus after the dialog.</summary>
    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    private Modal dialogModal = default!;

    /// <summary>The project the dialog is about.</summary>
    private int dialogProjectId;

    /// <summary>The status filter, or <see langword="null" /> for every project.</summary>
    private PbProjectStatus? filter;

    /// <summary>The project whose Delete button takes focus after the next render.</summary>
    private int? focusDeleteId;

    private bool focusStatus;

    /// <summary>Whether Delete was confirmed; it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusDeleteId is { } id)
        {
            focusDeleteId = null;

            if (deleteButtons.TryGetValue(id, out var button) && State.FindProject(id) is not null)
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

    #region Internal Methods

    /// <summary>Returns a project's client name, or "Internal (no client)".</summary>
    internal static string ClientName(PbState state, int? clientId) => state.FindClient(clientId)?.Name ?? (clientId is null ? "Internal (no client)" : "Unknown client");

    /// <summary>Returns the route of a project's editor.</summary>
    internal static string EditUrl(int projectId) => FormattableString.Invariant($"{ListUrl}/{projectId}/edit");

    /// <summary>
    /// Returns the projects a won deal created that still need planning: planned, with nobody on the team. These are what
    /// an account manager hands to the project managers.
    /// </summary>
    internal static IReadOnlyList<PbProject> NewFromDeals(PbState state) =>
        state.Projects.Where(project => project.FromDealId is not null && project.Status == PbProjectStatus.Planned && project.MemberIds.Count == 0).ToList();

    /// <summary>Returns a share as a percentage from 0 to 100 (0 when there is nothing to count).</summary>
    internal static double Percent(int part, int whole) => whole <= 0 ? 0 : Math.Clamp(part * 100.0 / whole, 0, 100);

    /// <summary>Returns a count with its noun, like "1 task" or "3 tasks".</summary>
    internal static string Plural(int count, string noun, string? plural = null) =>
        string.Create(CultureInfo.InvariantCulture, $"{count} {(count == 1 ? noun : plural ?? noun + "s")}");

    /// <summary>Returns tasks done in words, like "3 of 5 tasks done" or "No tasks yet".</summary>
    internal static string ProgressText(int done, int total) =>
        total == 0 ? "No tasks yet" : string.Create(CultureInfo.InvariantCulture, $"{done} of {Plural(total, "task")} done");

    /// <summary>Returns the badge color of a project status (always shown with its name).</summary>
    internal static BadgeColor StatusBadge(PbProjectStatus status) => PbDisplay.ProjectStatusColor(status) switch
    {
        "success" => BadgeColor.Success,
        "info" => BadgeColor.Info,
        "warning" => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private async Task ConfirmDeleteAsync()
    {
        pendingDelete = true;
        await dialogModal.HideAsync();
    }

    /// <summary>Says what deleting a project takes with it.</summary>
    private string DeleteConsequence(PbProject project)
    {
        var tasks = State.TasksOf(project.Id);
        var open = tasks.Count(task => task.Status != PbTaskStatus.Done);
        var deal = State.FindDeal(project.FromDealId);
        var taskText = tasks.Count == 0
            ? "It has no tasks."
            : $"Its {Plural(tasks.Count, "task")} {(tasks.Count == 1 ? "is" : "are")} deleted too ({open} open), with their comments and checklists.";
        var milestoneText = project.Milestones.Count == 0 ? null : $" Its {Plural(project.Milestones.Count, "milestone")} {(project.Milestones.Count == 1 ? "goes" : "go")} with it.";
        var dealText = deal is null ? null : $" The deal “{deal.Title}” it came from stays won.";

        return taskText + milestoneText + dealText;
    }

    private void OnDialogHidden()
    {
        var id = dialogProjectId;

        if (!pendingDelete)
        {
            focusDeleteId = id;

            return;
        }

        pendingDelete = false;

        if (State.FindProject(id) is not { } project)
        {
            Announce(false, "That project was already deleted, so nothing changed.");
            focusStatus = true;

            return;
        }

        var taskCount = State.TasksOf(id).Count;
        var deal = State.FindDeal(project.FromDealId);
        var label = $"{project.Key} · {project.Name}";
        var result = State.DeleteProject(id);

        switch (result)
        {
            case PbResult.Ok:
                Announce(true, $"Deleted {label} and its {Plural(taskCount, "task")}." + (deal is null ? null : $" The deal “{deal.Title}” stays won."));
                focusStatus = true;

                break;

            case PbResult.NotFound:
                Announce(false, "That project was already deleted, so nothing changed.");
                focusStatus = true;

                break;

            case PbResult.NotAllowed:
                Announce(false, "Only project managers can delete projects. Nothing was deleted.");
                focusStatus = true;

                break;

            default:
                Announce(false, $"{label} couldn't be deleted. Try again.");
                focusDeleteId = id;

                break;
        }
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteAsync(int projectId)
    {
        dialogProjectId = projectId;
        pendingDelete = false;
        await dialogModal.ShowAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private PbProject? DialogProject => State.FindProject(dialogProjectId);

    private static IReadOnlyList<FilterOption> FilterOptions { get; } = new[]
    {
        new FilterOption("All", null),
        new FilterOption("Active", PbProjectStatus.Active),
        new FilterOption("Planned", PbProjectStatus.Planned),
        new FilterOption("On hold", PbProjectStatus.OnHold),
        new FilterOption("Completed", PbProjectStatus.Completed),
    };

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>A status filter button.</summary>
    private sealed record FilterOption(string Text, PbProjectStatus? Status);

    #endregion
}
