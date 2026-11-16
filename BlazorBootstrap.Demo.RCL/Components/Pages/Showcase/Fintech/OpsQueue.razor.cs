using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The operations review queue: every transfer held for a check (<see cref="FintechTransferStatus.Held" />), oldest first
/// (first in, first out), with the reference, the customer (linked to their page), the amount, the payee, the transfer
/// date, the hold reason, how long it has waited, and a Review link to <see cref="OpsTransfer" />. The heading carries
/// the count, an empty queue says so, and a note sums up the hold rules. The URL and waiting-time helpers are shared with
/// <see cref="OpsTransfer" /> and <see cref="OpsScheduled" />.
/// </summary>
public partial class OpsQueue : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the operations page of a customer.</summary>
    internal static string CustomerUrl(string customerId) => $"{FintechOpsLayout.Base}/customers/{Uri.EscapeDataString(customerId)}";

    /// <summary>Returns the operations page of a transfer.</summary>
    internal static string TransferUrl(string reference) => $"{FintechOpsLayout.Base}/transfers/{Uri.EscapeDataString(reference)}";

    /// <summary>Says how long something has waited, like "45 min", "3 h", or "2 d 4 h".</summary>
    internal static string WaitText(DateTime since, DateTime now)
    {
        var waited = now - since;

        if (waited < TimeSpan.FromMinutes(1))
            return "Under 1 min";

        if (waited < TimeSpan.FromHours(1))
            return string.Create(CultureInfo.InvariantCulture, $"{(int)waited.TotalMinutes} min");

        if (waited < TimeSpan.FromDays(1))
            return string.Create(CultureInfo.InvariantCulture, $"{(int)waited.TotalHours} h");

        return waited.Hours == 0
            ? string.Create(CultureInfo.InvariantCulture, $"{waited.Days} d")
            : string.Create(CultureInfo.InvariantCulture, $"{waited.Days} d {waited.Hours} h");
    }

    #endregion

    #region Private Methods

    private static string CountText(int count) => string.Create(CultureInfo.InvariantCulture, $"{count} waiting");

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the held transfers, oldest first.</summary>
    private List<FintechTransfer> Held => State.AllTransfers.Where(transfer => transfer.Status == FintechTransferStatus.Held)
        .OrderBy(transfer => transfer.CreatedAt).ThenBy(transfer => transfer.Reference, StringComparer.Ordinal).ToList();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
