using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Routes and wording of the performance review pages (My reviews, Team reviews, a review, and the HR admin's cycles):
/// stages, cycle statuses, and ratings in words, each with a badge color that is never the only cue. Formatting uses the
/// invariant culture, like <see cref="HrDisplay" />.
/// </summary>
internal static class ReviewDisplay
{
    #region Fields and Constants

    /// <summary>The route of the employee's own reviews.</summary>
    internal const string MyReviewsUrl = HrLayout.Base + "/reviews";

    /// <summary>The route of the HR admin's review cycles.</summary>
    internal const string CyclesUrl = HrLayout.Base + "/admin/reviews";

    /// <summary>The route of the manager's team reviews.</summary>
    internal const string TeamReviewsUrl = HrLayout.Base + "/reviews/team";

    #endregion

    #region Methods

    #region Internal Methods

    /// <summary>The route of a cycle's page (HR admin).</summary>
    internal static string CycleUrl(int cycleId) => HrReviewSeedData.CycleUrl(cycleId);

    /// <summary>A rating in words, like "4 · Exceeds expectations", or "Not rated yet".</summary>
    internal static string Rating(int? rating) => rating is { } value ? string.Create(CultureInfo.InvariantCulture, $"{value} · {HrState.RatingName(value)}") : "Not rated yet";

    /// <summary>The route of one review.</summary>
    internal static string ReviewUrl(int reviewId) => HrReviewSeedData.ReviewUrl(reviewId);

    /// <summary>The badge color of a review's stage (always shown with its name).</summary>
    internal static BadgeColor StageColor(HrReviewStage stage) => stage switch
    {
        HrReviewStage.SelfReview => BadgeColor.Warning,
        HrReviewStage.ManagerReview => BadgeColor.Info,
        HrReviewStage.Calibration => BadgeColor.Primary,
        HrReviewStage.SignOff => BadgeColor.Warning,
        HrReviewStage.Done => BadgeColor.Success,
        _ => BadgeColor.Secondary,
    };

    /// <summary>A review's stage in words.</summary>
    internal static string StageLabel(HrReviewStage stage) => stage switch
    {
        HrReviewStage.SelfReview => "Self review",
        HrReviewStage.ManagerReview => "Manager review",
        HrReviewStage.Calibration => "With HR",
        HrReviewStage.SignOff => "Sign-off",
        HrReviewStage.Done => "Done",
        _ => "Left out",
    };

    /// <summary>The badge color of a cycle's status (always shown with its name).</summary>
    internal static BadgeColor StatusColor(HrCycleStatus status) => status switch
    {
        HrCycleStatus.Open => BadgeColor.Info,
        HrCycleStatus.Calibration => BadgeColor.Primary,
        HrCycleStatus.SignOff => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    /// <summary>A cycle's status in words.</summary>
    internal static string StatusLabel(HrCycleStatus status) => status switch
    {
        HrCycleStatus.Open => "Open: reviews being written",
        HrCycleStatus.Calibration => "Calibration",
        HrCycleStatus.SignOff => "Sign-off",
        _ => "Closed",
    };

    /// <summary>A cycle's status in one word, for grids.</summary>
    internal static string StatusShort(HrCycleStatus status) => status switch
    {
        HrCycleStatus.Open => "Open",
        HrCycleStatus.Calibration => "Calibration",
        HrCycleStatus.SignOff => "Sign-off",
        _ => "Closed",
    };

    #endregion

    #endregion
}
