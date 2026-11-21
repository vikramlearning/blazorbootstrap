using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// The audit log of a site that implements <see cref="IShowcaseEnterpriseState" />, as a <see cref="Grid{TItem}" />
/// with filters, sorting, paging, the column chooser, and CSV export. Newest entries first.
/// </summary>
public partial class ShowcaseAuditLog : ComponentBase, IDisposable
{
    #region Fields and Constants

    private List<ShowcaseAuditEntry> entries = new();

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        Session.State.Changed += OnStateChanged;
        Load();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening for state changes.</summary>
    public void Dispose() => Session.State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Copies the entries into a new list, so the grid sees a new data source and refreshes.</summary>
    private void Load() => entries = Enterprise?.AuditEntries.ToList() ?? new List<ShowcaseAuditEntry>();

    private void OnStateChanged() =>
        _ = InvokeAsync(() =>
        {
            Load();
            StateHasChanged();
        });

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the grid's accessible name.</summary>
    [Parameter] public string AriaLabel { get; set; } = "Audit log";

    private IShowcaseEnterpriseState? Enterprise => Session.State as IShowcaseEnterpriseState;

    /// <summary>Gets or sets the CSV file name for export.</summary>
    [Parameter] public string ExportFileName { get; set; } = "audit-log";

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    #endregion
}
