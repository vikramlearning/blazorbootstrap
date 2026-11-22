using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Subscribing to status page updates: an <see cref="EditForm" /> (rule WR-002) with an email address (kept in this tab;
/// nothing is sent; its format checked, and checked as you type against the tab's subscriptions) for all components or
/// chosen ones (at least one), with a linked summary on a failed submit, and the state's answers (an address already
/// subscribed, a component gone, a full tab) mapped to the field or the status message. Below, the tab's subscriptions, each with its components (edited inline), Unsubscribe (after a confirmation
/// naming what is deleted), and its demo outbox: the emails "sent" when engineers post an update, newest first, each
/// expandable to its plain-text body and a link to the incident's public page. A <c>component</c> query parameter
/// preselects one component (the incident page links here with it). Focus moves to the first invalid field after the
/// render that shows its error, and outcomes are announced in status regions.
/// </summary>
public partial class Subscribe : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool editAll = true;

    private ElementReference editAllInput;

    private readonly Dictionary<int, ElementReference> editButtons = new();

    private readonly HashSet<string> editChosen = new();

    private string? editError;

    private ElementReference editFirstInput;

    /// <summary>The subscription whose components are being edited inline, if any.</summary>
    private int? editingId;

    private ShowcaseUniqueCheck? emailCheck;

    private SubscribeForm form = new();

    private EditContext formContext = new(new SubscribeForm());

    private string? formStatus;

    private bool formStatusIsError;

    private string? listStatus;

    private bool listStatusIsError;

    /// <summary>The element to focus once the next render has put it on the page.</summary>
    private Func<ElementReference?>? pendingFocus;

    private bool preselected;

    /// <summary>The state's answers that belong to a field (a component gone, an address taken), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private ElementReference subscriptionsHeading;

    private readonly Dictionary<int, ElementReference> unsubscribeButtons = new();

    /// <summary>Whether "Unsubscribe" was confirmed; the dialog's hidden handler then removes the subscription.</summary>
    private bool unsubscribeConfirmed;

    private MonitoringSubscription? unsubscribing;

    private Modal unsubscribeModal = default!;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (pendingFocus is { } target)
        {
            pendingFocus = null;
            await MonitoringDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        NewForm(new SubscribeForm());
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        // The incident page links here with its component; choose it once, and leave later choices to the user.
        // Only a component the status page shows can be chosen (a hidden one has no checkbox).
        if (preselected || State.FindService(Component) is not { IsPublic: true } service)
            return;

        preselected = true;
        form.AllComponents = false;
        form.ServiceIds = new List<string> { service.Id };
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private async Task AskUnsubscribeAsync(MonitoringSubscription subscription)
    {
        unsubscribing = subscription;
        unsubscribeConfirmed = false;
        await unsubscribeModal.ShowAsync();
    }

    private void CancelEdit()
    {
        var id = editingId;
        editingId = null;
        editError = null;
        listStatus = null;

        if (id is { } subscriptionId)
            pendingFocus = () => editButtons.TryGetValue(subscriptionId, out var button) ? button : subscriptionsHeading;
    }

    /// <summary>The chosen IDs in the status page's order.</summary>
    private List<string> ChosenInOrder(HashSet<string> selection) => State.StatusPageServices.Where(service => selection.Contains(service.Id)).Select(service => service.Id).ToList();

    /// <summary>The components in a sentence, like "all components" or "Payments API, Search".</summary>
    private string ComponentsPhrase(IReadOnlyList<string> serviceIds) => serviceIds.Count == 0 ? "all components" : ComponentsText(serviceIds);

    private string ComponentsText(IReadOnlyList<string> serviceIds) =>
        serviceIds.Count == 0 ? "All components" : string.Join(", ", State.StatusPageServices.Where(service => serviceIds.Contains(service.Id)).Select(service => service.Name));

    /// <summary>Hides the dialog first; the hidden handler unsubscribes, so the page changes only after the dialog is gone.</summary>
    private async Task ConfirmUnsubscribeAsync()
    {
        unsubscribeConfirmed = true;
        await unsubscribeModal.HideAsync();
    }

    private static string EmailCountText(int count) => count switch
    {
        0 => "no emails",
        1 => "1 email",
        _ => $"{count.ToString(CultureInfo.InvariantCulture)} emails",
    };

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: it goes back to the button, or to the list's heading once the subscription is gone.</summary>
    private void OnUnsubscribeHidden()
    {
        var subscription = unsubscribing;
        unsubscribing = null;

        if (subscription is null)
            return;

        if (!unsubscribeConfirmed)
        {
            pendingFocus = () => unsubscribeButtons.TryGetValue(subscription.Id, out var button) ? button : subscriptionsHeading;

            return;
        }

        unsubscribeConfirmed = false;
        var result = State.Unsubscribe(subscription.Id);
        listStatusIsError = result is not (MonitoringResult.Ok or MonitoringResult.NotFound);
        listStatus = result switch
        {
            MonitoringResult.Ok => $"Unsubscribed {subscription.Email}. Its demo emails were deleted from this tab.",
            MonitoringResult.NotFound => $"{subscription.Email} was already unsubscribed.",
            // NotAllowed, Invalid, WrongState, NoChange, and Duplicate don't apply to unsubscribing.
            _ => $"{subscription.Email} is still subscribed. Try again.",
        };

        if (result is MonitoringResult.Ok or MonitoringResult.NotFound)
        {
            editButtons.Remove(subscription.Id);
            unsubscribeButtons.Remove(subscription.Id);

            if (editingId == subscription.Id)
                editingId = null;
        }

        pendingFocus = () => subscriptionsHeading;
    }

    private void SaveEdit()
    {
        if (editingId is not { } id)
            return;

        editError = null;

        // Counted over the components the page shows, so a choice never shrinks to "all components" without a word.
        if (!editAll && ChosenInOrder(editChosen).Count == 0)
        {
            editError = "Choose at least one component, or choose All components.";
            pendingFocus = () => editFirstInput;

            return;
        }

        var services = editAll ? new List<string>() : ChosenInOrder(editChosen);
        var subscription = State.Subscriptions.FirstOrDefault(item => item.Id == id);
        var result = State.UpdateSubscription(id, services);
        var address = subscription?.Email ?? "The subscription";

        switch (result)
        {
            case MonitoringResult.Ok:
                listStatusIsError = false;
                listStatus = $"Saved. {address} now gets updates for {ComponentsPhrase(services)}.";
                break;

            case MonitoringResult.NoChange:
                listStatusIsError = false;
                listStatus = $"Nothing changed: {address} already gets updates for {ComponentsPhrase(services)}.";
                break;

            case MonitoringResult.NotFound:
                listStatusIsError = true;
                listStatus = "That subscription is no longer in this tab.";
                editingId = null;
                pendingFocus = () => subscriptionsHeading;

                return;

            case MonitoringResult.Invalid:
                editError = "One of the chosen components isn't on the status page any more. Choose again.";
                pendingFocus = () => editAllInput;

                return;

            default:
                // NotAllowed, WrongState, and Duplicate don't apply to a subscription's components.
                editError = "The components weren't saved. Try again.";
                pendingFocus = () => editAllInput;

                return;
        }

        editingId = null;
        pendingFocus = () => editButtons.TryGetValue(id, out var button) ? button : subscriptionsHeading;
    }

    private void SetEditAll(bool value)
    {
        editAll = value;
        editError = null;
    }

    private void StartEdit(MonitoringSubscription subscription)
    {
        editingId = subscription.Id;
        editAll = subscription.ServiceIds.Count == 0;
        editChosen.Clear();
        editChosen.UnionWith(subscription.ServiceIds.Where(id => State.FindService(id) is { IsPublic: true }));
        editError = null;
        listStatus = null;
        pendingFocus = () => editAllInput;
    }

    /// <summary>Whether the address already has a subscription in this tab.</summary>
    private Task<bool> IsSubscribedAsync(string address) => Task.FromResult(State.IsSubscribed(address));

    /// <summary>Puts a form in a new edit context, so no old message stays.</summary>
    private void NewForm(SubscribeForm value)
    {
        form = value;
        formContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(formContext);
        formContext.OnFieldChanged += (_, args) => serverErrors?.Clear(args.FieldIdentifier);
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(FieldIdentifier field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(field, message);
        formContext.NotifyValidationStateChanged();
        formStatus = "Not subscribed. Fix the error shown with the field.";
        formStatusIsError = true;
    }

    /// <summary>
    /// Subscribes once the form is valid: the address check finishes first, then every rule runs, and a failed submit shows
    /// the summary (rule WR-002). The state checks again; its answer maps to a field or the status message.
    /// </summary>
    private async Task SubmitAsync()
    {
        formStatus = null;
        formStatusIsError = false;
        serverErrors?.Clear();
        form.ServiceIds = ChosenInOrder(form.ServiceIds.ToHashSet());

        var free = emailCheck is null || await emailCheck.CheckNowAsync();

        if (!await formContext.ValidateAsync() || !free)
        {
            formStatus = "Not subscribed. Fix the errors listed above the form.";
            formStatusIsError = true;

            return;
        }

        var address = form.Email.Trim();
        var services = form.AllComponents ? new List<string>() : form.ServiceIds;
        var result = State.Subscribe(address, services, out var subscription);

        switch (result)
        {
            case MonitoringResult.Ok when subscription is not null:
                formStatus = $"Subscribed {subscription.Email} to {ComponentsPhrase(subscription.ServiceIds)}. Its emails will appear under \"Your subscriptions in this tab\".";
                listStatus = null;
                NewForm(new SubscribeForm());
                break;

            case MonitoringResult.Duplicate:
                ServerError(FieldIdentifier.Create(() => form.Email), $"{address.ToLowerInvariant()} is already subscribed in this tab. Change its components under \"Your subscriptions in this tab\".");
                break;

            case MonitoringResult.Invalid when State.Subscriptions.Count >= MonitoringState.MaxSubscriptions:
                formStatus = $"Not subscribed: this tab holds up to {MonitoringState.MaxSubscriptions.ToString(CultureInfo.InvariantCulture)} subscriptions. Unsubscribe from one to add another.";
                formStatusIsError = true;
                break;

            case MonitoringResult.Invalid when services.Any(id => State.FindService(id) is not { IsPublic: true }):
                form.ServiceIds = ChosenInOrder(form.ServiceIds.ToHashSet());
                ServerError(FieldIdentifier.Create(() => form.ServiceIds), "One of the chosen components isn't on the status page any more. Choose again.");
                break;

            case MonitoringResult.Invalid:
                ServerError(FieldIdentifier.Create(() => form.Email), "Enter an email address like name@example.com.");
                break;

            default:
                // NotAllowed, NotFound, WrongState, and NoChange don't apply to subscribing.
                formStatus = "You weren't subscribed. Try again.";
                formStatusIsError = true;
                break;
        }
    }

    /// <summary>Chooses or unchooses a component of the new subscription; the rule "at least one" is checked again.</summary>
    private void ToggleComponent(string serviceId, ChangeEventArgs e)
    {
        var selection = form.ServiceIds.ToHashSet();
        Toggle(selection, serviceId, e);
        form.ServiceIds = ChosenInOrder(selection);
        formContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.ServiceIds));
    }

    private static void Toggle(HashSet<string> selection, string serviceId, ChangeEventArgs e)
    {
        if (e.Value is true || (e.Value is string text && bool.TryParse(text, out var isOn) && isOn))
            selection.Add(serviceId);
        else
            selection.Remove(serviceId);
    }

    private string UnsubscribeConsequence(MonitoringSubscription subscription)
    {
        var count = State.EmailsFor(subscription.Id).Count;
        var outbox = count switch
        {
            0 => "Its demo outbox is empty.",
            1 => "The 1 email in its demo outbox is deleted from this tab.",
            _ => $"The {count.ToString(CultureInfo.InvariantCulture)} emails in its demo outbox are deleted from this tab.",
        };

        return $"{subscription.Email} stops getting status page updates. {outbox} You can subscribe again at any time.";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets a component to preselect, from the <c>component</c> query parameter.</summary>
    [SupplyParameterFromQuery(Name = "component")] public string? Component { get; set; }

    [Inject] private MonitoringState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The new subscription's fields, with the limits of <see cref="MonitoringState.Subscribe" />, and "at least one component" unless all are followed.</summary>
    private sealed class SubscribeForm : IValidatableObject
    {
        public bool AllComponents { get; set; } = true;

        [Required(ErrorMessage = "Enter your email address.")]
        [StringLength(MonitoringState.MaxEmailLength, ErrorMessage = "Use an email address of up to 120 characters.")]
        [RegularExpression(@"^\s*[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}\s*$", ErrorMessage = "Enter an email address like name@example.com.")]
        public string Email { get; set; } = string.Empty;

        public List<string> ServiceIds { get; set; } = new();

        /// <summary>Runs once the address is valid: specific components need at least one chosen.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!AllComponents && ServiceIds.Count == 0)
                yield return new ValidationResult("Choose at least one component, or choose All components.", new[] { nameof(ServiceIds) });
        }
    }

    #endregion
}
