using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Layout of the Tallymint operations back office (D-031): a top bar with the account menu, a sidebar with the operations
/// modules, and the page behind the operations guard (password, then a one-time code). Scheduled transfers that have come
/// due are posted as pages load.
/// </summary>
public partial class FintechOpsLayout : LayoutComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>Base route of the operations back office.</summary>
    public const string Base = FintechLayout.Base + "/ops";

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet() => State.CatchUp();

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The operations modules.</summary>
    private Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)
    {
        var items = new List<NavItem>
        {
            new() { Id = "home", Href = Base, IconName = IconName.Speedometer2, Text = "Overview", Match = NavLinkMatch.All },
            new() { Id = "queue", Href = $"{Base}/queue", IconName = IconName.ShieldCheck, Text = "Review queue" },
            new() { Id = "disputes", Href = $"{Base}/disputes", IconName = IconName.ExclamationOctagon, Text = "Disputes" },
            new() { Id = "customers", Href = $"{Base}/customers", IconName = IconName.People, Text = "Customers" },
            new() { Id = "scheduled", Href = $"{Base}/scheduled", IconName = IconName.CalendarEvent, Text = "Scheduled postings" },
            new() { Id = "audit", Href = $"{Base}/audit", IconName = IconName.JournalText, Text = "Audit log" },
            new() { Id = "account", Href = $"{Base}/account", IconName = IconName.PersonGear, Text = "Account" },
        };

        return Task.FromResult(request.ApplyTo(items));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
