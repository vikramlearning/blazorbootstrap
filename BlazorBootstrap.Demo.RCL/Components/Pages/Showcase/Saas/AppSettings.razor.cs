using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The workspace's settings in an <see cref="EditForm" /> (rule WR-002): its name (unique on Lumora, checked as you type),
/// time zone, first day of the week, data region (a workspace in an EU country keeps its data in the European Union, a
/// cross-field rule), and the four email switches. A failed save shows a summary that links to each field. Owners and
/// admins save them with <see cref="SaasState.UpdateWorkspace" />, and the result shows in a status message (a taken name on
/// its field); everyone else (and everyone while the workspace is suspended or archived) sees the values read-only, with a
/// note saying why.
/// </summary>
public partial class AppSettings : ComponentBase, IDisposable
{
    #region Fields and Constants

    private static readonly EmailOption[] emailOptions =
    {
        new("digest", "Weekly digest", "A summary of events, top pages, and traffic at the start of each week.",
            form => form.WeeklyDigest, (form, value) => form.WeeklyDigest = value),
        new("security", "Security alerts", "New sign-ins by owners and admins, and changes to who can sign in.",
            form => form.SecurityAlerts, (form, value) => form.SecurityAlerts = value),
        new("billing", "Billing emails", "Invoices, failed payments, and plan changes.",
            form => form.BillingEmails, (form, value) => form.BillingEmails = value),
        new("updates", "Product updates", "New features and release notes, about twice a month.",
            form => form.ProductUpdates, (form, value) => form.ProductUpdates = value),
    };

    private EditContext editContext = default!;

    private string? errorMessage;

    private SettingsForm form = new();

    /// <summary>The workspace the form was filled from, so a different sign-in refills it but other changes keep the edits.</summary>
    private int? formWorkspaceId;

    private int messageCount;

    private ShowcaseUniqueCheck? nameCheck;

    /// <summary>The state's answers that belong to a field (a taken name), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private string? messageText;

    private string? statusMessage;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        editContext = new EditContext(form);
        FillForm();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void ClearMessages()
    {
        statusMessage = null;
        errorMessage = null;
    }

    private void Discard()
    {
        ResetForm();
        ShowStatus("Changes discarded. The form shows the saved settings again.");
    }

    /// <summary>Fills the form from the signed-in workspace when it shows a different workspace (or none yet).</summary>
    private void FillForm()
    {
        if (State.CurrentWorkspace is { } workspace && workspace.Id != formWorkspaceId)
        {
            ResetForm();
            ClearMessages();
        }
    }

    private void OnStateChanged() => InvokeAsync(() =>
    {
        FillForm();
        StateHasChanged();
    });

    private static bool IsEuCountry(string country) => SaasSeedData.EuCountries.Contains(country);

    /// <summary>Whether another workspace has this name; this workspace doesn't count.</summary>
    private Task<bool> IsNameTakenAsync(string name) => Task.FromResult(State.IsWorkspaceNameTaken(name, State.CurrentWorkspace?.Id ?? 0));

    private string NameTakenMessage(string name) =>
        $"Another workspace on {SaasSeedData.ProductName} is already called \"{name.Trim()}\". Choose a different name, for example with your team or city.";

    /// <summary>Returns why the settings are read-only for the signed-in member.</summary>
    private static string ReadOnlyText(SaasWorkspace workspace, SaasMember member) =>
        member.Role is SaasMemberRole.Owner or SaasMemberRole.Admin
            ? $"{workspace.Name} is {SaasDisplay.StatusName(workspace.Status).ToLowerInvariant()}, so its settings can't be changed right now."
            : $"Only owners and admins can change these settings. You're {SaasDisplay.RoleName(member.Role).ToLowerInvariant()} here, so you can see them but not change them.";

    private void ResetForm()
    {
        if (State.CurrentWorkspace is not { } workspace)
            return;

        var settings = workspace.Settings;
        form = new SettingsForm
        {
            Name = workspace.Name,
            TimeZone = settings.TimeZone,
            WeekStartsOn = settings.WeekStartsOn,
            DataRegion = settings.DataRegion,
            WeeklyDigest = settings.WeeklyDigest,
            SecurityAlerts = settings.SecurityAlerts,
            BillingEmails = settings.BillingEmails,
            ProductUpdates = settings.ProductUpdates,
            Country = workspace.Country,
        };
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += (_, args) => serverErrors?.Clear(args.FieldIdentifier);
        formWorkspaceId = workspace.Id;
    }

    /// <summary>
    /// Saves once the form is valid: the name check finishes first, then every rule runs, and a failed submit shows the
    /// summary (rule WR-002). The state checks again; a taken name goes on its field.
    /// </summary>
    private async Task SaveAsync()
    {
        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!editContext.Validate() || !nameFree)
            return;

        var settings = new SaasWorkspaceSettings(form.TimeZone, form.WeekStartsOn, form.DataRegion, form.WeeklyDigest, form.SecurityAlerts, form.BillingEmails, form.ProductUpdates);
        var result = State.UpdateWorkspace(form.Name, settings);

        switch (result)
        {
            case SaasResult.Ok:
                ResetForm();
                ShowStatus($"Settings saved. They now apply to everyone in {form.Name}.");
                break;
            case SaasResult.NoChange:
                ShowStatus("Nothing to save: these are already the workspace's settings.");
                break;

            case SaasResult.Duplicate:
                serverErrors?.Add(FieldIdentifier.Create(() => form.Name), NameTakenMessage(form.Name));
                editContext.NotifyValidationStateChanged();
                break;
            case SaasResult.Invalid:
                ShowError($"These settings can't be saved. Use a workspace name of 1 to {SaasState.MaxWorkspaceNameLength} characters, and pick the time zone and data region from the lists.");
                break;
            case SaasResult.NotAllowed:
                ShowError("You can't change these settings: only owners and admins can, and not while the workspace is suspended or archived.");
                break;
            default:
                ShowError("The settings couldn't be saved. Try again.");
                break;
        }
    }

    /// <summary>Shows an error in the alert region; the same message again gets a counter, so it is announced again.</summary>
    private void ShowError(string message)
    {
        statusMessage = null;
        errorMessage = Counted(message);
    }

    /// <summary>Shows a message in the status region; the same message again gets a counter, so it is announced again.</summary>
    private void ShowStatus(string message)
    {
        errorMessage = null;
        statusMessage = Counted(message);
    }

    private string Counted(string message)
    {
        messageCount = message == messageText ? messageCount + 1 : 1;
        messageText = message;

        return messageCount == 1 ? message : $"{message} ({messageCount})";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One email switch: how to read and set it on the form.</summary>
    private sealed record EmailOption(string Key, string Title, string Description, Func<SettingsForm, bool> Get, Action<SettingsForm, bool> Set);

    /// <summary>
    /// The values being edited, before they are saved, with the rules of <see cref="SaasState.UpdateWorkspace" /> and one
    /// cross-field rule: a workspace in an EU country keeps its data in the European Union.
    /// </summary>
    private sealed class SettingsForm : IValidatableObject
    {
        public bool BillingEmails { get; set; }

        /// <summary>The workspace's country, for the data region rule; set by the page, not by a field.</summary>
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose a data region.")]
        public string DataRegion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a workspace name.")]
        [StringLength(SaasState.MaxWorkspaceNameLength, ErrorMessage = "Use 60 characters or fewer for the workspace name.")]
        public string Name { get; set; } = string.Empty;

        public bool ProductUpdates { get; set; }

        public bool SecurityAlerts { get; set; }

        public string TimeZone { get; set; } = string.Empty;

        public string WeekStartsOn { get; set; } = "Monday";

        public bool WeeklyDigest { get; set; }

        /// <summary>Runs once the fields are filled in: an EU workspace's data stays in the EU.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SaasSeedData.EuCountries.Contains(Country) && DataRegion != "European Union")
            {
                yield return new ValidationResult($"A workspace in {Country} keeps its data in the European Union. Choose European Union as the data region.",
                    new[] { nameof(DataRegion) });
            }
        }
    }

    #endregion
}
