namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>Lumora's demo sign-in page for customers and the operator; new customers start from the trial sign-up.</summary>
public partial class SignIn : ComponentBase
{
    #region Methods

    #region Protected Override Methods

    /// <summary>
    /// The shared account menu offers "Create an account" as <c>?mode=register</c>; at Lumora an account comes with a workspace,
    /// so that goes to the trial sign-up.
    /// </summary>
    protected override void OnInitialized()
    {
        if (string.Equals(Mode, "register", StringComparison.OrdinalIgnoreCase))
            Navigation.NavigateTo($"{SaasLayout.Base}/signup", replace: true);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets <c>?mode=register</c>, sent by the account menu's "Create an account".</summary>
    [SupplyParameterFromQuery(Name = "mode")] public string? Mode { get; set; }

    /// <summary>Gets or sets the page to return to after signing in; only a page inside the site is used.</summary>
    [SupplyParameterFromQuery(Name = "returnUrl")] public string? ReturnUrl { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    #endregion
}
