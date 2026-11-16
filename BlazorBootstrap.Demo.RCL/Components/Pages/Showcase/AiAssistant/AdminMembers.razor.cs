using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Members and roles of the Parley workspace admin back office: an invite form (email, name, role; <see cref="AiState.Invite" />)
/// and a table of members and invitations (name, email, role, status in words, joined, this month's tokens) with a filter by
/// status. Each row offers Change role (active members and invitations), Deactivate or Reactivate, or Remove invitation; each
/// opens a <see cref="Modal" /> that names the person and the consequence, and the change runs once the dialog is hidden.
/// Demoting yourself warns that you'll lose access to Workspace admin. The state's refusals (last active admin, deactivating
/// yourself, an invitation's status) are mapped to specific messages in an always-rendered status region; focus returns to
/// the pressed button, or to the status message when the button is gone.
/// </summary>
public partial class AdminMembers : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The row buttons by <see cref="ButtonKey" />, for focus after a dialog.</summary>
    private readonly Dictionary<string, Button> actionButtons = new();

    /// <summary>The member the dialog is about (by email).</summary>
    private string? dialogEmail;

    private DialogKind dialogKind;

    private Modal dialogModal = default!;

    /// <summary>The role chosen in the change-role dialog.</summary>
    private AiMemberRole dialogRole;

    private string? emailError;

    private ElementReference? emailInput;

    /// <summary>The status filter, or <see langword="null" /> for everyone.</summary>
    private AiMemberStatus? filter;

    /// <summary>The button key to focus after the next render (when its button is still there; otherwise the status message).</summary>
    private string? focusButtonKey;

    private bool focusStatus;

    /// <summary>Returns the field to focus after the next render (the first invalid one).</summary>
    private Func<ElementReference?>? focusTarget;

    private string inviteEmail = string.Empty;

    private string inviteName = string.Empty;

    private AiMemberRole inviteRole = AiMemberRole.Member;

    private string? nameError;

    private ElementReference? nameInput;

    /// <summary>The confirmed change, run once the dialog is hidden.</summary>
    private Action? pendingAction;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

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

        if (focusButtonKey is { } key)
        {
            focusButtonKey = null;

            if (HasButton(key) && actionButtons.TryGetValue(key, out var button))
                await AiDisplay.FocusAsync(button.Element);
            else
                await AiDisplay.FocusAsync(statusRegion);
        }

        if (focusStatus)
        {
            focusStatus = false;
            await AiDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Formats a count with its noun, like "1 member" or "3 members" (or the given plural).</summary>
    internal static string Plural(int count, string noun, string? plural = null) =>
        count == 1 ? $"1 {noun}" : string.Create(CultureInfo.InvariantCulture, $"{count} {plural ?? noun + "s"}");

    #endregion

    #region Private Methods

    private static string ButtonKey(DialogKind kind, string email) => $"{kind}-{email}";

    [GeneratedRegex(@"^[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}$")]
    private static partial Regex EmailPattern();

    private static string RoleSummary(AiMemberRole role) => role == AiMemberRole.Admin
        ? "Chats, and also manages members, shared prompts, usage, and the model and policy settings."
        : "Chats with the assistant, keeps their own prompts, and follows their usage.";

    private static BadgeColor StatusColor(AiMemberStatus status) => status switch
    {
        AiMemberStatus.Active => BadgeColor.Success,
        AiMemberStatus.Invited => BadgeColor.Info,
        _ => BadgeColor.Secondary,
    };

    private static IconName StatusIcon(AiMemberStatus status) => status switch
    {
        AiMemberStatus.Active => IconName.CheckCircle,
        AiMemberStatus.Invited => IconName.Envelope,
        _ => IconName.SlashCircle,
    };

    private void Announce(bool ok, string message)
    {
        statusIsError = !ok;
        statusMessage = message;
    }

    /// <summary>Changes a member's role (or an invitation's) and says what happened.</summary>
    private void ApplyRole(string email, AiMemberRole role)
    {
        if (State.FindMember(email) is not { } member)
        {
            Announce(false, "That person is no longer in the workspace, so nothing changed.");

            return;
        }

        var wasMe = IsMe(member);
        var result = State.SetMember(email, role, member.Status);
        var roleText = AiDisplay.RoleLabel(role).ToLowerInvariant();

        Announce(result is AiResult.Ok or AiResult.NoChange, result switch
        {
            AiResult.Ok when member.Status == AiMemberStatus.Invited => $"{member.Name}'s invitation is now for a {roleText}: they join as one when they create an account.",
            AiResult.Ok when wasMe && role == AiMemberRole.Member => "You're a member now and no longer a workspace admin.",
            AiResult.Ok => role == AiMemberRole.Admin
                ? $"{member.Name} is now a workspace admin: they can manage members, shared prompts, and the model and policy settings."
                : $"{member.Name} is now a member and can no longer open Workspace admin.",
            AiResult.NoChange => $"{member.Name} is already a {roleText}, so nothing changed.",
            AiResult.WrongState => member.Status == AiMemberStatus.Invited
                ? $"{member.Name}'s invitation can't change that way: its status changes only when they create an account. Nothing changed."
                : LastAdminMessage(member),
            _ => OtherMessage(result, member),
        });
    }

    /// <summary>Deactivates or reactivates a member and says what happened.</summary>
    private void ApplyStatus(string email, AiMemberStatus status)
    {
        if (State.FindMember(email) is not { } member)
        {
            Announce(false, "That person is no longer in the workspace, so nothing changed.");

            return;
        }

        var result = State.SetMember(email, member.Role, status);

        Announce(result is AiResult.Ok or AiResult.NoChange, result switch
        {
            AiResult.Ok when status == AiMemberStatus.Deactivated => $"{member.Name} is deactivated: they can't sign in to chat. Their conversations are kept, and you can reactivate them here.",
            AiResult.Ok => $"{member.Name} is active again and can sign in to chat as a {AiDisplay.RoleLabel(member.Role).ToLowerInvariant()}.",
            AiResult.NoChange => status == AiMemberStatus.Deactivated ? $"{member.Name} is already deactivated, so nothing changed." : $"{member.Name} is already active, so nothing changed.",
            AiResult.WrongState when member.Status == AiMemberStatus.Invited =>
                $"{member.Name} hasn't joined yet, so there's nothing to deactivate: an invitation becomes active only when they create an account. Remove the invitation instead. Nothing changed.",
            AiResult.WrongState when IsMe(member) => "You can't deactivate yourself. Ask another workspace admin to do it. Nothing changed.",
            AiResult.WrongState => LastAdminMessage(member),
            _ => OtherMessage(result, member),
        });
    }

    /// <summary>Removes an invitation and says what happened.</summary>
    private void ApplyRemoveInvitation(string email)
    {
        var member = State.FindMember(email);
        var name = member?.Name ?? email;
        var result = State.RemoveInvitation(email);

        Announce(result == AiResult.Ok, result switch
        {
            AiResult.Ok => $"The invitation for {name} ({email}) is removed. They can no longer join with it.",
            AiResult.WrongState => $"{name} already joined, so there's no invitation to remove. Deactivate them instead if they shouldn't use {AiSeedData.ProductName}. Nothing changed.",
            AiResult.NotFound => $"The invitation for {email} was already removed, so nothing changed.",
            _ => OtherMessage(result, member),
        });
    }

    private async Task ConfirmAsync()
    {
        if (pendingAction is not null || dialogEmail is not { } email)
            return;

        var role = dialogRole;
        pendingAction = dialogKind switch
        {
            DialogKind.Role => () => ApplyRole(email, role),
            DialogKind.Deactivate => () => ApplyStatus(email, AiMemberStatus.Deactivated),
            DialogKind.Reactivate => () => ApplyStatus(email, AiMemberStatus.Active),
            _ => () => ApplyRemoveInvitation(email),
        };

        await dialogModal.HideAsync();
    }

    /// <summary>Whether the button with that key is rendered now: the row is shown (filter) and offers that action.</summary>
    private bool HasButton(string key)
    {
        if (dialogEmail is not { } email || State.FindMember(email) is not { } member || (filter is { } status && member.Status != status))
            return false;

        return key == ButtonKey(DialogKind.Role, email) ? member.Status != AiMemberStatus.Deactivated
            : key == ButtonKey(DialogKind.Deactivate, email) ? member.Status == AiMemberStatus.Active && !IsMe(member)
            : key == ButtonKey(DialogKind.Reactivate, email) ? member.Status == AiMemberStatus.Deactivated
            : key == ButtonKey(DialogKind.RemoveInvitation, email) && member.Status == AiMemberStatus.Invited;
    }

    /// <summary>Checks the invite form, invites, and maps the state's answer to a field or the status message.</summary>
    private void Invite()
    {
        var email = inviteEmail.Trim().ToLowerInvariant();
        var name = inviteName.Trim();

        emailError = email.Length == 0 ? "Enter the email address of the person to invite."
            : email.Length > ShowcaseIdentity.MaxEmailLength ? $"Shorten the email address to {ShowcaseIdentity.MaxEmailLength} characters or fewer."
            : !EmailPattern().IsMatch(email) ? "Enter an email address like name@example.com."
            : State.FindMember(email) is { } existing ? DuplicateText(existing)
            : null;
        nameError = name.Length == 0 ? "Enter their name, as members will see it."
            : name.Length > ShowcaseIdentity.MaxNameLength ? $"Shorten the name to {ShowcaseIdentity.MaxNameLength} characters or fewer (now {name.Length})."
            : null;

        focusTarget = emailError is not null ? () => emailInput : nameError is not null ? () => nameInput : null;

        if (focusTarget is not null)
        {
            Announce(false, "Fix the highlighted fields, then send the invitation again.");

            return;
        }

        if (State.Members.Count >= AiState.MaxMembers)
        {
            Announce(false, $"The workspace is full ({AiState.MaxMembers} members and invitations). Remove an invitation first. Nobody was invited.");
            focusStatus = true;

            return;
        }

        var result = State.Invite(email, name, inviteRole, out var invited);

        switch (result)
        {
            case AiResult.Ok when invited is not null:
                Announce(true, $"{invited.Name} ({invited.Email}) is invited as a {AiDisplay.RoleLabel(invited.Role).ToLowerInvariant()}. They join when they create an account with that email."
                    + (filter is null or AiMemberStatus.Invited ? null : " Show \"Invited\" to see the invitation."));
                inviteEmail = inviteName = string.Empty;
                inviteRole = AiMemberRole.Member;
                focusTarget = () => emailInput;

                return;

            case AiResult.Duplicate:
                emailError = State.FindMember(email) is { } taken ? DuplicateText(taken) : "That email address is already a member or invited.";
                focusTarget = () => emailInput;
                Announce(false, "That person is already in the workspace. Nobody was invited.");

                return;

            case AiResult.Invalid:
                if (State.Members.Count >= AiState.MaxMembers)
                {
                    Announce(false, $"The workspace is full ({AiState.MaxMembers} members and invitations). Remove an invitation first. Nobody was invited.");
                    focusStatus = true;
                }
                else
                {
                    emailError = "Check the email address and the name: one of them isn't valid.";
                    focusTarget = () => emailInput;
                    Announce(false, "The invitation couldn't be sent. Nobody was invited.");
                }

                return;

            case AiResult.NotAllowed:
                Announce(false, "Only active workspace admins can invite people. Nobody was invited.");
                focusStatus = true;

                return;

            default:
                Announce(false, "That didn't work, and nobody was invited. Try again.");
                focusStatus = true;

                return;
        }
    }

    private bool IsMe(AiMember member) => member.AccountId is not null && member.AccountId == State.Identity.Current?.Id;

    private string LastAdminMessage(AiMember member) =>
        State.Members.Count(item => item.Role == AiMemberRole.Admin && item.Status == AiMemberStatus.Active && item.AccountId is not null) <= 1 && member is { Role: AiMemberRole.Admin, Status: AiMemberStatus.Active }
            ? $"{(IsMe(member) ? "You're" : $"{member.Name} is")} the only active workspace admin who can sign in here, and the workspace needs one. Make another member with an account a workspace admin first. Nothing changed."
            : $"{member.Name} can't be changed that way right now. Nothing changed.";

    /// <summary>Runs a confirmed change now that the dialog is hidden; focus goes back to its button, or to the status message.</summary>
    private Task OnDialogHiddenAsync()
    {
        var action = pendingAction;
        pendingAction = null;
        action?.Invoke();
        focusButtonKey = dialogEmail is { } email ? ButtonKey(dialogKind, email) : null;
        StateHasChanged();

        return Task.CompletedTask;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenAsync(DialogKind kind, AiMember member)
    {
        dialogKind = kind;
        dialogEmail = member.Email;
        dialogRole = member.Role;
        pendingAction = null;

        await dialogModal.ShowAsync();
    }

    private string OtherMessage(AiResult result, AiMember? member) => result switch
    {
        AiResult.NotFound => $"{member?.Name ?? "That person"} is no longer in the workspace, so nothing changed.",
        AiResult.NotAllowed => "Only active workspace admins can manage members. Nothing changed.",
        AiResult.Invalid => "That role or status isn't valid. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    private string DuplicateText(AiMember existing) => existing.Status switch
    {
        AiMemberStatus.Invited => $"{existing.Email} is already invited (as {existing.Name}).",
        AiMemberStatus.Deactivated => $"{existing.Email} belongs to {existing.Name}, who is deactivated. Reactivate them in the table instead.",
        _ => $"{existing.Email} is already a member ({existing.Name}).",
    };

    #endregion

    #endregion

    #region Properties, Indexers

    private string ConfirmText => dialogKind switch
    {
        DialogKind.Role => "Save role",
        DialogKind.Deactivate => "Deactivate",
        DialogKind.Reactivate => "Reactivate",
        _ => "Remove invitation",
    };

    private AiMember? DialogMember => State.FindMember(dialogEmail);

    private string DialogTitle => DialogMember is not { } member ? "Person not found" : dialogKind switch
    {
        DialogKind.Role => member.Status == AiMemberStatus.Invited ? $"Change the role {member.Name} is invited with" : $"Change the role of {member.Name}",
        DialogKind.Deactivate => $"Deactivate {member.Name}?",
        DialogKind.Reactivate => $"Reactivate {member.Name}?",
        _ => $"Remove the invitation for {member.Name}?",
    };

    private static IReadOnlyList<(AiMemberStatus? Status, string Text)> FilterOptions { get; } = new (AiMemberStatus?, string)[]
    {
        (null, "All"),
        (AiMemberStatus.Active, "Active"),
        (AiMemberStatus.Invited, "Invited"),
        (AiMemberStatus.Deactivated, "Deactivated"),
    };

    private string FilterWord => filter is { } status ? AiDisplay.StatusLabel(status).ToLowerInvariant() : string.Empty;

    private IReadOnlyList<AiMember> Rows => State.Members.Where(member => filter is not { } status || member.Status == status).ToList();

    [Inject] private AiState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>What the dialog asks to confirm.</summary>
    private enum DialogKind
    {
        Role,
        Deactivate,
        Reactivate,
        RemoveInvitation,
    }

    #endregion
}
