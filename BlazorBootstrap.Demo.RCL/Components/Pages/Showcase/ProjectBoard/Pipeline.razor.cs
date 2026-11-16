using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The deal pipeline board of Tandem (account managers): one column per stage (Lead, Qualified, Proposal, Negotiation,
/// Won, Lost) with its count, total, and weighted value, and a card per deal (title, client link, value, chance and
/// weighted value, close date with an overdue date in words, owner initials with the name for screen readers).
/// </summary>
/// <remarks>
/// <para>
/// Drag and drop: each column is a <see cref="SortableList{TItem}" /> in the shared group "pb-pipeline", named after its
/// stage, dragged by the card's grip handle. <c>OnUpdate</c> reorders within a column and <c>OnRemove</c> (raised on the
/// source column) moves to another: the dragged deal is looked up in the source column's rendered list at
/// <c>OldIndex</c>, and <c>NewIndex</c> in the (possibly filtered) target list is mapped to a position among all the
/// stage's deals before calling <see cref="PbState.MoveDeal" />. SortableJS moves DOM nodes Blazor doesn't know about, so
/// after every drag the lists are rebuilt from the state and <see cref="pipelineVersion" />, part of each list's
/// <c>@key</c>, is bumped so Blazor recreates them; a refused drop therefore puts the card back. A won deal whose project
/// exists is locked: no handle, not draggable (<c>DisableItem</c>), and no "Move to stage".
/// </para>
/// <para>
/// Keyboard and single-pointer alternative: every card has a labelled "Move to stage" select with a Move button, and Move
/// up/down buttons. Moving to Won or Lost with the select asks first in a <see cref="Modal" /> (Won creates a project; Lost
/// closes the deal) and runs once the dialog is hidden; a drop has already happened, so it is applied and its outcome
/// announced. Winning announces the created project with a link to its board. Every outcome goes to the always-rendered
/// status region; after a keyboard move focus follows the card.
/// </para>
/// <para>
/// New deal and the cards' Edit (open deals) use the shared <see cref="DealEditor" />; Delete asks first and explains when
/// the deal's project still exists. Filters: owner, closing this month, and a search of titles and client names.
/// </para>
/// </remarks>
public partial class Pipeline : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> cards = new();

    private readonly Dictionary<int, ElementReference> deleteButtons = new();

    private readonly Dictionary<int, ElementReference> downButtons = new();

    private readonly Dictionary<int, ElementReference> editButtons = new();

    /// <summary>The stage chosen in each card's "Move to stage" select, until it is used.</summary>
    private readonly Dictionary<int, PbDealStage> moveChoices = new();

    private readonly Dictionary<int, ElementReference> moveSelects = new();

    /// <summary>The lists each column last rendered, so a drag's indexes are read against what the person saw.</summary>
    private readonly Dictionary<PbDealStage, List<PbDeal>> renderedColumns = new();

    private readonly Dictionary<int, ElementReference> upButtons = new();

    private bool closeThisMonth;

    /// <summary>The project the last move created, linked from the status message.</summary>
    private int? createdProjectId;

    private DealEditor dealEditor = default!;

    /// <summary>The deal the editor was opened for (0 for New deal), for focus when it closes.</summary>
    private int dealEditorFor;

    /// <summary>The deal the delete dialog is about.</summary>
    private int deleteId;

    private Modal deleteModal = default!;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The deal the Won/Lost dialog is about.</summary>
    private int moveDealId;

    private Modal moveModal = default!;

    /// <summary>The stage the Won/Lost dialog moves to.</summary>
    private PbDealStage moveStage;

    private ElementReference? newDealButton;

    /// <summary>The owner to show (a person ID), or empty for everyone.</summary>
    private string ownerFilter = string.Empty;

    /// <summary>Whether Delete was confirmed in the dialog: it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    /// <summary>Whether the Won/Lost move was confirmed in the dialog: it runs once the dialog is hidden.</summary>
    private bool pendingMove;

    /// <summary>Part of each column list's <c>@key</c>: bumped after every drag so Blazor recreates the lists SortableJS touched.</summary>
    private int pipelineVersion;

    private string search = string.Empty;

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

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void Announce(bool ok, string message, int? projectId = null)
    {
        statusIsError = !ok;
        statusMessage = message;
        createdProjectId = projectId;
    }

    /// <summary>Applies a move and announces it; returns whether the deal moved.</summary>
    private bool ApplyMove(PbDeal deal, PbDealStage stage, int index, bool fromDrag)
    {
        var from = deal.Stage;
        var title = deal.Title;
        var result = State.MoveDeal(deal.Id, stage, index, out var created);

        moveChoices.Remove(deal.Id);

        switch (result)
        {
            case PbResult.Ok when from == stage:
                var column = State.DealsIn(stage);
                var position = column.ToList().FindIndex(item => item.Id == deal.Id) + 1;
                Announce(true, $"Moved “{title}” to position {position} of {column.Count} in {PbDisplay.StageLabel(stage)}.");

                return true;

            case PbResult.Ok when stage == PbDealStage.Won && created is not null:
                Announce(true, $"Won “{title}”. Project {created.Key} was created for the project managers to plan, and the client's timeline records it.", created.Id);

                return true;

            case PbResult.Ok when stage == PbDealStage.Won:
                Announce(true, $"Won “{title}”. The client's timeline records it.", deal.ProjectId);

                return true;

            case PbResult.Ok when stage == PbDealStage.Lost:
                Announce(true, $"Marked “{title}” as lost. It's closed and leaves the open pipeline and the forecast; the client's timeline records it.");

                return true;

            case PbResult.Ok when from is PbDealStage.Won or PbDealStage.Lost:
                Announce(true, $"Reopened “{title}” in {PbDisplay.StageLabel(stage)} ({DealDisplay.Percent(PbState.Probability(stage))} chance).");

                return true;

            case PbResult.Ok:
                Announce(true, $"Moved “{title}” from {PbDisplay.StageLabel(from)} to {PbDisplay.StageLabel(stage)} ({DealDisplay.Percent(PbState.Probability(stage))} chance).");

                return true;

            case PbResult.NoChange:
                Announce(true, $"“{title}” is already there, so nothing changed.");

                return false;

            case PbResult.WrongState:
                var project = State.FindProject(deal.ProjectId);
                Announce(false, $"“{title}” was won and became project {project?.Key ?? "on the board"}, so it stays in Won{(fromDrag ? " and went back" : null)}. A project manager would have to delete the project first.", project?.Id);

                return false;

            case PbResult.Invalid when stage == PbDealStage.Won:
                Announce(false, $"“{title}” can't be marked as won: the board already holds the most projects ({PbState.MaxProjects}) or activity entries ({PbState.MaxActivities}) it can. Nothing changed{(fromDrag ? ", and the card went back" : null)}.");

                return false;

            case PbResult.NotFound:
                Announce(false, "That deal was deleted meanwhile, so nothing moved.");

                return false;

            case PbResult.NotAllowed:
                Announce(false, "Only account managers can move deals. Nothing changed.");

                return false;

            default:
                Announce(false, $"That didn't work, and “{title}” didn't move. Try again.");

                return false;
        }
    }

    private void ClearFilters()
    {
        search = string.Empty;
        ownerFilter = string.Empty;
        closeThisMonth = false;
    }

    private async Task ConfirmDeleteAsync()
    {
        pendingDelete = true;
        await deleteModal.HideAsync();
    }

    private async Task ConfirmMoveAsync()
    {
        pendingMove = true;
        await moveModal.HideAsync();
    }

    /// <summary>The element to focus on a card after a keyboard move: its select, else its Move up button, else the card.</summary>
    private ElementReference? FocusOnCard(int dealId)
    {
        if (State.FindDeal(dealId) is not { } deal || !Matches(deal))
            return statusRegion;

        if (!IsLocked(deal) && moveSelects.TryGetValue(dealId, out var select))
            return select;

        return cards.TryGetValue(dealId, out var card) ? card : statusRegion;
    }

    /// <summary>
    /// Maps a position in a column's shown deals (without the moving one) to a position among all the stage's deals: before
    /// the shown deal now at that position, or just after the last shown one.
    /// </summary>
    private int FullIndex(PbDealStage stage, PbDeal moving, IReadOnlyList<PbDeal> shown, int shownIndex)
    {
        var all = State.DealsIn(stage).Where(item => item.Id != moving.Id).ToList();
        var rest = shown.Where(item => item.Id != moving.Id).ToList();

        if (rest.Count == 0)
            return all.Count;

        shownIndex = Math.Clamp(shownIndex, 0, rest.Count);

        return shownIndex < rest.Count
            ? Math.Max(0, all.FindIndex(item => item.Id == rest[shownIndex].Id))
            : all.FindIndex(item => item.Id == rest[^1].Id) + 1;
    }

    /// <summary>A won deal whose project exists can't leave Won: it isn't draggable and has no "Move to stage".</summary>
    private bool IsLocked(PbDeal deal) => deal.Stage == PbDealStage.Won && State.FindProject(deal.ProjectId) is not null;

    private bool Matches(PbDeal deal)
    {
        var text = search.Trim();
        var today = State.Clock.Today;
        var closes = deal.ClosedAt ?? deal.CloseDate;

        if (ownerFilter.Length > 0 && deal.OwnerId != ownerFilter)
            return false;

        if (closeThisMonth && (closes.Year != today.Year || closes.Month != today.Month))
            return false;

        return text.Length == 0
            || deal.Title.Contains(text, StringComparison.OrdinalIgnoreCase)
            || (State.FindClient(deal.ClientId)?.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private PbDealStage MoveChoice(PbDeal deal) => moveChoices.TryGetValue(deal.Id, out var stage) ? stage : deal.Stage;

    /// <summary>"Move to stage" with the select: asks first for Won or Lost; other stages move at once, to the end of the column.</summary>
    private async Task MoveWithSelectAsync(int dealId)
    {
        if (State.FindDeal(dealId) is not { } deal)
        {
            Announce(false, "That deal was deleted meanwhile, so nothing moved.");
            focusTarget = () => statusRegion;

            return;
        }

        var stage = MoveChoice(deal);

        if (stage == deal.Stage)
        {
            Announce(true, $"“{deal.Title}” is already in {PbDisplay.StageLabel(stage)}. Choose another stage in Move to stage first.");
            focusTarget = () => moveSelects.TryGetValue(dealId, out var select) ? select : statusRegion;

            return;
        }

        if (stage is PbDealStage.Won or PbDealStage.Lost)
        {
            moveDealId = dealId;
            moveStage = stage;
            pendingMove = false;
            await moveModal.ShowAsync();

            return;
        }

        ApplyMove(deal, stage, State.DealsIn(stage).Count, false);
        focusTarget = () => FocusOnCard(dealId);
    }

    /// <summary>Move up (-1) or down (+1) among the column's shown deals.</summary>
    private Task MoveWithinAsync(int dealId, int step)
    {
        if (State.FindDeal(dealId) is not { } deal)
        {
            Announce(false, "That deal was deleted meanwhile, so nothing moved.");
            focusTarget = () => statusRegion;

            return Task.CompletedTask;
        }

        var shown = Visible(deal.Stage);
        var position = shown.FindIndex(item => item.Id == dealId);
        var target = position + step;

        if (position < 0 || target < 0 || target >= shown.Count)
        {
            Announce(true, $"“{deal.Title}” is already {(step < 0 ? "first" : "last")} in {PbDisplay.StageLabel(deal.Stage)}.");

            return Task.CompletedTask;
        }

        ApplyMove(deal, deal.Stage, FullIndex(deal.Stage, deal, shown, target), false);

        // Focus stays on the pressed button while it can still move that way; otherwise it goes to the card.
        var after = Visible(deal.Stage).FindIndex(item => item.Id == dealId);
        var canStill = step < 0 ? after > 0 : after >= 0 && after < Visible(deal.Stage).Count - 1;
        focusTarget = () => canStill && (step < 0 ? upButtons : downButtons).TryGetValue(dealId, out var button) ? button : FocusOnCard(dealId);

        return Task.CompletedTask;
    }

    /// <summary>Runs a confirmed deletion now that the dialog is hidden, and says what happened.</summary>
    private void OnDeleteHidden()
    {
        var id = deleteId;

        if (!pendingDelete)
        {
            focusTarget = () => deleteButtons.TryGetValue(id, out var button) && State.FindDeal(id) is { } deal && Matches(deal) ? button : statusRegion;
            StateHasChanged();

            return;
        }

        pendingDelete = false;
        var target = State.FindDeal(id);
        var result = State.DeleteDeal(id);

        Announce(result == PbResult.Ok, result switch
        {
            PbResult.Ok => $"“{target?.Title}” is deleted. It's no longer in the pipeline or the forecast.",
            PbResult.WrongState => $"“{target?.Title}” can't be deleted: it became project {State.FindProject(target?.ProjectId)?.Key}, which still exists. Nothing was deleted.",
            PbResult.NotFound => "That deal was already deleted.",
            PbResult.NotAllowed => "Only account managers can delete deals. Nothing was deleted.",
            _ => "That didn't work, and nothing was deleted. Try again.",
        }, result == PbResult.WrongState ? target?.ProjectId : null);
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    /// <summary>A card dropped in another column (raised on the source column).</summary>
    private void OnDragRemove(PbDealStage fromStage, SortableListEventArgs args)
    {
        if (!Enum.TryParse<PbDealStage>(args.ToListName, out var toStage) || !renderedColumns.TryGetValue(fromStage, out var source)
            || args.OldIndex < 0 || args.OldIndex >= source.Count)
        {
            Announce(false, "That drop couldn't be read, so nothing moved. Try again, or use Move to stage on the card.");
            pipelineVersion++;

            return;
        }

        var deal = source[args.OldIndex];
        var shown = renderedColumns.TryGetValue(toStage, out var target) ? target : Visible(toStage);

        ApplyMove(deal, toStage, FullIndex(toStage, deal, shown, args.NewIndex), true);
        pipelineVersion++;
    }

    /// <summary>A card dropped at another place in its own column.</summary>
    private void OnDragUpdate(PbDealStage stage, SortableListEventArgs args)
    {
        if (!renderedColumns.TryGetValue(stage, out var column) || args.OldIndex < 0 || args.OldIndex >= column.Count)
        {
            Announce(false, "That drop couldn't be read, so nothing moved. Try again, or use Move up and Move down on the card.");
            pipelineVersion++;

            return;
        }

        var deal = column[args.OldIndex];

        ApplyMove(deal, stage, FullIndex(stage, deal, column, args.NewIndex), true);
        pipelineVersion++;
    }

    /// <summary>Announces what the deal form did; focus goes to the message after a save, or back to the button that opened it.</summary>
    private void OnDealEditorClosed(DealEditorResult result)
    {
        if (result.Message is { } message)
        {
            var hidden = result.Saved is { } saved && !Matches(saved) ? " It's hidden by the filters; Show all deals to see it." : null;
            Announce(true, message + hidden);
            focusTarget = () => statusRegion;
        }
        else
        {
            var id = dealEditorFor;
            focusTarget = () => id != 0 && editButtons.TryGetValue(id, out var button) && State.FindDeal(id) is { ClosedAt: null } deal && Matches(deal) ? button : newDealButton;
        }
    }

    /// <summary>Runs a confirmed Won or Lost move now that the dialog is hidden; focus follows the card.</summary>
    private void OnMoveHidden()
    {
        var id = moveDealId;

        if (!pendingMove)
        {
            focusTarget = () => FocusOnCard(id);
            StateHasChanged();

            return;
        }

        pendingMove = false;

        if (State.FindDeal(id) is { } deal)
            ApplyMove(deal, moveStage, State.DealsIn(moveStage).Count, false);
        else
            Announce(false, "That deal was deleted meanwhile, so nothing moved.");

        focusTarget = () => FocusOnCard(id);
        StateHasChanged();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteAsync(int dealId)
    {
        deleteId = dealId;
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    private async Task OpenEditDealAsync(int dealId)
    {
        dealEditorFor = dealId;

        if (!await dealEditor.OpenEditAsync(dealId))
        {
            Announce(false, State.FindDeal(dealId) is { } deal
                ? $"“{deal.Title}” is {PbDisplay.StageLabel(deal.Stage).ToLowerInvariant()}, so it can't be edited. Move it back to an open stage first."
                : "That deal was deleted meanwhile.");
            focusTarget = () => statusRegion;
        }
    }

    private async Task OpenNewDealAsync()
    {
        dealEditorFor = 0;
        await dealEditor.OpenNewAsync();
    }

    private void SetMoveChoice(int dealId, object? value)
    {
        if (Enum.TryParse<PbDealStage>(value?.ToString(), out var stage))
            moveChoices[dealId] = stage;
    }

    /// <summary>The deals of a stage that the filters show, in their order (a new list every time).</summary>
    private List<PbDeal> Visible(PbDealStage stage) => State.DealsIn(stage).Where(Matches).ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    private bool IsFiltered => search.Trim().Length > 0 || ownerFilter.Length > 0 || closeThisMonth;

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
