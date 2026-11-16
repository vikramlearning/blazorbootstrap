using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// The client list of Tandem (account managers): a table with a caption and header scopes of every client, its industry,
/// owner, primary contact, open deals (count and value), projects, and last activity (relative, with the full time in a
/// title and visually hidden text). A search box (name, industry, or contact) and an owner filter narrow it; New client
/// opens the client form. A client deleted on its edit page comes back here with <c>?deleted=1</c>, announced in the
/// always-rendered status region.
/// </summary>
public partial class Clients : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The owner to show (a person ID), or empty for everyone.</summary>
    private string ownerFilter = string.Empty;

    private string search = string.Empty;

    private string? statusMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet() => statusMessage = Deleted == true ? "The client was deleted, with its contacts and activity." : null;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private bool Matches(PbClient client)
    {
        var text = search.Trim();

        if (ownerFilter.Length > 0 && client.OwnerId != ownerFilter)
            return false;

        return text.Length == 0
            || client.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
            || client.Industry.Contains(text, StringComparison.OrdinalIgnoreCase)
            || State.ContactsOf(client.Id).Any(contact => contact.Name.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether a client was just deleted on its edit page.</summary>
    [SupplyParameterFromQuery(Name = "deleted")] public bool? Deleted { get; set; }

    private IReadOnlyList<PbClient> Rows => State.Clients.Where(Matches).ToList();

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
