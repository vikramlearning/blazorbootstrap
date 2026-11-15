using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Lumora's trial sign-up: a self-serve plan (preselected from <c>?plan=</c>), monthly or annual billing, and the new
/// owner's name, email, password, company, and country. It creates the customer account (signed in) and the workspace, then
/// opens the workspace. Growth starts a 14-day trial without a card; Starter is free. A signed-in visitor sees who they are
/// signed in as instead of the form.
/// </summary>
public partial class Signup : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string ContactPath = SaasLayout.Base + "/contact";

    private static readonly SaasTestimonial quote = SaasSeedData.Testimonials()[1];

    /// <summary>Signs in and returns to the workspace, for an email that already has an account.</summary>
    private static readonly string signInHref = $"{SaasLayout.SignInPath}?returnUrl={Uri.EscapeDataString(SaasAppLayout.Base)}";

    private int attempts;

    /// <summary>Whether a sign-up is being created or has just succeeded, so the "already signed in" view doesn't flash before the workspace opens.</summary>
    private bool busy;

    private string? emailInUse;

    private string? error;

    private SignupForm form = new();

    private ElementReference formHeading;

    private string? lastPlanQuery;

    /// <summary>The plan asked for in <c>?plan=</c> when it is sold through sales (Enterprise), to point the visitor to sales.</summary>
    private SaasPlan? requestedSalesPlan;

    private ElementReference signedInHeading;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        form.PlanId = DefaultPlanId;
        State.Changed += OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        if (lastPlanQuery == Plan)
            return;

        lastPlanQuery = Plan;
        requestedSalesPlan = null;

        if (State.FindPlan(Plan) is not { } plan)
            return;

        if (plan.IsSelfServe)
            form.PlanId = plan.Id;
        else
            requestedSalesPlan = plan;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnBillingChanged(bool annual) => State.AnnualBilling = annual;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private string PlanPriceText(SaasPlan plan) => plan.MonthlyPricePerSeat == 0
        ? "Free"
        : $"{SaasDisplay.Money(SaasPricing.MonthlySeatPrice(plan, Period))} / seat / mo";

    private async Task SignOutAsync()
    {
        State.Identity.SignOut();
        attempts = 0;
        emailInUse = null;
        error = null;
        StateHasChanged();
        await Task.Yield();
        await SaasDisplay.FocusAsync(formHeading);
    }

    private void Submit()
    {
        emailInUse = null;
        error = null;

        if (State.FindPlan(form.PlanId) is not { IsSelfServe: true } plan)
        {
            Fail("Choose one of the plans above. Enterprise is set up with our sales team.");
            return;
        }

        var email = form.Email.Trim();
        busy = true;
        var result = State.StartTrial(form.Name.Trim(), email, form.Password!, form.Company.Trim(), form.Country, plan.Id, Period);

        if (result == ShowcaseAccountResult.Success)
        {
            // The new account is signed in; the workspace opens on its dashboard.
            Navigation.NavigateTo(SaasAppLayout.Base);
            return;
        }

        busy = false;

        if (result == ShowcaseAccountResult.EmailInUse)
        {
            attempts++;
            emailInUse = email;
            return;
        }

        Fail("The workspace couldn't be created with these details. Check the plan, company, and country. If they look right, this tab has reached the demo's limit of accounts or workspaces: reset the demo data to start over.");
    }

    private void Fail(string message)
    {
        attempts++;
        error = message;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>A count in the text makes a repeated alert announce again.</summary>
    private string AttemptSuffix => attempts <= 1 ? string.Empty : $" (attempt {attempts})";

    /// <summary>The trial plan when it is self-serve, otherwise the first self-serve plan.</summary>
    private string DefaultPlanId => State.FindPlan(SaasSeedData.TrialPlanId) is { IsSelfServe: true } trial
        ? trial.Id
        : SelfServePlans.FirstOrDefault()?.Id ?? string.Empty;

    private static int DiscountPercent => (int)Math.Round(SaasPricing.AnnualDiscount * 100, MidpointRounding.AwayFromZero);

    private string IncludedHeading => SelectedPlan switch
    {
        null => "What you get",
        { MonthlyPricePerSeat: > 0 } => "Included in your trial",
        { } plan => $"Included in {plan.Name}",
    };

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private SaasBillingPeriod Period => State.AnnualBilling ? SaasBillingPeriod.Annual : SaasBillingPeriod.Monthly;

    /// <summary>Gets or sets the plan to preselect, from <c>?plan=</c>.</summary>
    [SupplyParameterFromQuery(Name = "plan")] public string? Plan { get; set; }

    private string PlanHelp
    {
        get
        {
            if (SelectedPlan is not { } plan)
                return "Choose a plan.";

            if (plan.MonthlyPricePerSeat == 0)
                return plan.SeatLimit > 0
                    ? $"{plan.Name} is free for as long as you like, for up to {plan.SeatLimit} seats. There's no trial to end and no card to add."
                    : $"{plan.Name} is free for as long as you like. There's no trial to end and no card to add.";

            return $"{plan.Name} starts a {SaasSeedData.TrialDays}-day free trial, and no card is needed to start. If you keep it afterwards, it's "
                + $"{SaasDisplay.Money(SaasPricing.MonthlySeatPrice(plan, Period))} per seat / month, {SaasSeedData.PeriodLabel(Period)}.";
        }
    }

    /// <summary>The chosen plan while it is still a self-serve plan, otherwise <see langword="null" />.</summary>
    private SaasPlan? SelectedPlan => State.FindPlan(form.PlanId) is { IsSelfServe: true } plan ? plan : null;

    private IReadOnlyList<SaasPlan> SelfServePlans => State.Plans.Where(plan => plan.IsSelfServe).ToList();

    [Inject] private SaasState State { get; set; } = default!;

    private string SubmitText => SelectedPlan is { MonthlyPricePerSeat: 0 } ? "Create free workspace" : "Start free trial";

    /// <summary>Where a signed-in visitor works: the console for the operator, the workspace for a customer.</summary>
    private string WorkspacePath => State.IsOperator ? SaasAdminLayout.Base : SaasAppLayout.Base;

    private string WorkspaceText => State.IsOperator ? "Open console" : "Open workspace";

    #endregion

    #region Nested Types

    private sealed class SignupForm
    {
        [Required(ErrorMessage = "Choose a country.")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your company.")]
        [StringLength(SaasState.MaxWorkspaceNameLength, ErrorMessage = "Use {1} characters or fewer.")]
        public string Company { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your work email.")]
        [EmailAddress(ErrorMessage = "Enter an email address like name@example.com.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use {1} characters or fewer.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your name.")]
        [StringLength(ShowcaseIdentity.MaxNameLength, MinimumLength = 2, ErrorMessage = "Use {2} to {1} characters.")]
        [RegularExpression(@".*\S.*\S.*", ErrorMessage = "Enter your name.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Make up a password.")]
        [StringLength(128, MinimumLength = ShowcaseIdentity.MinPasswordLength, ErrorMessage = "Use at least {2} characters.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Choose a plan.")]
        public string PlanId { get; set; } = string.Empty;
    }

    #endregion
}
