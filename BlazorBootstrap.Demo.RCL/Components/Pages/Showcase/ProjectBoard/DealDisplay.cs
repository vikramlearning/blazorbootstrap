using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.ProjectBoard;
using BlazorBootstrap.Demo.RCL.Services.Showcase.ProjectBoard;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.ProjectBoard;

/// <summary>
/// Routes, labels, and formatting shared by the account manager screens of Tandem: clients, the deal pipeline, activity,
/// and the forecast. Formatting uses the invariant culture, like <see cref="PbDisplay" />.
/// </summary>
public static class DealDisplay
{
    #region Fields and Constants

    /// <summary>The route of the activity timeline of every client.</summary>
    public const string ActivityUrl = PbLayout.Base + "/activity";

    /// <summary>The route of the client list.</summary>
    public const string ClientsUrl = PbLayout.Base + "/clients";

    /// <summary>The route of the forecast.</summary>
    public const string ForecastUrl = PbLayout.Base + "/forecast";

    /// <summary>The route of the new-client form.</summary>
    public const string NewClientUrl = PbLayout.Base + "/clients/new";

    /// <summary>The route of the deal pipeline board.</summary>
    public const string PipelineUrl = PbLayout.Base + "/pipeline";

    /// <summary>What to say when a change didn't fit in the tab's storage.</summary>
    public const string StorageFull = "The storage for this demo tab is full, so nothing was saved. Delete something you don't need, or reset the demo, and try again.";

    /// <summary>The kinds an account manager can log by hand, in the order the form offers them.</summary>
    public static readonly IReadOnlyList<PbActivityKind> LoggableKinds = new[] { PbActivityKind.Note, PbActivityKind.Call, PbActivityKind.Email, PbActivityKind.Meeting };

    /// <summary>The open stages, in pipeline order.</summary>
    public static readonly IReadOnlyList<PbDealStage> OpenStages = new[] { PbDealStage.Lead, PbDealStage.Qualified, PbDealStage.Proposal, PbDealStage.Negotiation };

    /// <summary>Every stage, in pipeline order.</summary>
    public static readonly IReadOnlyList<PbDealStage> Stages = Enum.GetValues<PbDealStage>();

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the route of a client's edit form.</summary>
    public static string ClientEditUrl(int clientId) => FormattableString.Invariant($"{PbLayout.Base}/clients/{clientId}/edit");

    /// <summary>Describes a deal's close date against today, like "Closes Dec 1", "Close date passed 3 days ago", "Won Nov 10".</summary>
    public static string CloseText(PbDeal deal, DateTime today)
    {
        if (deal.ClosedAt is { } closed)
            return (deal.Stage == PbDealStage.Won ? "Won " : "Lost ") + PbDisplay.Date(closed);

        var days = (deal.CloseDate.Date - today.Date).Days;

        return days switch
        {
            < -1 => FormattableString.Invariant($"Close date passed {-days} days ago ({deal.CloseDate:MMM d})"),
            -1 => FormattableString.Invariant($"Close date passed yesterday ({deal.CloseDate:MMM d})"),
            0 => "Closes today",
            1 => "Closes tomorrow",
            _ => "Closes " + PbDisplay.Date(deal.CloseDate),
        };
    }

    /// <summary>Formats a count against a limit, like "12 of 1000 characters" (or how far over it is).</summary>
    public static string CounterText(string? value, int max)
    {
        var length = (value ?? string.Empty).Trim().Length;

        return length > max
            ? string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters: {length - max} over the limit")
            : string.Create(CultureInfo.InvariantCulture, $"{length} of {max} characters");
    }

    /// <summary>Returns a heading for a day of the timeline: "Today", "Yesterday", or a date like "Friday, Nov 20, 2026".</summary>
    public static string DayLabel(DateTime day, DateTime today) => (today.Date - day.Date).Days switch
    {
        0 => "Today",
        1 => "Yesterday",
        _ => day.ToString("dddd, MMM d, yyyy", CultureInfo.InvariantCulture),
    };

    /// <summary>Formats a time in full, like "Nov 24, 2026 at 9:05 AM", for a title or visually hidden text.</summary>
    public static string FullTime(DateTime time) => time.ToString("MMM d, yyyy 'at' h:mm tt", CultureInfo.InvariantCulture);

    /// <summary>Returns whether an open deal's expected close date has passed.</summary>
    public static bool IsCloseOverdue(PbDeal deal, DateTime today) => deal.ClosedAt is null && deal.CloseDate.Date < today.Date;

    /// <summary>Returns whether a deal is still open (not won or lost).</summary>
    public static bool IsOpen(PbDeal deal) => deal.Stage is not (PbDealStage.Won or PbDealStage.Lost);

    /// <summary>Returns whether an activity entry was recorded by Tandem itself (a deal or project change), not logged by a person.</summary>
    public static bool IsSystem(PbActivityKind kind) => kind is PbActivityKind.Deal or PbActivityKind.Project;

    /// <summary>Returns the icon of an activity kind (always shown with the kind in words).</summary>
    public static IconName KindIcon(PbActivityKind kind) => kind switch
    {
        PbActivityKind.Note => IconName.JournalText,
        PbActivityKind.Call => IconName.Telephone,
        PbActivityKind.Email => IconName.Envelope,
        PbActivityKind.Meeting => IconName.People,
        PbActivityKind.Deal => IconName.Funnel,
        _ => IconName.Kanban,
    };

    /// <summary>Returns an activity kind in words.</summary>
    public static string KindLabel(PbActivityKind kind) => kind switch
    {
        PbActivityKind.Deal => "Deal update",
        PbActivityKind.Project => "Project update",
        _ => kind.ToString(),
    };

    /// <summary>Formats a percentage like "75%".</summary>
    public static string Percent(int percent) => string.Create(CultureInfo.InvariantCulture, $"{percent}%");

    /// <summary>Formats a count with its noun, like "1 deal" or "3 deals" (or the given plural).</summary>
    public static string Plural(int count, string noun, string? plural = null) =>
        count == 1 ? $"1 {noun}" : string.Create(CultureInfo.InvariantCulture, $"{count} {plural ?? noun + "s"}");

    /// <summary>Formats a date for a native date input (<c>yyyy-MM-dd</c>).</summary>
    public static string ToDateInput(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>Reads a native date input's value (<c>yyyy-MM-dd</c>).</summary>
    public static bool TryParseDateInput(string? value, out DateTime date) =>
        DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    /// <summary>Describes a deal's chance and weighted value, like "50% chance, weighted $24,000".</summary>
    public static string WeightedText(PbDeal deal) => $"{Percent(PbState.Probability(deal.Stage))} chance, weighted {PbDisplay.Money(PbState.Weighted(deal))}";

    #endregion

    #endregion
}
