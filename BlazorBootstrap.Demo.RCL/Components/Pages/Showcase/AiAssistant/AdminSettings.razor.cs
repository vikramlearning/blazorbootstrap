using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Models and policy of the Parley back office (Admins and the Owner) as one <see cref="EditForm" /> with Save and Reset
/// (rule WR-002): the workspace name (required, <see cref="AiState.MinWorkspaceNameLength" /> to
/// <see cref="AiState.MaxWorkspaceNameLength" /> characters), the enabled models (at least one) and the default (which must
/// be one of the models that are on: a cross-field rule against the model policy), each member's monthly token quota (a
/// range), attachments and their largest size (required in range while attachments are on: cross-field), retention (a
/// range; the Owner only, read-only for Admins), and blocked terms (added in their own small form and removed in a list,
/// kept in lower case). Messages show after a field is touched or on submit, with a summary linking each field. A retention
/// that would delete conversations now asks first in a <see cref="Modal" /> naming how many. <see cref="AiState.SavePolicy" />'s
/// results are mapped to messages that say what changed; an unsaved-changes line names the sections that differ.
/// </summary>
public partial class AdminSettings : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The Remove buttons of the blocked terms, for focus after a removal.</summary>
    private readonly Dictionary<string, ElementReference> removeButtons = new();

    /// <summary>How many conversations the retention dialog said would be deleted.</summary>
    private int confirmCount;

    /// <summary>The retention the dialog asked about.</summary>
    private int confirmDays;

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private bool focusSaveButton;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (a blocked term's neighbour).</summary>
    private Func<ElementReference?>? focusTarget;

    private SettingsForm form = new();

    /// <summary>Whether the retention dialog was confirmed; the policy is saved once it is hidden.</summary>
    private bool pendingSave;

    private Modal retentionModal = default!;

    private Button? saveButton;

    /// <summary>The state's answers that belong to a field, shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private EditContext termContext;

    private TermForm termForm = new();

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with an empty term form; the settings form is filled from the policy.</summary>
    public AdminSettings() => termContext = new EditContext(termForm);

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is { } target)
        {
            focusTarget = null;
            await AiDisplay.FocusAsync(target());
        }

        if (focusSaveButton)
        {
            focusSaveButton = false;
            await AiDisplay.FocusAsync(saveButton?.Element);
        }

        if (focusStatus)
        {
            focusStatus = false;
            await AiDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        Load();
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

    private static string Number(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string RetentionText(int days) => days == 0 ? "keep forever" : AdminMembers.Plural(days, "day");

    /// <summary>Adds a blocked term to the list once its form is valid (it is saved with the rest of the settings).</summary>
    private void AddTerm()
    {
        termForm.Existing = form.Terms.ToList();

        if (!termContext.Validate())
            return;

        var term = termForm.Term.Trim().ToLowerInvariant();
        form.Terms.Add(term);
        termForm = new TermForm();
        termContext = new EditContext(termForm);
        Announce(true, $"\"{term}\" is added to the blocked terms. Save the settings to apply it.");
    }

    private void Announce(bool ok, string message)
    {
        statusCount = statusMessage?.StartsWith(message, StringComparison.Ordinal) == true ? statusCount + 1 : 1;
        statusIsError = !ok;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Names the sections that differ from the saved policy.</summary>
    private List<string> ChangedSections()
    {
        var policy = State.Policy;
        var list = new List<string>();

        if (form.WorkspaceName.Trim() != policy.WorkspaceName)
            list.Add("workspace name");

        if (!form.EnabledModels.SetEquals(policy.EnabledModelIds))
            list.Add("models");

        if (form.DefaultModelId != policy.DefaultModelId)
            list.Add("default model");

        if (form.Quota != policy.MonthlyTokenQuota)
            list.Add("quota");

        if (form.AllowAttachments != policy.AllowAttachments || (form.AllowAttachments && form.MaxAttachmentMb != policy.MaxAttachmentMb))
            list.Add("attachments");

        if (form.RetentionDays != policy.RetentionDays)
            list.Add("retention");

        if (TermsChanged)
            list.Add("blocked terms");

        return list;
    }

    private async Task ConfirmRetentionAsync()
    {
        if (pendingSave)
            return;

        pendingSave = true;
        await retentionModal.HideAsync();
    }

    /// <summary>Describes what a successful save changed, from the policy before and after.</summary>
    private string DescribeChanges(AiPolicy before, AiPolicy after)
    {
        var parts = new List<string>();

        if (before.WorkspaceName != after.WorkspaceName)
            parts.Add($"workspace name: {after.WorkspaceName}");

        if (!before.EnabledModelIds.Order().SequenceEqual(after.EnabledModelIds.Order()))
            parts.Add("models on: " + string.Join(", ", State.Models.Where(model => after.EnabledModelIds.Contains(model.Id)).Select(model => model.Name)));

        if (before.DefaultModelId != after.DefaultModelId)
            parts.Add($"default model: {State.FindModel(after.DefaultModelId)?.Name ?? after.DefaultModelId}");

        if (before.MonthlyTokenQuota != after.MonthlyTokenQuota)
            parts.Add($"monthly quota: {Number(after.MonthlyTokenQuota)} tokens per member");

        if (before.AllowAttachments != after.AllowAttachments || (after.AllowAttachments && before.MaxAttachmentMb != after.MaxAttachmentMb))
            parts.Add(after.AllowAttachments ? $"attachments on, up to {after.MaxAttachmentMb} MB" : "attachments off");

        if (before.RetentionDays != after.RetentionDays)
            parts.Add($"retention: {RetentionText(after.RetentionDays)}");

        if (!before.BlockedTerms.SequenceEqual(after.BlockedTerms))
            parts.Add($"blocked terms: {after.BlockedTerms.Count}");

        return parts.Count == 0 ? "Saved." : "Saved. Changed " + string.Join("; ", parts) + ".";
    }

    /// <summary>Fills the form from the saved policy and clears the messages.</summary>
    private void Load()
    {
        var policy = State.Policy;

        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = new SettingsForm
        {
            WorkspaceName = policy.WorkspaceName,
            EnabledModels = policy.EnabledModelIds.ToHashSet(StringComparer.Ordinal),
            DefaultModelId = policy.DefaultModelId,
            Quota = policy.MonthlyTokenQuota,
            AllowAttachments = policy.AllowAttachments,
            MaxAttachmentMb = policy.MaxAttachmentMb,
            RetentionDays = policy.RetentionDays,
            Terms = policy.BlockedTerms.ToList(),
            ModelNames = State.Models.ToDictionary(model => model.Id, model => model.Name),
        };
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
        removeButtons.Clear();
        errorMessage = null;
    }

    /// <summary>A change clears the state's earlier answer on that field and the alert; no field changes another, so nothing re-enters.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);
        errorMessage = null;
    }

    /// <summary>Saves once the retention dialog is hidden, or returns focus to Save when it was cancelled.</summary>
    private void OnRetentionHidden()
    {
        if (!pendingSave)
        {
            focusSaveButton = true;

            return;
        }

        pendingSave = false;
        SavePolicy();
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Says how many active members already used more than the quota in the form, when it differs from the saved one.</summary>
    private string? QuotaPreview()
    {
        if (form.Quota is not { } quota || quota is < AiState.MinQuota or > AiState.MaxQuota || quota == State.Policy.MonthlyTokenQuota)
            return null;

        var usage = State.MonthUsageByEmail();
        var over = State.Members.Count(member => member.Status == AiMemberStatus.Active && usage.TryGetValue(member.Email, out var used) && used.Tokens >= quota);

        return over == 0
            ? $"With {Number(quota)}, no active member has used it up this month."
            : $"With {Number(quota)}, {AdminMembers.Plural(over, "active member")} already used it up this month and can't send messages until next month.";
    }

    /// <summary>Removes a blocked term from the list; focus moves to the next term's Remove button, or to the input.</summary>
    private void RemoveTerm(string term)
    {
        var index = form.Terms.IndexOf(term);

        if (index < 0)
            return;

        form.Terms.RemoveAt(index);
        removeButtons.Remove(term);
        var next = index < form.Terms.Count ? form.Terms[index] : index > 0 ? form.Terms[index - 1] : null;
        focusTarget = () => next is not null && removeButtons.TryGetValue(next, out var button) ? button : null;
        Announce(true, $"\"{term}\" is removed from the blocked terms. Save the settings to apply it.");
    }

    private void Reset()
    {
        Load();
        Announce(true, "The form is back to the saved settings.");
        focusStatus = true;
    }

    /// <summary>Says how many conversations a retention in the form would delete now.</summary>
    private string? RetentionPreview()
    {
        if (form.RetentionDays is not { } days || days is <= 0 or > AiState.MaxRetentionDays)
            return null;

        var count = State.ConversationsOlderThan(days);

        return count == 0
            ? $"With {AdminMembers.Plural(days, "day")}, no conversation is old enough to be deleted now."
            : $"With {AdminMembers.Plural(days, "day")}, {AdminMembers.Plural(count, "conversation")} would be deleted when you save.";
    }

    /// <summary>Checks the form (a failed submit shows the summary); when retention would delete conversations now, asks first; otherwise saves.</summary>
    private async Task SaveAsync()
    {
        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();

        if (!editContext.Validate())
            return;

        var days = form.RetentionDays ?? 0;
        var count = days != State.Policy.RetentionDays ? State.ConversationsOlderThan(days) : 0;

        if (days > 0 && count > 0)
        {
            confirmDays = days;
            confirmCount = count;
            pendingSave = false;
            await retentionModal.ShowAsync();

            return;
        }

        SavePolicy();
    }

    /// <summary>Saves the policy from the (valid) form and maps the state's answer to the status message, a field, or the alert.</summary>
    private void SavePolicy()
    {
        if (!editContext.Validate())
            return;

        var before = State.Policy;
        var models = State.Models.Where(model => form.EnabledModels.Contains(model.Id)).Select(model => model.Id).ToList();
        var input = new AiPolicyInput(form.WorkspaceName, models, form.DefaultModelId ?? string.Empty, form.Quota ?? 0, form.AllowAttachments,
            form.AllowAttachments ? form.MaxAttachmentMb ?? 0 : form.MaxAttachmentMb is >= 1 and <= AiState.MaxAttachmentMb ? form.MaxAttachmentMb.Value : before.MaxAttachmentMb,
            form.RetentionDays ?? 0, form.Terms.ToList());

        var result = State.SavePolicy(input, out var deleted);

        switch (result)
        {
            case AiResult.Ok:
                var message = DescribeChanges(before, State.Policy);

                if (deleted > 0)
                    message += $" Retention deleted {AdminMembers.Plural(deleted, "conversation")}.";

                Load();
                Announce(true, message);
                focusStatus = true;

                return;

            case AiResult.NoChange:
                Announce(true, "Nothing changed: these are already the workspace's settings.");
                focusStatus = true;

                return;

            case AiResult.NotAllowed when form.RetentionDays != before.RetentionDays && State.Can(AiPermission.ManageSettings):
                ServerError(() => form.RetentionDays, $"Only the Owner ({State.OwnerName}) changes retention. Put it back to {before.RetentionDays} to save the rest.");

                return;

            case AiResult.NotAllowed:
                ShowError("Only Admins and the Owner change the models and policy. Nothing was saved.");

                return;

            case AiResult.Invalid:
                ShowError("The settings couldn't be saved: a value is out of range. Nothing was saved.");

                return;

            default:
                ShowError("That didn't work, and nothing was saved. Try again.");

                return;
        }
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    private void ShowError(string message)
    {
        errorCount = errorMessage?.StartsWith(message, StringComparison.Ordinal) == true ? errorCount + 1 : 1;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    /// <summary>Turns a model on or off (a click, not a field handler), and checks the models and the default again.</summary>
    private void ToggleModel(string modelId, bool isOn)
    {
        if (isOn)
            form.EnabledModels.Add(modelId);
        else
            form.EnabledModels.Remove(modelId);

        editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.EnabledModels));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private AiState State { get; set; } = default!;

    private bool TermsChanged => !form.Terms.SequenceEqual(State.Policy.BlockedTerms);

    #endregion

    #region Nested Types

    /// <summary>
    /// The settings' fields, with the limits of <see cref="AiState.SavePolicy" />: a required name, ranges, and the cross-field
    /// rules (the default among the models that are on; a file size while attachments are on).
    /// </summary>
    private sealed class SettingsForm : IValidatableObject
    {
        public bool AllowAttachments { get; set; }

        public string? DefaultModelId { get; set; }

        public HashSet<string> EnabledModels { get; set; } = new();

        [Range(1, AiState.MaxAttachmentMb, ErrorMessage = "Use from 1 to 100 MB for the largest file.")]
        public int? MaxAttachmentMb { get; set; }

        /// <summary>The models' names, for messages.</summary>
        public Dictionary<string, string> ModelNames { get; set; } = new();

        [Required(ErrorMessage = "Enter the monthly tokens per member.")]
        [Range(AiState.MinQuota, AiState.MaxQuota, ErrorMessage = "Use from 1,000 to 10,000,000 tokens per member.")]
        public int? Quota { get; set; }

        [Required(ErrorMessage = "Enter the number of days, or 0 to keep conversations forever.")]
        [Range(0, AiState.MaxRetentionDays, ErrorMessage = "Use from 0 to 3,650 days.")]
        public int? RetentionDays { get; set; }

        public List<string> Terms { get; set; } = new();

        [Required(ErrorMessage = "Enter the workspace's name.")]
        [StringLength(AiState.MaxWorkspaceNameLength, MinimumLength = AiState.MinWorkspaceNameLength, ErrorMessage = "Use 2 to 60 characters for the workspace name.")]
        public string WorkspaceName { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: a model on, the default among them, and a file size while attachments are on.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EnabledModels.Count == 0)
                yield return new ValidationResult("Turn on at least one model: members need one to chat.", new[] { nameof(EnabledModels) });
            else if (DefaultModelId is null || !EnabledModels.Contains(DefaultModelId))
                yield return new ValidationResult(
                    $"Choose the default among the models that are on{(DefaultModelId is not null && ModelNames.TryGetValue(DefaultModelId, out var name) ? $": {name} is off" : null)}.",
                    new[] { nameof(DefaultModelId) });

            if (AllowAttachments && MaxAttachmentMb is null)
                yield return new ValidationResult("Enter the largest file size in MB while attachments are on, like 10.", new[] { nameof(MaxAttachmentMb) });
        }
    }

    /// <summary>A blocked term to add: required, up to 40 characters, not already listed, and room for it (rules against the list).</summary>
    private sealed class TermForm : IValidatableObject
    {
        /// <summary>The terms already listed, set by the page before it validates.</summary>
        public List<string> Existing { get; set; } = new();

        [Required(ErrorMessage = "Enter a word or phrase to block.")]
        [StringLength(AiState.MaxBlockedTermLength, ErrorMessage = "Use 40 characters or fewer for the term.")]
        public string Term { get; set; } = string.Empty;

        /// <summary>Runs once the term is filled in: not listed already, and room for it.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var term = Term.Trim().ToLowerInvariant();

            if (Existing.Contains(term))
                yield return new ValidationResult($"\"{term}\" is already blocked.", new[] { nameof(Term) });
            else if (Existing.Count >= AiState.MaxBlockedTerms)
                yield return new ValidationResult($"There are already {AiState.MaxBlockedTerms} blocked terms, the most allowed. Remove one first.", new[] { nameof(Term) });
        }
    }

    #endregion
}
