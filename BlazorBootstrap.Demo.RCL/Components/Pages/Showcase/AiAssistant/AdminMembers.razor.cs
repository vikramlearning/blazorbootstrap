using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Members and roles of the Parley back office (Admins and the Owner, AC-4):
/// <list type="bullet">
/// <item>An invite <see cref="EditForm" /> (rule WR-002): email (required, of the right form, not already a member or
/// invited: an asynchronous check with a pending state, run again on submit), name (required, up to 80 characters), and a
/// role up to Admin (a rule against what the inviter may grant); <see cref="AiState.Invite" />'s refusals land on the field.</item>
/// <item>A "Roles and permissions" panel with the matrix of <see cref="AiState.RoleAllows" /> and the rules.</item>
/// <item>For the Owner, a transfer form (a new Owner who can sign in, and the workspace name typed to confirm), then a
/// confirmation dialog (<see cref="AiState.TransferOwnership" />).</item>
/// <item>The members and invitations in a <see cref="Grid{TItem}" /> with a saved view, search (also from <c>?search=</c>,
/// the header search's and notifications' links), status and role filters, sorting, paging, the column chooser, and CSV
/// export. A row opens the person's dialog (role, remove or restore access, resend or withdraw an invitation), and the bulk
/// bar changes roles, removes access (after a confirmation), restores access, or resends invitations for the selection.</item>
/// </list>
/// Every refusal of the state (yourself, the Owner, a role above Admin, a deactivated member) is said in words in the status
/// region or the dialog; outcomes are announced with a repeat counter, and focus goes back to the row or to the status line.
/// </summary>
public partial class AdminMembers : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string RemoveValue = "remove";

    private const string ResendValue = "resend";

    private const string RestoreValue = "restore";

    private const string RolePrefix = "role:";

    /// <summary>The rows' buttons, by member number, so focus returns to one when its dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    private EditContext bulkContext;

    private BulkForm bulkForm = new();

    private ElementReference? countRegion;

    private ElementReference? dialogCancel;

    private int dialogErrorCount;

    private string? dialogError;

    private ShowcaseUniqueCheck? emailCheck;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private Grid<MemberRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private EditContext inviteContext = default!;

    /// <summary>The state's answers that belong to an invite field (a taken email, a role above what you may grant).</summary>
    private ValidationMessageStore? inviteErrors;

    private InviteForm inviteForm = new();

    /// <summary>The <c>?search=</c> value the filter was last set from, so a new link sets it again.</summary>
    private string? loadedSearchQuery;

    private Modal memberModal = default!;

    /// <summary>The person the dialog is about.</summary>
    private int openedId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The outcome of the dialog, announced once it is hidden.</summary>
    private (string Message, bool IsError)? pendingAnnouncement;

    /// <summary>Whether the bulk removal was confirmed; it runs once its dialog is hidden.</summary>
    private bool pendingRemove;

    /// <summary>Whether the transfer was confirmed; it runs once its dialog is hidden.</summary>
    private bool pendingTransfer;

    private Modal removeModal = default!;

    private string roleFilter = string.Empty;

    private EditContext roleContext;

    private RoleForm roleForm = new();

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The people selected in the grid (on its current page).</summary>
    private List<MemberRow> selected = new();

    private int statusCount;

    private string statusFilter = string.Empty;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private EditContext transferContext;

    private TransferForm transferForm = new();

    private Modal transferModal = default!;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with empty forms.</summary>
    public AdminMembers()
    {
        bulkContext = new EditContext(bulkForm);
        roleContext = new EditContext(roleForm);
        transferContext = new EditContext(transferForm);
        NewInviteForm();
    }

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive (it lives in the tab's session storage), then shows the grid.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusTarget is { } target)
        {
            focusTarget = null;
            await AiDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "members", DefaultPageSize, "name", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    /// <summary>A link with <c>?search=</c> (the header search, a notification) filters the list to that person.</summary>
    protected override async Task OnParametersSetAsync()
    {
        if (SearchQuery == loadedSearchQuery)
            return;

        loadedSearchQuery = SearchQuery;
        search = searchBoxValue = SearchQuery?.Trim() ?? string.Empty;
        statusFilter = roleFilter = string.Empty;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        inviteContext.OnFieldChanged -= OnInviteFieldChanged;
    }

    #endregion

    #region Internal Methods

    /// <summary>Formats a count with its noun, like "1 member" or "3 members" (or the given plural).</summary>
    internal static string Plural(int count, string noun, string? plural = null) =>
        count == 1 ? $"1 {noun}" : string.Create(CultureInfo.InvariantCulture, $"{count} {plural ?? noun + "s"}");

    #endregion

    #region Private Methods

    private static string RoleWords(AiMemberRole role) => role switch
    {
        AiMemberRole.Owner => "the Owner",
        AiMemberRole.Admin => "an Admin",
        AiMemberRole.PromptEditor => "a prompt editor",
        _ => "a member",
    };

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

    /// <summary>Runs the bulk bar's action on the selection once its form is valid; removing access asks first.</summary>
    private async Task ApplyBulkAsync()
    {
        if (!await bulkContext.ValidateAsync())
            return;

        ClearError();

        if (bulkForm.Action == RemoveValue)
        {
            pendingRemove = false;
            await removeModal.ShowAsync();

            return;
        }

        var ids = selected.Select(row => row.Id).ToList();
        AiBulkOutcome outcome;
        string done;

        if (bulkForm.Action.StartsWith(RolePrefix, StringComparison.Ordinal) && Enum.TryParse<AiMemberRole>(bulkForm.Action[RolePrefix.Length..], out var role))
        {
            outcome = State.BulkSetRole(ids, role);
            done = $"Changed {Plural(outcome.Changed, "person's role", "people's roles")} to {AiDisplay.RoleLabel(role)}. Each person with a sign-in hears about it.";
        }
        else if (bulkForm.Action == RestoreValue)
        {
            outcome = State.BulkRestoreAccess(ids);
            done = $"Restored access for {Plural(outcome.Changed, "person", "people")}. They can chat again.";
        }
        else
        {
            outcome = State.BulkResendInvitations(ids);
            done = $"Resent {Plural(outcome.Changed, "invitation", "invitations")}. Demo only: no email is sent.";
        }

        AnnounceBulk(outcome, done);
        await FinishBulkAsync();
    }

    /// <summary>Says what a bulk action did, including who the role rules left as they were.</summary>
    private void AnnounceBulk(AiBulkOutcome outcome, string done)
    {
        var kept = outcome.Protected > 0 ? $" {Plural(outcome.Protected, "person was", "people were")} left as they were: nobody changes their own role or access, or the Owner's." : null;
        var unchanged = outcome.Unchanged > 0 ? $" {Plural(outcome.Unchanged, "person was", "people were")} already that way, or the action doesn't fit them." : null;

        switch (outcome.Result)
        {
            case AiResult.Ok:
                SetStatus(done + unchanged + kept, false);
                break;

            case AiResult.NoChange:
                SetStatus("Nobody changed." + unchanged + kept, false);
                break;

            case AiResult.NotGrantable:
                ShowError("Owner can't be given this way: only the Owner hands the role on, by a transfer. Nothing changed.");
                break;

            case AiResult.NotAllowed:
                ShowError("Only Admins and the Owner manage members. Nothing changed.");
                break;

            default:
                ShowError("That didn't work, and nothing changed. Try again.");
                break;
        }
    }

    /// <summary>Checks the transfer form, then asks to confirm in a dialog.</summary>
    private async Task AskTransferAsync()
    {
        transferForm.WorkspaceName = State.WorkspaceName;

        if (!await transferContext.ValidateAsync())
            return;

        pendingTransfer = false;
        await transferModal.ShowAsync();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        statusFilter = roleFilter = string.Empty;
        await FiltersChangedAsync();
        await AiDisplay.FocusAsync(countRegion);
    }

    private void ClearError()
    {
        errorMessage = errorText = null;
        errorCount = 0;
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<MemberRow>();
        ClearError();
        SetStatus("Selection cleared.", false);
        focusTarget = () => statusRegion;
    }

    private async Task ConfirmRemoveAsync()
    {
        pendingRemove = true;
        await removeModal.HideAsync();
    }

    private async Task ConfirmTransferAsync()
    {
        pendingTransfer = true;
        await transferModal.HideAsync();
    }

    private string CountText(int total)
    {
        var shown = Shown.Count;

        return shown == total ? $"{Plural(total, "person is", "people are")} in the workspace (members and invitations)." : $"Showing {shown.ToString(CultureInfo.InvariantCulture)} of {Plural(total, "person", "people")}.";
    }

    /// <summary>The duplicate email in words, naming who has it.</summary>
    private string EmailTakenMessage(string email)
    {
        var existing = State.FindMember(email);

        return existing?.Status switch
        {
            AiMemberStatus.Invited => $"{email.Trim().ToLowerInvariant()} is already invited (as {existing.Name}). Resend that invitation instead.",
            AiMemberStatus.Deactivated => $"{email.Trim().ToLowerInvariant()} belongs to {existing.Name}, whose access is removed. Restore their access instead.",
            AiMemberStatus.Active => $"{email.Trim().ToLowerInvariant()} is already a member ({existing.Name}).",
            _ => $"{email.Trim().ToLowerInvariant()} is already in the workspace.",
        };
    }

    /// <summary>A filter changed: back to the first page of the filtered people.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>A bulk action ended: the bar's form is emptied, the selection cleared, and focus moves to the announcement.</summary>
    private async Task FinishBulkAsync()
    {
        bulkForm = new BulkForm();
        bulkContext = new EditContext(bulkForm);

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<MemberRow>();
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    /// <summary>
    /// Sends the invitation once the form is valid: the email check finishes first, then every rule runs, and a failed submit
    /// shows the summary (rule WR-002). The state checks again; its answer maps to a field, the status line, or the alert.
    /// </summary>
    private async Task InviteAsync()
    {
        inviteErrors?.Clear();
        inviteContext.NotifyValidationStateChanged();

        var emailFree = emailCheck is null || await emailCheck.CheckNowAsync();

        if (!await inviteContext.ValidateAsync() || !emailFree)
            return;

        ClearError();
        var result = State.Invite(inviteForm.Email, inviteForm.Name, inviteForm.Role, out var invited);

        switch (result)
        {
            case AiResult.Ok when invited is not null:
                SetStatus($"{invited.Name} ({invited.Email}) is invited as {RoleWords(invited.Role)}. They join when they create an account with that email."
                    + (statusFilter is "" or nameof(AiMemberStatus.Invited) ? null : " Show \"Invited\" to see the invitation."), false);
                NewInviteForm();
                await RefreshGridAsync();
                focusTarget = () => statusRegion;

                return;

            case AiResult.Duplicate:
                InviteError(() => inviteForm.Email, EmailTakenMessage(inviteForm.Email));

                return;

            case AiResult.NotGrantable:
                InviteError(() => inviteForm.Role, "Choose Member, Prompt editor, or Admin: Owner comes only through a transfer.");

                return;

            case AiResult.Invalid when State.Members.Count >= AiState.MaxMembers:
                ShowError($"The workspace is full ({AiState.MaxMembers} members and invitations). Withdraw an invitation first. Nobody was invited.");

                return;

            case AiResult.Invalid:
                ShowError("The invitation couldn't be saved: a value is out of range, or this tab's storage is full. Nobody was invited.");

                return;

            case AiResult.NotAllowed:
                ShowError("Only Admins and the Owner invite people. Nobody was invited.");

                return;

            default:
                ShowError("That didn't work, and nobody was invited. Try again.");

                return;
        }
    }

    /// <summary>Shows a state answer under its invite field, like the form's own messages; the summary and focus follow.</summary>
    private void InviteError(System.Linq.Expressions.Expression<Func<object?>> field, string message)
    {
        inviteErrors?.Clear();
        inviteErrors?.Add(FieldIdentifier.Create(field), message);
        inviteContext.NotifyValidationStateChanged();
    }

    private bool IsMe(AiMember member) => member.AccountId is not null && member.AccountId == State.Identity.Current?.Id;

    private Task<bool> IsEmailTakenAsync(string email) => Task.FromResult(State.IsEmailTaken(email));

    /// <summary>A refusal of one person's change, in words.</summary>
    private string MemberRefusal(AiResult result, AiMember member) => result switch
    {
        AiResult.OwnRole => "Nobody changes their own role or access. Ask another Admin or the Owner. Nothing changed.",
        AiResult.OwnerProtected => $"{member.Name} is the Owner: the Owner's role and access can't be changed. Only the Owner hands the role on. Nothing changed.",
        AiResult.NotGrantable => "Owner can't be given this way: only the Owner hands the role on, by a transfer. Nothing changed.",
        AiResult.WrongState when member.Status == AiMemberStatus.Deactivated => $"{member.Name}'s access is removed. Restore it first, then change their role. Nothing changed.",
        AiResult.WrongState when member.Status == AiMemberStatus.Invited => $"{member.Name} hasn't joined yet: an invitation has no access to remove. Withdraw it instead. Nothing changed.",
        AiResult.WrongState => $"{member.Name} already joined, so there's no invitation to change. Nothing changed.",
        AiResult.NotFound => $"{member.Name} is no longer in the workspace, so nothing changed.",
        AiResult.NotAllowed => "Only Admins and the Owner manage members. Nothing changed.",
        _ => "That didn't work, and nothing changed. Try again.",
    };

    /// <summary>A fresh invite form, listening for its fields' changes.</summary>
    private void NewInviteForm()
    {
        if (inviteContext is not null)
            inviteContext.OnFieldChanged -= OnInviteFieldChanged;

        inviteForm = new InviteForm();
        inviteContext = new EditContext(inviteForm);
        inviteErrors = new ValidationMessageStore(inviteContext);
        inviteContext.OnFieldChanged += OnInviteFieldChanged;
    }

    private Task OnDialogShownAsync() => AiDisplay.FocusAsync(dialogCancel);

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: after a change focus moves to the announcement, otherwise back to the row.</summary>
    private Task OnDialogHiddenAsync()
    {
        var id = openedId;

        if (pendingAnnouncement is { } announcement)
        {
            pendingAnnouncement = null;

            if (announcement.IsError)
                ShowError(announcement.Message);
            else
                SetStatus(announcement.Message, false);

            focusTarget = () => statusRegion;
        }
        else
        {
            focusTarget = () => openers.TryGetValue(id, out var opener) ? opener : statusRegion;
        }

        StateHasChanged();

        return Task.CompletedTask;
    }

    /// <summary>A change to an invite field clears the state's earlier answer on that field (the email check handles its own).</summary>
    private void OnInviteFieldChanged(object? sender, FieldChangedEventArgs args) => inviteErrors?.Clear(args.FieldIdentifier);

    /// <summary>Removes access for the selection once its dialog is hidden (or returns focus to the bar when cancelled).</summary>
    private async Task OnRemoveHiddenAsync()
    {
        if (!pendingRemove)
        {
            focusTarget = () => statusRegion;

            return;
        }

        pendingRemove = false;
        var outcome = State.BulkRemoveAccess(selected.Select(row => row.Id).ToList());
        AnnounceBulk(outcome, $"Removed access for {Plural(outcome.Changed, "person", "people")}: members are deactivated (their conversations are kept), invitations withdrawn.");
        await FinishBulkAsync();
    }

    private async Task OnRoleFilterChangedAsync(ChangeEventArgs args)
    {
        roleFilter = Enum.TryParse<AiMemberRole>(args.Value?.ToString(), out var role) ? role.ToString() : string.Empty;
        await FiltersChangedAsync();
    }

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    /// <summary>Filters on every keystroke without changing the box's bound value, so a slower render never overwrites newer typing.</summary>
    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<MemberRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            ClearError();
    }

    /// <summary>Another screen or a change here changed the state: the grid reads the people again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusFilterChangedAsync(ChangeEventArgs args)
    {
        statusFilter = Enum.TryParse<AiMemberStatus>(args.Value?.ToString(), out var status) ? status.ToString() : string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>Transfers ownership once its dialog is hidden, and says what happened.</summary>
    private async Task OnTransferHiddenAsync()
    {
        if (!pendingTransfer || transferForm.NewOwnerId is not { } id)
            return;

        pendingTransfer = false;
        var target = State.FindMember(id);
        var name = target?.Name ?? "That member";
        var result = State.TransferOwnership(id);

        switch (result)
        {
            case AiResult.Ok:
                SetStatus($"{name} is now the Owner of {State.WorkspaceName}, and you're an Admin. They and the other Admins heard about it.", false);
                transferForm = new TransferForm();
                transferContext = new EditContext(transferForm);
                break;

            case AiResult.OwnRole:
                ShowError("You're already the Owner: choose someone else to hand the role to. Nothing changed.");
                break;

            case AiResult.WrongState:
                ShowError($"{name} can't become the Owner: the Owner must be an active member who can sign in. Nothing changed.");
                break;

            case AiResult.NotFound:
                ShowError("That member is no longer in the workspace. Nothing changed.");
                break;

            case AiResult.NotAllowed:
                ShowError("Only the Owner transfers ownership. Nothing changed.");
                break;

            default:
                ShowError("That didn't work, and nothing changed. Try again.");
                break;
        }

        await RefreshGridAsync();
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    /// <summary>Opens the dialog of one person, with their current role chosen.</summary>
    private async Task OpenAsync(int memberId)
    {
        openedId = memberId;
        roleForm = new RoleForm { Role = State.FindMember(memberId)?.Role is { } role && role <= AiMemberRole.Admin ? role : AiMemberRole.Member };
        roleContext = new EditContext(roleForm);
        dialogError = null;
        dialogErrorCount = 0;
        pendingAnnouncement = null;
        await memberModal.ShowAsync();
    }

    /// <summary>The page of people the grid asked for: by name, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<MemberRow>> ProvideMembersAsync(GridDataProviderRequest<MemberRow> request) => view.ApplyAsync(request, Shown);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<MemberRow>();
        gridVersion++;
        SetStatus("The view is reset: by name, 25 a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await AiDisplay.FocusAsync(countRegion);
    }

    private string RowClass(MemberRow row) => selected.Any(item => item.Id == row.Id) ? "pa-row-selected" : string.Empty;

    /// <summary>Runs one of the dialog's access buttons; the outcome is announced once the dialog is hidden.</summary>
    private async Task RunDialogActionAsync(DialogAction action)
    {
        if (Opened is not { } member)
        {
            pendingAnnouncement = ("That person is no longer in the workspace, so nothing changed.", true);
            await memberModal.HideAsync();

            return;
        }

        var result = action switch
        {
            DialogAction.Remove => State.SetAccess(member.Id, false),
            DialogAction.Restore => State.SetAccess(member.Id, true),
            DialogAction.Resend => State.ResendInvitation(member.Id),
            _ => State.RemoveInvitation(member.Id),
        };

        if (result is not (AiResult.Ok or AiResult.NoChange))
        {
            ShowDialogError(MemberRefusal(result, member));

            return;
        }

        pendingAnnouncement = (result == AiResult.NoChange ? $"{member.Name} is already that way, so nothing changed." : action switch
        {
            DialogAction.Remove => $"Removed {member.Name}'s access: they can't use {AiSeedData.ProductName}. Their conversations are kept, and you can restore access.",
            DialogAction.Restore => $"Restored {member.Name}'s access: they can chat again as {RoleWords(member.Role)}.",
            DialogAction.Resend => $"Resent the invitation for {member.Email}. Demo only: no email is sent.",
            _ => $"Withdrew the invitation for {member.Email}. They join as a member if they sign up anyway.",
        }, false);
        await memberModal.HideAsync();
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Saves the dialog's role once its form is valid; the outcome is announced once the dialog is hidden, a refusal in the dialog.</summary>
    private async Task SaveRoleAsync()
    {
        if (!await roleContext.ValidateAsync())
            return;

        if (Opened is not { } member)
        {
            pendingAnnouncement = ("That person is no longer in the workspace, so nothing changed.", true);
            await memberModal.HideAsync();

            return;
        }

        var before = member.Role;
        var result = State.SetRole(member.Id, roleForm.Role);

        switch (result)
        {
            case AiResult.Ok:
                pendingAnnouncement = (member.Status == AiMemberStatus.Invited
                    ? $"{member.Name}'s invitation is now for {RoleWords(roleForm.Role)}: they join as one when they create an account."
                    : $"{member.Name} is now {RoleWords(roleForm.Role)} (was {RoleWords(before)}). {AiDisplay.RoleSummary(roleForm.Role)}", false);
                await memberModal.HideAsync();

                return;

            case AiResult.NoChange:
                pendingAnnouncement = ($"{member.Name} is already {RoleWords(roleForm.Role)}, so nothing changed.", false);
                await memberModal.HideAsync();

                return;

            default:
                ShowDialogError(MemberRefusal(result, member));

                return;
        }
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Shows a refusal in the dialog's alert; the same one again gets a counter, so it is announced again.</summary>
    private void ShowDialogError(string problem)
    {
        dialogErrorCount = dialogError?.StartsWith(problem, StringComparison.Ordinal) == true ? dialogErrorCount + 1 : 1;
        dialogError = dialogErrorCount == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({dialogErrorCount})");
    }

    /// <summary>Shows a refusal in the page's alert; the same one again gets a counter, so it is announced again.</summary>
    private void ShowError(string message)
    {
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({errorCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>The roles the signed-in person may give: up to Admin (Owner comes only through a transfer).</summary>
    private static IReadOnlyList<AiMemberRole> GrantableRoles { get; } = new[] { AiMemberRole.Member, AiMemberRole.PromptEditor, AiMemberRole.Admin };

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || roleFilter.Length > 0;

    private AiMember? Opened => State.FindMember(openedId) is { } member && State.Can(AiPermission.ManageMembers) ? member : null;

    /// <summary>What the bulk removal would do, in words.</summary>
    private string RemoveSummary
    {
        get
        {
            var rows = selected.Select(row => State.FindMember(row.Id)).Where(member => member is not null).ToList();
            var kept = rows.Count(member => IsMe(member!) || member!.Role == AiMemberRole.Owner);
            var invitations = rows.Count(member => member!.Status == AiMemberStatus.Invited);
            var active = rows.Count(member => member!.Status == AiMemberStatus.Active && !IsMe(member) && member.Role != AiMemberRole.Owner);

            return $"Of the selection: {Plural(active, "active member", "active members")} to deactivate, {Plural(invitations, "invitation", "invitations")} to withdraw"
                + (kept > 0 ? $", and {Plural(kept, "person", "people")} (you or the Owner) left as they are." : ".");
        }
    }

    /// <summary>Gets or sets the person to show, from a link like <c>?search=sam.rivera@example.com</c>.</summary>
    [SupplyParameterFromQuery(Name = "search")] public string? SearchQuery { get; set; }

    /// <summary>The people that pass the filters, as grid rows, by name.</summary>
    private IReadOnlyList<MemberRow> Shown
    {
        get
        {
            var term = search.Trim();
            var usage = State.MonthUsageByEmail();

            return State.Members
                .Where(member => statusFilter.Length == 0 || member.Status.ToString() == statusFilter)
                .Where(member => roleFilter.Length == 0 || member.Role.ToString() == roleFilter)
                .Where(member => term.Length == 0 || member.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || member.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(member => new MemberRow(
                    member.Id,
                    member.Name,
                    member.Email,
                    member.Role,
                    AiDisplay.RoleLabel(member.Role),
                    member.Status,
                    AiDisplay.StatusLabel(member.Status),
                    member.JoinedAt,
                    (member.Status == AiMemberStatus.Invited ? "Invited " : string.Empty) + member.JoinedAt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture),
                    usage.TryGetValue(member.Email, out var used) ? used.Tokens : 0,
                    member.AccountId is not null ? "Can sign in" : "No sign-in",
                    IsMe(member)))
                .OrderBy(row => row.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    [Inject] private AiState State { get; set; } = default!;

    private AiMember? TransferTarget => transferForm.NewOwnerId is { } id ? State.FindMember(id) : null;

    #endregion

    #region Nested Types

    /// <summary>What a button of the member dialog does.</summary>
    private enum DialogAction
    {
        Remove,
        Restore,
        Resend,
        Withdraw,
    }

    /// <summary>The bulk bar's action.</summary>
    private sealed class BulkForm
    {
        [Required(ErrorMessage = "Choose what to do with the selected people.")]
        public string Action { get; set; } = string.Empty;
    }

    /// <summary>
    /// The invite fields, with the limits of <see cref="AiState.Invite" />: a required email of the right form, a required
    /// name, and a role up to Admin (a cross-field rule with what the inviter may grant).
    /// </summary>
    private sealed class InviteForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the email address of the person to invite.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer for the email address.")]
        [RegularExpression(@"^\s*[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}\s*$", ErrorMessage = "Enter the email address like name@example.com.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter their name, as members will see it.")]
        [StringLength(ShowcaseIdentity.MaxNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        public AiMemberRole Role { get; set; } = AiMemberRole.Member;

        /// <summary>Runs once the fields are filled in: the role must be one an Admin or the Owner may give.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(Role) || Role > AiMemberRole.Admin)
                yield return new ValidationResult("Choose Member, Prompt editor, or Admin: Owner comes only through a transfer.", new[] { nameof(Role) });
        }
    }

    /// <summary>One person as the grid shows and exports them: every column is a property, so CSV export includes it.</summary>
    private sealed record MemberRow(
        int Id,
        string Name,
        string Email,
        AiMemberRole RoleValue,
        string Role,
        AiMemberStatus StatusValue,
        string Status,
        DateTime Joined,
        string JoinedText,
        long Tokens,
        string SignIn,
        bool IsYou);

    /// <summary>The member dialog's role.</summary>
    private sealed class RoleForm : IValidatableObject
    {
        public AiMemberRole Role { get; set; } = AiMemberRole.Member;

        /// <summary>The role must be one an Admin or the Owner may give.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(Role) || Role > AiMemberRole.Admin)
                yield return new ValidationResult("Choose Member, Prompt editor, or Admin: Owner comes only through a transfer.", new[] { nameof(Role) });
        }
    }

    /// <summary>The ownership transfer: a new Owner, and the workspace name typed to confirm (a rule against the name).</summary>
    private sealed class TransferForm : IValidatableObject
    {
        [Required(ErrorMessage = "Type the workspace name to confirm.")]
        public string Confirm { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose who becomes the Owner.")]
        public int? NewOwnerId { get; set; }

        /// <summary>The workspace's name, set by the page before it validates.</summary>
        public string WorkspaceName { get; set; } = string.Empty;

        /// <summary>Runs once the fields are filled in: the typed name must match the workspace's.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Confirm.Trim().Length > 0 && !string.Equals(Confirm.Trim(), WorkspaceName, StringComparison.OrdinalIgnoreCase))
                yield return new ValidationResult($"Type \"{WorkspaceName}\" exactly, to confirm the transfer.", new[] { nameof(Confirm) });
        }
    }

    #endregion
}
