using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The operator's form for a new tenant: the workspace name, its owner (added as an invitation with no demo sign-in), the
/// plan (Enterprise included, since it is sold through sales), the billing period for a paid plan, seats, country, and an
/// optional 14-day trial on a paid self-serve plan. A live summary shows the price per period and whether a first invoice is
/// issued. <c>?lead={id}</c> prefills the form from a sales lead and closes that lead when the tenant is created. Creating
/// calls <see cref="SaasState.CreateWorkspaceAsOperator" /> and opens the new tenant; any other result is explained.
/// </summary>
public partial class AdminTenantNew : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The plan a lead of 101 people or more starts on; smaller teams start on <see cref="SaasSeedData.TrialPlanId" />.</summary>
    private const string EnterprisePlanId = "enterprise";

    /// <summary>The position of "101–500" in <see cref="SaasSeedData.TeamSizes" />: that size and every larger one start on Enterprise.</summary>
    private const int LargeTeamIndex = 2;

    private const string TenantsPath = SaasAdminLayout.Base + "/tenants";

    private int? appliedLeadQuery;

    /// <summary>Whether the tenant was created and the page is moving to it.</summary>
    private bool busy;

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private TenantForm form = new();

    /// <summary>The lead the tenant comes from, passed on so it is closed; <see langword="null" /> for none (or a lead already closed).</summary>
    private int? linkedLeadId;

    /// <summary>What the form says about the lead in <c>?lead=</c>.</summary>
    private string? leadNote;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        ResetForm(NewForm());
        State.Changed += OnStateChanged;
    }

    /// <summary><c>?lead=</c> prefills the form once; a link to another lead starts over from that lead.</summary>
    protected override void OnParametersSet()
    {
        if (LeadQuery == appliedLeadQuery)
            return;

        appliedLeadQuery = LeadQuery;
        linkedLeadId = null;
        leadNote = null;
        ClearError();

        if (LeadQuery is not { } leadId)
        {
            ResetForm(NewForm());

            return;
        }

        if (State.FindLead(leadId) is not { } lead)
        {
            ResetForm(NewForm());
            leadNote = string.Create(CultureInfo.InvariantCulture, $"Lead #{leadId} isn't in this tab, so the form starts empty.");

            return;
        }

        var plan = State.FindPlan(IsLargeTeam(lead.TeamSize) ? EnterprisePlanId : SaasSeedData.TrialPlanId) ?? DefaultPlan;
        ResetForm(new TenantForm
        {
            Name = lead.Company,
            OwnerName = lead.Name,
            OwnerEmail = lead.Email,
            PlanId = plan.Id,
            Period = SaasBillingPeriod.Annual,
            Seats = Math.Clamp(SeatsFor(lead.TeamSize), 1, SeatCap(plan)),
            Country = SaasSeedData.Countries[0],
        });

        if (lead.Status == SaasLeadStatus.Closed)
        {
            leadNote = $"Filled in from the lead of {lead.Name} ({lead.Company}, {lead.TeamSize} people). That lead is already closed, so creating the tenant leaves it as it is.";

            return;
        }

        linkedLeadId = lead.Id;
        leadNote = $"Filled in from the lead of {lead.Name} ({lead.Company}, {lead.TeamSize} people), on {plan.Name} for the team size. "
            + "Creating the tenant closes the lead and adds a note that it was won.";
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        editContext.OnFieldChanged -= OnFieldChanged;
    }

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>Whether a lead's team size is 101 people or more, which starts on Enterprise.</summary>
    private static bool IsLargeTeam(string teamSize) => TeamSizeIndex(teamSize) >= LargeTeamIndex;

    private static string PeriodWord(SaasBillingPeriod period) => period == SaasBillingPeriod.Annual ? "year" : "month";

    private static int SeatCap(SaasPlan plan) => plan.SeatLimit == 0 ? SaasSeedData.MaxSeats : plan.SeatLimit;

    /// <summary>A starting seat count for a lead's team size; the operator adjusts it.</summary>
    private static int SeatsFor(string teamSize) => TeamSizeIndex(teamSize) switch
    {
        0 => 10,
        1 => 25,
        2 => 101,
        _ => SaasSeedData.MaxSeats,
    };

    private static string SeatsText(int seats) => seats == 1 ? "1 seat" : $"{Count(seats)} seats";

    /// <summary>The position of a team size in <see cref="SaasSeedData.TeamSizes" />, or -1.</summary>
    private static int TeamSizeIndex(string teamSize) => SaasSeedData.TeamSizes.ToList().IndexOf(teamSize);

    private static bool TrialAllowed(SaasPlan plan) => plan.MonthlyPricePerSeat > 0 && plan.IsSelfServe;

    private string BillingText(SaasPlan plan)
    {
        var today = State.Clock.Today;

        if (plan.MonthlyPricePerSeat == 0)
            return $"{plan.Name} is free, so no invoices are issued.";

        var period = form.Period;
        var amount = SaasDisplay.MoneyCents(SaasPricing.PeriodAmount(plan, period, form.Seats));

        if (StartsTrial)
        {
            return $"No invoice during the trial. When it ends on {SaasDisplay.Date(today.AddDays(SaasSeedData.TrialDays))}, the first invoice of {amount} is issued; "
                + "with no card on file it stays open and the tenant becomes past due.";
        }

        return $"An open first invoice of {amount} is issued today, for {SaasDisplay.Date(today)} to {SaasDisplay.Date(SaasPricing.PeriodEnd(today, period))}. "
            + "The owner has no sign-in or card in this demo, so it stays open and the tenant becomes past due at its next renewal.";
    }

    private void ClearError()
    {
        errorMessage = errorText = null;
        errorCount = 0;
    }

    private string ErrorFor(SaasResult result, SaasPlan plan) => result switch
    {
        SaasResult.SeatsOutOfRange => $"Choose 1 to {Count(SeatCap(plan))} seats for the {plan.Name} plan.",
        SaasResult.NotFound => "That plan is no longer offered in this tab, or a trial was asked for on a free plan. Choose the plan again.",
        SaasResult.Invalid => $"Check the workspace name (up to {SaasState.MaxWorkspaceNameLength} characters), the owner's name and email, and the country. "
            + "This demo tab also holds at most 500 tenants and 5,000 members.",
        SaasResult.NotAllowed => "Only a signed-in Lumora operator can create a tenant. Sign in as an operator and try again.",
        _ => "The tenant couldn't be created. Check the form and try again.",
    };

    private TenantForm NewForm()
    {
        var plan = DefaultPlan;

        return new TenantForm { PlanId = plan.Id, Seats = Math.Min(5, SeatCap(plan)), Country = SaasSeedData.Countries[0] };
    }

    /// <summary>
    /// A plan change keeps the seats inside the new plan's limit and drops a trial the plan can't have; any change clears
    /// the last error.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        if (args.FieldIdentifier.FieldName == nameof(TenantForm.PlanId))
        {
            var plan = SelectedPlan;
            form.Seats = Math.Clamp(form.Seats, 1, SeatCap(plan));

            if (!TrialAllowed(plan))
                form.StartTrial = false;
        }

        ClearError();
    }

    /// <summary><see cref="NumberInput{TValue}" /> raises ValueChanged on its first render; an unchanged value is ignored.</summary>
    private void OnSeatsChanged(int value)
    {
        if (value == form.Seats)
            return;

        form.Seats = value;
        ClearError();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private string PlanHint(SaasPlan plan)
    {
        var price = plan.MonthlyPricePerSeat == 0 ? "Free" : $"{SaasDisplay.MoneyCents(plan.MonthlyPricePerSeat)} per seat a month";
        var limit = plan.SeatLimit == 0 ? "no seat limit" : $"up to {Count(plan.SeatLimit)} seats";
        var sales = plan.IsSelfServe ? string.Empty : ", sold through sales";

        return $"{price}, {limit}{sales}";
    }

    private string PriceText(SaasPlan plan) => plan.MonthlyPricePerSeat == 0
        ? "Free"
        : $"{SaasDisplay.MoneyCents(SaasPricing.PeriodAmount(plan, form.Period, form.Seats))} per {PeriodWord(form.Period)}";

    private void ResetForm(TenantForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        editContext = new EditContext(form);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    private string SeatPriceText(SaasPlan plan) => form.Period == SaasBillingPeriod.Annual
        ? $"{SaasDisplay.MoneyCents(SaasPricing.MonthlySeatPrice(plan, form.Period))} per seat a month, billed annually"
        : $"{SaasDisplay.MoneyCents(plan.MonthlyPricePerSeat)} per seat a month, billed monthly";

    private string SeatsHelp(SaasPlan plan) => plan.SeatLimit == 0
        ? $"From 1 to {Count(SeatCap(plan))}. {plan.Name} has no seat limit; {SaasSeedData.ProductName} allows up to {Count(SaasSeedData.MaxSeats)}."
        : $"From 1 to {Count(plan.SeatLimit)}, the {plan.Name} plan's limit. The owner's invitation takes one seat.";

    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : $"{message} ({errorCount})";
    }

    private void Submit()
    {
        if (busy)
            return;

        var plan = SelectedPlan;
        var period = plan.MonthlyPricePerSeat > 0 ? form.Period : SaasBillingPeriod.Monthly;
        var (result, workspaceId) = State.CreateWorkspaceAsOperator(form.Name, form.OwnerName, form.OwnerEmail, plan.Id, period, form.Seats, form.Country, StartsTrial,
            linkedLeadId);

        if (result == SaasResult.Ok && workspaceId is { } id)
        {
            busy = true;
            Navigation.NavigateTo(string.Create(CultureInfo.InvariantCulture, $"{TenantsPath}/{id}"));

            return;
        }

        ShowError(ErrorFor(result, plan));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private List<BreadcrumbItem> BreadcrumbItems => new()
    {
        new() { Text = "Overview", Href = SaasAdminLayout.Base },
        new() { Text = "Tenants", Href = TenantsPath },
        new() { Text = "New tenant", IsCurrentPage = true },
    };

    /// <summary>Back to the lead's inbox when the form came from a lead, otherwise to the tenant list.</summary>
    private string CancelPath => appliedLeadQuery is { } leadId && State.FindLead(leadId) is not null
        ? string.Create(CultureInfo.InvariantCulture, $"{SaasState.LeadsPath}?lead={leadId}")
        : TenantsPath;

    private SaasPlan DefaultPlan => State.FindPlan(SaasSeedData.TrialPlanId) ?? State.Plans[0];

    private static string DiscountPercent => (SaasPricing.AnnualDiscount * 100).ToString("0", CultureInfo.InvariantCulture);

    /// <summary>Gets or sets the sales lead to fill the form from, such as <c>?lead=1</c> from the leads inbox.</summary>
    [SupplyParameterFromQuery(Name = "lead")] public int? LeadQuery { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private SaasPlan SelectedPlan => State.FindPlan(form.PlanId) ?? DefaultPlan;

    [Inject] private SaasState State { get; set; } = default!;

    /// <summary>Whether the tenant starts on a trial: asked for, and allowed on the chosen plan.</summary>
    private bool StartsTrial => form.StartTrial && TrialAllowed(SelectedPlan);

    #endregion

    #region Nested Types

    private sealed class TenantForm
    {
        [Required(ErrorMessage = "Choose a country.")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the workspace name.")]
        [StringLength(SaasState.MaxWorkspaceNameLength, ErrorMessage = "Use 60 characters or fewer.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the owner's email.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer.")]
        [RegularExpression(@"^\s*[^@\s]+@[^@\s]+\s*$", ErrorMessage = "Enter an email such as name@example.com.")]
        public string OwnerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the owner's name.")]
        [StringLength(ShowcaseIdentity.MaxNameLength, ErrorMessage = "Use 80 characters or fewer.")]
        public string OwnerName { get; set; } = string.Empty;

        public SaasBillingPeriod Period { get; set; } = SaasBillingPeriod.Monthly;

        [Required(ErrorMessage = "Choose a plan.")]
        public string PlanId { get; set; } = string.Empty;

        public int Seats { get; set; } = 5;

        public bool StartTrial { get; set; }
    }

    #endregion
}
