using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Operations' customer list, by name: search by name, email, or city, and filter by status (active or on watch). Each row
/// has the name linking to the customer's profile, the email, city, segment, customer-since date, the status in text, the
/// number of accounts, the total balance (live for the demo customer), and the open items: held transfers and open
/// disputes, counted and named. The status and path helpers are shared with <see cref="OpsCustomer" />.
/// </summary>
public partial class OpsCustomers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string search = string.Empty;

    private string statusFilter = string.Empty;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the operations route of a customer's profile.</summary>
    internal static string CustomerPath(string customerId) => $"{FintechOpsLayout.Base}/customers/{Uri.EscapeDataString(customerId)}";

    /// <summary>Formats a customer-since date, like "Apr 2, 2019".</summary>
    internal static string SinceText(DateOnly date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Returns the badge color of a customer status; the badge always carries the status text too.</summary>
    internal static BadgeColor StatusColor(FintechCustomerStatus status) => status == FintechCustomerStatus.Watch ? BadgeColor.Warning : BadgeColor.Success;

    /// <summary>Returns a customer status in words.</summary>
    internal static string StatusText(FintechCustomerStatus status) => status == FintechCustomerStatus.Watch ? "Watch" : "Active";

    #endregion

    #region Private Methods

    private void ClearFilters()
    {
        search = string.Empty;
        statusFilter = string.Empty;
    }

    private bool Matches(FintechCustomer customer)
    {
        if (statusFilter.Length > 0 && customer.Status.ToString() != statusFilter)
            return false;

        var text = search.Trim();

        return text.Length == 0
            || customer.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
            || customer.Email.Contains(text, StringComparison.OrdinalIgnoreCase)
            || customer.City.Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    private (int Held, int Disputes, int Total) OpenItems(string customerId)
    {
        var held = State.AllTransfers.Count(transfer => transfer.CustomerId == customerId && transfer.Status == FintechTransferStatus.Held);
        var disputes = State.AllDisputes.Count(dispute => dispute.CustomerId == customerId && OpsDisputes.IsOpen(dispute));

        return (held, disputes, held + disputes);
    }

    private static string OpenItemsText((int Held, int Disputes, int Total) open)
    {
        var parts = new List<string>();

        if (open.Held > 0)
            parts.Add(open.Held == 1 ? "1 held transfer" : $"{open.Held.ToString(CultureInfo.InvariantCulture)} held transfers");

        if (open.Disputes > 0)
            parts.Add(open.Disputes == 1 ? "1 open dispute" : $"{open.Disputes.ToString(CultureInfo.InvariantCulture)} open disputes");

        return string.Join(", ", parts);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private void OnStatusChanged(ChangeEventArgs args) => statusFilter = args.Value?.ToString() ?? string.Empty;

    #endregion

    #endregion

    #region Properties, Indexers

    private bool HasFilters => search.Trim().Length > 0 || statusFilter.Length > 0;

    private string MatchText
    {
        get
        {
            var total = State.Customers.Count;
            var count = Shown.Count;

            if (!HasFilters)
                return total == 1 ? "1 customer" : $"{total.ToString(CultureInfo.InvariantCulture)} customers";

            return count switch
            {
                0 => "No customers match",
                1 => $"1 of {total.ToString(CultureInfo.InvariantCulture)} customers matches",
                _ => $"{count.ToString(CultureInfo.InvariantCulture)} of {total.ToString(CultureInfo.InvariantCulture)} customers match",
            };
        }
    }

    private IReadOnlyList<FintechCustomer> Shown => State.Customers
        .Where(Matches)
        .OrderBy(customer => customer.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion
}
