namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>Tells a signed-in retailer that staff accounts see the shop but don't buy, with the way back to the admin (D-032).</summary>
public partial class EcommerceStaffNotice : ComponentBase
{
    #region Properties, Indexers

    /// <summary>Gets or sets extra CSS classes.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Gets or sets the message.</summary>
    [Parameter] public string Text { get; set; } = "You're viewing the store as the retailer. Staff accounts don't shop or check out.";

    #endregion
}
