using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The rows of one group on <see cref="MyTasks" />: the task key, the title (a link to the task), the project (a link to
/// its board), the status and priority in words, the due date in words (overdue with an icon and words), checklist
/// progress, and a "Mark done" or "Reopen" button whose name includes the key. The page does the move.
/// </summary>
public partial class MyTasksList : ComponentBase
{
    #region Properties, Indexers

    /// <summary>Gets or sets the group's ID, used in the row keys.</summary>
    [Parameter, EditorRequired] public string GroupId { get; set; } = string.Empty;

    /// <summary>Gets or sets the callback for Mark done and Reopen.</summary>
    [Parameter] public EventCallback<PbTask> OnToggleDone { get; set; }

    /// <summary>Gets or sets the tasks.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<PbTask> Tasks { get; set; } = Array.Empty<PbTask>();

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
