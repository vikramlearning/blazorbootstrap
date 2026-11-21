using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// The prompt library: each member's own prompts, and the shared prompts that prompt editors, admins, and the Owner write,
/// publish, and delete (<see cref="AiPermission.ManageSharedPrompts" />). Members see only published shared prompts. A
/// title is unique within its scope (a member's own, or the shared library), and a prompt's variables agree with its text
/// (<see cref="AiPromptText" />). Shared prompt changes are audited (personal prompts are private and aren't); the author
/// hears when someone else changes their prompt, and every member hears when one is published.
/// </summary>
public sealed partial class AiState
{
    #region Methods

    #region Public Methods

    /// <summary>
    /// Deletes several prompts at once, each like <see cref="DeletePrompt" />: the signed-in member's own, and (with the
    /// right) shared ones. Prompts deleted meanwhile, or not theirs to delete, are counted as unchanged.
    /// </summary>
    public AiBulkOutcome BulkDeletePrompts(IReadOnlyCollection<int> promptIds) => BulkPrompts(promptIds, DeletePrompt, ActiveAccount() is not null);

    /// <summary>
    /// Publishes or unpublishes several shared prompts at once (prompt editors, admins, and the Owner). Members hear once
    /// about the prompts published, not once per prompt.
    /// </summary>
    public AiBulkOutcome BulkSetPublished(IReadOnlyCollection<int> promptIds, bool publish)
    {
        var published = new List<AiPrompt>();
        var outcome = BulkPrompts(promptIds, id =>
        {
            var result = SetPublished(id, publish, notifyMembers: false);

            if (result == AiResult.Ok && publish && FindPrompt(id) is { } prompt)
                published.Add(prompt);

            return result;
        }, Can(AiPermission.ManageSharedPrompts));

        if (published.Count == 1)
            NotifyPublished(published[0]);
        else if (published.Count > 1)
            NotifyMembers($"{published.Count} new shared prompts", $"{ActorName()} published {string.Join(", ", published.Take(3).Select(prompt => $"\"{prompt.Title}\""))}{(published.Count > 3 ? ", and more" : null)}.",
                AiSeedData.SiteBase + "/prompts");

        return outcome;
    }

    /// <summary>Deletes one of the member's own prompts, or (prompt editors, admins, and the Owner) a shared prompt.</summary>
    public AiResult DeletePrompt(int promptId)
    {
        if (ActiveAccount() is not { } account)
            return AiResult.NotAllowed;

        if (prompts.FirstOrDefault(prompt => prompt.Id == promptId) is not { } prompt)
            return AiResult.NotFound;

        if (prompt.IsShared ? !Can(AiPermission.ManageSharedPrompts) : prompt.OwnerId != account.Id)
            return prompt.IsShared && prompt.IsPublished ? AiResult.NotAllowed : AiResult.NotFound;

        prompts.Remove(prompt);

        if (prompt.IsShared)
        {
            Record("Deleted shared prompt", SharedTarget(prompt), null, $"{(prompt.IsPublished ? "Was published" : "Was a draft")}; used {prompt.UseCount} times");
            NotifyAuthor(prompt, $"{ActorName()} deleted your shared prompt", $"\"{prompt.Title}\" is no longer in the library.", AiSeedData.AdminBase + "/prompts");
        }

        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Returns a prompt the signed-in member can see, or <see langword="null" />: their own, a published shared one, or (prompt
    /// editors, admins, and the Owner) any shared one, drafts included.
    /// </summary>
    public AiPrompt? FindPrompt(int promptId) =>
        Identity.Current is { } account
            ? prompts.FirstOrDefault(prompt => prompt.Id == promptId && (prompt.IsShared ? prompt.IsPublished || Can(AiPermission.ManageSharedPrompts) : prompt.OwnerId == account.Id))
            : null;

    /// <summary>Returns whether a prompt title is taken in its scope (the member's own prompts, or the shared library), ignoring case and another prompt being edited.</summary>
    public bool IsPromptTitleTaken(string? title, bool shared, int exceptPromptId = 0)
    {
        var value = title?.Trim() ?? string.Empty;
        var accountId = Identity.Current?.Id;

        return value.Length > 0 && prompts.Any(prompt => prompt.Id != exceptPromptId && (shared ? prompt.IsShared : !prompt.IsShared && prompt.OwnerId == accountId)
            && string.Equals(prompt.Title, value, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Creates (<paramref name="promptId" /> 0) or edits a saved prompt: a member's own, or with <paramref name="shared" /> a
    /// workspace prompt (prompt editors, admins, and the Owner), published or kept as a draft. Checks the title (unique in its
    /// scope: Duplicate), the text, the category, the variables against the text, and the tags (lower-case words; at most
    /// <see cref="MaxTags" />).
    /// </summary>
    public AiResult SavePrompt(int promptId, AiPromptInput input, bool shared, out AiPrompt? saved)
    {
        saved = null;

        if (ActiveAccount() is not { } account || input is null)
            return AiResult.NotAllowed;

        if (shared && !Can(AiPermission.ManageSharedPrompts))
            return AiResult.NotAllowed;

        var existing = promptId == 0 ? null : prompts.FirstOrDefault(prompt => prompt.Id == promptId);

        if (promptId != 0 && (existing is null || (existing.IsShared ? !shared : existing.OwnerId != account.Id || shared)))
            return existing is { IsShared: true } && !Can(AiPermission.ManageSharedPrompts) ? AiResult.NotAllowed : AiResult.NotFound;

        var title = input.Title?.Trim() ?? string.Empty;
        var text = input.Text?.Trim() ?? string.Empty;
        var tags = (input.Tags ?? Array.Empty<string>()).Select(tag => tag?.Trim().ToLowerInvariant() ?? string.Empty).Where(tag => tag.Length > 0).Distinct().ToList();
        var variables = (input.Variables ?? Array.Empty<string>()).Select(name => name?.Trim().ToLowerInvariant() ?? string.Empty).Where(name => name.Length > 0).Distinct().ToList();
        var publish = shared && input.Publish;

        if (!IsText(title, MaxPromptTitleLength) || !IsText(text, MaxPromptTextLength) || text.Length < MinPromptTextLength || !Enum.IsDefined(input.Category)
            || tags.Count > MaxTags || tags.Any(tag => tag.Length > MaxTagLength || !TagPattern().IsMatch(tag)) || !AiPromptText.IsConsistent(text, variables))
            return AiResult.Invalid;

        if (IsPromptTitleTaken(title, shared, existing?.Id ?? 0))
            return AiResult.Duplicate;

        if (existing is null)
        {
            if (prompts.Count >= MaxPrompts || nextPromptId >= NextIdLimit)
                return AiResult.Invalid;

            saved = new AiPrompt
            {
                Id = nextPromptId++, OwnerId = shared ? null : account.Id, AuthorId = account.Id, Author = Clip(account.Name, ShowcaseIdentity.MaxNameLength), Title = title, Text = text,
                Category = input.Category, Variables = variables, Tags = tags, IsPublished = publish, UpdatedAt = Clock.Now,
            };
            prompts.Add(saved);

            if (!FitsSnapshot())
            {
                prompts.Remove(saved);
                saved = null;

                return AiResult.Invalid;
            }

            if (shared)
            {
                Record(publish ? "Published shared prompt" : "Created shared prompt (draft)", SharedTarget(saved), AiSeedData.SharedPromptPath(saved.Id), PromptFacts(saved));

                if (publish)
                    NotifyPublished(saved);
            }

            RaiseChanged();

            return AiResult.Ok;
        }

        if (existing.Title == title && existing.Text == text && existing.Category == input.Category && existing.Variables.SequenceEqual(variables) && existing.Tags.SequenceEqual(tags)
            && existing.IsPublished == publish)
        {
            saved = existing;

            return AiResult.NoChange;
        }

        var before = (existing.Title, existing.Text, existing.Category, existing.Variables, existing.Tags, existing.IsPublished, existing.UpdatedAt);
        var changes = PromptChanges(existing, title, text, input.Category, variables, tags);
        (existing.Title, existing.Text, existing.Category, existing.Variables, existing.Tags, existing.IsPublished, existing.UpdatedAt) = (title, text, input.Category, variables, tags, publish, Clock.Now);

        if (!FitsSnapshot())
        {
            (existing.Title, existing.Text, existing.Category, existing.Variables, existing.Tags, existing.IsPublished, existing.UpdatedAt) = before;

            return AiResult.Invalid;
        }

        if (shared)
        {
            var link = AiSeedData.SharedPromptPath(existing.Id);

            if (changes.Count > 0)
                Record("Edited shared prompt", SharedTarget(existing), link, string.Join("; ", changes));

            if (before.IsPublished != publish)
                Record(publish ? "Published shared prompt" : "Unpublished shared prompt", SharedTarget(existing), link, publish ? "Draft → Published" : "Published → Draft");

            if (changes.Count > 0)
                NotifyAuthor(existing, $"{ActorName()} edited your shared prompt", $"\"{existing.Title}\" changed: {string.Join("; ", changes)}.", link);

            if (publish && !before.IsPublished)
                NotifyPublished(existing);
        }

        saved = existing;
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Publishes a shared prompt to every member, or takes it back to a draft only editors see (prompt editors, admins, and the
    /// Owner). Every member hears about a published prompt.
    /// </summary>
    public AiResult SetPublished(int promptId, bool publish) => SetPublished(promptId, publish, notifyMembers: true);

    #endregion

    #region Private Methods

    /// <summary>What a prompt holds, for the audit log: never its text, only its shape.</summary>
    private static string PromptFacts(AiPrompt prompt) =>
        FormattableString.Invariant($"{prompt.Category} · {prompt.Variables.Count} variables · {prompt.Text.Length} characters");

    /// <summary>What an edit changes, in words (the text by its length only).</summary>
    private static List<string> PromptChanges(AiPrompt prompt, string title, string text, AiPromptCategory category, List<string> variables, List<string> tags)
    {
        var changes = new List<string>();

        if (prompt.Title != title)
            changes.Add($"title \"{prompt.Title}\" → \"{title}\"");

        if (prompt.Text != text)
            changes.Add(FormattableString.Invariant($"text {prompt.Text.Length} → {text.Length} characters"));

        if (prompt.Category != category)
            changes.Add($"category {prompt.Category} → {category}");

        if (!prompt.Variables.SequenceEqual(variables))
            changes.Add($"variables {(variables.Count == 0 ? "none" : string.Join(", ", variables))}");

        if (!prompt.Tags.SequenceEqual(tags))
            changes.Add($"tags {(tags.Count == 0 ? "none" : string.Join(", ", tags))}");

        return changes;
    }

    /// <summary>A shared prompt as the audit log names it.</summary>
    private static string SharedTarget(AiPrompt prompt) => $"Shared prompt \"{prompt.Title}\"";

    /// <summary>Runs one prompt change on several prompts, raising <see cref="Changed" /> once; NotAllowed without the right, Invalid for an empty list.</summary>
    private AiBulkOutcome BulkPrompts(IReadOnlyCollection<int> promptIds, Func<int, AiResult> change, bool allowed)
    {
        if (!allowed)
            return new AiBulkOutcome(AiResult.NotAllowed, 0, 0, 0);

        var list = (promptIds ?? Array.Empty<int>()).Distinct().ToList();

        if (list.Count == 0)
            return new AiBulkOutcome(AiResult.Invalid, 0, 0, 0);

        int changed = 0, unchanged = 0;
        batchDepth++;

        try
        {
            foreach (var id in list)
            {
                if (change(id) == AiResult.Ok)
                    changed++;
                else
                    unchanged++;
            }
        }
        finally
        {
            batchDepth--;
        }

        FlushBatch();

        return new AiBulkOutcome(changed == 0 ? AiResult.NoChange : AiResult.Ok, changed, unchanged, 0);
    }

    /// <summary>Tells a shared prompt's author, when they can sign in and didn't make the change themselves.</summary>
    private void NotifyAuthor(AiPrompt prompt, string title, string text, string? link) => Notify(prompt.AuthorId, title, text, link);

    /// <summary>Tells every active member who can sign in (except whoever caused it).</summary>
    private void NotifyMembers(string title, string text, string? link)
    {
        foreach (var member in members.Where(member => member.Status == AiMemberStatus.Active).ToList())
            Notify(member.AccountId, title, text, link);
    }

    /// <summary>Tells every active member that a shared prompt is published.</summary>
    private void NotifyPublished(AiPrompt prompt) =>
        NotifyMembers($"New shared prompt: \"{prompt.Title}\"", $"Published by {ActorName()}. Use it from the prompt library.", AiSeedData.PromptPath(prompt.Id));

    private AiResult SetPublished(int promptId, bool publish, bool notifyMembers)
    {
        if (!Can(AiPermission.ManageSharedPrompts))
            return AiResult.NotAllowed;

        if (prompts.FirstOrDefault(prompt => prompt.Id == promptId && prompt.IsShared) is not { } prompt)
            return AiResult.NotFound;

        if (prompt.IsPublished == publish)
            return AiResult.NoChange;

        prompt.IsPublished = publish;
        prompt.UpdatedAt = Clock.Now;
        Record(publish ? "Published shared prompt" : "Unpublished shared prompt", SharedTarget(prompt), AiSeedData.SharedPromptPath(prompt.Id), publish ? "Draft → Published" : "Published → Draft");

        if (publish && notifyMembers)
            NotifyPublished(prompt);

        RaiseChanged();

        return AiResult.Ok;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets every shared prompt, drafts included (prompt editors, admins, and the Owner; empty for anyone else), the most recently changed first.</summary>
    public IReadOnlyList<AiPrompt> AllSharedPrompts => Can(AiPermission.ManageSharedPrompts)
        ? prompts.Where(prompt => prompt.IsShared).OrderByDescending(prompt => prompt.UpdatedAt).ToList()
        : Array.Empty<AiPrompt>();

    /// <summary>Gets the signed-in member's own prompts, the most recently changed first.</summary>
    public IReadOnlyList<AiPrompt> MyPrompts =>
        Identity.Current is { } account ? prompts.Where(prompt => prompt.OwnerId == account.Id).OrderByDescending(prompt => prompt.UpdatedAt).ToList() : Array.Empty<AiPrompt>();

    /// <summary>Gets how many prompts the tab holds, personal and shared together (at most <see cref="MaxPrompts" />).</summary>
    public int PromptCount => prompts.Count;

    /// <summary>Gets the published shared prompts every member sees, the most used first.</summary>
    public IReadOnlyList<AiPrompt> SharedPrompts => Identity.Current is null
        ? Array.Empty<AiPrompt>()
        : prompts.Where(prompt => prompt.IsShared && prompt.IsPublished).OrderByDescending(prompt => prompt.UseCount).ThenBy(prompt => prompt.Title, StringComparer.OrdinalIgnoreCase).ToList();

    #endregion
}
