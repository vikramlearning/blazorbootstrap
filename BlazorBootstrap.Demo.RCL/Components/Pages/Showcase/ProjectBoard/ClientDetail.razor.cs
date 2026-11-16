using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// A client's page (account managers): the header (name, industry, website as text, owner, client since, open pipeline)
/// with an Edit link, then the contacts (<see cref="ClientContacts" />), the client's deals (stage in words, value, close
/// date with overdue in words, weighted value; titles link to their card on the pipeline; open deals have Edit and the
/// section has New deal, both in the shared <see cref="DealEditor" />), the client's projects with status and progress
/// linking to their boards, and the activity timeline with its log form (<see cref="ClientTimeline" />). The client form
/// comes back here with <c>?created=1</c> or <c>?saved=1</c>, announced in the page's status region. An unknown ID shows
/// <see cref="PbNotFound" />.
/// </summary>
public partial class ClientDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly Dictionary<int, ElementReference> editDealButtons = new();

    private DealEditor dealEditor = default!;

    /// <summary>The deal the editor was opened for (0 for New deal), for focus when it closes.</summary>
    private int dealEditorFor;

    private string? dealMessage;

    private bool dealStatusIsError;

    private ElementReference? dealStatusRegion;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    /// <summary>The route ID the page was loaded for.</summary>
    private int? loadedId;

    private ElementReference? newDealButton;

    private string? pageMessage;

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

    protected override void OnParametersSet()
    {
        if (loadedId == Id)
            return;

        loadedId = Id;
        dealMessage = null;
        dealStatusIsError = false;
        editDealButtons.Clear();
        var name = State.FindClient(Id)?.Name;
        pageMessage = name is null ? null
            : Created == true ? $"Added {name}. Add its contacts, deals, and activity below."
            : Saved == true ? $"Saved the changes to {name}."
            : null;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static BadgeColor ProjectBadge(PbProjectStatus status) => PbDisplay.ProjectStatusColor(status) switch
    {
        "success" => BadgeColor.Success,
        "info" => BadgeColor.Info,
        "warning" => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Announces what the deal form did; focus goes to the message after a save, or back to the button that opened it.</summary>
    private void OnDealEditorClosed(DealEditorResult result)
    {
        if (result.Message is { } message)
        {
            dealStatusIsError = false;
            dealMessage = message;
            focusTarget = () => dealStatusRegion;
        }
        else
        {
            var id = dealEditorFor;
            focusTarget = () => id != 0 && editDealButtons.TryGetValue(id, out var button) && State.FindDeal(id) is { ClosedAt: null } ? button : newDealButton;
        }
    }

    private async Task OpenEditDealAsync(int dealId)
    {
        dealEditorFor = dealId;

        if (!await dealEditor.OpenEditAsync(dealId))
        {
            dealStatusIsError = true;
            dealMessage = State.FindDeal(dealId) is { } deal
                ? $"“{deal.Title}” is {PbDisplay.StageLabel(deal.Stage).ToLowerInvariant()}, so it can't be edited. Move it back to an open stage on the pipeline first."
                : "That deal was deleted meanwhile.";
            focusTarget = () => dealStatusRegion;
        }
    }

    private async Task OpenNewDealAsync()
    {
        dealEditorFor = 0;
        await dealEditor.OpenNewAsync(Id);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether the client was just added.</summary>
    [SupplyParameterFromQuery(Name = "created")] public bool? Created { get; set; }

    /// <summary>Gets or sets the client.</summary>
    [Parameter] public int Id { get; set; }

    /// <summary>Gets or sets whether the client was just saved.</summary>
    [SupplyParameterFromQuery(Name = "saved")] public bool? Saved { get; set; }

    [Inject] private PbState State { get; set; } = default!;

    #endregion
}
