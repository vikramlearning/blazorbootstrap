using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// A printable invoice of the signed-in customer's workspace: Lumora (fictitious) as the seller, the workspace as the
/// buyer, the number, dates, period, description, amount, status, how it was paid, and any refund; an open invoice links to
/// Billing to pay it. An invoice of another workspace,
/// or an unknown number, shows the not-found screen with a way back to billing.
/// </summary>
public partial class AppInvoice : ComponentBase, IDisposable
{
    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>How the invoice was paid and what was refunded. The state keeps no refund date, so none is shown.</summary>
    private static string PaymentText(SaasInvoice invoice, string? cardText)
    {
        if (invoice.Status == SaasInvoiceStatus.Open)
            return "Open: this invoice isn't paid yet. Pay it in Billing.";

        var paid = cardText is null ? "Paid." : $"Paid with the {cardText}.";

        if (invoice.RefundedAmount <= 0)
            return paid;

        var refunded = $"{SaasDisplay.MoneyCents(invoice.RefundedAmount)} refunded to the card";

        return invoice.Status == SaasInvoiceStatus.Refunded
            ? $"{paid} {refunded}: the invoice is fully refunded."
            : $"{paid} {refunded}; {SaasDisplay.MoneyCents(invoice.Amount - invoice.RefundedAmount)} stays paid.";
    }

    private static string PeriodText(SaasInvoice invoice) => $"{SaasDisplay.Date(invoice.PeriodStart)} to {SaasDisplay.Date(invoice.PeriodEnd)}";

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the invoice number from the route, such as <c>LUM-001-0002</c>.</summary>
    [Parameter] public string Number { get; set; } = string.Empty;

    [Inject] private SaasState State { get; set; } = default!;

    #endregion
}
