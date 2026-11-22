using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// An invoice (account managers, AC-7f): its status (Draft, Issued, Overdue, Paid), client, total, the hours it covers, and
/// its dates; its lines by project and person (a plain line-item table) with Remove on a draft
/// (<see cref="PbState.RemoveInvoiceLine" />). A draft is issued with payment terms in an <see cref="EditForm" /> (rule WR-002:
/// terms required; <see cref="PbState.IssueInvoice" />) or deleted after a confirmation; an issued invoice is marked paid
/// with the payment date (required, from the issue date to today; <see cref="PbState.MarkInvoicePaid" />). Outcomes are
/// announced in an always-rendered status region; <c>?created=1</c> announces a new draft. An unknown ID shows
/// <see cref="PbNotFound" />.
/// </summary>
public partial class InvoiceDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? deleteButton;

    private Modal deleteModal = default!;

    private bool focusStatus;

    private EditContext issueContext = default!;

    private IssueForm issueForm = new();

    private bool leaving;

    private int? loadedId;

    private EditContext paidContext = default!;

    private PaidForm paidForm = new();

    private bool pendingDelete;

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        ResetForms();
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        statusMessage = statusText = null;
        ResetForms();

        if (Created == 1 && State.FindInvoice(Id) is { } invoice)
            SetStatus($"Created the draft: {invoice.Lines.Count} line(s), {Money(invoice.Currency, PbState.TotalOf(invoice))}. Check the lines, then issue it.", false);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Money(string currency, decimal amount) => string.Create(CultureInfo.InvariantCulture, $"{currency} {amount:#,0.00}");

    private async Task ConfirmDeleteAsync()
    {
        pendingDelete = true;
        await deleteModal.HideAsync();
    }

    private async Task Issue()
    {
        if (!await issueContext.ValidateAsync())
            return;

        var result = State.IssueInvoice(Id, issueForm.TermsDays);
        var invoice = State.FindInvoice(Id);
        SetStatus(result switch
        {
            PbResult.Ok => $"Issued {invoice?.Number}, due {PbDisplay.Date(invoice!.DueOn!.Value)}. Its lines are locked; the project managers are told.",
            PbResult.WrongState => "This invoice was issued or deleted meanwhile. Nothing changed.",
            PbResult.Invalid => "A draft needs at least one line and payment terms to be issued. Nothing changed.",
            PbResult.NotAllowed => "Only account managers can issue invoices. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        }, result != PbResult.Ok);
        focusStatus = true;
    }

    private async Task MarkPaid()
    {
        if (State.FindInvoice(Id) is not { IssuedOn: { } issued })
            return;

        paidForm.IssuedOn = issued;
        paidForm.Today = State.Clock.Today;

        if (!await paidContext.ValidateAsync())
            return;

        var result = State.MarkInvoicePaid(Id, paidForm.PaidOn!.Value);
        SetStatus(result switch
        {
            PbResult.Ok => $"Marked paid on {PbDisplay.Date(paidForm.PaidOn.Value)}.",
            PbResult.WrongState => "This invoice was paid meanwhile, or isn't issued. Nothing changed.",
            PbResult.Invalid => "Pick a payment date from the issue date to today. Nothing changed.",
            PbResult.NotAllowed => "Only account managers can mark invoices paid. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        }, result != PbResult.Ok);
        focusStatus = true;
    }

    private void OnDeleteHidden()
    {
        if (!pendingDelete)
        {
            _ = PbDisplay.FocusAsync(deleteButton?.Element);

            return;
        }

        pendingDelete = false;
        var result = State.DeleteDraftInvoice(Id);

        if (result == PbResult.Ok)
        {
            leaving = true;
            Navigation.NavigateTo(Invoices.ListUrl);

            return;
        }

        SetStatus(result switch
        {
            PbResult.WrongState => "This invoice was issued meanwhile, so it can't be deleted.",
            PbResult.NotFound => "That draft was already deleted.",
            PbResult.NotAllowed => "Only account managers can delete drafts. Nothing was deleted.",
            _ => "That didn't work, and nothing was deleted. Try again.",
        }, true);
        focusStatus = true;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void RemoveLine(int lineId)
    {
        var line = State.FindInvoice(Id)?.Lines.FirstOrDefault(item => item.Id == lineId);
        var label = line is null ? "That line" : $"{State.FindPerson(line.PersonId)?.Name}'s line on {State.FindProject(line.ProjectId)?.Key}";
        var result = State.RemoveInvoiceLine(Id, lineId);
        SetStatus(result switch
        {
            PbResult.Ok => $"Removed {label}. Its {line!.Hours.ToString("0.##", CultureInfo.InvariantCulture)} hours are ready to invoice again.",
            PbResult.WrongState => "This invoice was issued meanwhile, so its lines are locked.",
            PbResult.NotFound => "That line was already removed.",
            PbResult.NotAllowed => "Only account managers can change drafts. Nothing changed.",
            _ => "That didn't work, and nothing changed. Try again.",
        }, result != PbResult.Ok);
        focusStatus = true;
    }

    private void ResetForms()
    {
        issueForm = new IssueForm { TermsDays = 30 };
        issueContext = new EditContext(issueForm);
        paidForm = new PaidForm { PaidOn = State.Clock.Today };
        paidContext = new EditContext(paidForm);
    }

    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether the draft was just created.</summary>
    [SupplyParameterFromQuery(Name = "created")] public int? Created { get; set; }

    /// <summary>Gets or sets the invoice.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The payment terms: one of 14, 30, 45, or 60 days.</summary>
    private sealed class IssueForm : IValidatableObject
    {
        public int TermsDays { get; set; }

        /// <summary>Runs on submit: the terms are one the agency offers.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!PbSeedData.PaymentTerms.Contains(TermsDays))
                yield return new ValidationResult("Choose the payment terms: Net 14, 30, 45, or 60.", new[] { nameof(TermsDays) });
        }
    }

    /// <summary>The payment date: required, from the issue date to today (set by the page before it validates).</summary>
    private sealed class PaidForm : IValidatableObject
    {
        /// <summary>The invoice's issue date, set by the page.</summary>
        public DateTime IssuedOn { get; set; } = DateTime.MinValue;

        [Required(ErrorMessage = "Pick the day the payment arrived.")]
        public DateTime? PaidOn { get; set; }

        /// <summary>The board's today, set by the page.</summary>
        public DateTime Today { get; set; } = DateTime.MaxValue;

        /// <summary>Runs once the date is filled in: it's within the issue date and today.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaidOn is { } paid && paid.Date < IssuedOn.Date)
                yield return new ValidationResult($"Pick {PbDisplay.Date(IssuedOn)} or later: the invoice was issued then.", new[] { nameof(PaidOn) });
            else if (PaidOn is { } late && late.Date > Today.Date)
                yield return new ValidationResult("Pick today or an earlier day: a payment is recorded once it arrives.", new[] { nameof(PaidOn) });
        }
    }

    #endregion
}
