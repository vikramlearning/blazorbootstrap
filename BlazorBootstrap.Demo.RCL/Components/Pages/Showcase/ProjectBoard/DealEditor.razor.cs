using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>What happened in the <see cref="DealEditor" /> once its dialog is hidden.</summary>
/// <param name="Saved">The deal that was added or saved, or <see langword="null" /> when the dialog was cancelled.</param>
/// <param name="Created">Whether a new deal was added.</param>
/// <param name="Message">What to announce, or <see langword="null" /> when cancelled.</param>
public sealed record DealEditorResult(PbDeal? Saved, bool Created, string? Message);

/// <summary>
/// The shared deal form of the account manager screens, in a <see cref="Modal" /> with an <see cref="EditForm" /> (rule
/// WR-002): client (required), title (required, with a counter), value (whole dollars from 1 to
/// <see cref="PbState.MaxDealValue" />), the expected close date (required, today or later: an open deal can't be expected
/// to close in the past), and, for a new deal, its open stage (required; Won and Lost are set on the pipeline). New deals are
/// owned by the signed-in account manager; only open deals can be edited (<see cref="PbState.SaveDeal" />). Messages show
/// after a field is touched or on submit, with a linked summary; the state's refusals go to their field or the dialog's
/// alert. The page hears the outcome in <see cref="OnClosed" /> after the dialog is hidden, to announce it and move focus
/// back.
/// </summary>
public partial class DealEditor : ComponentBase, IDisposable
{
    #region Fields and Constants

    private InputSelect<int>? clientInput;

    /// <summary>The deal being edited, or 0 for a new one.</summary>
    private int dealId;

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorText;

    private DealForm form = new();

    private string? formMessage;

    private Modal modal = default!;

    /// <summary>The outcome to report once the dialog is hidden.</summary>
    private DealEditorResult? outcome;

    /// <summary>The state's answers that belong to a field (a deleted client), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private InputText? titleInput;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        ResetForm(new DealForm());
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    /// <summary>Opens the form for an open deal. Returns <see langword="false" /> (and stays closed) when the deal can't be edited.</summary>
    public async Task<bool> OpenEditAsync(int id)
    {
        if (State.FindDeal(id) is not { ClosedAt: null } deal)
            return false;

        Reset();
        dealId = deal.Id;
        ResetForm(new DealForm { ClientId = deal.ClientId, Title = deal.Title, Value = deal.Value, CloseDate = deal.CloseDate, Stage = deal.Stage, Today = State.Clock.Today });
        await modal.ShowAsync();

        return true;
    }

    /// <summary>Opens the form for a new deal, optionally for a client, as a lead closing in 30 days by default.</summary>
    public async Task OpenNewAsync(int? forClientId = null)
    {
        Reset();
        dealId = 0;
        ResetForm(new DealForm
        {
            ClientId = forClientId is { } id && State.FindClient(id) is not null ? id : 0, CloseDate = State.Clock.Today.AddDays(30), Stage = PbDealStage.Lead, Today = State.Clock.Today,
        });
        await modal.ShowAsync();
    }

    #endregion

    #region Private Methods

    private async Task CancelAsync()
    {
        outcome = null;
        await modal.HideAsync();
    }

    /// <summary>A change clears the dialog's alert and the state's message on that field.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);
        formMessage = errorText = null;
        errorCount = 0;
    }

    private Task OnHiddenAsync()
    {
        var result = outcome ?? new DealEditorResult(null, false, null);
        outcome = null;

        return OnClosed.InvokeAsync(result);
    }

    private async Task OnShownAsync() => await PbDisplay.FocusAsync(form.ClientId == 0 ? clientInput?.Element : titleInput?.Element);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void Reset()
    {
        outcome = null;
        formMessage = errorText = null;
        errorCount = 0;
    }

    private void ResetForm(DealForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>
    /// Saves once the form is valid (every rule runs, and a failed submit shows the summary); the state checks again, and
    /// its answer maps to a field or the dialog's alert.
    /// </summary>
    private async Task SaveAsync()
    {
        form.Today = State.Clock.Today;
        form.ClientExists = State.FindClient(form.ClientId) is not null;
        serverErrors?.Clear();

        if (!editContext.Validate())
            return;

        var trimmed = form.Title.Trim();
        var existing = dealId == 0 ? null : State.FindDeal(dealId);
        var result = State.SaveDeal(dealId, new PbDealInput(form.ClientId, trimmed, form.Value!.Value, form.CloseDate!.Value, form.Stage ?? PbDealStage.Lead), out var saved);
        var clientName = State.FindClient(form.ClientId)?.Name ?? "the client";

        switch (result)
        {
            case PbResult.Ok when saved is not null:
                outcome = new DealEditorResult(saved, dealId == 0, dealId == 0
                    ? $"Added “{saved.Title}” for {clientName} in {PbDisplay.StageLabel(saved.Stage)}: {PbDisplay.Money(saved.Value)}, {LowerFirst(DealDisplay.CloseText(saved, State.Clock.Today))}."
                    : $"Saved “{saved.Title}” for {clientName}: {PbDisplay.Money(saved.Value)}, {LowerFirst(DealDisplay.CloseText(saved, State.Clock.Today))}.");
                await modal.HideAsync();

                return;

            case PbResult.NoChange:
                outcome = new DealEditorResult(existing, false, $"Nothing changed in “{existing?.Title ?? trimmed}”.");
                await modal.HideAsync();

                return;

            case PbResult.WrongState:
                ShowError($"“{existing?.Title ?? trimmed}” was {(existing?.Stage == PbDealStage.Won ? "won" : "lost")} meanwhile, so it can't be edited. Move it back to an open stage on the pipeline first. Nothing was saved.");

                return;

            case PbResult.NotFound when dealId != 0 && existing is null:
                ShowError("This deal was deleted meanwhile, so there's nothing to save. Cancel to close the form.");

                return;

            case PbResult.NotFound:
            case PbResult.Invalid when State.FindClient(form.ClientId) is null:
                ServerError(() => form.ClientId, "That client was deleted meanwhile. Choose another client.");

                return;

            case PbResult.Invalid when form.CloseDate!.Value.Date < State.Clock.Today:
                ServerError(() => form.CloseDate, $"Choose a close date of {PbDisplay.Date(State.Clock.Today)} or later.");

                return;

            case PbResult.Invalid when dealId == 0 && State.Deals.Count >= PbState.MaxDeals:
                ShowError($"The pipeline holds up to {PbState.MaxDeals} deals, and it's full. Delete a deal you don't need first. Nothing was added.");

                return;

            case PbResult.Invalid:
                ShowError(DealDisplay.StorageFull);

                return;

            case PbResult.NotAllowed:
                ShowError("Only account managers can add or edit deals. Nothing was saved.");

                return;

            default:
                ShowError("That didn't work, and nothing was saved. Try again.");

                return;
        }
    }

    private static string LowerFirst(string text) => text.Length == 0 ? text : char.ToLowerInvariant(text[0]) + text[1..];

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    /// <summary>Shows the dialog's alert; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        formMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets what to do once the dialog is hidden: announce the outcome and move focus back.</summary>
    [Parameter] public EventCallback<DealEditorResult> OnClosed { get; set; }

    private string DialogTitle => dealId == 0 ? "New deal" : $"Edit “{State.FindDeal(dealId)?.Title ?? form.Title}”";

    /// <summary>The latest close date the board accepts (three years from the demo's start).</summary>
    private static DateTime MaxDate => PbSeedData.Now.Date.AddYears(3);

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// A deal's fields, with the limits of <see cref="PbState.SaveDeal" />: a client, a title, a value in range, a close date
    /// of today or later (a cross-field rule against the board's clock, set by the page), and an open stage.
    /// </summary>
    private sealed class DealForm : IValidatableObject
    {
        /// <summary>Whether the chosen client still exists, set by the page before it validates.</summary>
        public bool ClientExists { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Choose the client the deal is with.")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Enter the expected close date.")]
        public DateTime? CloseDate { get; set; }

        [Required(ErrorMessage = "Choose the deal's stage.")]
        public PbDealStage? Stage { get; set; } = PbDealStage.Lead;

        [Required(ErrorMessage = "Enter a title for the deal.")]
        [StringLength(PbState.MaxDealTitleLength, ErrorMessage = "Use 100 characters or fewer for the title.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>The board's today, set by the page.</summary>
        public DateTime Today { get; set; } = DateTime.MinValue;

        [Required(ErrorMessage = "Enter the value of the deal in whole dollars.")]
        [Range(1, PbState.MaxDealValue, ErrorMessage = "Enter a value from $1 to $5,000,000.")]
        public int? Value { get; set; }

        /// <summary>Runs once the fields are filled in: the client still exists, and the close date isn't in the past or beyond the demo's range.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ClientId != 0 && !ClientExists)
                yield return new ValidationResult("That client was deleted meanwhile. Choose another client.", new[] { nameof(ClientId) });

            if (CloseDate is { } close && close.Date < Today.Date)
                yield return new ValidationResult($"An open deal can't close in the past. Choose {PbDisplay.Date(Today)} or later.", new[] { nameof(CloseDate) });
            else if (CloseDate is { } late && late.Date > MaxDate)
                yield return new ValidationResult($"Choose a close date on or before {PbDisplay.Date(MaxDate)}.", new[] { nameof(CloseDate) });
        }
    }

    #endregion
}
