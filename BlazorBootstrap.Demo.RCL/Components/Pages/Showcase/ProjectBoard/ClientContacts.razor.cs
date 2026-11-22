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
/// The contacts section of the client page (account managers): the client's people in a <see cref="Grid{TItem}" /> (name
/// with "Primary" in words, role, job title, email and phone as plain text, and Delete) with paging, sorting, the column
/// chooser, and CSV export; the view (page size, sort, columns) is saved for the visit and can be reset. A contact has no
/// page of its own, so the name is a stretched button that opens the edit dialog from anywhere on the row. Add contact and
/// the name open a <see cref="Modal" /> with an <see cref="EditForm" /> (rule WR-002): name (required), role (required), job
/// title, email (required, an email, unique at this client, checked as you type), phone (digits and + - ( ) .), and the
/// primary checkbox (<see cref="PbState.SaveContact" />). Messages show after a field is touched or on submit, with a linked
/// summary; the state's refusals go to their field or the dialog's alert. Delete asks first and runs once the dialog is
/// hidden. Focus goes back to the pressed button, or to the section's status message when it is gone.
/// </summary>
public partial class ClientContacts : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private readonly Dictionary<int, ElementReference> editButtons = new();

    private ElementReference? addButton;

    /// <summary>The contact the delete dialog is about.</summary>
    private int deleteId;

    private Modal deleteModal = default!;

    private EditContext editContext = default!;

    /// <summary>The contact the form edits, or 0 for a new one.</summary>
    private int editId;

    private Modal editorModal = default!;

    private ShowcaseUniqueCheck? emailCheck;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private ContactForm form = new();

    private int formErrorCount;

    private string? formMessage;

    private string? formText;

    private Grid<ContactRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private InputText? nameInput;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>Whether Delete was confirmed in the dialog: it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    /// <summary>The message to announce once the form's dialog is hidden after a save.</summary>
    private string? savedMessage;

    private bool saving;

    /// <summary>The state's answers that belong to a field (a taken email), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await PbDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "client-contacts", DefaultPageSize, null, SortDirection.None);
        ResetForm(new ContactForm());
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

    private string EmailTakenMessage(string email) =>
        State.ContactsOf(ClientId).FirstOrDefault(contact => contact.Id != editId && string.Equals(contact.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)) is { } other
            ? $"{other.Name} at this client already has {email.Trim().ToLowerInvariant()}. Use another email address."
            : $"Another contact at this client already has {email.Trim().ToLowerInvariant()}.";

    private Task<bool> IsEmailTakenAsync(string email) => Task.FromResult(State.IsContactEmailTaken(ClientId, email, editId));

    /// <summary>A change clears the dialog's alert and the state's message on that field.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);
        formMessage = formText = null;
        formErrorCount = 0;
    }

    /// <summary>Returns the button that opened the form (the row's name, or Add contact), or the status message when it is gone.</summary>
    private ElementReference? OpenerButton(int contactId) =>
        contactId == 0 ? addButton : State.FindContact(contactId) is not null && editButtons.TryGetValue(contactId, out var button) ? button : statusRegion;

    /// <summary>Runs a confirmed deletion now that the dialog is hidden, and says what happened.</summary>
    private void OnDeleteHidden()
    {
        var id = deleteId;

        if (!pendingDelete)
        {
            focusTarget = () => statusRegion;
            StateHasChanged();

            return;
        }

        pendingDelete = false;
        var contact = State.FindContact(id);
        var result = State.DeleteContact(id);

        Announce(result == PbResult.Ok, result switch
        {
            PbResult.Ok => $"{contact?.Name ?? "The contact"} is deleted.{(contact?.IsPrimary == true ? " The client has no primary contact now." : null)}",
            PbResult.NotFound => "That contact was already deleted.",
            PbResult.NotAllowed => "Only account managers can delete contacts. Nothing was deleted.",
            _ => "That didn't work, and nothing was deleted. Try again.",
        });
        editButtons.Remove(id);
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    /// <summary>Announces a save once the form's dialog is hidden; focus goes back to the button that opened it.</summary>
    private void OnEditorHidden()
    {
        var id = editId;
        saving = false;

        if (savedMessage is { } message)
        {
            savedMessage = null;
            Announce(true, message);
            focusTarget = () => statusRegion;
        }
        else
        {
            focusTarget = () => OpenerButton(id) ?? statusRegion;
        }

        StateHasChanged();
    }

    private async Task OnEditorShownAsync() => await PbDisplay.FocusAsync(nameInput?.Element);

    /// <summary>Another screen or this one changed the state: the grid reads the contacts again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenDeleteAsync(int contactId)
    {
        deleteId = contactId;
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    private async Task OpenEditorAsync(int contactId)
    {
        var contact = contactId == 0 ? null : State.FindContact(contactId);

        editId = contact?.Id ?? 0;
        ResetForm(new ContactForm
        {
            Name = contact?.Name ?? string.Empty,
            Title = contact?.Title ?? string.Empty,
            Email = contact?.Email ?? string.Empty,
            Phone = contact?.Phone ?? string.Empty,
            Role = contact?.Role,
            IsPrimary = contact?.IsPrimary ?? State.ContactsOf(ClientId).Count == 0,
        });
        formMessage = formText = savedMessage = null;
        formErrorCount = 0;
        saving = false;
        await editorModal.ShowAsync();
    }

    private string PrimaryHelp(string clientName)
    {
        var current = State.ContactsOf(ClientId).FirstOrDefault(contact => contact.IsPrimary);

        return current is null || current.Id == editId
            ? $"The main person to talk to at {clientName}. A client has one primary contact."
            : $"The main person to talk to at {clientName}. Now {current.Name}; choosing this contact replaces them.";
    }

    /// <summary>The client's contacts as grid rows: the primary contact first, then by name, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<ContactRow>> ProvideContactsAsync(GridDataProviderRequest<ContactRow> request) =>
        view.ApplyAsync(request, State.ContactsOf(ClientId).Select(contact => new ContactRow(contact.Id, contact.Name, contact.IsPrimary, PbState.RoleWord(contact.Role), (int)contact.Role,
            contact.Title, contact.Email, contact.Phone)).ToList());

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private void ResetForm(ContactForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the status message.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        Announce(true, "The contacts view is reset: primary contact first, 10 a page, and every column shown.");
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    /// <summary>
    /// Saves once the form is valid: the email check finishes first, then every rule runs, and a failed submit shows the
    /// summary (rule WR-002). The state checks again; its answer maps to a field or the dialog's alert.
    /// </summary>
    private async Task SaveAsync()
    {
        if (saving)
            return;

        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();

        var emailFree = emailCheck is null || await emailCheck.CheckNowAsync();

        if (!await editContext.ValidateAsync() || !emailFree)
            return;

        var input = new PbContactInput(form.Name, form.Title ?? string.Empty, form.Email, form.Phone ?? string.Empty, form.Role!.Value, form.IsPrimary);
        var result = State.SaveContact(editId, ClientId, input, out var saved);

        switch (result)
        {
            case PbResult.Ok when saved is not null:
                savedMessage = editId == 0
                    ? $"Added {saved.Name}{(saved.IsPrimary ? " as the primary contact" : null)}."
                    : $"Saved {saved.Name}{(saved.IsPrimary ? ", the primary contact" : null)}.";
                saving = true;
                await editorModal.HideAsync();

                return;

            case PbResult.NoChange:
                savedMessage = $"Nothing changed for {form.Name.Trim()}.";
                saving = true;
                await editorModal.HideAsync();

                return;

            case PbResult.Duplicate:
                ServerError(() => form.Email, EmailTakenMessage(form.Email));

                return;

            case PbResult.NotFound:
                ShowFormError(State.FindClient(ClientId) is null ? "This client was deleted meanwhile, so nothing was saved." : "This contact was deleted meanwhile, so nothing was saved.");

                return;

            case PbResult.Invalid when editId == 0 && State.AllContacts.Count >= PbState.MaxContacts:
                ShowFormError($"The board holds up to {PbState.MaxContacts} contacts, and it's full. Delete a contact you don't need first. Nothing was added.");

                return;

            case PbResult.Invalid:
                ShowFormError(DealDisplay.StorageFull);

                return;

            case PbResult.NotAllowed:
                ShowFormError("Only account managers can add or edit contacts. Nothing was saved.");

                return;

            default:
                ShowFormError("That didn't work, and nothing was saved. Try again.");

                return;
        }
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    /// <summary>Shows the dialog's alert; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void ShowFormError(string message)
    {
        formErrorCount = message == formText ? formErrorCount + 1 : 1;
        formText = message;
        formMessage = formErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the client.</summary>
    [Parameter, EditorRequired] public int ClientId { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// A contact's fields, with the limits of <see cref="PbState.SaveContact" />: a required name and role, a required email
    /// in its form, and optional job title and phone.
    /// </summary>
    private sealed class ContactForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the contact's email address.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer for the email address.")]
        [RegularExpression(@"^\s*[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}\s*$", ErrorMessage = "Enter an email address like name@example.com.")]
        public string Email { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        [Required(ErrorMessage = "Enter the contact's name.")]
        [StringLength(PbState.MaxContactFieldLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(PbState.MaxPhoneLength, ErrorMessage = "Use 30 characters or fewer for the phone number.")]
        [RegularExpression(@"^\s*\+?[0-9 ().\-]+\s*$", ErrorMessage = "Enter the phone number with digits, spaces, and + - ( ) . only, like +1 555 0101.")]
        public string? Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose the contact's role, like Decision maker or Billing contact.")]
        public PbContactRole? Role { get; set; }

        [StringLength(PbState.MaxContactFieldLength, ErrorMessage = "Use 80 characters or fewer for the job title.")]
        public string? Title { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: a phone number has at least 7 digits.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Phone) && Phone.Trim().Length <= PbState.MaxPhoneLength && !PbState.IsPhone(Phone.Trim()))
                yield return new ValidationResult("Enter a phone number with at least 7 digits, like +1 555 0101.", new[] { nameof(Phone) });
        }
    }

    /// <summary>One contact as the grid shows and exports it.</summary>
    private sealed record ContactRow(int Id, string Name, bool IsPrimary, string Role, int RoleOrder, string JobTitle, string Email, string Phone);

    #endregion
}
