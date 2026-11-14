using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Transfer form: source account, recipient, amount, date, and note. Field rules are data annotations; balance and
/// account rules come from <see cref="FintechState.ValidateTransfer" />. A valid form goes to the confirm screen.
/// Returning from that screen ("Edit") restores the pending transfer into the form.
/// </summary>
public partial class Transfer : ComponentBase
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private TransferForm form = new();

    private string? ruleError;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        form = State.PendingTransfer is { } pending
            ? new TransferForm { FromAccountId = pending.FromAccountId, ToId = pending.ToId, Amount = pending.Amount, Date = pending.Date, Note = pending.Note ?? string.Empty }
            : new TransferForm { FromAccountId = "checking", Date = Today };

        editContext = new EditContext(form);
    }

    #endregion

    #region Private Methods

    private void Review()
    {
        var draft = new FintechTransferDraft(form.FromAccountId, form.ToId, form.Amount, form.Date, string.IsNullOrWhiteSpace(form.Note) ? null : form.Note.Trim());
        ruleError = State.StartTransfer(draft);

        if (ruleError is null)
            Navigation.NavigateTo($"{FintechLayout.Base}/transfer/confirm");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IEnumerable<FintechAccount> SourceAccounts => State.Accounts.Where(account => account.Kind != FintechAccountKind.CreditCard);

    [Inject] private FintechState State { get; set; } = default!;

    private static DateOnly Today => DateOnly.FromDateTime(FintechSeedData.Today);

    #endregion

    #region Nested Types

    private sealed class TransferForm
    {
        // Invariant limits: RangeAttribute parses "0.01" with the current culture, which throws under a comma-decimal locale (de-DE).
        [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true, ErrorMessage = "Enter an amount greater than $0.00.")]
        public decimal Amount { get; set; }

        public DateOnly Date { get; set; }

        public string FromAccountId { get; set; } = string.Empty;

        [StringLength(40, ErrorMessage = "Use 40 characters or fewer.")]
        public string Note { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose who to send money to.")]
        public string ToId { get; set; } = string.Empty;
    }

    #endregion
}
