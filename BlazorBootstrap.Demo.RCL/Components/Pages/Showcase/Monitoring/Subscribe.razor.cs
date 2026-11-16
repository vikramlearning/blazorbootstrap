using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Monitoring;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Monitoring;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Monitoring;

/// <summary>
/// Subscribing to status page updates: an email address (kept in this tab; nothing is sent) for all components or chosen
/// ones, with field errors for a missing or invalid address, an address already subscribed, no component chosen, and a
/// full tab. Below, the tab's subscriptions, each with its components (edited inline), Unsubscribe (after a confirmation
/// naming what is deleted), and its demo outbox: the emails "sent" when engineers post an update, newest first, each
/// expandable to its plain-text body and a link to the incident's public page. A <c>component</c> query parameter
/// preselects one component (the incident page links here with it). Focus moves to the first invalid field after the
/// render that shows its error, and outcomes are announced in status regions.
/// </summary>
public partial class Subscribe : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool allComponents = true;

    private readonly HashSet<string> chosen = new();

    private string? componentsError;

    private bool editAll = true;

    private ElementReference editAllInput;

    private readonly Dictionary<int, ElementReference> editButtons = new();

    private readonly HashSet<string> editChosen = new();

    private string? editError;

    private ElementReference editFirstInput;

    /// <summary>The subscription whose components are being edited inline, if any.</summary>
    private int? editingId;

    private string email = string.Empty;

    private string? emailError;

    private ElementReference emailInput;

    private ElementReference firstComponentInput;

    private string? formError;

    private string? formStatus;

    private bool formStatusIsError;

    private string? listStatus;

    private bool listStatusIsError;

    /// <summary>The element to focus once the next render has put it on the page.</summary>
    private Func<ElementReference?>? pendingFocus;

    private bool preselected;

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

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // The incident page links here with its component; choose it once, and leave later choices to the user.
        if (preselected || State.FindService(Component) is not { } service)
            return;

        preselected = true;
        allComponents = false;
        chosen.Clear();
        chosen.Add(service.Id);
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
    private List<string> ChosenInOrder(HashSet<string> selection) => State.Services.Where(service => selection.Contains(service.Id)).Select(service => service.Id).ToList();

    /// <summary>The components in a sentence, like "all components" or "Payments API, Search".</summary>
    private string ComponentsPhrase(IReadOnlyList<string> serviceIds) => serviceIds.Count == 0 ? "all components" : ComponentsText(serviceIds);

    private string ComponentsText(IReadOnlyList<string> serviceIds) =>
        serviceIds.Count == 0 ? "All components" : string.Join(", ", State.Services.Where(service => serviceIds.Contains(service.Id)).Select(service => service.Name));

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
            StateHasChanged();

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
        StateHasChanged();
    }

    private void SaveEdit()
    {
        if (editingId is not { } id)
            return;

        editError = null;

        if (!editAll && editChosen.Count == 0)
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

    private void SetAllComponents(bool value)
    {
        allComponents = value;
        componentsError = null;
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
        editChosen.UnionWith(subscription.ServiceIds);
        editError = null;
        listStatus = null;
        pendingFocus = () => editAllInput;
    }

    private void Submit()
    {
        emailError = null;
        componentsError = null;
        formError = null;
        formStatus = null;
        formStatusIsError = false;

        var address = email.Trim();

        if (address.Length == 0)
            emailError = "Enter your email address.";
        else if (address.Length > MonitoringState.MaxEmailLength)
            emailError = $"Use an email address of up to {MonitoringState.MaxEmailLength.ToString(CultureInfo.InvariantCulture)} characters.";

        if (!allComponents && chosen.Count == 0)
            componentsError = "Choose at least one component, or choose All components.";

        if (emailError is not null || componentsError is not null)
        {
            ShowFormErrors();

            return;
        }

        var services = allComponents ? new List<string>() : ChosenInOrder(chosen);
        var result = State.Subscribe(address, services, out var subscription);

        switch (result)
        {
            case MonitoringResult.Ok when subscription is not null:
                formStatus = $"Subscribed {subscription.Email} to {ComponentsPhrase(subscription.ServiceIds)}. Its emails will appear under \"Your subscriptions in this tab\".";
                listStatus = null;
                email = string.Empty;
                allComponents = true;
                chosen.Clear();
                break;

            case MonitoringResult.Duplicate:
                emailError = $"{address.ToLowerInvariant()} is already subscribed in this tab. Change its components under \"Your subscriptions in this tab\".";
                ShowFormErrors();
                break;

            case MonitoringResult.Invalid when State.Subscriptions.Count >= MonitoringState.MaxSubscriptions:
                formError = $"This tab holds up to {MonitoringState.MaxSubscriptions.ToString(CultureInfo.InvariantCulture)} subscriptions. Unsubscribe from one to add another.";
                formStatus = "Not subscribed: this tab is full.";
                formStatusIsError = true;
                break;

            case MonitoringResult.Invalid when services.Any(id => State.FindService(id) is null):
                componentsError = "One of the chosen components isn't on the status page any more. Choose again.";
                ShowFormErrors();
                break;

            case MonitoringResult.Invalid:
                emailError = "Enter an email address like name@example.com.";
                ShowFormErrors();
                break;

            default:
                // NotAllowed, NotFound, WrongState, and NoChange don't apply to subscribing.
                formStatus = "You weren't subscribed. Try again.";
                formStatusIsError = true;
                break;
        }
    }

    /// <summary>Announces the errors and moves focus to the first invalid field once the render shows its error.</summary>
    private void ShowFormErrors()
    {
        var count = (emailError is null ? 0 : 1) + (componentsError is null ? 0 : 1);
        formStatus = count == 1 ? "Not subscribed. Fix the error shown with the field." : "Not subscribed. Fix the 2 errors shown with the fields.";
        formStatusIsError = true;
        pendingFocus = emailError is not null ? () => emailInput : () => firstComponentInput;
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
}
