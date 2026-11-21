using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// The workspace's group roles (AC-4): the permission matrix (<see cref="RoleAllows" />), the members and invitations, and
/// the rules every change follows here, whatever the screen shows. There is exactly one Owner, active and able to sign in;
/// only the Owner hands that role on (<see cref="TransferOwnership" />). Nobody changes their own role or access. Admins and
/// the Owner grant roles up to Admin, and nobody changes or removes the Owner. Role and access changes are audited, and the
/// person they're about hears about them when they can sign in.
/// </summary>
public sealed partial class AiState
{
    #region Methods

    #region Public Methods

    /// <summary>Returns whether a workspace role has a permission: each role has the rights of the roles below it (<see cref="AiMemberRole" />).</summary>
    public static bool RoleAllows(AiMemberRole role, AiPermission permission) => permission switch
    {
        AiPermission.UseAssistant => true,
        AiPermission.ManageSharedPrompts => role >= AiMemberRole.PromptEditor,
        AiPermission.ViewAnalytics or AiPermission.ManageMembers or AiPermission.ManageSettings or AiPermission.ViewAudit or AiPermission.ManageSpend
            or AiPermission.ReviewSensitiveData => role >= AiMemberRole.Admin,
        AiPermission.ManageRetention or AiPermission.TransferOwnership => role == AiMemberRole.Owner,
        _ => false,
    };

    /// <summary>
    /// Changes the role of several members or invitations at once (Admins and the Owner), each like <see cref="SetRole" />:
    /// yourself and the Owner are left as they are (counted as protected), and so are deactivated members.
    /// </summary>
    public AiBulkOutcome BulkSetRole(IReadOnlyCollection<int> memberIds, AiMemberRole role)
    {
        if (!Enum.IsDefined(role))
            return new AiBulkOutcome(Can(AiPermission.ManageMembers) ? AiResult.Invalid : AiResult.NotAllowed, 0, 0, 0);

        if (role == AiMemberRole.Owner)
            return new AiBulkOutcome(Can(AiPermission.ManageMembers) ? AiResult.NotGrantable : AiResult.NotAllowed, 0, 0, 0);

        return BulkMembers(memberIds, id => SetRole(id, role));
    }

    /// <summary>
    /// Removes several people's access at once (Admins and the Owner): an active member is deactivated (their conversations
    /// stay, and access can be restored), and an invitation is withdrawn. Yourself and the Owner are left as they are.
    /// </summary>
    public AiBulkOutcome BulkRemoveAccess(IReadOnlyCollection<int> memberIds) =>
        BulkMembers(memberIds, id => FindMember(id) is { Status: AiMemberStatus.Invited } ? RemoveInvitation(id) : SetAccess(id, false));

    /// <summary>Resends several invitations at once (Admins and the Owner); members who already joined are left as they are.</summary>
    public AiBulkOutcome BulkResendInvitations(IReadOnlyCollection<int> memberIds) => BulkMembers(memberIds, ResendInvitation);

    /// <summary>Restores several deactivated members' access at once (Admins and the Owner).</summary>
    public AiBulkOutcome BulkRestoreAccess(IReadOnlyCollection<int> memberIds) => BulkMembers(memberIds, id => SetAccess(id, true));

    /// <summary>Returns whether the signed-in account is an active member whose role has the permission.</summary>
    public bool Can(AiPermission permission) => CurrentMember is { Status: AiMemberStatus.Active } member && RoleAllows(member.Role, permission);

    /// <summary>Finds a member (or invitation) by email address, or returns <see langword="null" />.</summary>
    public AiMember? FindMember(string? email) => members.FirstOrDefault(member => string.Equals(member.Email, email?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Finds a member (or invitation) by number, or returns <see langword="null" />.</summary>
    public AiMember? FindMember(int memberId) => members.FirstOrDefault(member => member.Id == memberId);

    /// <summary>
    /// Invites someone by email (Admins and the Owner): they join with the role when they create an account with that
    /// address. NotGrantable for Owner (it comes only through a transfer); Duplicate when the email is a member or invited.
    /// </summary>
    public AiResult Invite(string email, string name, AiMemberRole role, out AiMember? invited)
    {
        invited = null;

        if (!Can(AiPermission.ManageMembers))
            return AiResult.NotAllowed;

        email = (email ?? string.Empty).Trim().ToLowerInvariant();
        name = name?.Trim() ?? string.Empty;

        if (!IsEmail(email) || !IsText(name, ShowcaseIdentity.MaxNameLength) || !Enum.IsDefined(role))
            return AiResult.Invalid;

        if (!CanGrant(role))
            return AiResult.NotGrantable;

        if (FindMember(email) is not null)
            return AiResult.Duplicate;

        if (members.Count >= MaxMembers || nextMemberId >= NextIdLimit)
            return AiResult.Invalid;

        invited = new AiMember { Id = nextMemberId++, Email = email, Name = name, Role = role, Status = AiMemberStatus.Invited, JoinedAt = Clock.Now };
        members.Add(invited);

        if (!FitsSnapshot())
        {
            members.Remove(invited);
            invited = null;

            return AiResult.Invalid;
        }

        Record("Invited", $"Invitation {email}", AiSeedData.MembersPath(email), $"As {RoleWords(role)}; no email is sent in this demo");
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Returns whether an email address is already a member or invited (the invite form's check).</summary>
    public bool IsEmailTaken(string? email) => FindMember(email) is not null;

    /// <summary>Withdraws an invitation that wasn't taken up (Admins and the Owner).</summary>
    public AiResult RemoveInvitation(int memberId)
    {
        if (!Can(AiPermission.ManageMembers))
            return AiResult.NotAllowed;

        if (FindMember(memberId) is not { } member)
            return AiResult.NotFound;

        if (member.Status != AiMemberStatus.Invited)
            return AiResult.WrongState;

        members.Remove(member);
        Record("Withdrew invitation", $"Invitation {member.Email}", AiSeedData.MembersPath(), $"Was for {RoleWords(member.Role)}");
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>Sends an invitation again (Admins and the Owner): its date moves to now. Demo only: no email is sent. WrongState once they joined.</summary>
    public AiResult ResendInvitation(int memberId)
    {
        if (!Can(AiPermission.ManageMembers))
            return AiResult.NotAllowed;

        if (FindMember(memberId) is not { } member)
            return AiResult.NotFound;

        if (member.Status != AiMemberStatus.Invited)
            return AiResult.WrongState;

        member.JoinedAt = Clock.Now;
        Record("Resent invitation", $"Invitation {member.Email}", AiSeedData.MembersPath(member.Email), $"As {RoleWords(member.Role)}");
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Removes or restores a member's access (Admins and the Owner): a deactivated member can't use the workspace; their
    /// conversations are kept. OwnRole: yourself; OwnerProtected: the Owner; WrongState: an invitation (withdraw it instead).
    /// </summary>
    public AiResult SetAccess(int memberId, bool active)
    {
        if (!Can(AiPermission.ManageMembers))
            return AiResult.NotAllowed;

        if (FindMember(memberId) is not { } member)
            return AiResult.NotFound;

        if (IsSelf(member))
            return AiResult.OwnRole;

        if (member.Role == AiMemberRole.Owner)
            return AiResult.OwnerProtected;

        if (member.Status == AiMemberStatus.Invited)
            return AiResult.WrongState;

        var status = active ? AiMemberStatus.Active : AiMemberStatus.Deactivated;

        if (member.Status == status)
            return AiResult.NoChange;

        member.Status = status;
        Record(active ? "Restored access" : "Removed access", MemberTarget(member), AiSeedData.MembersPath(member.Email),
            active ? $"Deactivated → Active, as {RoleWords(member.Role)}" : "Active → Deactivated; conversations kept");

        if (active)
            Notify(member.AccountId, "Your access is back", $"{ActorName()} restored your access to {policy.WorkspaceName}. Your conversations are where you left them.", AiSeedData.SiteBase + "/chat");

        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Changes the role of a member or an invitation (Admins and the Owner). OwnRole: yourself; OwnerProtected: the Owner;
    /// NotGrantable: Owner (use <see cref="TransferOwnership" />); WrongState: a deactivated member (restore their access first).
    /// The person hears about it when they can sign in.
    /// </summary>
    public AiResult SetRole(int memberId, AiMemberRole role)
    {
        if (!Can(AiPermission.ManageMembers))
            return AiResult.NotAllowed;

        if (FindMember(memberId) is not { } member)
            return AiResult.NotFound;

        if (!Enum.IsDefined(role))
            return AiResult.Invalid;

        if (IsSelf(member))
            return AiResult.OwnRole;

        if (member.Role == AiMemberRole.Owner)
            return AiResult.OwnerProtected;

        if (!CanGrant(role))
            return AiResult.NotGrantable;

        if (member.Status == AiMemberStatus.Deactivated)
            return AiResult.WrongState;

        if (member.Role == role)
            return AiResult.NoChange;

        var before = member.Role;
        member.Role = role;
        Record(member.Status == AiMemberStatus.Invited ? "Changed invitation role" : "Changed role", MemberTarget(member), AiSeedData.MembersPath(member.Email),
            $"{RoleLabel(before)} → {RoleLabel(role)}");
        Notify(member.AccountId, $"{ActorName()} changed your role", $"{RoleLabel(before)} → {RoleLabel(role)}. {RoleSummary(role)}", AiSeedData.SiteBase + "/profile");
        RaiseChanged();

        return AiResult.Ok;
    }

    /// <summary>
    /// Hands the Owner role to another member (the Owner only, after their confirmation): the new Owner must be active and able
    /// to sign in, and the previous Owner becomes an Admin. OwnRole: yourself; WrongState: not active, or no sign-in. Audited,
    /// and the new Owner and the other admins hear about it.
    /// </summary>
    public AiResult TransferOwnership(int memberId)
    {
        if (!Can(AiPermission.TransferOwnership) || CurrentMember is not { } owner)
            return AiResult.NotAllowed;

        if (FindMember(memberId) is not { } member)
            return AiResult.NotFound;

        if (IsSelf(member))
            return AiResult.OwnRole;

        if (member.Status != AiMemberStatus.Active || member.AccountId is null)
            return AiResult.WrongState;

        var before = member.Role;
        member.Role = AiMemberRole.Owner;
        owner.Role = AiMemberRole.Admin;
        Record("Transferred ownership", MemberTarget(member), AiSeedData.MembersPath(member.Email),
            $"{owner.Name}: Owner → Admin; {member.Name}: {RoleLabel(before)} → Owner");
        Notify(member.AccountId, $"You're now the Owner of {policy.WorkspaceName}", $"{owner.Name} transferred ownership to you. You can now change retention and hand the role on.", AiSeedData.MembersPath());

        foreach (var admin in members.Where(item => item.Role == AiMemberRole.Admin && item.Status == AiMemberStatus.Active && item.Id != owner.Id).ToList())
            Notify(admin.AccountId, $"{member.Name} is now the Owner", $"{owner.Name} transferred ownership of {policy.WorkspaceName}; they're an Admin now.", AiSeedData.MembersPath());

        RaiseChanged();

        return AiResult.Ok;
    }

    #endregion

    #region Internal Methods

    /// <summary>Returns a role in words, like "Prompt editor".</summary>
    internal static string RoleLabel(AiMemberRole role) => role switch
    {
        AiMemberRole.Owner => "Owner",
        AiMemberRole.Admin => "Admin",
        AiMemberRole.PromptEditor => "Prompt editor",
        _ => "Member",
    };

    /// <summary>Returns what a role may do, in one sentence.</summary>
    internal static string RoleSummary(AiMemberRole role) => role switch
    {
        AiMemberRole.Owner => "Everything an admin does, plus retention and handing ownership on.",
        AiMemberRole.Admin => "Manages members and roles up to Admin, the settings and model policy, shared prompts, usage analytics, and the audit log.",
        AiMemberRole.PromptEditor => "Writes, publishes, and deletes the workspace's shared prompts; no access to members or settings.",
        _ => "Chats with the assistant, keeps their own prompts, and follows their own usage.",
    };

    #endregion

    #region Private Methods

    private static bool IsEmail(string? value) => value is not null && value.Length <= ShowcaseIdentity.MaxEmailLength && EmailPattern().IsMatch(value);

    /// <summary>A member as the audit log names them, like "Member Sam Rivera (sam.rivera@example.com)".</summary>
    private static string MemberTarget(AiMember member) => $"{(member.Status == AiMemberStatus.Invited ? "Invitation" : "Member")} {member.Name} ({member.Email})";

    /// <summary>A role in words for the middle of a sentence, with its article: "a member", "an admin", "a prompt editor".</summary>
    private static string RoleWords(AiMemberRole role) => role switch
    {
        AiMemberRole.Owner => "the Owner",
        AiMemberRole.Admin => "an Admin",
        AiMemberRole.PromptEditor => "a prompt editor",
        _ => "a member",
    };

    /// <summary>The signed-in person's name for messages, or the workspace's name for its own actions.</summary>
    private string ActorName() => Identity.Current?.Name is { Length: > 0 } name ? name : AiSeedData.SystemActor;

    /// <summary>Runs one member change on several members, raising <see cref="Changed" /> once; protected ones (yourself, the Owner) are counted apart.</summary>
    private AiBulkOutcome BulkMembers(IReadOnlyCollection<int> memberIds, Func<int, AiResult> change)
    {
        if (!Can(AiPermission.ManageMembers))
            return new AiBulkOutcome(AiResult.NotAllowed, 0, 0, 0);

        var list = (memberIds ?? Array.Empty<int>()).Distinct().ToList();

        if (list.Count == 0)
            return new AiBulkOutcome(AiResult.Invalid, 0, 0, 0);

        int changed = 0, unchanged = 0, kept = 0;
        batchDepth++;

        try
        {
            foreach (var id in list)
            {
                switch (change(id))
                {
                    case AiResult.Ok:
                        changed++;
                        break;

                    case AiResult.OwnRole or AiResult.OwnerProtected:
                        kept++;
                        break;

                    default:
                        unchanged++;
                        break;
                }
            }
        }
        finally
        {
            batchDepth--;
        }

        FlushBatch();

        return new AiBulkOutcome(changed == 0 ? AiResult.NoChange : AiResult.Ok, changed, unchanged, kept);
    }

    /// <summary>Whether the signed-in person may grant a role: up to Admin (Owner comes only through a transfer).</summary>
    private bool CanGrant(AiMemberRole role) => Can(AiPermission.ManageMembers) && role <= AiMemberRole.Admin;

    /// <summary>Raises <see cref="Changed" /> once a bulk change that changed something ends.</summary>
    private void FlushBatch()
    {
        if (batchDepth == 0 && changedInBatch)
        {
            changedInBatch = false;
            Changed?.Invoke();
        }
    }

    private bool IsSelf(AiMember member) => member.AccountId is not null && member.AccountId == Identity.Current?.Id;

    /// <summary>Tells the Owner and the active admins who can sign in (except whoever caused it).</summary>
    private void NotifyManagers(string title, string text, string? link, bool fromSystem = false)
    {
        foreach (var manager in members.Where(member => member.Status == AiMemberStatus.Active && RoleAllows(member.Role, AiPermission.ManageMembers)).ToList())
            Notify(manager.AccountId, title, text, link, fromSystem);
    }

    /// <summary>The workspace's Owner.</summary>
    private AiMember? OwnerMember() => members.FirstOrDefault(member => member.Role == AiMemberRole.Owner);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the signed-in account's workspace role when they're an active member, or <see langword="null" />.</summary>
    public AiMemberRole? CurrentRole => CurrentMember is { Status: AiMemberStatus.Active } member ? member.Role : null;

    /// <summary>
    /// Gets the members and invitations (Admins and the Owner; empty for anyone else): invitations, then active, then
    /// deactivated members; the highest role first within each, then by name.
    /// </summary>
    public IReadOnlyList<AiMember> Members => Can(AiPermission.ManageMembers) || Can(AiPermission.ViewAnalytics)
        ? members.OrderBy(member => member.Status).ThenByDescending(member => member.Role).ThenBy(member => member.Name, StringComparer.OrdinalIgnoreCase).ToList()
        : Array.Empty<AiMember>();

    /// <summary>Gets the active members who can sign in, other than the signed-in Owner: who ownership can go to.</summary>
    public IReadOnlyList<AiMember> OwnershipCandidates => Can(AiPermission.TransferOwnership)
        ? members.Where(member => member.Status == AiMemberStatus.Active && member.AccountId is not null && !IsSelf(member)).OrderByDescending(member => member.Role)
            .ThenBy(member => member.Name, StringComparer.OrdinalIgnoreCase).ToList()
        : Array.Empty<AiMember>();

    /// <summary>Gets the Owner's name, for pages that say who to ask.</summary>
    public string OwnerName => OwnerMember()?.Name ?? AiSeedData.OwnerName;

    #endregion
}
