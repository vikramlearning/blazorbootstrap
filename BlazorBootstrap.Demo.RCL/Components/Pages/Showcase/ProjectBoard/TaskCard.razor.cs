using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// A task card on a project board (<see cref="ProjectBoard" />): the task key, the title as a link to <see cref="TaskDetail" />,
/// the priority badge with its name, the assignee's initials (name in <c>title</c> and visually hidden text), the due date
/// in words (<see cref="PbDisplay.Due" />, overdue with an icon and words, not color alone), the estimate, and checklist
/// progress like "2/5". When <see cref="CanMove" /> is set it adds a labelled "Move to" select and, with
/// <see cref="CanReorder" />, Move up/down buttons: the keyboard and single-pointer alternative to dragging. The board
/// does the moves (<see cref="OnMoveTo" />, <see cref="OnReorder" />) and calls <see cref="FocusAsync" /> after it
/// re-renders, because the moved card is a new component in its new place.
/// </summary>
public partial class TaskCard : ComponentBase
{
    #region Fields and Constants

    private ElementReference? downButton;

    private ElementReference? moveSelect;

    private ElementReference? upButton;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Moves focus to one of the card's move controls; a disabled Move up/down falls back to the other one, then to the select.</summary>
    public async Task FocusAsync(TaskCardControl control)
    {
        var canUp = CanMove && CanReorder && Index > 0;
        var canDown = CanMove && CanReorder && Index < Count - 1;

        var target = control switch
        {
            TaskCardControl.Up when canUp => upButton,
            TaskCardControl.Up when canDown => downButton,
            TaskCardControl.Down when canDown => downButton,
            TaskCardControl.Down when canUp => upButton,
            _ => moveSelect,
        };

        await PbDisplay.FocusAsync(target);
    }

    #endregion

    #region Internal Methods

    /// <summary>Formats hours like "1 hour" or "16 hours".</summary>
    internal static string HoursText(int hours) => PbDisplay.Plural(hours, "hour");

    #endregion

    #region Private Methods

    /// <summary>The due date in words, or when a done task was finished.</summary>
    private string DueText(DateTime today) => Item.Status == PbTaskStatus.Done && Item.CompletedAt is { } completed
        ? "Done " + completed.ToString("MMM d", CultureInfo.InvariantCulture)
        : PbDisplay.Due(Item.DueDate, today);

    private async Task OnMoveToChanged(ChangeEventArgs args)
    {
        if (Enum.TryParse<PbTaskStatus>(args.Value?.ToString(), out var status))
            await OnMoveTo.InvokeAsync((Item, status));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether the card can be dragged now (shows a grab cursor).</summary>
    [Parameter] public bool CanDrag { get; set; }

    /// <summary>Gets or sets whether the card shows the "Move to" select (the board isn't read-only).</summary>
    [Parameter] public bool CanMove { get; set; }

    /// <summary>Gets or sets whether the card shows Move up/down (no filter is on, so the order is the column's).</summary>
    [Parameter] public bool CanReorder { get; set; }

    /// <summary>Gets or sets how many cards the column shows.</summary>
    [Parameter] public int Count { get; set; }

    /// <summary>Gets or sets the card's place in its column, from 0.</summary>
    [Parameter] public int Index { get; set; }

    /// <summary>Gets or sets the task.</summary>
    [Parameter, EditorRequired] public PbTask Item { get; set; } = default!;

    /// <summary>Gets or sets the callback for "Move to": the task and the chosen column.</summary>
    [Parameter] public EventCallback<(PbTask Task, PbTaskStatus Status)> OnMoveTo { get; set; }

    /// <summary>Gets or sets the callback for Move up (-1) and Move down (+1).</summary>
    [Parameter] public EventCallback<(PbTask Task, int Offset)> OnReorder { get; set; }

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}

/// <summary>A move control of a <see cref="TaskCard" />, to focus after the board re-renders.</summary>
public enum TaskCardControl
{
    /// <summary>The "Move to" select.</summary>
    MoveTo,

    /// <summary>The Move up button.</summary>
    Up,

    /// <summary>The Move down button.</summary>
    Down,
}
