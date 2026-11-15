using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The workspace's team: every member and open invitation with role, status, and last activity, the seats in use, and,
/// for owners and admins, an invite dialog, a change-role dialog, "Resend" for invitations, and a confirmed remove or
/// cancel-invitation. Nobody acts on themselves, and only owners see actions on owners or grant the owner role; the
/// state enforces the same rules. Focus returns to the button that opened a dialog, or to the status message when that
/// button is gone.
/// </summary>
public partial class AppTeam : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext inviteContext = default!;

    private InputText? emailInput;

    private Button? inviteButton;

    private string? inviteError;

    private bool inviteErrorLinksBilling;

    private int inviteErrors;

    private InviteForm inviteForm = new();

    private Modal inviteModal = default!;

    private string? pageError;

    private int pageErrors;

    private readonly Dictionary<int, Button> removeButtons = new();

    private bool removed;

    private Modal removeModal = default!;

    private SaasMember? removeTarget;

    private readonly Dictionary<int, Button> roleButtons = new();

    private EditContext roleContext = default!;

    private string? roleError;

    private int roleErrors;

    private RoleForm roleForm = new();

    private Modal roleModal = default!;

    private SaasMember? roleTarget;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized()
    {
        inviteContext = new EditContext(inviteForm);
        roleContext = new EditContext(roleForm);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Whether the signed-in member may act on a member: owners and admins, never on themselves, and only owners on owners.</summary>
    private bool CanActOn(SaasMember me, SaasMember member) =>
        State.CanManageTeam && member.Id != me.Id && (me.Role == SaasMemberRole.Owner || member.Role != SaasMemberRole.Owner);

    private static string FreeSeatsText(int free) => free == 1 ? "1 seat is free." : $"{free.ToString(CultureInfo.InvariantCulture)} seats are free.";

    private static string MemberCountText(IReadOnlyList<SaasMember> team)
    {
        var invitations = team.Count(member => member.Status == SaasMemberStatus.Invited);
        var active = team.Count - invitations;
        var people = active == 1 ? "1 member" : $"{active.ToString(CultureInfo.InvariantCulture)} members";

        return invitations switch
        {
            0 => people,
            1 => $"{people} and 1 open invitation",
            _ => $"{people} and {invitations.ToString(CultureInfo.InvariantCulture)} open invitations",
        };
    }

    private static string ReadOnlyNote(SaasMember me) => me.Role is SaasMemberRole.Owner or SaasMemberRole.Admin
        ? "Team changes are turned off while the workspace is suspended or archived."
        : "Only owners and admins manage the team. You can see who's in it; ask an owner or admin to invite someone or change a role.";

    private static BadgeColor RoleColor(SaasMemberRole role) => role switch
    {
        SaasMemberRole.Owner => BadgeColor.Primary,
        SaasMemberRole.Admin => BadgeColor.Info,
        SaasMemberRole.Billing => BadgeColor.Warning,
        _ => BadgeColor.Secondary,
    };

    /// <summary>Returns "an owner", "an admin", "a billing member", or "a member".</summary>
    private static string RoleWithArticle(SaasMemberRole role) => role switch
    {
        SaasMemberRole.Owner => "an owner",
        SaasMemberRole.Admin => "an admin",
        SaasMemberRole.Billing => "a billing member",
        _ => "a member",
    };

    /// <summary>Returns the roles the signed-in member may give: the owner role only to an owner.</summary>
    private static IReadOnlyList<SaasMemberRole> RolesFor(SaasMember? me) => me?.Role == SaasMemberRole.Owner
        ? new[] { SaasMemberRole.Owner, SaasMemberRole.Admin, SaasMemberRole.Billing, SaasMemberRole.Member }
        : new[] { SaasMemberRole.Admin, SaasMemberRole.Billing, SaasMemberRole.Member };

    private static double SeatPercent(int used, int seats) => seats <= 0 ? 0 : Math.Min(100, used * 100d / seats);

    private static string SeatsText(int used, int seats) =>
        string.Create(CultureInfo.InvariantCulture, $"{used} of {seats} {(seats == 1 ? "seat" : "seats")} used");

    private async Task AskRemoveAsync(SaasMember member)
    {
        removeTarget = member;
        removed = false;
        pageError = null;

        await removeModal.ShowAsync();
    }

    private async Task ConfirmRemoveAsync()
    {
        if (removeTarget is not { } member)
            return;

        var result = State.RemoveMember(member.Id);
        var invitation = member.Status == SaasMemberStatus.Invited;

        if (result == SaasResult.Ok)
        {
            removed = true;
            SetStatus(invitation
                ? $"The invitation for {member.Email} is cancelled, and its seat is free."
                : $"{member.Name} is removed from the team, and their seat is free.");
        }
        else
        {
            SetPageError(result switch
            {
                SaasResult.NotFound => $"{member.Name} is no longer in the team.",
                _ => invitation
                    ? "You can't cancel this invitation. Only owners and admins manage the team, and only owners act on owners."
                    : "You can't remove this person. Only owners and admins manage the team, nobody removes themselves, and only owners remove owners.",
            });
        }

        await removeModal.HideAsync();
    }

    private async Task InviteAsync()
    {
        var email = inviteForm.Email.Trim();
        var result = State.InviteMember(inviteForm.Name, email, inviteForm.Role);

        if (result == SaasResult.Ok)
        {
            SetStatus($"{email} is invited as {RoleWithArticle(inviteForm.Role)}. In this demo no email is sent; the invitation holds a seat until it's cancelled.");
            await inviteModal.HideAsync();

            return;
        }

        var seats = State.CurrentWorkspace?.Seats ?? 0;
        inviteErrorLinksBilling = result == SaasResult.NoFreeSeat;
        SetInviteError(result switch
        {
            SaasResult.NoFreeSeat => string.Create(CultureInfo.InvariantCulture,
                $"All {seats} seats are in use by members and open invitations. To invite someone new, cancel an invitation or add seats."),
            SaasResult.AlreadyMember => $"{email} is already in the team or has an open invitation.",
            SaasResult.Invalid => "Check the details: the email needs an @ and no spaces (up to 120 characters), and the name can have up to 60 characters.",
            _ => "You can't send this invitation. Only owners and admins invite people, and only owners invite owners.",
        });
    }

    /// <summary>The invite dialog opens from code, so focus goes back to its button, which stays after an invitation.</summary>
    private async Task OnInviteHiddenAsync()
    {
        ResetInvite();

        if (inviteButton is not null)
            await SaasDisplay.FocusAsync(inviteButton.Element);
        else
            await SaasDisplay.FocusAsync(statusRegion);
    }

    private Task OnInviteShownAsync() => SaasDisplay.FocusAsync(emailInput?.Element);

    private async Task OnRemoveHiddenAsync()
    {
        var member = removeTarget;
        var wasRemoved = removed;
        removeTarget = null;
        removed = false;

        // The row is gone after a removal, so focus goes to the status message.
        if (wasRemoved)
            await SaasDisplay.FocusAsync(statusRegion);
        else if (member is not null && removeButtons.TryGetValue(member.Id, out var button))
            await SaasDisplay.FocusAsync(button.Element);
        else
            await SaasDisplay.FocusAsync(statusRegion);
    }

    private async Task OnRoleHiddenAsync()
    {
        var member = roleTarget;
        roleTarget = null;
        roleError = null;

        // An owner who makes someone else an owner keeps the button; without it (the row lost its actions), focus goes to the status.
        if (member is not null && State.CurrentMember is { } me && State.MembersOf(me.WorkspaceId).FirstOrDefault(other => other.Id == member.Id) is { } current
            && CanActOn(me, current) && roleButtons.TryGetValue(member.Id, out var button))
            await SaasDisplay.FocusAsync(button.Element);
        else
            await SaasDisplay.FocusAsync(statusRegion);
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    private async Task OpenInviteAsync()
    {
        pageError = null;
        ResetInvite();

        await inviteModal.ShowAsync();
    }

    private async Task OpenRoleAsync(SaasMember member)
    {
        pageError = null;
        roleTarget = member;
        roleError = null;
        roleErrors = 0;
        roleForm = new RoleForm { Role = member.Role };
        roleContext = new EditContext(roleForm);

        await roleModal.ShowAsync();
    }

    private void Resend(SaasMember member)
    {
        pageError = null;
        var result = State.ResendInvite(member.Id);

        if (result == SaasResult.Ok)
            SetStatus($"The invitation to {member.Email} is sent again, dated {SaasDisplay.Date(State.Clock.Now)}. In this demo no email is sent.");
        else
            SetPageError(result == SaasResult.NotFound
                ? $"There's no open invitation for {member.Email} any more."
                : "You can't resend this invitation. Only owners and admins manage the team.");
    }

    private void ResetInvite()
    {
        inviteForm = new InviteForm();
        inviteContext = new EditContext(inviteForm);
        inviteError = null;
        inviteErrors = 0;
        inviteErrorLinksBilling = false;
    }

    private async Task SaveRoleAsync()
    {
        if (roleTarget is not { } member)
            return;

        var role = roleForm.Role;
        var result = State.ChangeMemberRole(member.Id, role);

        if (result == SaasResult.Ok)
        {
            SetStatus($"{member.Name} is now {RoleWithArticle(role)}.");
            await roleModal.HideAsync();

            return;
        }

        roleErrors++;
        var message = result switch
        {
            SaasResult.NoChange => $"{member.Name} already has this role. Choose another role, or cancel.",
            SaasResult.NotFound => $"{member.Name} is no longer in the team.",
            _ => "You can't give this role. Only owners and admins manage the team, nobody changes their own role, and only owners grant or take away the owner role.",
        };
        roleError = roleErrors == 1 ? message : $"{message} ({roleErrors})";
    }

    private void SetInviteError(string message)
    {
        inviteErrors++;
        inviteError = inviteErrors == 1 ? message : $"{message} ({inviteErrors})";
    }

    private void SetPageError(string message)
    {
        pageErrors++;
        pageError = pageErrors == 1 ? message : $"{message} ({pageErrors})";
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        pageError = null;
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class InviteForm
    {
        [Required(ErrorMessage = "Enter the email address of the person to invite.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(120, ErrorMessage = "Use 120 characters or fewer.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(60, ErrorMessage = "Use 60 characters or fewer.")]
        public string? Name { get; set; }

        public SaasMemberRole Role { get; set; } = SaasMemberRole.Member;
    }

    private sealed class RoleForm
    {
        public SaasMemberRole Role { get; set; } = SaasMemberRole.Member;
    }

    #endregion
}
