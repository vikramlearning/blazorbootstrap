using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The activity section of the client page (account managers): a "Log activity" form (kind Note, Call, Email, or Meeting;
/// optionally one of the client's deals; plain text with a counter; <see cref="PbState.LogActivity" />) and the client's
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

    private string? dealError;

    private int dealId;

    private ElementReference? dealInput;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private PbActivityKind kind = PbActivityKind.Note;

    /// <summary>Whether Delete was confirmed in the dialog: it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string text = string.Empty;

    private string? textError;

    private ElementReference? textInput;

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

    /// <summary>Checks the form, logs the entry, and maps the state's answer to a field or the status message.</summary>
    private void Log()
    {
        var trimmed = text.Trim();
        int? about = dealId == 0 ? null : dealId;

        dealError = about is { } id && State.FindDeal(id)?.ClientId != ClientId ? "That deal was deleted or moved to another client. Choose another, or No deal." : null;
        textError = trimmed.Length == 0 ? "Write what happened."
            : trimmed.Length > PbState.MaxActivityLength ? $"Shorten the text to {PbState.MaxActivityLength} characters or fewer (now {trimmed.Length})."
            : null;

        if (dealError is not null || textError is not null)
        {
            focusTarget = dealError is not null ? () => dealInput : () => textInput;
            Announce(false, "Fix the highlighted fields, then log it again.");

            return;
        }

        var result = State.LogActivity(ClientId, kind, trimmed, about, out var logged);

        switch (result)
        {
            case PbResult.Ok when logged is not null:
                Announce(true, $"Logged the {DealDisplay.KindLabel(logged.Kind).ToLowerInvariant()}. It's at the top of the timeline.");
                text = string.Empty;
                dealId = 0;
                focusTarget = () => textInput;

                return;

            case PbResult.NotFound:
                Announce(false, "This client was deleted meanwhile, so nothing was logged.");
                focusTarget = () => statusRegion;

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
}
