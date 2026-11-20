using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// A client's page (account managers): the header (name, industry, website as text, owner, phone, client since, billing
/// currency and email, contract dates, open pipeline) with an Edit link, then the contacts (<see cref="ClientContacts" />),
/// the client's deals in a <see cref="Grid{TItem}" /> with a saved view, the column chooser, and CSV export (stage in words,
/// value, close date with overdue in words, weighted value; the row opens the deal's card on the pipeline; open deals have
/// Edit and the section has New deal, both in the shared <see cref="DealEditor" />), the client's projects with status and progress
/// linking to their boards, and the activity timeline with its log form (<see cref="ClientTimeline" />). The client form
/// comes back here with <c>?created=1</c> or <c>?saved=1</c>, announced in the page's status region. An unknown ID shows
/// <see cref="PbNotFound" />.
/// </summary>
public partial class ClientDetail : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultDealPageSize = 5;

    private const string DealResetText = "The deals view is reset: open deals first, by close date, 5 a page, and every column shown.";

    private readonly Dictionary<int, ElementReference> editDealButtons = new();

    private DealEditor dealEditor = default!;

    private Grid<DealRow>? dealGrid;

    /// <summary>Changes when the deals' view is reset, so the grid is built again with its defaults.</summary>
    private int dealGridVersion;

    /// <summary>The deals grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int dealPageSize = DefaultDealPageSize;

    private ShowcaseGridView dealView = default!;

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

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await dealView.LoadAsync();
            dealPageSize = (await dealView.GetSettingsAsync()).PageSize;
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
        dealView = GridViews.Get(State.SiteSlug, "client-deals", DefaultDealPageSize, null, SortDirection.None);
        State.Changed += OnStateChanged;
    }

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

    /// <summary>Another screen or the deal form changed the state: the deals grid reads the deals again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshDealsAsync();
        StateHasChanged();
    });

    /// <summary>The client's deals as grid rows: open ones first, by close date, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<DealRow>> ProvideDealsAsync(GridDataProviderRequest<DealRow> request)
    {
        var today = State.Clock.Today;
        var rows = State.DealsOf(Id).Select(deal =>
        {
            var project = deal.ProjectId is { } projectId ? State.FindProject(projectId) : null;

            return new DealRow(deal.Id, deal.Title, PbDisplay.StageLabel(deal.Stage), deal.Stage, DealDisplay.Percent(PbState.Probability(deal.Stage)), deal.Value, deal.CloseDate,
                DealDisplay.CloseText(deal, today), DealDisplay.IsCloseOverdue(deal, today), PbState.Weighted(deal), deal.ClosedAt is null, project?.Id, project?.Key);
        }).ToList();

        return dealView.ApplyAsync(request, rows);
    }

    /// <summary>Forgets the deals' saved view and builds the grid again with its defaults; focus moves to the deals' status message.</summary>
    private async Task ResetDealViewAsync()
    {
        await dealView.ResetAsync();
        dealPageSize = DefaultDealPageSize;
        dealGridVersion++;
        dealStatusIsError = false;
        dealMessage = dealMessage == DealResetText ? DealResetText + " (2)" : DealResetText;
        focusTarget = () => dealStatusRegion;
        StateHasChanged();
    }

    private async Task RefreshDealsAsync()
    {
        if (dealGrid is not null)
            await dealGrid.RefreshDataAsync();
    }

    /// <summary>Keeps the deals grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveDealSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            dealPageSize = settings.PageSize;

        await dealView.SaveSettingsAsync(settings);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether the client was just added.</summary>
    [SupplyParameterFromQuery(Name = "created")] public bool? Created { get; set; }

    /// <summary>Gets or sets the client.</summary>
    [Parameter] public int Id { get; set; }

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>Gets or sets whether the client was just saved.</summary>
    [SupplyParameterFromQuery(Name = "saved")] public bool? Saved { get; set; }

    [Inject] private PbState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One deal as the grid shows and exports it.</summary>
    private sealed record DealRow(
        int Id,
        string Title,
        string Stage,
        PbDealStage StageValue,
        string Chance,
        int Value,
        DateTime CloseDate,
        string CloseText,
        bool IsOverdue,
        int Weighted,
        bool IsOpen,
        int? ProjectId,
        string? ProjectKey);

    #endregion
}
