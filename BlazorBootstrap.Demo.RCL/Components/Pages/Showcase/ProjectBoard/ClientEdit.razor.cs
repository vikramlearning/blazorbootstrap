using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Adds a client (<c>/clients/new</c>) or edits one (<c>/clients/{id}/edit</c>), for account managers, in an
/// <see cref="EditForm" /> (rule WR-002): name (required, with a counter, unique, checked as you type), industry
/// (required), website (a domain, optional; plain text, never a link), owner (an account manager; the signed-in one by
/// default), phone (digits and + - ( ) ., optional), billing currency (required), billing email (an email, optional), and the
/// contract's start and end (optional; an end needs a start and falls on or after it, a cross-field rule). Messages show
/// after a field is touched or on submit, and a failed submit shows a summary that links to each field. The state's
/// refusals (a taken name, an owner who is no longer an account manager) go to their field; the others to the alert. A
/// save goes to the client's page, which announces it. On the edit page, Delete opens a <see cref="Modal" /> that names what
/// goes with the client (contacts and activity), or lists the projects and deals that keep it; the deletion runs once the
/// dialog is hidden and goes back to the client list. An unknown ID shows <see cref="PbNotFound" />.
/// </summary>
public partial class ClientEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Whether the client was saved and the page is moving to it.</summary>
    private bool busy;

    private Button? deleteButton;

    private Modal deleteModal = default!;

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private bool focusDelete;

    private bool focusStatus;

    private ClientForm form = new();

    /// <summary>The client was just deleted and the list is loading: no not-found flash in between.</summary>
    private bool leaving;

    /// <summary>The route ID the form was loaded for (0 for a new client), or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private ShowcaseUniqueCheck? nameCheck;

    /// <summary>Whether Delete was confirmed in the dialog: it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    /// <summary>The state's answers that belong to a field (a taken name, an owner who can't own clients), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusDelete)
        {
            focusDelete = false;
            await PbDisplay.FocusAsync(deleteButton?.Element ?? statusRegion);
        }

        if (focusStatus)
        {
            focusStatus = false;
            await PbDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        var routeId = Id ?? 0;

        if (loadedId == routeId)
            return;

        loadedId = routeId;
        statusMessage = null;
        statusIsError = false;
        pendingDelete = false;
        ClearError();

        if (Client is { } client)
        {
            ResetForm(new ClientForm
            {
                Name = client.Name, Industry = client.Industry, Website = client.Website, OwnerId = client.OwnerId, Phone = client.Phone, BillingCurrency = client.BillingCurrency,
                BillingEmail = client.BillingEmail, ContractStart = client.ContractStart, ContractEnd = client.ContractEnd,
            });
        }
        else
        {
            ResetForm(new ClientForm
            {
                OwnerId = State.AccountManagers.Any(person => person.Id == State.CurrentPerson?.Id) ? State.CurrentPerson!.Id : State.AccountManagers.FirstOrDefault()?.Id ?? string.Empty,
                BillingCurrency = PbSeedData.Currencies[0],
            });
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    /// <summary>
    /// Clears <see cref="Id" /> first: the new-client route has no ID, and the router reuses this page when moving between
    /// the two routes, so the previous client's ID would otherwise stay.
    /// </summary>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Id = null;

        return base.SetParametersAsync(parameters);
    }

    #endregion

    #region Private Methods

    private static string CurrencyName(string code) => code switch
    {
        "USD" => "US dollar",
        "EUR" => "Euro",
        "GBP" => "Pound sterling",
        "CAD" => "Canadian dollar",
        "AUD" => "Australian dollar",
        _ => code,
    };

    private static string DateValue(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string DuplicateText(string name) => $"A client named “{name.Trim()}” already exists. Use another name, or open the existing client from the client list.";

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>The projects and deals that keep a client from being deleted, in words.</summary>
    private List<string> Blockers(PbClient client) =>
        State.Projects.Where(project => project.ClientId == client.Id).Select(project => $"project {project.Key}")
            .Concat(State.DealsOf(client.Id).Select(deal => $"deal “{deal.Title}”"))
            .ToList();

    private void ClearError()
    {
        errorMessage = errorText = null;
        errorCount = 0;
    }

    private async Task ConfirmDeleteAsync()
    {
        pendingDelete = true;
        await deleteModal.HideAsync();
    }

    private string DeleteConsequence(PbClient client)
    {
        var contacts = State.ContactsOf(client.Id).Count;
        var activities = State.ActivitiesOf(client.Id).Count;

        return $"{client.Name} leaves the client list, with its {DealDisplay.Plural(contacts, "contact")} and {DealDisplay.Plural(activities, "activity entry", "activity entries")}.";
    }

    private Task<bool> IsNameTakenAsync(string name) => Task.FromResult(State.IsClientNameTaken(name, Client?.Id ?? 0));

    private string NameTakenMessage(string name) => DuplicateText(name);

    /// <summary>Runs a confirmed deletion now that the dialog is hidden, and says what happened.</summary>
    private void OnDeleteHidden()
    {
        if (!pendingDelete || Client is not { } client)
        {
            pendingDelete = false;
            focusDelete = true;
            StateHasChanged();

            return;
        }

        pendingDelete = false;
        var result = State.DeleteClient(client.Id);

        switch (result)
        {
            case PbResult.Ok:
                leaving = true;
                Navigation.NavigateTo(DealDisplay.ClientsUrl + "?deleted=1");

                return;

            case PbResult.WrongState:
                Announce(false, $"{client.Name} can't be deleted: it still has {string.Join(", ", Blockers(client))}. Nothing was deleted.");
                break;

            case PbResult.NotFound:
                Announce(false, "That client was already deleted.");
                break;

            case PbResult.NotAllowed:
                Announce(false, "Only account managers can delete clients. Nothing was deleted.");
                break;

            default:
                Announce(false, "That didn't work, and nothing was deleted. Try again.");
                break;
        }

        focusDelete = true;
        StateHasChanged();
    }

    /// <summary>A change clears the last error and the state's message on that field; a new start date checks the end again.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);

        if (args.FieldIdentifier.FieldName == nameof(ClientForm.ContractStart))
            editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.ContractEnd));

        ClearError();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteAsync()
    {
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    private void ResetForm(ClientForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>
    /// Saves once the form is valid: the uniqueness check finishes first, then every rule runs, and a failed submit shows the
    /// summary (rule WR-002). The state checks again; its answer maps to a field, the status message, or the alert.
    /// </summary>
    private async Task SaveAsync()
    {
        if (busy)
            return;

        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!editContext.Validate() || !nameFree)
            return;

        var existing = Client;

        if (existing is null && State.Clients.Count >= PbState.MaxClients)
        {
            ShowError($"The board holds up to {PbState.MaxClients} clients, and it's full. Delete a client you don't need first. Nothing was added.");

            return;
        }

        var input = new PbClientInput(form.Name, form.Industry, form.Website ?? string.Empty, form.OwnerId, form.BillingCurrency, form.BillingEmail ?? string.Empty, form.Phone ?? string.Empty,
            form.ContractStart, form.ContractEnd);
        var result = State.SaveClient(existing?.Id ?? 0, input, out var saved);

        switch (result)
        {
            case PbResult.Ok when saved is not null:
                busy = true;
                Navigation.NavigateTo(PbDisplay.ClientUrl(saved.Id) + (existing is null ? "?created=1" : "?saved=1"));

                return;

            case PbResult.NoChange:
                Announce(true, "Nothing changed, so there was nothing to save.");
                focusStatus = true;

                return;

            case PbResult.Duplicate:
                ServerError(() => form.Name, DuplicateText(form.Name));

                return;

            case PbResult.NotFound:
                ShowError("This client was deleted meanwhile, so there's nothing to save.");

                return;

            case PbResult.Invalid when State.AccountManagers.All(person => person.Id != form.OwnerId):
                ServerError(() => form.OwnerId, "That person is no longer an account manager. Choose another owner.");

                return;

            case PbResult.Invalid:
                ShowError(DealDisplay.StorageFull);

                return;

            case PbResult.NotAllowed:
                ShowError("Only account managers can add or edit clients. Nothing was saved.");

                return;

            default:
                ShowError("That didn't work, and nothing was saved. Try again.");

                return;
        }
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the client to edit, or <see langword="null" /> for a new one.</summary>
    [Parameter] public int? Id { get; set; }

    private PbClient? Client => Id is { } id ? State.FindClient(id) : null;

    /// <summary>The latest date the state accepts (three years after the seed time).</summary>
    private static DateTime MaxDate => PbSeedData.Now.AddYears(3).Date;

    /// <summary>The earliest date the state accepts (three years before the seed time, rounded up to a whole day).</summary>
    private static DateTime MinDate => PbSeedData.Now.AddYears(-3).Date.AddDays(1);

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The client's fields, with the limits of <see cref="PbState.SaveClient" />: required text with lengths, a website, an
    /// email, and a phone number's form, a currency, and two cross-field rules on the contract dates.
    /// </summary>
    private sealed class ClientForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose the currency the client is billed in.")]
        public string BillingCurrency { get; set; } = string.Empty;

        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer for the billing email.")]
        [RegularExpression(@"^\s*[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}\s*$", ErrorMessage = "Enter the billing email like accounts@example.com, or leave it empty.")]
        public string? BillingEmail { get; set; } = string.Empty;

        public DateTime? ContractEnd { get; set; }

        public DateTime? ContractStart { get; set; }

        [Required(ErrorMessage = "Enter the client's industry, like Retail.")]
        [StringLength(PbState.MaxContactFieldLength, ErrorMessage = "Use 80 characters or fewer for the industry.")]
        public string Industry { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the client's name.")]
        [StringLength(PbState.MaxClientNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose the account manager who owns the client.")]
        public string OwnerId { get; set; } = string.Empty;

        [StringLength(PbState.MaxPhoneLength, ErrorMessage = "Use 30 characters or fewer for the phone number.")]
        [RegularExpression(@"^\s*\+?[0-9 ().\-]+\s*$", ErrorMessage = "Enter the phone number with digits, spaces, and + - ( ) . only, like +1 555 0100.")]
        public string? Phone { get; set; } = string.Empty;

        [StringLength(PbState.MaxWebsiteLength, ErrorMessage = "Use 80 characters or fewer for the website.")]
        public string? Website { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: the website's and phone's forms, and the contract dates in order.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Website) && !PbState.IsWebsite(Website.Trim()))
                yield return new ValidationResult("Enter the website as a domain like example.com, or leave it empty.", new[] { nameof(Website) });

            if (!string.IsNullOrWhiteSpace(Phone) && Phone.Trim().Length <= PbState.MaxPhoneLength && !PbState.IsPhone(Phone.Trim()))
                yield return new ValidationResult("Enter a phone number with at least 7 digits, like +1 555 0100.", new[] { nameof(Phone) });

            if (ContractEnd is not null && ContractStart is null)
                yield return new ValidationResult("Enter when the contract starts, or clear its end date.", new[] { nameof(ContractStart) });

            if (ContractStart is { } start && ContractEnd is { } end && end.Date < start.Date)
            {
                yield return new ValidationResult($"Choose a contract end on or after the start, {PbDisplay.Date(start)}.", new[] { nameof(ContractEnd) });
            }

            if ((ContractStart is { } from && (from < MinDate || from > MaxDate)) || (ContractEnd is { } to && (to < MinDate || to > MaxDate)))
                yield return new ValidationResult($"Choose contract dates from {PbDisplay.Date(MinDate)} to {PbDisplay.Date(MaxDate)}.", new[] { ContractStart is { } s && (s < MinDate || s > MaxDate) ? nameof(ContractStart) : nameof(ContractEnd) });
        }
    }

    #endregion
}
