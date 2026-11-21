using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Ecommerce;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Ecommerce;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>
/// The customer's address book: up to ten addresses as cards with the default marked and the same-day delivery area
/// noted, a dialog to add or edit an address (with browser autofill tokens), "Set as default", and a confirmed delete.
/// Focus returns to the button that opened a dialog, or to the status message when that button is gone.
/// </summary>
public partial class AccountAddresses : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int MaxAddresses = EcommerceSeedData.MaxSavedAddresses;

    /// <summary>The countries checkout offers.</summary>
    private static readonly IReadOnlyList<string> countries = EcommerceSeedData.Countries;

    private Button? addButton;

    private bool deleted;

    private readonly Dictionary<int, Button> deleteButtons = new();

    private EcommerceSavedAddress? deleting;

    private Modal deleteModal = default!;

    private readonly Dictionary<int, Button> editButtons = new();

    private EditContext editContext = default!;

    /// <summary>The ID of the address being edited; 0 while adding one.</summary>
    private int editingId;

    /// <summary>Whether the address being edited is the default.</summary>
    private bool editingIsDefault;

    /// <summary>Whether the address being added is the account's first, and so its default.</summary>
    private bool editingIsFirst;

    private Modal editorModal = default!;

    private AddressForm form = new();

    private string? formError;

    private int formErrors;

    private ShowcaseUniqueCheck? labelCheck;

    private InputText? labelInput;

    private ValidationMessageStore messageStore = default!;

    private string? pageError;

    private int pageErrors;

    private bool saved;

    private int savedId;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        ResetForm(new AddressForm());
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

    private async Task AddAsync(int count)
    {
        if (count >= MaxAddresses)
        {
            pageErrors++;
            var message = $"Your address book is full ({MaxAddresses} of {MaxAddresses}). Delete an address to add another.";
            pageError = pageErrors == 1 ? message : $"{message} ({pageErrors})";

            return;
        }

        pageError = null;
        await OpenEditorAsync(null, isFirst: count == 0);
    }

    private async Task AskDeleteAsync(EcommerceSavedAddress address)
    {
        deleting = address;
        deleted = false;
        pageError = null;

        await deleteModal.ShowAsync();
    }

    private async Task ConfirmDeleteAsync()
    {
        if (deleting is { } address && State.DeleteAddress(address.Id))
        {
            deleted = true;
            var next = address.IsDefault && State.Identity.Current is { } account
                ? State.AddressesFor(account.Id).FirstOrDefault(other => other.IsDefault)
                : null;

            SetStatus(next is null
                ? $"The address \"{address.Label}\" is deleted."
                : $"The address \"{address.Label}\" is deleted. \"{next.Label}\" is now your default address.");
        }

        await deleteModal.HideAsync();
    }

    private Task EditAsync(EcommerceSavedAddress address)
    {
        pageError = null;

        return OpenEditorAsync(address, isFirst: false);
    }

    /// <summary>The dialog opens from code, so Bootstrap does not return focus: after a delete the card is gone, so focus goes to the status.</summary>
    private async Task OnDeleteHiddenAsync()
    {
        var address = deleting;
        deleting = null;

        if (deleted)
        {
            deleted = false;
            await EcommerceDisplay.FocusAsync(statusRegion);
        }
        else if (address is not null && deleteButtons.TryGetValue(address.Id, out var button))
        {
            await EcommerceDisplay.FocusAsync(button.Element);
        }
    }

    /// <summary>
    /// Returns focus after the editor closes: to the saved card's Edit button, to the status after adding an address, or
    /// to the button that opened the dialog when it was cancelled.
    /// </summary>
    private async Task OnEditorHiddenAsync()
    {
        var id = editingId;
        var wasSaved = saved;
        var editId = wasSaved ? savedId : id;

        editingId = 0;
        saved = false;
        savedId = 0;
        formError = null;
        ResetForm(new AddressForm());

        if (wasSaved && id == 0)
            await EcommerceDisplay.FocusAsync(statusRegion);
        else if (editId > 0 && editButtons.TryGetValue(editId, out var editButton))
            await EcommerceDisplay.FocusAsync(editButton.Element);
        else if (id == 0)
            await EcommerceDisplay.FocusAsync(addButton?.Element);
        else
            await EcommerceDisplay.FocusAsync(statusRegion);
    }

    private Task OnEditorShownAsync() => EcommerceDisplay.FocusAsync(labelInput?.Element);

    /// <summary>A server-side error (an unlisted country) clears once the field changes, so the form can be sent again.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        messageStore.Clear(args.FieldIdentifier);
        editContext.NotifyValidationStateChanged();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenEditorAsync(EcommerceSavedAddress? address, bool isFirst)
    {
        editingId = address?.Id ?? 0;
        editingIsDefault = address?.IsDefault ?? false;
        editingIsFirst = isFirst;
        saved = false;
        savedId = 0;
        formError = null;
        formErrors = 0;
        ResetForm(address is null
            ? new AddressForm { IsDefault = isFirst }
            : new AddressForm
            {
                Label = address.Label,
                FullName = address.FullName,
                Street = address.Street,
                City = address.City,
                PostalCode = address.PostalCode,
                Country = countries.Contains(address.Country) ? address.Country : countries[0],
                Phone = address.Phone,
                IsDefault = address.IsDefault,
            });

        await editorModal.ShowAsync();
    }

    private void ResetForm(AddressForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        editContext.OnFieldChanged += OnFieldChanged;
        messageStore = new ValidationMessageStore(editContext);
    }

    private Task<bool> IsLabelTakenAsync(string label) => Task.FromResult(State.IsAddressLabelTaken(label, editingId));

    private static string LabelTakenMessage(string label) => $"You already have an address called “{label.Trim()}”. Give this one another label.";

    private async Task SaveAsync()
    {
        // Rules the attributes can't express: the country must be one checkout ships to, and the label is unique in the book.
        messageStore.Clear();

        if (labelCheck is not null && !await labelCheck.CheckNowAsync())
            return;

        if (!countries.Contains(form.Country))
        {
            messageStore.Add(() => form.Country, "Choose a country from the list.");
            editContext.NotifyValidationStateChanged();

            return;
        }

        if (State.Identity.Current is not { } account)
            return;

        if (State.IsAddressLabelTaken(form.Label, editingId))
        {
            messageStore.Add(() => form.Label, LabelTakenMessage(form.Label));
            editContext.NotifyValidationStateChanged();

            return;
        }

        var address = new EcommerceSavedAddress(
            editingId,
            account.Id,
            form.Label,
            form.FullName,
            form.Street,
            form.City,
            form.PostalCode,
            form.Country,
            string.IsNullOrWhiteSpace(form.Phone) ? null : form.Phone,
            form.IsDefault || DefaultLocked);

        if (State.SaveAddress(address) is not { } result)
        {
            formErrors++;
            var message = editingId == 0
                ? $"The address couldn't be added: your address book holds up to {MaxAddresses} addresses. Delete one first."
                : "The address couldn't be saved: it's no longer in your address book.";
            formError = formErrors == 1 ? message : $"{message} ({formErrors})";

            return;
        }

        saved = true;
        savedId = result.Id;
        var verb = editingId == 0 ? "added" : "saved";
        SetStatus(result.IsDefault
            ? $"The address \"{result.Label}\" is {verb} and is your default address."
            : $"The address \"{result.Label}\" is {verb}.");

        await editorModal.HideAsync();
    }

    /// <summary>The card moves to the top and loses its "Set as default" button, so focus goes to the status.</summary>
    private async Task SetDefaultAsync(EcommerceSavedAddress address)
    {
        pageError = null;

        if (!State.SetDefaultAddress(address.Id))
            return;

        SetStatus($"\"{address.Label}\" is now your default address.");
        StateHasChanged();

        await Task.Yield();
        await EcommerceDisplay.FocusAsync(statusRegion);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets whether the default switch is fixed on: the default can only move to another address, and a first address is always the default.</summary>
    private bool DefaultLocked => editingIsDefault || editingIsFirst;

    private string DefaultHelp => editingIsDefault
        ? "This is your default address. To change it, set another address as the default."
        : editingIsFirst
            ? "Your first address is your default."
            : "Your default address is listed first in your address book.";

    [Inject] private EcommerceState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class AddressForm
    {
        [Required(ErrorMessage = "Enter the city.")]
        [StringLength(60, ErrorMessage = "Use 60 characters or fewer.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose a country.")]
        public string Country { get; set; } = "United States";

        [Required(ErrorMessage = "Enter the full name for delivery.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Use 2 to 60 characters.")]
        public string FullName { get; set; } = string.Empty;

        public bool IsDefault { get; set; }

        [Required(ErrorMessage = "Name this address, for example Home.")]
        [StringLength(30, ErrorMessage = "Use 30 characters or fewer.")]
        public string Label { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Use 20 characters or fewer.")]
        [RegularExpression(@"^\+?[0-9 ()\-]{7,20}$", ErrorMessage = "Use 7 to 20 digits, spaces, brackets, or hyphens, with an optional + at the start.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Enter the postal code.")]
        [RegularExpression(@"^[A-Za-z0-9 \-]{3,10}$", ErrorMessage = "Use 3 to 10 letters, digits, spaces, or hyphens.")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the street address.")]
        [StringLength(100, ErrorMessage = "Use 100 characters or fewer.")]
        public string Street { get; set; } = string.Empty;
    }

    #endregion
}
