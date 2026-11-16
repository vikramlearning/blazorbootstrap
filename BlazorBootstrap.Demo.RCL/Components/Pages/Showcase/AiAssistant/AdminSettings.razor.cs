using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Models and policy of the Parley workspace admin back office, as one form with Save and Reset: the enabled models (at least
/// one) and the default among them, each member's monthly token quota, attachments and their largest size, retention (0
/// keeps conversations forever), and blocked terms (added and removed in a list, kept in lower case). Native inputs with
/// labels; each problem is shown under its field and the first invalid field takes focus after the render that shows it.
/// A retention that would delete conversations now asks first in a <see cref="Modal" /> naming how many, and saves once the
/// dialog is hidden. <see cref="AiState.SavePolicy" />'s results are mapped to messages that say what changed and how many
/// conversations retention deleted; an unsaved-changes line names the sections that differ from the saved policy.
/// </summary>
public partial class AdminSettings : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The default-model radio buttons by model ID, for focus.</summary>
    private readonly Dictionary<string, ElementReference> defaultInputs = new();

    /// <summary>The models that are on in the form.</summary>
    private readonly HashSet<string> enabled = new();

    /// <summary>The model checkboxes by model ID, for focus.</summary>
    private readonly Dictionary<string, ElementReference> modelInputs = new();

    /// <summary>The Remove buttons of the blocked terms, for focus after a removal.</summary>
    private readonly Dictionary<string, ElementReference> removeButtons = new();

    /// <summary>The blocked terms in the form, in the order they were added.</summary>
    private readonly List<string> terms = new();

    private bool allowAttachments;

    /// <summary>How many conversations the retention dialog said would be deleted.</summary>
    private int confirmCount;

    /// <summary>The retention the dialog asked about.</summary>
    private int confirmDays;

    private string? defaultError;

    private string? defaultModelId;

    private bool focusSaveButton;

    private bool focusStatus;

    /// <summary>Returns the element to focus after the next render (the first invalid field, or a blocked term's neighbour).</summary>
    private Func<ElementReference?>? focusTarget;

    private string? maxMbError;

    private ElementReference? maxMbInput;

    private string maxMbText = string.Empty;

    private string? modelsError;

    private string newTerm = string.Empty;

    /// <summary>Whether the retention dialog was confirmed; the policy is saved once it is hidden.</summary>
    private bool pendingSave;

    private string? quotaError;

    private ElementReference? quotaInput;

    private string quotaText = string.Empty;

    private Modal retentionModal = default!;

    private string? retentionError;

    private ElementReference? retentionInput;

    private string retentionText = string.Empty;

    private Button? saveButton;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? termError;

    private ElementReference? termInput;

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

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string Number(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static bool TryInt(string value, out int result) => int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

    private static string RetentionText(int days) => days == 0 ? "keep forever" : AdminMembers.Plural(days, "day");

    /// <summary>Adds a blocked term to the list (it is saved with the rest of the form).</summary>
    private void AddTerm()
    {
        var term = newTerm.Trim().ToLowerInvariant();

        termError = term.Length == 0 ? "Enter a word or phrase to block."
            : term.Length > AiState.MaxBlockedTermLength ? $"Shorten the term to {AiState.MaxBlockedTermLength} characters or fewer (now {term.Length})."
            : terms.Contains(term) ? $"\"{term}\" is already blocked."
            : terms.Count >= AiState.MaxBlockedTerms ? $"There are already {AiState.MaxBlockedTerms} blocked terms, the most allowed. Remove one first."
            : null;

        if (termError is not null)
        {
            focusTarget = () => termInput;

            return;
        }

        terms.Add(term);
        newTerm = string.Empty;
        Announce(true, $"\"{term}\" is added to the blocked terms. Save the settings to apply it.");
    }

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Names the sections that differ from the saved policy (enabled models, default, quota, attachments, retention, blocked terms).</summary>
    private List<string> ChangedSections()
    {
        var policy = State.Policy;
        var list = new List<string>();

        if (!enabled.SetEquals(policy.EnabledModelIds))
            list.Add("models");

        if (defaultModelId != policy.DefaultModelId)
            list.Add("default model");

        if (!TryInt(quotaText, out var quota) || quota != policy.MonthlyTokenQuota)
            list.Add("quota");

        if (allowAttachments != policy.AllowAttachments || !TryInt(maxMbText, out var size) || size != policy.MaxAttachmentMb)
            list.Add("attachments");

        if (!TryInt(retentionText, out var days) || days != policy.RetentionDays)
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

        if (!before.EnabledModelIds.Order().SequenceEqual(after.EnabledModelIds.Order()))
            parts.Add("models on: " + string.Join(", ", State.Models.Where(model => after.EnabledModelIds.Contains(model.Id)).Select(model => model.Name)));

        if (before.DefaultModelId != after.DefaultModelId)
            parts.Add($"default model: {State.FindModel(after.DefaultModelId)?.Name ?? after.DefaultModelId}");

        if (before.MonthlyTokenQuota != after.MonthlyTokenQuota)
            parts.Add($"monthly quota: {Number(after.MonthlyTokenQuota)} tokens per member");

        if (before.AllowAttachments != after.AllowAttachments || (after.AllowAttachments && before.MaxAttachmentMb != after.MaxAttachmentMb))
            parts.Add(after.AllowAttachments ? $"attachments on, up to {after.MaxAttachmentMb} MB" : "attachments off");
        else if (before.MaxAttachmentMb != after.MaxAttachmentMb)
            parts.Add($"largest attachment: {after.MaxAttachmentMb} MB (attachments stay off)");

        if (before.RetentionDays != after.RetentionDays)
            parts.Add($"retention: {RetentionText(after.RetentionDays)}");

        if (!before.BlockedTerms.SequenceEqual(after.BlockedTerms))
            parts.Add($"blocked terms: {after.BlockedTerms.Count}");

        return parts.Count == 0 ? "Saved." : "Saved. Changed " + string.Join("; ", parts) + ".";
    }

    /// <summary>Fills the form from the saved policy and clears the errors.</summary>
    private void Load()
    {
        var policy = State.Policy;

        enabled.Clear();
        enabled.UnionWith(policy.EnabledModelIds);
        defaultModelId = policy.DefaultModelId;
        quotaText = policy.MonthlyTokenQuota.ToString(CultureInfo.InvariantCulture);
        allowAttachments = policy.AllowAttachments;
        maxMbText = policy.MaxAttachmentMb.ToString(CultureInfo.InvariantCulture);
        retentionText = policy.RetentionDays.ToString(CultureInfo.InvariantCulture);
        terms.Clear();
        terms.AddRange(policy.BlockedTerms);
        removeButtons.Clear();
        newTerm = string.Empty;
        modelsError = defaultError = quotaError = maxMbError = retentionError = termError = null;
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
        if (!TryInt(quotaText, out var quota) || quota is < AiState.MinQuota or > AiState.MaxQuota || quota == State.Policy.MonthlyTokenQuota)
            return null;

        var over = State.Members.Count(member => member.Status == AiMemberStatus.Active && State.MonthTokens(member.Email) >= quota);

        return over == 0
            ? $"With {Number(quota)}, no active member has used it up this month."
            : $"With {Number(quota)}, {AdminMembers.Plural(over, "active member")} already used it up this month and can't send messages until next month.";
    }

    /// <summary>Removes a blocked term from the list; focus moves to the next term's Remove button, or to the input.</summary>
    private void RemoveTerm(string term)
    {
        var index = terms.IndexOf(term);

        if (index < 0)
            return;

        terms.RemoveAt(index);
        removeButtons.Remove(term);
        var next = index < terms.Count ? terms[index] : index > 0 ? terms[index - 1] : null;
        focusTarget = () => next is not null && removeButtons.TryGetValue(next, out var button) ? button : termInput;
        Announce(true, $"\"{term}\" is removed from the blocked terms. Save the settings to apply it.");
    }

    private void Reset()
    {
        Load();
        Announce(true, "The form is back to the saved settings.");
    }

    /// <summary>Says how many conversations a retention in the form would delete now.</summary>
    private string? RetentionPreview()
    {
        if (!TryInt(retentionText, out var days) || days is <= 0 or > AiState.MaxRetentionDays)
            return null;

        var count = State.ConversationsOlderThan(days);

        return count == 0
            ? $"With {AdminMembers.Plural(days, "day")}, no conversation is old enough to be deleted now."
            : $"With {AdminMembers.Plural(days, "day")}, {AdminMembers.Plural(count, "conversation")} would be deleted when you save.";
    }

    /// <summary>Checks the form; when retention would delete conversations now, asks first; otherwise saves.</summary>
    private async Task SaveAsync()
    {
        if (!Validate())
        {
            Announce(false, "Fix the highlighted fields, then save again.");

            return;
        }

        TryInt(retentionText, out var days);
        var count = State.ConversationsOlderThan(days);

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

    /// <summary>Saves the policy from the (valid) form and maps the state's answer to the status message or a field.</summary>
    private void SavePolicy()
    {
        if (!Validate())
        {
            Announce(false, "Fix the highlighted fields, then save again.");

            return;
        }

        var before = State.Policy;
        TryInt(quotaText, out var quota);
        TryInt(retentionText, out var days);
        var size = TryInt(maxMbText, out var parsed) && parsed is >= 1 and <= 100 ? parsed : before.MaxAttachmentMb;
        var models = State.Models.Where(model => enabled.Contains(model.Id)).Select(model => model.Id).ToList();

        var result = State.SavePolicy(models, defaultModelId ?? string.Empty, quota, allowAttachments, size, days, terms.ToList(), out var deleted);

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

            case AiResult.Invalid:
                if (!Validate())
                {
                    Announce(false, "Fix the highlighted fields, then save again. Nothing was saved.");

                    return;
                }

                Announce(false, "The settings couldn't be saved: a value is out of range. Nothing was saved.");
                focusStatus = true;

                return;

            case AiResult.NotAllowed:
                Announce(false, "Only active workspace admins can change the models and policy. Nothing was saved.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nothing was saved. Try again.");
                focusStatus = true;

                return;
        }
    }

    private void SetDefault(string modelId)
    {
        defaultModelId = modelId;
        defaultError = null;
    }

    private void ToggleModel(string modelId, bool isOn)
    {
        if (isOn)
            enabled.Add(modelId);
        else
            enabled.Remove(modelId);

        if (enabled.Count > 0)
            modelsError = null;
    }

    /// <summary>Checks every field like <see cref="AiState.SavePolicy" />; sets the errors and the field to focus. Returns whether all are valid.</summary>
    private bool Validate()
    {
        modelsError = enabled.Count == 0 ? "Turn on at least one model: members need one to chat." : null;
        defaultError = enabled.Count > 0 && (defaultModelId is null || !enabled.Contains(defaultModelId))
            ? $"Choose the default among the models that are on{(defaultModelId is not null && State.FindModel(defaultModelId) is { } off ? $": {off.Name} is off" : null)}."
            : null;

        quotaError = !TryInt(quotaText, out var quota) ? $"Enter the monthly tokens per member as a whole number, like {Number(200_000)}."
            : quota is < AiState.MinQuota or > AiState.MaxQuota ? $"Use from {Number(AiState.MinQuota)} to {Number(AiState.MaxQuota)} tokens."
            : null;

        maxMbError = !allowAttachments ? null
            : !TryInt(maxMbText, out var size) ? "Enter the largest file size in MB as a whole number, like 10."
            : size is < 1 or > 100 ? "Use from 1 to 100 MB."
            : null;

        retentionError = !TryInt(retentionText, out var days) ? "Enter the number of days as a whole number, or 0 to keep conversations forever."
            : days is < 0 or > AiState.MaxRetentionDays ? $"Use from 0 to {Number(AiState.MaxRetentionDays)} days."
            : null;

        var firstOn = State.Models.FirstOrDefault(model => enabled.Contains(model.Id))?.Id;
        var firstModel = State.Models[0].Id;

        focusTarget = modelsError is not null ? () => modelInputs.TryGetValue(firstModel, out var input) ? input : null
            : defaultError is not null ? () => firstOn is not null && defaultInputs.TryGetValue(firstOn, out var input) ? input : null
            : quotaError is not null ? () => quotaInput
            : maxMbError is not null ? () => maxMbInput
            : retentionError is not null ? () => retentionInput
            : null;

        return focusTarget is null;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private AiState State { get; set; } = default!;

    private bool TermsChanged => !terms.SequenceEqual(State.Policy.BlockedTerms);

    #endregion
}
