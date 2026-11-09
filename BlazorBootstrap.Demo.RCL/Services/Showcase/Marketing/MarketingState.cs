using BlazorBootstrap.Demo.RCL.Models.Showcase.Marketing;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Marketing;

/// <summary>
/// In-memory state of the marketing site for the current session. Nothing is persisted or sent.
/// </summary>
public sealed class MarketingState
{
    #region Fields and Constants

    private readonly List<MarketingSignup> signups = new();

    private bool annualBilling = true;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Finds a plan by its ID, or returns <see langword="null" />.</summary>
    public MarketingPlan? FindPlan(string? planId) => Plans.FirstOrDefault(plan => string.Equals(plan.Id, planId, StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns the effective monthly price of a plan for the selected billing period.</summary>
    public decimal MonthlyPriceFor(MarketingPlan plan) => annualBilling
        ? Math.Round(plan.MonthlyPrice * (1 - MarketingSeedData.AnnualDiscount), 0, MidpointRounding.AwayFromZero)
        : plan.MonthlyPrice;

    /// <summary>Records a sign-up in memory and returns it.</summary>
    public MarketingSignup SignUp(string name, string email, string company, string planId)
    {
        var signup = new MarketingSignup(name.Trim(), email.Trim(), company.Trim(), planId, annualBilling, DateTime.Now);
        signups.Add(signup);
        Changed?.Invoke();
        return signup;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets whether prices are shown for annual billing.</summary>
    public bool AnnualBilling
    {
        get => annualBilling;
        set
        {
            if (annualBilling == value)
                return;

            annualBilling = value;
            Changed?.Invoke();
        }
    }

    /// <summary>Gets the case studies.</summary>
    public IReadOnlyList<MarketingCaseStudy> CaseStudies { get; } = MarketingSeedData.CaseStudies();

    /// <summary>Gets the plan comparison rows.</summary>
    public IReadOnlyList<MarketingComparisonRow> Comparison { get; } = MarketingSeedData.Comparison();

    /// <summary>Gets the FAQ entries.</summary>
    public IReadOnlyList<MarketingFaq> Faqs { get; } = MarketingSeedData.Faqs();

    /// <summary>Gets the product features.</summary>
    public IReadOnlyList<MarketingFeature> Features { get; } = MarketingSeedData.Features();

    /// <summary>Gets the most recent sign-up of this session, if any.</summary>
    public MarketingSignup? LastSignup => signups.LastOrDefault();

    /// <summary>Gets the pricing plans.</summary>
    public IReadOnlyList<MarketingPlan> Plans { get; } = MarketingSeedData.Plans();

    /// <summary>Gets the customer quotes.</summary>
    public IReadOnlyList<MarketingTestimonial> Testimonials { get; } = MarketingSeedData.Testimonials();

    #endregion

    #region Events

    /// <summary>Raised when state changes.</summary>
    public event Action? Changed;

    #endregion
}
