namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Ecommerce;

/// <summary>The storefront's demo sign-in page: shoppers sign in or register; the retailer signs in with the listed account.</summary>
public partial class SignIn : ComponentBase
{
    #region Properties, Indexers

    /// <summary>Gets or sets <c>?mode=register</c>, which opens the register form.</summary>
    [SupplyParameterFromQuery(Name = "mode")] public string? Mode { get; set; }

    /// <summary>Gets or sets the page to return to after signing in; only a page inside the store is used.</summary>
    [SupplyParameterFromQuery(Name = "returnUrl")] public string? ReturnUrl { get; set; }

    #endregion
}
