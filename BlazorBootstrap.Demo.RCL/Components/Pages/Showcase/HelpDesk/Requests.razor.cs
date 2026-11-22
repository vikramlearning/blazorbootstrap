using BlazorBootstrap.Demo.RCL.Models.Showcase.HelpDesk;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HelpDesk;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HelpDesk;

/// <summary>
/// The signed-in customer's requests, most recently updated first, filtered to Open (anything not resolved) or Solved.
/// Each shows its number, subject, category, customer-facing status, when it was last updated, and a "New reply" badge
/// for an agent reply the customer hasn't opened. Only customers see it (<c>ShowcaseRoleGuard</c>).
/// </summary>
public partial class Requests : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly RequestFilter[] filters = { RequestFilter.Open, RequestFilter.Solved };

    private readonly Dictionary<RequestFilter, Button> filterButtons = new();

    private RequestFilter filter = RequestFilter.Open;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string FilterLabel(RequestFilter option) => option == RequestFilter.Open ? "Open" : "Solved";

    private static bool InFilter(HelpDeskTicket ticket, RequestFilter option) =>
        option == RequestFilter.Solved ? ticket.Status == HelpDeskStatus.Resolved : ticket.Status != HelpDeskStatus.Resolved;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The empty filter's button goes away, so focus moves to the filter it switched to.</summary>
    private async Task ShowOtherAsync()
    {
        filter = filter == RequestFilter.Open ? RequestFilter.Solved : RequestFilter.Open;

        await Task.Yield();

        if (filterButtons.TryGetValue(filter, out var button))
            await HelpDeskDisplay.FocusAsync(button.Element);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HelpDeskState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private enum RequestFilter
    {
        Open,
        Solved,
    }

    #endregion
}
