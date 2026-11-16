using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The contacts section of the client page (account managers): a table with a caption and header scopes of the client's
/// people (name with "Primary" in words, job title, email and phone as plain text), Add contact, and per row Edit and
/// Delete. Add and Edit open a <see cref="Modal" /> form (name, job title, email, phone, primary checkbox;
/// <see cref="PbState.SaveContact" />): each problem is shown under its field and the first invalid field takes focus
/// after the render that shows it; Duplicate means the email is taken at this client. Delete asks first and runs once the
/// dialog is hidden. Focus goes back to the pressed button, or to the section's status message when it is gone.
/// </summary>
public partial class ClientContacts : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> editButtons = new();

    private ElementReference? addButton;

    /// <summary>The contact the delete dialog is about.</summary>
    private int deleteId;

    private Modal deleteModal = default!;

    /// <summary>The contact the form edits, or 0 for a new one.</summary>
    private int editId;

    private Modal editorModal = default!;

    private string email = string.Empty;

    private string? emailError;

    private ElementReference? emailInput;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private string? formMessage;

    private ElementReference? formStatusRegion;

    private bool isPrimary;

    private string jobTitle = string.Empty;

    private string name = string.Empty;

    private string? nameError;

    private ElementReference? nameInput;

    /// <summary>Whether Delete was confirmed in the dialog: it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    private string phone = string.Empty;

    private string? phoneError;

    private ElementReference? phoneInput;

    /// <summary>The message to announce once the form's dialog is hidden after a save.</summary>
    private string? savedMessage;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? titleError;

    private ElementReference? titleInput;

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

    [GeneratedRegex(@"^[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}$")]
    private static partial Regex EmailPattern();

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

    /// <summary>Returns the button that opened the form (the row's Edit, or Add contact), or the status message when it is gone.</summary>
    private ElementReference? OpenerButton(int contactId) =>
        contactId == 0 ? addButton : State.FindContact(contactId) is not null && editButtons.TryGetValue(contactId, out var button) ? button : statusRegion;

    /// <summary>Runs a confirmed deletion now that the dialog is hidden, and says what happened.</summary>
    private void OnDeleteHidden()
    {
        var id = deleteId;

        if (!pendingDelete)
        {
            focusTarget = () => OpenerButton(id) ?? statusRegion;
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

    private async Task OnEditorShownAsync() => await PbDisplay.FocusAsync(nameInput);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

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
        name = contact?.Name ?? string.Empty;
        jobTitle = contact?.Title ?? string.Empty;
        email = contact?.Email ?? string.Empty;
        phone = contact?.Phone ?? string.Empty;
        isPrimary = contact?.IsPrimary ?? State.ContactsOf(ClientId).Count == 0;
        nameError = titleError = emailError = phoneError = formMessage = savedMessage = null;
        await editorModal.ShowAsync();
    }

    private string PrimaryHelp(string clientName)
    {
        var current = State.ContactsOf(ClientId).FirstOrDefault(contact => contact.IsPrimary);

        return current is null || current.Id == editId
            ? $"The main person to talk to at {clientName}. A client has one primary contact."
            : $"The main person to talk to at {clientName}. Now {current.Name}; choosing this contact replaces them.";
    }

    /// <summary>Checks the form, saves, and maps the state's answer to a field or the dialog's status line.</summary>
    private async Task SaveAsync()
    {
        var trimmedName = name.Trim();
        var trimmedTitle = jobTitle.Trim();
        var trimmedEmail = email.Trim().ToLowerInvariant();
        var trimmedPhone = phone.Trim();

        formMessage = null;
        nameError = trimmedName.Length == 0 ? "Enter the contact's name."
            : trimmedName.Length > PbState.MaxContactFieldLength ? $"Shorten the name to {PbState.MaxContactFieldLength} characters or fewer (now {trimmedName.Length})."
            : null;
        titleError = trimmedTitle.Length > PbState.MaxContactFieldLength ? $"Shorten the job title to {PbState.MaxContactFieldLength} characters or fewer (now {trimmedTitle.Length})." : null;
        emailError = trimmedEmail.Length == 0 ? "Enter the contact's email address."
            : trimmedEmail.Length > ShowcaseIdentity.MaxEmailLength ? $"Shorten the email address to {ShowcaseIdentity.MaxEmailLength} characters or fewer."
            : !EmailPattern().IsMatch(trimmedEmail) ? "Enter an email address like name@example.com."
            : TakenBy(trimmedEmail) is { } taken ? $"{taken.Name} at this client already has {trimmedEmail}. Use another email address."
            : null;
        phoneError = trimmedPhone.Length > PbState.MaxPhoneLength ? $"Shorten the phone number to {PbState.MaxPhoneLength} characters or fewer (now {trimmedPhone.Length})." : null;

        focusTarget = nameError is not null ? () => nameInput
            : titleError is not null ? () => titleInput
            : emailError is not null ? () => emailInput
            : phoneError is not null ? () => phoneInput
            : null;

        if (focusTarget is not null)
            return;

        var result = State.SaveContact(editId, ClientId, trimmedName, trimmedTitle, trimmedEmail, trimmedPhone, isPrimary, out var saved);

        switch (result)
        {
            case PbResult.Ok when saved is not null:
                savedMessage = editId == 0
                    ? $"Added {saved.Name}{(saved.IsPrimary ? " as the primary contact" : null)}."
                    : $"Saved {saved.Name}{(saved.IsPrimary ? ", the primary contact" : null)}.";
                await editorModal.HideAsync();

                return;

            case PbResult.NoChange:
                savedMessage = $"Nothing changed for {trimmedName}.";
                await editorModal.HideAsync();

                return;

            case PbResult.Duplicate:
                emailError = TakenBy(trimmedEmail) is { } other ? $"{other.Name} at this client already has {trimmedEmail}. Use another email address." : $"Another contact at this client already has {trimmedEmail}.";
                focusTarget = () => emailInput;

                return;

            case PbResult.NotFound:
                formMessage = State.FindClient(ClientId) is null ? "This client was deleted meanwhile, so nothing was saved." : "This contact was deleted meanwhile, so nothing was saved.";
                focusTarget = () => formStatusRegion;

                return;

            case PbResult.Invalid when editId == 0 && State.Clients.Sum(client => State.ContactsOf(client.Id).Count) >= PbState.MaxContacts:
                formMessage = $"The board holds up to {PbState.MaxContacts} contacts, and it's full. Delete a contact you don't need first. Nothing was added.";
                focusTarget = () => formStatusRegion;

                return;

            case PbResult.Invalid:
                formMessage = DealDisplay.StorageFull;
                focusTarget = () => formStatusRegion;

                return;

            case PbResult.NotAllowed:
                formMessage = "Only account managers can add or edit contacts. Nothing was saved.";
                focusTarget = () => formStatusRegion;

                return;

            default:
                formMessage = "That didn't work, and nothing was saved. Try again.";
                focusTarget = () => formStatusRegion;

                return;
        }
    }

    private PbContact? TakenBy(string address) => State.ContactsOf(ClientId).FirstOrDefault(contact => contact.Id != editId && contact.Email == address);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the client.</summary>
    [Parameter, EditorRequired] public int ClientId { get; set; }

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
