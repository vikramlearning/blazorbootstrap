namespace BlazorBootstrap.Demo.RCL.Models.Showcase.Marketing;

/// <summary>A product capability shown on the marketing site.</summary>
public sealed record MarketingFeature(string Id, string Title, string Summary, string Details, IconName IconName, string Category);

/// <summary>A pricing plan.</summary>
public sealed record MarketingPlan(string Id, string Name, string Description, decimal MonthlyPrice, bool IsPopular, IReadOnlyList<string> Highlights, string CallToAction);

/// <summary>One row of the plan comparison table.</summary>
public sealed record MarketingComparisonRow(string Feature, string Starter, string Growth, string Enterprise);

/// <summary>A customer quote.</summary>
public sealed record MarketingTestimonial(string Quote, string Person, string Role, string Company, int Rating, string Metric, string MetricLabel);

/// <summary>A short customer story.</summary>
public sealed record MarketingCaseStudy(string Company, string Industry, string Headline, string Result, IconName IconName);

/// <summary>A frequently asked question.</summary>
public sealed record MarketingFaq(string Id, string Category, string Question, string Answer);

/// <summary>A sign-up kept only in memory for the current session.</summary>
public sealed record MarketingSignup(string Name, string Email, string Company, string PlanId, bool Annual, DateTime SubmittedAt);
