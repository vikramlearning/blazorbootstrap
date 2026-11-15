using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// A printable invoice of the signed-in customer's workspace: Lumora (fictitious) as the seller, the workspace as the
/// buyer, the number, dates, period, description, amount, status, and how it was paid. An invoice of another workspace,
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

    private static string PaymentText(SaasInvoice invoice, string? cardText) => invoice.Status switch
    {
        SaasInvoiceStatus.Open => "Open: this invoice isn't paid yet.",
        SaasInvoiceStatus.Refunded => cardText is null
            ? $"Refunded: {SaasDisplay.MoneyCents(invoice.RefundedAmount)} of {SaasDisplay.MoneyCents(invoice.Amount)}."
            : $"Paid with {cardText}. {SaasDisplay.MoneyCents(invoice.RefundedAmount)} was refunded to the same card.",
        _ => cardText is null ? "Paid." : $"Paid with {cardText}.",
    };

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
