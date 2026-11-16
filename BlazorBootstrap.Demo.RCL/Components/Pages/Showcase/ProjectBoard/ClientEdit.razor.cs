using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Adds a client (<c>/clients/new</c>) or edits one (<c>/clients/{id}/edit</c>), for account managers: name (with a
/// counter), industry, website (plain text, never a link), and owner (an account manager; the signed-in one by default).
/// Each problem is shown under its field and tied to it, and the first invalid field takes focus after the render that
/// shows it; the state's refusals (Duplicate name, Invalid, NotFound, NoChange) go to the field or the status message. A
/// save goes to the client's page, which announces it. On the edit page, Delete opens a <see cref="Modal" /> that names
/// what goes with the client (contacts and activity), or lists the projects and deals that keep it; the deletion runs once
/// the dialog is hidden and goes back to the client list. An unknown ID shows <see cref="PbNotFound" />.
/// </summary>
public partial class ClientEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Button? deleteButton;

    private Modal deleteModal = default!;

    private bool focusDelete;

    private bool focusStatus;

    /// <summary>Returns the field to focus after the next render (the first invalid one).</summary>
    private Func<ElementReference?>? focusTarget;

    private string industry = string.Empty;

    private string? industryError;

    private ElementReference? industryInput;

    /// <summary>The client was just deleted and the list is loading: no not-found flash in between.</summary>
    private bool leaving;

    /// <summary>The route ID the form was loaded for (0 for a new client), or <see langword="null" /> before the first load.</summary>
    private int? loadedId;

    private string name = string.Empty;

    private string? nameError;

    private ElementReference? nameInput;

    private string? ownerError;

    private string ownerId = string.Empty;

    private ElementReference? ownerInput;

    /// <summary>Whether Delete was confirmed in the dialog: it runs once the dialog is hidden.</summary>
    private bool pendingDelete;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string website = string.Empty;

    private string? websiteError;

    private ElementReference? websiteInput;

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
        nameError = industryError = websiteError = ownerError = null;

        if (Client is { } client)
        {
            name = client.Name;
            industry = client.Industry;
            website = client.Website;
            ownerId = client.OwnerId;
        }
        else
        {
            name = industry = website = string.Empty;
            ownerId = State.AccountManagers.Any(person => person.Id == State.CurrentPerson?.Id)
                ? State.CurrentPerson!.Id
                : State.AccountManagers.FirstOrDefault()?.Id ?? string.Empty;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

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

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenDeleteAsync()
    {
        pendingDelete = false;
        await deleteModal.ShowAsync();
    }

    /// <summary>Checks the fields, saves, and maps the state's answer to a field or the status message.</summary>
    private void Save()
    {
        var trimmedName = name.Trim();
        var trimmedIndustry = industry.Trim();
        var trimmedWebsite = website.Trim();
        var existing = Client;

        nameError = trimmedName.Length == 0 ? "Enter the client's name."
            : trimmedName.Length > PbState.MaxClientNameLength ? $"Shorten the name to {PbState.MaxClientNameLength} characters or fewer (now {trimmedName.Length})."
            : State.Clients.FirstOrDefault(item => item != existing && string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)) is { } taken ? DuplicateText(taken)
            : null;
        industryError = trimmedIndustry.Length == 0 ? "Enter the client's industry, like Retail."
            : trimmedIndustry.Length > PbState.MaxContactFieldLength ? $"Shorten the industry to {PbState.MaxContactFieldLength} characters or fewer (now {trimmedIndustry.Length})."
            : null;
        websiteError = trimmedWebsite.Length > PbState.MaxWebsiteLength ? $"Shorten the website to {PbState.MaxWebsiteLength} characters or fewer (now {trimmedWebsite.Length})." : null;
        ownerError = State.AccountManagers.All(person => person.Id != ownerId) ? "Choose the account manager who owns the client." : null;

        focusTarget = nameError is not null ? () => nameInput
            : industryError is not null ? () => industryInput
            : websiteError is not null ? () => websiteInput
            : ownerError is not null ? () => ownerInput
            : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then save again.");

            return;
        }

        if (existing is null && State.Clients.Count >= PbState.MaxClients)
        {
            Announce(false, $"The board holds up to {PbState.MaxClients} clients, and it's full. Delete a client you don't need first. Nothing was added.");
            focusStatus = true;

            return;
        }

        var result = State.SaveClient(existing?.Id ?? 0, trimmedName, trimmedIndustry, trimmedWebsite, ownerId, out var saved);

        switch (result)
        {
            case PbResult.Ok when saved is not null:
                Navigation.NavigateTo(PbDisplay.ClientUrl(saved.Id) + (existing is null ? "?created=1" : "?saved=1"));

                return;

            case PbResult.NoChange:
                Announce(true, "Nothing changed, so there was nothing to save.");
                focusStatus = true;

                return;

            case PbResult.Duplicate:
                nameError = State.Clients.FirstOrDefault(item => item != existing && string.Equals(item.Name, trimmedName, StringComparison.OrdinalIgnoreCase)) is { } other
                    ? DuplicateText(other)
                    : $"A client named “{trimmedName}” already exists.";
                focusTarget = () => nameInput;
                Announce(false, "That name is taken. Nothing was saved.");

                return;

            case PbResult.NotFound:
                Announce(false, "This client was deleted meanwhile, so there's nothing to save.");
                focusStatus = true;

                return;

            case PbResult.Invalid when State.AccountManagers.All(person => person.Id != ownerId):
                ownerError = "That person is no longer an account manager. Choose another owner.";
                focusTarget = () => ownerInput;
                Announce(false, "Nothing was saved.");

                return;

            case PbResult.Invalid:
                Announce(false, DealDisplay.StorageFull);
                focusStatus = true;

                return;

            case PbResult.NotAllowed:
                Announce(false, "Only account managers can add or edit clients. Nothing was saved.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                return;
        }
    }

    private static string DuplicateText(PbClient taken) => $"A client named “{taken.Name}” already exists. Use another name, or open the existing client.";

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the client to edit, or <see langword="null" /> for a new one.</summary>
    [Parameter] public int? Id { get; set; }

    private PbClient? Client => Id is { } id ? State.FindClient(id) : null;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
