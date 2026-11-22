using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The operator's form for a new tenant, in an <see cref="EditForm" /> (rule WR-002): the workspace name (unique on Lumora,
/// checked as you type), its subdomain (made from the name until edited; format and uniqueness checked as you type), its
/// owner (added as an invitation with no demo sign-in) and their email, the country, an account manager, the plan
/// (Enterprise included, since it is sold through sales), the billing period for a paid plan, seats (within the plan's
/// limit, a cross-field rule), and an optional 14-day trial on a paid self-serve plan. Messages show after a field is
/// touched or on submit, and a failed submit shows a summary that links to each field. A live summary shows the price per
/// period and whether a first invoice is issued. <c>?lead={id}</c> prefills the form from a sales lead and closes that lead
/// when the tenant is created. Creating calls <see cref="SaasState.CreateWorkspaceAsOperator" /> and opens the new tenant;
/// the state's refusals are shown on their field or in the alert.
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

    private ShowcaseUniqueCheck? nameCheck;

    /// <summary>The state's answers that belong to a field (a taken name or subdomain, seats out of range), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private ShowcaseUniqueCheck? subdomainCheck;

    /// <summary>Whether the operator typed in the subdomain; until then it follows the name.</summary>
    private bool subdomainEdited;

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
            Subdomain = State.FreeSubdomain(lead.Company),
            OwnerName = lead.Name,
            OwnerEmail = lead.Email,
            PlanId = plan.Id,
            Period = SaasBillingPeriod.Annual,
            Seats = Math.Clamp(SeatsFor(lead.TeamSize), 1, SeatCap(plan)),
            Country = SaasSeedData.Countries[0],
            ManagerId = State.Identity.Current?.Id is { } me && SaasSeedData.ManagerName(me) is not null ? me : string.Empty,
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
        var amount = SaasDisplay.MoneyCents(SaasPricing.PeriodAmount(plan, period, SeatsOrOne));

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

    private Task<bool> IsNameTakenAsync(string name) => Task.FromResult(State.IsWorkspaceNameTaken(name));

    private Task<bool> IsSubdomainTakenAsync(string subdomain) => Task.FromResult(State.IsSubdomainTaken(subdomain));

    private string NameTakenMessage(string name) =>
        $"A workspace called \"{name.Trim()}\" already exists on {SaasSeedData.ProductName}. Open it from the tenant list, or choose a different name.";

    private TenantForm NewForm()
    {
        var plan = DefaultPlan;

        return new TenantForm { PlanId = plan.Id, Seats = Math.Min(5, SeatCap(plan)) };
    }

    /// <summary>
    /// While the subdomain hasn't been typed in, it follows the name (a free one made from it), and its own check runs. A plan
    /// that can't have a trial drops it; any change clears the last error and the state's message on that field.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);

        if (args.FieldIdentifier.FieldName == nameof(TenantForm.Name) && !subdomainEdited)
        {
            var proposed = string.IsNullOrWhiteSpace(form.Name) ? string.Empty : State.FreeSubdomain(form.Name);

            if (proposed != form.Subdomain)
            {
                form.Subdomain = proposed;
                editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.Subdomain));
            }
        }

        if (args.FieldIdentifier.FieldName == nameof(TenantForm.PlanId))
        {
            form.SeatCap = SeatCap(SelectedPlan);
            form.SeatPlanName = SelectedPlan.Name;

            if (!TrialAllowed(SelectedPlan))
                form.StartTrial = false;
        }

        ClearError();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Typing in the subdomain stops it following the name; clearing it lets the name fill it again.</summary>
    private void OnSubdomainInput(ChangeEventArgs args) => subdomainEdited = !string.IsNullOrWhiteSpace(args.Value?.ToString());

    private string PlanHint(SaasPlan plan)
    {
        var price = plan.MonthlyPricePerSeat == 0 ? "Free" : $"{SaasDisplay.MoneyCents(plan.MonthlyPricePerSeat)} per seat a month";
        var limit = plan.SeatLimit == 0 ? "no seat limit" : $"up to {Count(plan.SeatLimit)} seats";
        var sales = plan.IsSelfServe ? string.Empty : ", sold through sales";

        return $"{price}, {limit}{sales}";
    }

    private string PriceText(SaasPlan plan) => plan.MonthlyPricePerSeat == 0
        ? "Free"
        : $"{SaasDisplay.MoneyCents(SaasPricing.PeriodAmount(plan, form.Period, SeatsOrOne))} per {PeriodWord(form.Period)}";

    private void ResetForm(TenantForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        form.SeatCap = SeatCap(SelectedPlan);
        form.SeatPlanName = SelectedPlan.Name;
        subdomainEdited = false;
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    private string SeatPriceText(SaasPlan plan) => form.Period == SaasBillingPeriod.Annual
        ? $"{SaasDisplay.MoneyCents(SaasPricing.MonthlySeatPrice(plan, form.Period))} per seat a month, billed annually"
        : $"{SaasDisplay.MoneyCents(plan.MonthlyPricePerSeat)} per seat a month, billed monthly";

    private string SeatsHelp(SaasPlan plan) => plan.SeatLimit == 0
        ? $"From 1 to {Count(SeatCap(plan))}. {plan.Name} has no seat limit; {SaasSeedData.ProductName} allows up to {Count(SaasSeedData.MaxSeats)}."
        : $"From 1 to {Count(plan.SeatLimit)}, the {plan.Name} plan's limit. The owner's invitation takes one seat.";

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : $"{message} ({errorCount})";
    }

    private string SubdomainTakenMessage(string subdomain) =>
        $"{subdomain.Trim().ToLowerInvariant()}.lumora.example is taken by another workspace. Choose a different subdomain, like {State.FreeSubdomain(subdomain)}.";

    /// <summary>
    /// Creates the tenant once the form is valid: the uniqueness checks finish first, then every rule runs, and a failed
    /// submit shows the summary (rule WR-002). The state checks again; its answer maps to a field or the alert.
    /// </summary>
    private async Task SubmitAsync()
    {
        if (busy)
            return;

        serverErrors?.Clear();
        form.SeatCap = SeatCap(SelectedPlan);
        form.SeatPlanName = SelectedPlan.Name;
        editContext.NotifyValidationStateChanged();

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();
        var subdomainFree = subdomainCheck is null || await subdomainCheck.CheckNowAsync();

        if (!await editContext.ValidateAsync() || !nameFree || !subdomainFree)
            return;

        var plan = SelectedPlan;
        var period = plan.MonthlyPricePerSeat > 0 ? form.Period : SaasBillingPeriod.Monthly;
        var subdomain = form.Subdomain.Trim().ToLowerInvariant();
        var (result, workspaceId) = State.CreateWorkspaceAsOperator(form.Name, subdomain, form.OwnerName, form.OwnerEmail, plan.Id, period, form.Seats!.Value, form.Country,
            StartsTrial, linkedLeadId, string.IsNullOrEmpty(form.ManagerId) ? null : form.ManagerId);

        switch (result)
        {
            case SaasResult.Ok when workspaceId is { } id:
                busy = true;
                Navigation.NavigateTo(string.Create(CultureInfo.InvariantCulture, $"{TenantsPath}/{id}"));

                return;

            case SaasResult.Duplicate when State.IsWorkspaceNameTaken(form.Name):
                ServerError(() => form.Name, NameTakenMessage(form.Name));

                return;

            case SaasResult.Duplicate:
                ServerError(() => form.Subdomain, SubdomainTakenMessage(subdomain));

                return;

            case SaasResult.SeatsOutOfRange:
                ServerError(() => form.Seats, $"Choose 1 to {Count(SeatCap(plan))} seats for the {plan.Name} plan.");

                return;

            case SaasResult.NotFound:
                ServerError(() => form.PlanId, "That plan is no longer offered in this tab, or a trial was asked for on a free plan. Choose the plan again.");

                return;

            case SaasResult.NotAllowed:
                ShowError("Only a signed-in Lumora operator can create a tenant. Sign in as an operator and try again.");

                return;

            default:
                ShowError(string.Create(CultureInfo.InvariantCulture,
                    $"The tenant couldn't be created. This demo tab holds at most {SaasState.MaxWorkspaces} tenants; delete an archived one, or reset the demo data."));

                return;
        }
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

    /// <summary>The seats typed, or 1 while the field is empty, for the live summary.</summary>
    private int SeatsOrOne => Math.Max(1, form.Seats ?? 1);

    private SaasPlan SelectedPlan => State.FindPlan(form.PlanId) ?? DefaultPlan;

    [Inject] private SaasState State { get; set; } = default!;

    /// <summary>Whether the tenant starts on a trial: asked for, and allowed on the chosen plan.</summary>
    private bool StartsTrial => form.StartTrial && TrialAllowed(SelectedPlan);

    #endregion

    #region Nested Types

    /// <summary>
    /// The new tenant's fields, with the limits of <see cref="SaasState.CreateWorkspaceAsOperator" />: required text with
    /// lengths, an email, a subdomain's form, and one cross-field rule (seats within the chosen plan's limit).
    /// </summary>
    private sealed class TenantForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose the country the customer is in.")]
        public string Country { get; set; } = string.Empty;

        /// <summary>The chosen account manager's ID, or empty for nobody.</summary>
        public string ManagerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the workspace name, usually the company's name.")]
        [StringLength(SaasState.MaxWorkspaceNameLength, ErrorMessage = "Use 60 characters or fewer for the workspace name.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the owner's email.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer for the email.")]
        [RegularExpression(@"^\s*[^@\s]+@[^@\s]+\.[^@\s]+\s*$", ErrorMessage = "Enter the owner's email like name@example.com.")]
        public string OwnerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the owner's name.")]
        [StringLength(ShowcaseIdentity.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the owner's name.")]
        public string OwnerName { get; set; } = string.Empty;

        public SaasBillingPeriod Period { get; set; } = SaasBillingPeriod.Monthly;

        [Required(ErrorMessage = "Choose a plan.")]
        public string PlanId { get; set; } = string.Empty;

        /// <summary>The chosen plan's seat limit (the most Lumora allows when it has none); set by the page, not by a field.</summary>
        public int SeatCap { get; set; } = SaasSeedData.MaxSeats;

        /// <summary>The chosen plan's name, for the seats message; set by the page.</summary>
        public string SeatPlanName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the number of seats.")]
        [Range(1, SaasSeedData.MaxSeats, ErrorMessage = "Use from 1 to 500 seats.")]
        public int? Seats { get; set; } = 5;

        public bool StartTrial { get; set; }

        [Required(ErrorMessage = "Enter a subdomain, like northwind-outfitters.")]
        [StringLength(SaasState.MaxSubdomainLength, MinimumLength = SaasState.MinSubdomainLength, ErrorMessage = "Use 3 to 30 characters for the subdomain.")]
        [RegularExpression(@"^\s*[a-z]([a-z0-9]|-(?!-))*[a-z0-9]\s*$", ErrorMessage = "Use lower-case letters, digits, and single hyphens, starting with a letter and ending with a letter or digit.")]
        public string Subdomain { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: the seats fit the chosen plan, and the subdomain isn't one Lumora keeps for itself.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Seats is { } seats && seats > SeatCap)
            {
                yield return new ValidationResult(
                    string.Create(CultureInfo.InvariantCulture, $"{SeatPlanName} allows up to {SeatCap} seats. Choose fewer seats, or a plan with more."), new[] { nameof(Seats) });
            }

            if (SaasSeedData.ReservedSubdomains.Contains(Subdomain.Trim().ToLowerInvariant()))
                yield return new ValidationResult($"\"{Subdomain.Trim()}\" is kept for {SaasSeedData.ProductName} itself. Choose a different subdomain.", new[] { nameof(Subdomain) });
        }
    }

    #endregion
}
