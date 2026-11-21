using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Words, badge colors, and routes the AML screens share. Every status and risk is said in words; the badge color only
/// repeats it.
/// </summary>
public static class AmlDisplay
{
    #region Fields and Constants

    /// <summary>The route of the AML alert queue.</summary>
    public const string AlertsPath = FintechOpsLayout.Base + "/aml/alerts";

    /// <summary>The route of the AML case list.</summary>
    public const string CasesPath = FintechOpsLayout.Base + "/aml/cases";

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the route of an alert.</summary>
    public static string AlertPath(int alertId) => $"{AlertsPath}/{alertId.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>Returns the words of an alert status.</summary>
    public static string AlertStatusText(FintechAmlAlertStatus status) => status switch
    {
        FintechAmlAlertStatus.New => "New",
        FintechAmlAlertStatus.InTriage => "In triage",
        FintechAmlAlertStatus.Dismissed => "Dismissed",
        _ => "In a case",
    };

    /// <summary>Returns the badge color of an alert status.</summary>
    public static BadgeColor AlertStatusColor(FintechAmlAlertStatus status) => status switch
    {
        FintechAmlAlertStatus.New => BadgeColor.Primary,
        FintechAmlAlertStatus.InTriage => BadgeColor.Info,
        FintechAmlAlertStatus.Dismissed => BadgeColor.Secondary,
        _ => BadgeColor.Dark,
    };

    /// <summary>Returns the route of a case.</summary>
    public static string CasePath(int caseId) => $"{CasesPath}/{caseId.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>Returns the words of a case status.</summary>
    public static string CaseStatusText(FintechAmlCaseStatus status) => status switch
    {
        FintechAmlCaseStatus.Open => "Open",
        FintechAmlCaseStatus.Investigating => "Investigating",
        FintechAmlCaseStatus.Escalated => "Escalated",
        _ => "Closed",
    };

    /// <summary>Returns the badge color of a case status.</summary>
    public static BadgeColor CaseStatusColor(FintechAmlCaseStatus status) => status switch
    {
        FintechAmlCaseStatus.Open => BadgeColor.Primary,
        FintechAmlCaseStatus.Investigating => BadgeColor.Info,
        FintechAmlCaseStatus.Escalated => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Formats a date like "Dec 10, 2026".</summary>
    public static string DateText(DateOnly date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Returns whether a case is past its due date and not closed.</summary>
    public static bool IsOverdue(FintechAmlCase amlCase, DateOnly today) => amlCase.Status != FintechAmlCaseStatus.Closed && amlCase.DueOn < today;

    /// <summary>Returns the badge color of a risk.</summary>
    public static BadgeColor RiskColor(FintechAmlRisk risk) => risk switch
    {
        FintechAmlRisk.High => BadgeColor.Danger,
        FintechAmlRisk.Medium => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns a risk in words, like "High risk".</summary>
    public static string RiskText(FintechAmlRisk risk) => $"{risk} risk";

    /// <summary>Returns a staff member's name, or "Unassigned".</summary>
    public static string StaffText(string? staffId) => FintechSeedData.StaffName(staffId) ?? "Unassigned";

    /// <summary>Says when a case is due: "Due Dec 10", "Due today", or "Overdue by 3 days".</summary>
    public static string DueText(FintechAmlCase amlCase, DateOnly today)
    {
        if (amlCase.Status == FintechAmlCaseStatus.Closed)
            return "Closed";

        var days = amlCase.DueOn.DayNumber - today.DayNumber;

        return days switch
        {
            0 => "Due today",
            < 0 => days == -1 ? "Overdue by 1 day" : $"Overdue by {(-days).ToString(CultureInfo.InvariantCulture)} days",
            _ => $"Due {amlCase.DueOn.ToString("MMM d", CultureInfo.InvariantCulture)}",
        };
    }

    /// <summary>Returns the message for an AML action's result; four-eyes refusals say why.</summary>
    public static string ResultText(FintechResult result) => result switch
    {
        FintechResult.Ok => "Done.",
        FintechResult.NotAllowed => "Only a senior reviewer can do that, or your sign-in has ended. Nothing changed.",
        FintechResult.NotFound => "That case, alert, or staff member doesn't exist any more. Nothing changed.",
        FintechResult.Invalid => "Something in that isn't valid. Check the fields and try again; nothing changed.",
        FintechResult.WrongState => "The case or alert has moved on meanwhile (it was closed, escalated, or handled). Nothing changed.",
        FintechResult.Duplicate => "That name is already used on this case. Nothing changed.",
        FintechResult.LimitReached => "The demo's limit is reached. Reset the demo to start again; nothing changed.",
        FintechResult.ApprovalRequired => "Reporting a case always needs a senior reviewer's approval: escalate it, proposing Reported.",
        FintechResult.SelfApproval => "Four-eyes rule: the person who escalated a case can't decide it. Another senior reviewer must.",
        FintechResult.NoChange => "Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    #endregion

    #endregion
}
