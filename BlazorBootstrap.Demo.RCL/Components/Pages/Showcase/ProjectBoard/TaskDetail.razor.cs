using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// A task's page: a breadcrumb (Project boards, the project, the key), the title, and its details. The Status select moves
/// it to the end of that column (<see cref="PbState.MoveTask" />), the Assignee select reassigns it to one of the project's
/// people (<see cref="PbState.Reassign" />), and "Edit details" opens the shared <see cref="TaskEditor" />. The checklist
/// has native checkboxes (<see cref="PbState.SetChecklistItem" />), a labelled add field, Remove buttons that name their
/// item, and "2 of 5 done" with a <see cref="Progress" />. Comments run oldest first with the author and how long ago; a
/// person can post (with a character count) and delete their own after a confirmation. A project manager can delete the
/// task after a confirmation naming its key and title; the delete runs once the dialog is hidden and then the board opens
/// with <c>?deleted=KEY</c>. Every result is mapped to a message in an always-rendered status region, and focus moves to
/// the control that stays, or the status message. A completed project's task is read-only except for comments.
/// </summary>
public partial class TaskDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> checkInputs = new();

    private readonly Dictionary<int, ElementReference> commentButtons = new();

    private ElementReference? assigneeSelect;

    /// <summary>The task this page was set up for.</summary>
    private int builtFor;

    /// <summary>The comment the confirmation is about.</summary>
    private int confirmCommentId;

    private ConfirmKind confirmKind;

    private Modal confirmModal = default!;

    /// <summary>Bumped after a refused change, to recreate the select or checkbox so it shows the stored value again.</summary>
    private int controlsVersion;

    private ElementReference? deleteTaskButton;

    private ElementReference? editButton;

    private TaskEditor editor = default!;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private string newComment = string.Empty;

    private string? newCommentError;

    private ElementReference? newCommentInput;

    private string newItem = string.Empty;

    private string? newItemError;

    private ElementReference? newItemInput;

    /// <summary>Whether the confirmation was accepted, so the delete runs once the dialog is hidden.</summary>
    private bool pendingConfirm;

    private ElementReference? statusSelect;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is { } target)
        {
            focusTarget = null;
            await PbDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        if (builtFor == Id)
            return;

        builtFor = Id;
        statusMessage = newItemError = newCommentError = null;
        newItem = newComment = string.Empty;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Excerpt(string text) => text.Length <= 160 ? text : text[..160].TrimEnd() + "…";

    private void AddChecklistItem()
    {
        if (State.FindTask(Id) is not { } task)
            return;

        var text = newItem.Trim();

        newItemError = text.Length == 0 ? "Enter what needs doing."
            : text.Length > PbState.MaxChecklistTextLength ? $"Shorten the item to {PbState.MaxChecklistTextLength} characters or fewer (now {text.Length})."
            : task.Checklist.Count >= PbState.MaxChecklistItems ? $"The checklist is full ({PbState.MaxChecklistItems} items). Remove one first."
            : null;

        if (newItemError is not null)
        {
            Announce(false, "The item wasn't added. Fix the field, then add it again.");
            focusTarget = () => newItemInput;

            return;
        }

        var result = State.AddChecklistItem(task.Id, text);

        switch (result)
        {
            case PbResult.Ok:
                newItem = string.Empty;
                Announce(true, $"Added “{text}” to the checklist. {task.Checklist.Count(item => item.Done)} of {task.Checklist.Count} done.");
                focusTarget = () => newItemInput;

                return;

            case PbResult.Invalid:
                newItemError = task.Checklist.Count >= PbState.MaxChecklistItems
                    ? $"The checklist is full ({PbState.MaxChecklistItems} items). Remove one first."
                    : "The storage for this demo tab is full, so the item wasn't added. Remove some comments or items, then try again.";
                Announce(false, "The item wasn't added.");
                focusTarget = () => newItemInput;

                return;

            default:
                Announce(false, OtherText(result, "added"));
                focusTarget = () => statusRegion;

                return;
        }
    }

    private void AddComment()
    {
        if (State.FindTask(Id) is not { } task)
            return;

        var text = newComment.Trim();

        newCommentError = text.Length == 0 ? "Write your comment first."
            : text.Length > PbState.MaxCommentLength ? $"Shorten the comment to {PbState.MaxCommentLength} characters or fewer (now {text.Length})."
            : task.Comments.Count >= PbState.MaxComments ? $"This task has the most comments it can hold ({PbState.MaxComments})."
            : null;

        if (newCommentError is not null)
        {
            Announce(false, "The comment wasn't posted. Fix it, then post it again.");
            focusTarget = () => newCommentInput;

            return;
        }

        var result = State.AddComment(task.Id, text);

        switch (result)
        {
            case PbResult.Ok:
                newComment = string.Empty;
                Announce(true, "Your comment is posted, at the end of the comments.");
                focusTarget = () => newCommentInput;

                return;

            case PbResult.Invalid:
                newCommentError = task.Comments.Count >= PbState.MaxComments
                    ? $"This task has the most comments it can hold ({PbState.MaxComments})."
                    : "The storage for this demo tab is full, so the comment wasn't posted. Remove some comments or tasks, then try again.";
                Announce(false, "The comment wasn't posted.");
                focusTarget = () => newCommentInput;

                return;

            default:
                Announce(false, OtherText(result, "posted"));
                focusTarget = () => statusRegion;

                return;
        }
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    private async Task ConfirmAsync()
    {
        pendingConfirm = true;

        await confirmModal.HideAsync();
    }

    /// <summary>Reassigns the task from the Assignee select.</summary>
    private void OnAssigneeChanged(ChangeEventArgs args)
    {
        if (State.FindTask(Id) is not { } task)
            return;

        var key = State.TaskKey(task);
        var value = args.Value?.ToString();
        var personId = string.IsNullOrEmpty(value) ? null : value;
        var person = State.FindPerson(personId);
        var result = State.Reassign(task.Id, personId);

        Announce(result is PbResult.Ok or PbResult.NoChange, result switch
        {
            PbResult.Ok => person is null ? $"{key} is unassigned now." : $"Assigned {key} to {(person.Id == State.CurrentPerson?.Id ? "you" : person.Name)}.",
            PbResult.NoChange => person is null ? $"{key} is already unassigned, so nothing changed." : $"{key} is already assigned to {person.Name}, so nothing changed.",
            PbResult.WrongState => $"{person?.Name ?? "That person"} isn't on {State.FindProject(task.ProjectId)?.Key ?? "the project"}, so {key} can't be assigned to them. Nothing changed.",
            _ => OtherText(result, "reassigned"),
        });

        if (result != PbResult.Ok)
        {
            controlsVersion++;
            focusTarget = () => assigneeSelect;
        }
    }

    /// <summary>Runs an accepted deletion now that the dialog is hidden, or returns focus to the button that opened it.</summary>
    private Task OnConfirmHiddenAsync()
    {
        var accepted = pendingConfirm;
        pendingConfirm = false;

        if (confirmKind == ConfirmKind.DeleteTask)
        {
            if (!accepted)
            {
                focusTarget = () => deleteTaskButton;
                StateHasChanged();

                return Task.CompletedTask;
            }

            if (State.FindTask(Id) is not { } task)
            {
                Announce(false, "This task was already deleted.");
                focusTarget = () => statusRegion;
                StateHasChanged();

                return Task.CompletedTask;
            }

            var key = State.TaskKey(task);
            var projectId = task.ProjectId;
            var result = State.DeleteTask(task.Id);

            if (result == PbResult.Ok)
            {
                Navigation.NavigateTo($"{PbDisplay.ProjectUrl(projectId)}?deleted={Uri.EscapeDataString(key)}");

                return Task.CompletedTask;
            }

            Announce(false, result switch
            {
                PbResult.NotAllowed => "Only project managers can delete tasks. Nothing was deleted.",
                PbResult.NotFound => "This task was already deleted.",
                _ => "That didn't work, and nothing was deleted. Try again.",
            });
            focusTarget = () => deleteTaskButton;
            StateHasChanged();

            return Task.CompletedTask;
        }

        var commentId = confirmCommentId;

        if (!accepted)
        {
            focusTarget = () => commentButtons.TryGetValue(commentId, out var button) ? button : statusRegion;
            StateHasChanged();

            return Task.CompletedTask;
        }

        var deleted = State.DeleteComment(Id, commentId);

        Announce(deleted == PbResult.Ok, deleted switch
        {
            PbResult.Ok => "Your comment is deleted.",
            PbResult.NotFound => "That comment was already deleted.",
            PbResult.WrongState => "You can delete only your own comments. Nothing was deleted.",
            _ => OtherText(deleted, "deleted"),
        });
        commentButtons.Remove(commentId);
        focusTarget = () => statusRegion;
        StateHasChanged();

        return Task.CompletedTask;
    }

    /// <summary>Announces what the task dialog did and returns focus to Edit details.</summary>
    private void OnEditorClosed(TaskEditorResult result)
    {
        if (result.Message is { } message)
            Announce(true, message);

        focusTarget = () => editButton;
        StateHasChanged();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Moves the task to the end of the chosen column from the Status select.</summary>
    private void OnStatusChanged(ChangeEventArgs args)
    {
        if (State.FindTask(Id) is not { } task || !Enum.TryParse<PbTaskStatus>(args.Value?.ToString(), out var status))
            return;

        var key = State.TaskKey(task);
        var label = PbDisplay.StatusLabel(status);
        var result = State.MoveTask(task.Id, status, State.TasksIn(task.ProjectId, status).Count);

        Announce(result is PbResult.Ok or PbResult.NoChange, result switch
        {
            PbResult.Ok => status == PbTaskStatus.Done ? $"Moved {key} to Done: it counts as done." : $"Moved {key} to {label}, at the end of the column.",
            PbResult.NoChange => $"{key} is already in {label}, so nothing changed.",
            PbResult.Invalid => "That status doesn't exist, so nothing changed.",
            _ => OtherText(result, "moved"),
        });

        if (result != PbResult.Ok)
        {
            controlsVersion++;
            focusTarget = () => statusSelect;
        }
    }

    private async Task OpenDeleteCommentAsync(PbComment comment)
    {
        confirmKind = ConfirmKind.DeleteComment;
        confirmCommentId = comment.Id;
        pendingConfirm = false;

        await confirmModal.ShowAsync();
    }

    private async Task OpenDeleteTaskAsync()
    {
        confirmKind = ConfirmKind.DeleteTask;
        pendingConfirm = false;

        await confirmModal.ShowAsync();
    }

    private async Task OpenEditAsync() => await editor.OpenEditAsync(Id);

    /// <summary>The message for a refusal every action can return.</summary>
    private string OtherText(PbResult result, string verb) => result switch
    {
        PbResult.NotAllowed => $"Your sign-in ended. Sign in again; nothing was {verb}.",
        PbResult.NotFound => $"This task or that item was deleted, so nothing was {verb}.",
        _ => $"That didn't work, and nothing was {verb}. Try again.",
    };

    /// <summary>Removes a checklist item; focus moves to the next item, the previous one, or the add field.</summary>
    private void RemoveChecklistItem(PbChecklistItem item)
    {
        if (State.FindTask(Id) is not { } task)
            return;

        var index = task.Checklist.IndexOf(item);
        var result = State.DeleteChecklistItem(task.Id, item.Id);

        Announce(result == PbResult.Ok, result switch
        {
            PbResult.Ok => $"Removed “{item.Text}” from the checklist.",
            PbResult.NotFound => $"“{item.Text}” was already removed.",
            _ => OtherText(result, "removed"),
        });

        checkInputs.Remove(item.Id);
        var next = task.Checklist.Count == 0 ? null : task.Checklist[Math.Clamp(index, 0, task.Checklist.Count - 1)];
        focusTarget = next is not null && checkInputs.TryGetValue(next.Id, out var input) ? () => input : () => newItemInput;
    }

    /// <summary>Ticks or unticks a checklist item.</summary>
    private void SetChecklistItem(PbChecklistItem item, ChangeEventArgs args)
    {
        if (State.FindTask(Id) is not { } task)
            return;

        var done = args.Value is bool value && value;
        var result = State.SetChecklistItem(task.Id, item.Id, done);
        var progress = $"{task.Checklist.Count(entry => entry.Done)} of {task.Checklist.Count} done";

        Announce(result is PbResult.Ok or PbResult.NoChange, result switch
        {
            PbResult.Ok => done ? $"Checked “{item.Text}”. {progress}." : $"Unchecked “{item.Text}”. {progress}.",
            PbResult.NoChange => $"“{item.Text}” is already {(done ? "checked" : "unchecked")}.",
            PbResult.NotFound => $"“{item.Text}” was removed from the checklist.",
            _ => OtherText(result, "changed"),
        });

        if (result != PbResult.Ok)
        {
            controlsVersion++;
            focusTarget = () => checkInputs.TryGetValue(item.Id, out var input) && task.Checklist.Contains(item) ? input : statusRegion;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the task ID from the route.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>What the confirmation dialog asks.</summary>
    private enum ConfirmKind
    {
        DeleteComment,
        DeleteTask,
    }

    #endregion
}
