using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Services.Showcase.SaasDashboard;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.SaasDashboard;

/// <summary>
/// Settings screen: general workspace details, notification emails, and billing, in tabs.
/// Saved values live in <see cref="SaasDashboardState" /> for the rest of the session.
/// </summary>
public partial class Settings : ComponentBase
{
    #region Fields and Constants

    private EditContext editContext = default!;

    private GeneralForm general = new();

    private IReadOnlyList<NotificationOption> notificationOptions = Array.Empty<NotificationOption>();

    private bool planNotice;

    private string? savedMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        ResetGeneral();

        var settings = State.Settings;
        notificationOptions = new[]
        {
            new NotificationOption("digest", "Weekly digest", "A Monday summary of revenue, sign-ups, and churn.", settings.WeeklyDigest),
            new NotificationOption("security", "Security alerts", "New admin sign-ins and changes to single sign-on.", settings.SecurityAlerts),
            new NotificationOption("billing", "Billing emails", "Invoices, failed payments, and plan changes.", settings.BillingEmails),
            new NotificationOption("updates", "Product updates", "New features and release notes, about twice a month.", settings.ProductUpdates),
        };
    }

    #endregion

    #region Private Methods

    private void ResetGeneral()
    {
        var settings = State.Settings;
        general = new GeneralForm
        {
            WorkspaceName = settings.WorkspaceName,
            SupportEmail = settings.SupportEmail,
            TimeZone = settings.TimeZone,
            WeekStartsOn = settings.WeekStartsOn,
        };
        editContext = new EditContext(general);
    }

    private void SaveGeneral()
    {
        State.SaveSettings(State.Settings with
        {
            WorkspaceName = general.WorkspaceName,
            SupportEmail = general.SupportEmail,
            TimeZone = general.TimeZone,
            WeekStartsOn = general.WeekStartsOn,
        });
        savedMessage = "Workspace settings saved for this session.";
    }

    private void ShowPlanNotice() => planNotice = true;

    private void ToggleNotification(NotificationOption option, bool value)
    {
        option.Value = value;

        var settings = State.Settings;
        State.SaveSettings(option.Key switch
        {
            "digest" => settings with { WeeklyDigest = value },
            "security" => settings with { SecurityAlerts = value },
            "billing" => settings with { BillingEmails = value },
            _ => settings with { ProductUpdates = value },
        });
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private SaasDashboardState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class GeneralForm
    {
        [Required(ErrorMessage = "Enter a support email.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string SupportEmail { get; set; } = string.Empty;

        public string TimeZone { get; set; } = "UTC";

        public string WeekStartsOn { get; set; } = "Monday";

        [Required(ErrorMessage = "Enter a workspace name.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Use 2 to 40 characters.")]
        public string WorkspaceName { get; set; } = string.Empty;
    }

    private sealed class NotificationOption(string key, string title, string description, bool value)
    {
        public string Description { get; } = description;

        public string Key { get; } = key;

        public string Title { get; } = title;

        public bool Value { get; set; } = value;
    }

    #endregion
}
