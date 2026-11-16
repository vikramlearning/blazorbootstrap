using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>What happened in the <see cref="DealEditor" /> once its dialog is hidden.</summary>
/// <param name="Saved">The deal that was added or saved, or <see langword="null" /> when the dialog was cancelled.</param>
/// <param name="Created">Whether a new deal was added.</param>
/// <param name="Message">What to announce, or <see langword="null" /> when cancelled.</param>
public sealed record DealEditorResult(PbDeal? Saved, bool Created, string? Message);

/// <summary>
/// The shared deal form of the account manager screens, in a <see cref="Modal" />: client, title (with a counter), value (a
/// native number from 1 to <see cref="PbState.MaxDealValue" />), and the expected close date (a native date). New deals
/// are leads owned by the signed-in account manager; only open deals can be edited (<see cref="PbState.SaveDeal" />).
/// Each problem is shown under its field and tied to it, and the first invalid field takes focus after the render that
/// shows it; the state's refusals are mapped to a field or to the dialog's status line. The page hears the outcome in
/// <see cref="OnClosed" /> after the dialog is hidden, to announce it and move focus back.
/// </summary>
public partial class DealEditor : ComponentBase, IDisposable
{
    #region Fields and Constants

    private ElementReference? clientInput;

    private string? clientError;

    private int clientId;

    private ElementReference? closeInput;

    private string? closeError;

    private string closeText = string.Empty;

    /// <summary>The deal being edited, or 0 for a new one.</summary>
    private int dealId;

    /// <summary>Returns the field to focus after the next render (the first invalid one).</summary>
    private Func<ElementReference?>? focusTarget;

    private bool focusStatus;

    private string? formMessage;

    private Modal modal = default!;

    /// <summary>The outcome to report once the dialog is hidden.</summary>
    private DealEditorResult? outcome;

    private ElementReference? statusRegion;

    private string title = string.Empty;

    private string? titleError;

    private ElementReference? titleInput;

    private string? valueError;

    private ElementReference? valueInput;

    private string valueText = string.Empty;

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

    /// <summary>Opens the form for an open deal. Returns <see langword="false" /> (and stays closed) when the deal can't be edited.</summary>
    public async Task<bool> OpenEditAsync(int id)
    {
        if (State.FindDeal(id) is not { ClosedAt: null } deal)
            return false;

        Reset();
        dealId = deal.Id;
        clientId = deal.ClientId;
        title = deal.Title;
        valueText = deal.Value.ToString(CultureInfo.InvariantCulture);
        closeText = DealDisplay.ToDateInput(deal.CloseDate);
        await modal.ShowAsync();

        return true;
    }

    /// <summary>Opens the form for a new deal, optionally for a client, closing in 30 days by default.</summary>
    public async Task OpenNewAsync(int? forClientId = null)
    {
        Reset();
        dealId = 0;
        clientId = forClientId is { } id && State.FindClient(id) is not null ? id : 0;
        title = string.Empty;
        valueText = string.Empty;
        closeText = DealDisplay.ToDateInput(State.Clock.Today.AddDays(30));
        await modal.ShowAsync();
    }

    #endregion

    #region Private Methods

    private async Task CancelAsync()
    {
        outcome = null;
        await modal.HideAsync();
    }

    private Task OnHiddenAsync()
    {
        var result = outcome ?? new DealEditorResult(null, false, null);
        outcome = null;

        return OnClosed.InvokeAsync(result);
    }

    private async Task OnShownAsync() => await PbDisplay.FocusAsync(clientId == 0 ? clientInput : titleInput);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void Reset()
    {
        outcome = null;
        formMessage = null;
        clientError = titleError = valueError = closeError = null;
    }

    /// <summary>Checks the fields, saves, and maps the state's answer to a field or the dialog's status line.</summary>
    private async Task SaveAsync()
    {
        var trimmed = title.Trim();
        var hasValue = int.TryParse(valueText, NumberStyles.None, CultureInfo.InvariantCulture, out var value);
        var hasDate = DealDisplay.TryParseDateInput(closeText, out var closeDate);

        formMessage = null;
        clientError = State.FindClient(clientId) is null ? "Choose the client the deal is with." : null;
        titleError = trimmed.Length == 0 ? "Enter a title for the deal."
            : trimmed.Length > PbState.MaxDealTitleLength ? $"Shorten the title to {PbState.MaxDealTitleLength} characters or fewer (now {trimmed.Length})."
            : null;
        valueError = string.IsNullOrWhiteSpace(valueText) ? "Enter the value of the deal in whole dollars."
            : !hasValue ? "Enter the value as a whole number of dollars, without a dollar sign or decimals."
            : value is < 1 or > PbState.MaxDealValue ? $"Enter a value from $1 to {PbDisplay.Money(PbState.MaxDealValue)}."
            : null;
        closeError = !hasDate ? "Enter the expected close date."
            : closeDate < MinDate || closeDate > MaxDate ? $"Choose a date from {PbDisplay.Date(MinDate)} to {PbDisplay.Date(MaxDate)}."
            : null;

        focusTarget = clientError is not null ? () => clientInput
            : titleError is not null ? () => titleInput
            : valueError is not null ? () => valueInput
            : closeError is not null ? () => closeInput
            : null;

        if (focusTarget is not null)
            return;

        var existing = dealId == 0 ? null : State.FindDeal(dealId);
        var result = State.SaveDeal(dealId, new PbDealInput(clientId, trimmed, value, closeDate), out var saved);
        var clientName = State.FindClient(clientId)?.Name ?? "the client";

        switch (result)
        {
            case PbResult.Ok when saved is not null:
                outcome = new DealEditorResult(saved, dealId == 0, dealId == 0
                    ? $"Added “{saved.Title}” for {clientName} as a lead: {PbDisplay.Money(saved.Value)}, {DealDisplay.CloseText(saved, State.Clock.Today).ToLowerInvariant()}."
                    : $"Saved “{saved.Title}” for {clientName}: {PbDisplay.Money(saved.Value)}, {DealDisplay.CloseText(saved, State.Clock.Today).ToLowerInvariant()}.");
                await modal.HideAsync();

                return;

            case PbResult.NoChange:
                outcome = new DealEditorResult(existing, false, $"Nothing changed in “{existing?.Title ?? trimmed}”.");
                await modal.HideAsync();

                return;

            case PbResult.WrongState:
                formMessage = $"“{existing?.Title ?? trimmed}” was {(existing?.Stage == PbDealStage.Won ? "won" : "lost")} meanwhile, so it can't be edited. Move it back to an open stage on the pipeline first. Nothing was saved.";
                focusStatus = true;

                return;

            case PbResult.NotFound when dealId != 0 && existing is null:
                formMessage = "This deal was deleted meanwhile, so there's nothing to save. Cancel to close the form.";
                focusStatus = true;

                return;

            case PbResult.NotFound:
            case PbResult.Invalid when State.FindClient(clientId) is null:
                clientError = "That client was deleted meanwhile. Choose another client.";
                focusTarget = () => clientInput;

                return;

            case PbResult.Invalid when dealId == 0 && State.Deals.Count >= PbState.MaxDeals:
                formMessage = $"The pipeline holds up to {PbState.MaxDeals} deals, and it's full. Delete a deal you don't need first. Nothing was added.";
                focusStatus = true;

                return;

            case PbResult.Invalid:
                formMessage = DealDisplay.StorageFull;
                focusStatus = true;

                return;

            case PbResult.NotAllowed:
                formMessage = "Only account managers can add or edit deals. Nothing was saved.";
                focusStatus = true;

                return;

            default:
                formMessage = "That didn't work, and nothing was saved. Try again.";
                focusStatus = true;

                return;
        }
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets what to do once the dialog is hidden: announce the outcome and move focus back.</summary>
    [Parameter] public EventCallback<DealEditorResult> OnClosed { get; set; }

    private string DialogTitle => dealId == 0 ? "New deal" : $"Edit “{State.FindDeal(dealId)?.Title ?? title}”";

    /// <summary>The latest close date the board accepts (three years from the demo's start).</summary>
    private static DateTime MaxDate => PbSeedData.Now.Date.AddYears(3);

    /// <summary>The earliest close date the board accepts (three years before the demo's start).</summary>
    private static DateTime MinDate => PbSeedData.Now.Date.AddYears(-3).AddDays(1);

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
