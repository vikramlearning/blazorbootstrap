using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The workspace's team: every member and open invitation in a <see cref="Grid{TItem}" /> (name, role, status, added, last
/// activity, and actions) with a saved view, sorting, paging, the column chooser, and CSV export, the seats in use, and,
/// for owners and admins, an invite dialog in an <see cref="EditForm" /> (rule WR-002: an email that isn't in the team yet,
/// checked as you type, and a role), a change-role dialog (the name opens it, and its stretched area makes the whole row do
/// so), "Resend" and "Accept (demo)" for invitations, and a confirmed remove or cancel-invitation. Nobody acts on
/// themselves, and only owners see actions on owners or grant the owner role; the state enforces the same rules. Focus
/// returns to the button that opened a dialog, or to the status message when that button is gone.
/// </summary>
public partial class AppTeam : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 10;

    private readonly Dictionary<int, ElementReference> nameButtons = new();

    private ElementReference? countRegion;

    private ShowcaseUniqueCheck? emailCheck;

    private Grid<SaasMember>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The state's answers that belong to a field of the invite form (someone already in the team).</summary>
    private ValidationMessageStore? inviteErrorsStore;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

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

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await view.LoadAsync();
        pageSize = (await view.GetSettingsAsync()).PageSize;
        viewLoaded = true;
        StateHasChanged();
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "team", DefaultPageSize);
        ResetInvite();
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

    /// <summary>
    /// Accepts an invitation on the invitee's behalf (a demo shortcut: invitees have no sign-in here). The row stays, so
    /// focus stays in the list; the status message says what happened and who was told.
    /// </summary>
    private void AcceptForDemo(SaasMember member)
    {
        pageError = null;
        var result = State.AcceptInvitationForDemo(member.Id);

        if (result == SaasResult.Ok)
            SetStatus($"{member.Name} accepted the invitation and is now an active member. In this demo you accepted it for them; the owners and admins who can sign in got a notification.");
        else
            SetPageError(result switch
            {
                SaasResult.NoChange => $"{member.Name} already accepted the invitation.",
                SaasResult.NotFound => $"There's no invitation for {member.Email} any more.",
                _ => "You can't do this. Only owners and admins manage the team.",
            });
    }

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

    /// <summary>Whether someone in the team (a member or an open invitation) has this email.</summary>
    private Task<bool> IsInTeamAsync(string email) => Task.FromResult(State.IsInTeam(email));

    private string InTeamMessage(string email) =>
        State.CurrentWorkspace is { } workspace && State.MembersOf(workspace.Id).FirstOrDefault(member => string.Equals(member.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)) is { } existing
            ? existing.Status == SaasMemberStatus.Invited
                ? $"{existing.Name} already has an open invitation, sent {SaasDisplay.Date(existing.AddedOn)}. Resend it from the list instead."
                : $"{existing.Name} is already in the team."
            : $"{email.Trim()} is already in the team or has an open invitation.";

    /// <summary>
    /// Sends the invitation once the form is valid: the team check finishes first, then every rule runs, and a failed submit
    /// shows the summary (rule WR-002). The state checks again; someone already in the team goes on the email field.
    /// </summary>
    private async Task InviteAsync()
    {
        inviteErrorsStore?.Clear();
        inviteContext.NotifyValidationStateChanged();

        var free = emailCheck is null || await emailCheck.CheckNowAsync();

        if (!inviteContext.Validate() || !free)
            return;

        var email = inviteForm.Email.Trim();
        var result = State.InviteMember(inviteForm.Name, email, inviteForm.Role);

        if (result == SaasResult.Ok)
        {
            SetStatus($"{email} is invited as {RoleWithArticle(inviteForm.Role)}. In this demo no email is sent; the invitation holds a seat until it's cancelled.");
            await inviteModal.HideAsync();

            return;
        }

        if (result == SaasResult.AlreadyMember)
        {
            inviteErrorsStore?.Add(FieldIdentifier.Create(() => inviteForm.Email), InTeamMessage(email));
            inviteContext.NotifyValidationStateChanged();

            return;
        }

        var seats = State.CurrentWorkspace?.Seats ?? 0;
        inviteErrorLinksBilling = result == SaasResult.NoFreeSeat;
        SetInviteError(result switch
        {
            SaasResult.NoFreeSeat => string.Create(CultureInfo.InvariantCulture,
                $"All {seats} seats are in use by members and open invitations. To invite someone new, cancel an invitation or add seats."),
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

    /// <summary>Another screen or a dialog changed the state: the grid reads the team again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>The page of people the grid asked for: active members by role and name, then invitations, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<SaasMember>> ProvideMembersAsync(GridDataProviderRequest<SaasMember> request) =>
        view.ApplyAsync(request, State.CurrentWorkspace is { } workspace ? State.MembersOf(workspace.Id) : Array.Empty<SaasMember>());

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the line above the list.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        gridVersion++;
        SetStatus("The view is reset: owners first, 10 people a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await SaasDisplay.FocusAsync(countRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

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
        inviteErrorsStore = new ValidationMessageStore(inviteContext);
        inviteContext.OnFieldChanged += (_, args) => inviteErrorsStore?.Clear(args.FieldIdentifier);
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

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class InviteForm
    {
        [Required(ErrorMessage = "Enter the email address of the person to invite.")]
        [RegularExpression(@"^\s*[^@\s]+@[^@\s]+\.[^@\s]+\s*$", ErrorMessage = "Enter an email address like name@example.com.")]
        [StringLength(120, ErrorMessage = "Use 120 characters or fewer for the email.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(60, ErrorMessage = "Use 60 characters or fewer for the name.")]
        public string? Name { get; set; }

        public SaasMemberRole Role { get; set; } = SaasMemberRole.Member;
    }

    private sealed class RoleForm
    {
        public SaasMemberRole Role { get; set; } = SaasMemberRole.Member;
    }

    #endregion
}
