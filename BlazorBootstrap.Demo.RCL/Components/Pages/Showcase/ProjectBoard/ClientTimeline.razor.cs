using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The activity section of the client page (account managers): a "Log activity" form, an <see cref="EditForm" /> (rule
/// WR-002: type Note, Call, Email, or Meeting, required; the day it happened, required, not in the future and not before
/// the client was added; optionally one of the client's deals; 1 to <see cref="PbState.MaxActivityLength" /> characters of
/// plain text with a counter; a linked summary on a failed submit; <see cref="PbState.LogActivity" />) and the client's
/// timeline newest first as an ordered list of <see cref="ActivityEntry" />. People delete their own entries after a
/// confirmation that names the entry (<see cref="PbState.DeleteActivity" />); entries Tandem recorded (deal and project
/// changes) stay and say so. Outcomes go to the section's always-rendered status region.
/// </summary>
public partial class ClientTimeline : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    /// <summary>The entry the delete dialog is about.</summary>
    private int deleteId;

    private Modal deleteModal = default!;

    private EditContext editContext = default!;

    private ActivityForm form = new();

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>Whether Delete was confirmed in the dialog: it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    /// <summary>The state's answers that belong to a field, shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private InputTextArea? textInput;

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

    protected override void OnInitialized()
    {
        ResetForm();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

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
        await deleteModal.HideAsync();
    }

    /// <summary>Logs the entry once the form is valid (a failed submit shows the summary); the state's answer maps to a field or the status message.</summary>
    private void Log()
    {
        form.Today = State.Clock.Today;
        form.ClientSince = State.FindClient(ClientId)?.CreatedAt.Date ?? DateTime.MinValue;
        form.DealOk = form.DealId == 0 || State.FindDeal(form.DealId)?.ClientId == ClientId;
        serverErrors?.Clear();

        if (!editContext.Validate())
            return;

        var trimmed = form.Text.Trim();
        int? about = form.DealId == 0 ? null : form.DealId;
        var result = State.LogActivity(ClientId, form.Kind!.Value, trimmed, about, form.Date!.Value, out var logged);

        switch (result)
        {
            case PbResult.Ok when logged is not null:
                Announce(true, $"Logged the {DealDisplay.KindLabel(logged.Kind).ToLowerInvariant()} from {PbDisplay.Date(logged.At)}. It's on the timeline, newest first.");
                ResetForm();
                focusTarget = () => textInput?.Element;

                return;

            case PbResult.NotFound:
                Announce(false, "This client was deleted meanwhile, so nothing was logged.");
                focusTarget = () => statusRegion;

                return;

            case PbResult.Invalid when about is { } id && State.FindDeal(id)?.ClientId != ClientId:
                serverErrors?.Add(FieldIdentifier.Create(() => form.DealId), "That deal was deleted or moved to another client. Choose another, or No deal.");
                editContext.NotifyValidationStateChanged();

                return;

            case PbResult.Invalid when State.Activities.Count >= PbState.MaxActivities:
                Announce(false, $"The board keeps up to {PbState.MaxActivities} activity entries, and it's full. Delete entries you don't need first. Nothing was logged.");
                focusTarget = () => statusRegion;

                return;

            case PbResult.Invalid:
                Announce(false, DealDisplay.StorageFull);
                focusTarget = () => statusRegion;

                return;

            case PbResult.NotAllowed:
                Announce(false, "Only account managers can log client activity. Nothing was logged.");
                focusTarget = () => statusRegion;

                return;

            default:
                Announce(false, "That didn't work, and nothing was logged. Try again.");
                focusTarget = () => statusRegion;

                return;
        }
    }

    /// <summary>Runs a confirmed deletion now that the dialog is hidden; focus goes to the status message (the button is gone).</summary>
    private void OnDeleteHidden()
    {
        if (!pendingDelete)
        {
            var id = deleteId;
            focusTarget = () => deleteButtons.TryGetValue(id, out var button) ? button : statusRegion;
            StateHasChanged();

            return;
        }

        pendingDelete = false;
        var result = State.DeleteActivity(deleteId);

        Announce(result == PbResult.Ok, result switch
        {
            PbResult.Ok => "The entry is deleted from the timeline.",
            PbResult.NotFound => "That entry was already deleted.",
            PbResult.WrongState => $"Only the person who logged an entry can delete it, and entries {PbSeedData.ProductName} recorded stay. Nothing was deleted.",
            PbResult.NotAllowed => "Only account managers can delete client activity. Nothing was deleted.",
            _ => "That didn't work, and nothing was deleted. Try again.",
        });
        deleteButtons.Remove(deleteId);
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>An empty form: a note, today, about no deal.</summary>
    private void ResetForm()
    {
        form = new ActivityForm { Kind = PbActivityKind.Note, Date = State.Clock.Today };
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += (_, args) => serverErrors?.Clear(args.FieldIdentifier);
    }

    private async Task OpenDeleteAsync(int activityId)
    {
        deleteId = activityId;
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the client.</summary>
    [Parameter, EditorRequired] public int ClientId { get; set; }

    [Inject] private PbState State { get; set; } = default!;

    #endregion
    #region Nested Types

    /// <summary>
    /// A logged activity's fields, with the limits of <see cref="PbState.LogActivity" />: a type, a day not in the future
    /// and not before the client was added (cross-field rules against values the page sets), a deal of this client, and the
    /// text.
    /// </summary>
    private sealed class ActivityForm : IValidatableObject
    {
        /// <summary>The day the client was added, set by the page.</summary>
        public DateTime ClientSince { get; set; } = DateTime.MinValue;

        [Required(ErrorMessage = "Pick the day it happened.")]
        public DateTime? Date { get; set; }

        public int DealId { get; set; }

        /// <summary>Whether the chosen deal is still this client's, set by the page.</summary>
        public bool DealOk { get; set; } = true;

        [Required(ErrorMessage = "Choose the type: note, call, email, or meeting.")]
        public PbActivityKind? Kind { get; set; }

        [Required(ErrorMessage = "Write what happened.")]
        [StringLength(PbState.MaxActivityLength, ErrorMessage = "Use 1,000 characters or fewer.")]
        public string Text { get; set; } = string.Empty;

        /// <summary>The board's today, set by the page.</summary>
        public DateTime Today { get; set; } = DateTime.MaxValue;

        /// <summary>Runs once the fields are filled in: the day is neither in the future nor before the client was added, and the deal is still the client's.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date is { } day && day.Date > Today.Date)
                yield return new ValidationResult($"Pick today or an earlier day; {PbDisplay.Date(day)} hasn't happened yet.", new[] { nameof(Date) });
            else if (Date is { } early && early.Date < ClientSince.Date)
                yield return new ValidationResult($"Pick {PbDisplay.Date(ClientSince)} or later: the client was added then.", new[] { nameof(Date) });

            if (!DealOk)
                yield return new ValidationResult("That deal was deleted or moved to another client. Choose another, or No deal.", new[] { nameof(DealId) });
        }
    }

    #endregion
}
