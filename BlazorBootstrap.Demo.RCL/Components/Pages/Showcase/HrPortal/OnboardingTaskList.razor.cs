using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// One onboarding plan's tasks, shared by My onboarding and New hires: grouped into "Before day one", "First week", and
/// "Later" by due date against the new hire's start date, each with its owner in words, due date, overdue in words, and who
/// ticked it and when. Tasks the viewer may tick (<see cref="CanTick" />) are native checkboxes that call
/// <see cref="OnToggle" />; the page runs <see cref="HrState.SetTaskDone" /> and announces the outcome. When a tick is
/// refused, the checkbox is rebuilt from the state (the browser would otherwise keep the refused tick) and keeps focus.
/// </summary>
public partial class OnboardingTaskList : ComponentBase
{
    #region Fields and Constants

    /// <summary>The checkboxes, by task.</summary>
    private readonly Dictionary<int, ElementReference> checkboxes = new();

    /// <summary>The task whose checkbox takes focus after the next render.</summary>
    private int? focusTaskId;

    /// <summary>A counter per task, bumped to rebuild a checkbox after a refused tick.</summary>
    private readonly Dictionary<int, int> versions = new();

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTaskId is { } taskId)
        {
            focusTaskId = null;
            await HrDisplay.FocusAsync(checkboxes.TryGetValue(taskId, out var checkbox) ? checkbox : null);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Says who may tick a task, for a refused tick: "the new hire, Theo Martin", "their manager, Rafael Costa", "your
    /// manager", or "HR".
    /// </summary>
    public static string OwnerWho(HrTaskOwner owner, HrEmployee employee, HrEmployee? manager, int viewerId) => owner switch
    {
        HrTaskOwner.NewHire => employee.Id == viewerId ? "you" : $"the new hire, {employee.Name}",
        HrTaskOwner.Manager when manager is not null && manager.Id == viewerId => "you",
        HrTaskOwner.Manager when employee.Id == viewerId => manager is null ? "your manager" : $"your manager, {manager.Name}",
        HrTaskOwner.Manager => manager is null ? "their manager" : $"their manager, {manager.Name}",
        _ => "HR",
    };

    #endregion

    #region Private Methods

    /// <summary>"you", the person who ticked it, or "a former colleague".</summary>
    private string DoneByText(HrOnboardingTask task) =>
        task.DoneById is not { } doneById ? "someone"
        : doneById == ViewerId ? "you"
        : State.FindEmployee(doneById)?.Name ?? "a former colleague";

    /// <summary>"Nov 5, 2026" or "today, Nov 24, 2026".</summary>
    private string DueText(HrOnboardingTask task) => task.Due.Date == Today ? $"today, {HrDisplay.Date(task.Due)}" : HrDisplay.Date(task.Due);

    /// <summary>The groups, in order, by due date against the start date.</summary>
    private IReadOnlyList<TaskGroup> Groups()
    {
        var start = Employee.StartDate.Date;
        var tasks = Plan.Tasks.OrderBy(task => task.Due).ThenBy(task => task.Id).ToList();

        return new[]
        {
            new TaskGroup("before", "Before day one", tasks.Where(task => task.Due.Date < start).ToList()),
            new TaskGroup("week", "First week", tasks.Where(task => task.Due.Date >= start && task.Due.Date < start.AddDays(7)).ToList()),
            new TaskGroup("later", "Later", tasks.Where(task => task.Due.Date >= start.AddDays(7)).ToList()),
        };
    }

    private HrEmployee? Manager() => Employee.ManagerId is { } managerId ? State.FindEmployee(managerId) : null;

    /// <summary>"New hire (you)", "Manager (Rafael Costa)", or "HR".</summary>
    private string OwnerText(HrTaskOwner owner)
    {
        var label = HrDisplay.OwnerLabel(owner);
        var manager = Manager();

        return owner switch
        {
            HrTaskOwner.NewHire => $"{label} ({(Employee.Id == ViewerId ? "you" : Employee.Name)})",
            HrTaskOwner.Manager when manager is not null => $"{label} ({(manager.Id == ViewerId ? "you" : manager.Name)})",
            _ => label,
        };
    }

    /// <summary>Hands the tick to the page; a refused one rebuilds the checkbox from the state and keeps focus on it.</summary>
    private void Toggle(HrOnboardingTask task, ChangeEventArgs args)
    {
        var done = args.Value is bool value ? value : bool.TryParse(args.Value?.ToString(), out var parsed) && parsed;
        var result = OnToggle?.Invoke(task, done) ?? HrResult.NotAllowed;

        if (result is not (HrResult.Ok or HrResult.NoChange))
        {
            versions[task.Id] = versions.GetValueOrDefault(task.Id) + 1;
            focusTaskId = task.Id;
        }
    }

    /// <summary>"Your manager does this", "Theo does this", or "HR does this", for a task the viewer can't tick.</summary>
    private string WhoDoesThis(HrTaskOwner owner) => owner switch
    {
        HrTaskOwner.NewHire => $"{LeaveText.FirstName(Employee)} does this",
        HrTaskOwner.Manager when Employee.Id == ViewerId => "Your manager does this",
        HrTaskOwner.Manager => Manager() is { } manager ? $"{manager.Name} does this" : "Their manager does this",
        _ => "HR does this",
    };

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether the viewer may tick a task (others are read-only).</summary>
    [Parameter] public Func<HrOnboardingTask, bool>? CanTick { get; set; }

    /// <summary>Gets or sets the new hire.</summary>
    [Parameter, EditorRequired] public HrEmployee Employee { get; set; } = default!;

    /// <summary>Gets or sets the level of the group headings (2 on My onboarding, 3 under a person on New hires).</summary>
    [Parameter] public int HeadingLevel { get; set; } = 2;

    /// <summary>Gets or sets the prefix of element IDs and keys, unique on the page.</summary>
    [Parameter, EditorRequired] public string IdPrefix { get; set; } = "hr-ob";

    /// <summary>Gets or sets the tick handler: the task and whether it is now done; returns the outcome.</summary>
    [Parameter] public Func<HrOnboardingTask, bool, HrResult>? OnToggle { get; set; }

    /// <summary>Gets or sets the plan.</summary>
    [Parameter, EditorRequired] public HrOnboarding Plan { get; set; } = default!;

    [Inject] private HrState State { get; set; } = default!;

    /// <summary>Gets or sets today on the site's clock.</summary>
    [Parameter] public DateTime Today { get; set; }

    /// <summary>Gets or sets the signed-in person's employee ID, to say "you".</summary>
    [Parameter] public int ViewerId { get; set; }

    #endregion

    #region Nested Types

    /// <summary>A group of tasks.</summary>
    /// <param name="Id">The ID used in element IDs and keys.</param>
    /// <param name="Title">The heading.</param>
    /// <param name="Tasks">The tasks, by due date.</param>
    private sealed record TaskGroup(string Id, string Title, IReadOnlyList<HrOnboardingTask> Tasks);

    #endregion
}
