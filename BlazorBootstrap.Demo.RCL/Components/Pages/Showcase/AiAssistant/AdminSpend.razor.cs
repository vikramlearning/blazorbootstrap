using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// Spend of the Parley back office (Admins and the Owner, AC-4): the workspace's tokens this month against its cap (a
/// progress bar and the same in words), an <see cref="EditForm" /> for the workspace cap and the default member cap (rule
/// WR-002: ranges, the workspace cap above what is used and at least every member's own cap, the default at most the
/// workspace cap), a "Demo: use tokens" control (<see cref="AiState.AddDemoUsage" />, <see cref="AiState.AddWorkspaceDemoUsage" />),
/// and the members in a <see cref="Grid{TItem}" /> with a saved view: used, cap (own or default), share used, and status
/// (OK, Warning, Blocked) in words, with search and filters, sorting, paging, the column chooser, and CSV export. A row opens
/// the member's cap dialog (their own cap above what they used, or the default), and the bulk bar sets or resets the
/// selection's caps (<see cref="AiState.BulkSetMemberCap" />). Every refusal of the state is mapped to its field or a message.
/// </summary>
public partial class AdminSpend : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    private const string DefaultValue = "default";

    private const string OwnValue = "own";

    private const string SetValue = "set";

    /// <summary>The rows' buttons, by member number, so focus returns to one when its dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    private EditContext bulkContext;

    private BulkForm bulkForm = new();

    private string capFilter = string.Empty;

    private Modal capModal = default!;

    private EditContext capsContext = default!;

    /// <summary>The state's answers that belong to a caps field, shown like the form's own messages.</summary>
    private ValidationMessageStore? capsErrors;

    private CapsForm capsForm = new();

    private ElementReference? countRegion;

    /// <summary>The member the demo control pushes.</summary>
    private int demoMemberId = 4;

    private ElementReference? dialogCancel;

    private int dialogErrorCount;

    private string? dialogError;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private Grid<SpendRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string levelFilter = string.Empty;

    private EditContext memberContext;

    private MemberCapForm memberForm = new();

    /// <summary>The member the dialog is about.</summary>
    private int openedId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The outcome of the dialog, announced once it is hidden.</summary>
    private string? pendingAnnouncement;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The members selected in the grid (on its current page).</summary>
    private List<SpendRow> selected = new();

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with empty dialog and bulk forms; the caps form is filled from the policy.</summary>
    public AdminSpend()
    {
        bulkContext = new EditContext(bulkForm);
        memberContext = new EditContext(memberForm);
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
        view = GridViews.Get(State.SiteSlug, "spend", DefaultPageSize, "percent", SortDirection.Descending);
        State.Changed += OnStateChanged;
        LoadCaps();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state and form changes.</summary>
    public void Dispose()
    {
        State.Changed -= OnStateChanged;
        capsContext.OnFieldChanged -= OnCapsFieldChanged;
    }

    #endregion

    #region Private Methods

    private static BadgeColor LevelColor(AiSpendLevel level) => level switch
    {
        AiSpendLevel.Blocked => BadgeColor.Danger,
        AiSpendLevel.Warning => BadgeColor.Warning,
        _ => BadgeColor.Success,
    };

    private static string LevelText(AiSpendLevel level) => level switch
    {
        AiSpendLevel.Blocked => "Blocked",
        AiSpendLevel.Warning => "Warning",
        _ => "OK",
    };

    private static ProgressColor ProgressColorOf(AiSpendLevel level) => level switch
    {
        AiSpendLevel.Blocked => ProgressColor.Danger,
        AiSpendLevel.Warning => ProgressColor.Warning,
        _ => ProgressColor.None,
    };

    private static string PercentText(AiSpendStatus status) => status.Percent.ToString("0", CultureInfo.InvariantCulture) + "%";

    private static string WorkspaceText(AiSpendStatus status) => status.Level switch
    {
        AiSpendLevel.Blocked => $"The workspace used {PercentText(status)} of its cap: chat is paused for everyone until you raise it.",
        AiSpendLevel.Warning => $"The workspace used {PercentText(status)} of its cap, {AiDisplay.Tokens(status.Left)} tokens left. At 100% chat pauses for everyone.",
        _ => $"The workspace used {PercentText(status)} of its cap, {AiDisplay.Tokens(status.Left)} tokens left.",
    };

    /// <summary>Sets or resets the selection's caps in one <see cref="AiState.BulkSetMemberCap" /> call once the bar's form is valid.</summary>
    private async Task ApplyBulkAsync()
    {
        bulkForm.WorkspaceCap = State.Policy.WorkspaceMonthlyCap;

        if (!await bulkContext.ValidateAsync())
            return;

        ClearError();
        var cap = bulkForm.Mode == DefaultValue ? null : bulkForm.Cap;
        var outcome = State.BulkSetMemberCap(selected.Select(row => row.Id).ToList(), cap);
        var target = cap is { } value ? $"{AiDisplay.Tokens(value)} tokens a month" : "the default cap";
        var below = outcome.Protected > 0 ? $" {AdminMembers.Plural(outcome.Protected, "member already used", "members already used")} more than that, so they keep their cap." : null;
        var same = outcome.Unchanged > 0 ? $" {AdminMembers.Plural(outcome.Unchanged, "member already had", "members already had")} it." : null;

        switch (outcome.Result)
        {
            case AiResult.Ok:
                SetStatus($"Set {AdminMembers.Plural(outcome.Changed, "member's cap", "members' caps")} to {target}. Each member with a sign-in hears about it." + same + below);
                break;

            case AiResult.NoChange:
                SetStatus("No caps changed." + same + below);
                break;

            case AiResult.AboveWorkspaceCap:
                ShowError($"A member's cap can't be above the workspace cap ({AiDisplay.Tokens(State.Policy.WorkspaceMonthlyCap)}). Nothing changed.");
                break;

            case AiResult.NotAllowed:
                ShowError("Only Admins and the Owner set caps. Nothing changed.");
                break;

            default:
                ShowError($"Use from {AiDisplay.Tokens(AiState.MinQuota)} to {AiDisplay.Tokens(AiState.MaxQuota)} tokens. Nothing changed.");
                break;
        }

        bulkForm = new BulkForm();
        bulkContext = new EditContext(bulkForm);

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<SpendRow>();
        focusTarget = () => statusRegion;
        StateHasChanged();
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        levelFilter = capFilter = string.Empty;
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

        selected = new List<SpendRow>();
        ClearError();
        SetStatus("Selection cleared.");
        focusTarget = () => statusRegion;
    }

    private string CountText(int total)
    {
        var shown = Shown.Count;

        return shown == total ? $"{AdminMembers.Plural(total, "member")} (invitations have no usage)." : $"Showing {shown.ToString(CultureInfo.InvariantCulture)} of {AdminMembers.Plural(total, "member")}.";
    }

    /// <summary>A filter changed: back to the first page of the filtered members.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Fills the caps form from the policy.</summary>
    private void LoadCaps()
    {
        if (capsContext is not null)
            capsContext.OnFieldChanged -= OnCapsFieldChanged;

        capsForm = new CapsForm { WorkspaceCap = State.Policy.WorkspaceMonthlyCap, DefaultCap = State.Policy.MonthlyTokenQuota };
        capsContext = new EditContext(capsForm);
        capsErrors = new ValidationMessageStore(capsContext);
        capsContext.OnFieldChanged += OnCapsFieldChanged;
    }

    /// <summary>A change clears the state's earlier answer on that field; no field changes another, so nothing re-enters.</summary>
    private void OnCapsFieldChanged(object? sender, FieldChangedEventArgs args) => capsErrors?.Clear(args.FieldIdentifier);

    private async Task OnCapFilterChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString();
        capFilter = value is OwnValue or DefaultValue ? value : string.Empty;
        await FiltersChangedAsync();
    }

    private void OnDemoMemberChanged(ChangeEventArgs args) => demoMemberId = int.TryParse(args.Value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : demoMemberId;

    private Task OnDialogShownAsync() => AiDisplay.FocusAsync(dialogCancel);

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: after a change focus moves to the announcement, otherwise back to the row.</summary>
    private Task OnDialogHiddenAsync()
    {
        var id = openedId;

        if (pendingAnnouncement is { } announcement)
        {
            pendingAnnouncement = null;
            SetStatus(announcement);
            focusTarget = () => statusRegion;
        }
        else
        {
            focusTarget = () => openers.TryGetValue(id, out var opener) ? opener : statusRegion;
        }

        StateHasChanged();

        return Task.CompletedTask;
    }

    private async Task OnLevelFilterChangedAsync(ChangeEventArgs args)
    {
        levelFilter = Enum.TryParse<AiSpendLevel>(args.Value?.ToString(), out var level) ? level.ToString() : string.Empty;
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

    private void OnSelectionChanged(HashSet<SpendRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            ClearError();
    }

    /// <summary>Another screen or a change here changed the state: the grid reads the members again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>Opens the cap dialog of one member, with their own cap (or the default) filled in.</summary>
    private async Task OpenAsync(int memberId)
    {
        openedId = memberId;
        var member = State.FindMember(memberId);
        memberForm = new MemberCapForm { UseDefault = member?.MonthlyCap is null, Cap = member?.MonthlyCap };
        memberContext = new EditContext(memberForm);
        dialogError = null;
        dialogErrorCount = 0;
        pendingAnnouncement = null;
        await capModal.ShowAsync();
    }

    /// <summary>The page of members the grid asked for: the most used share first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<SpendRow>> ProvideRowsAsync(GridDataProviderRequest<SpendRow> request) => view.ApplyAsync(request, Shown);

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
        selected = new List<SpendRow>();
        gridVersion++;
        SetStatus("The view is reset: the most used share of the cap first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await AiDisplay.FocusAsync(countRegion);
    }

    private string RowClass(SpendRow row) => selected.Any(item => item.Id == row.Id) ? "pa-row-selected" : string.Empty;

    /// <summary>Saves the workspace and default caps once the form is valid; the state's refusals land on their fields.</summary>
    private async Task SaveCaps()
    {
        var workspace = State.WorkspaceSpend;
        capsForm.WorkspaceUsed = workspace.Used;
        capsForm.SavedWorkspaceCap = State.Policy.WorkspaceMonthlyCap;
        capsForm.HighestOwnCap = State.SpendByMember().Keys.Select(id => State.FindMember(id)?.MonthlyCap ?? 0).DefaultIfEmpty(0).Max();
        capsErrors?.Clear();
        capsContext.NotifyValidationStateChanged();

        if (!await capsContext.ValidateAsync())
            return;

        ClearError();
        var wasBlocked = workspace.Level == AiSpendLevel.Blocked;
        var result = State.SaveSpendCaps(capsForm.WorkspaceCap!.Value, capsForm.DefaultCap!.Value);

        switch (result)
        {
            case AiResult.Ok:
                SetStatus($"Saved: the workspace cap is {AiDisplay.Tokens(State.Policy.WorkspaceMonthlyCap)} tokens and the default member cap {AiDisplay.Tokens(State.Policy.MonthlyTokenQuota)}."
                    + (wasBlocked && State.WorkspaceSpend.Level != AiSpendLevel.Blocked ? " Chat is open again for everyone, and they heard about it." : null));
                LoadCaps();
                focusTarget = () => statusRegion;
                break;

            case AiResult.NoChange:
                SetStatus("Nothing changed: these are already the caps.");
                focusTarget = () => statusRegion;
                break;

            case AiResult.BelowUse:
                CapsError(nameof(CapsForm.WorkspaceCap), $"Set the workspace cap above what it used this month ({AiDisplay.Tokens(State.WorkspaceSpend.Used)} tokens).");
                break;

            case AiResult.AboveWorkspaceCap:
                CapsError(nameof(CapsForm.DefaultCap), "The default member cap can't be above the workspace cap.");
                break;

            case AiResult.WrongState:
                CapsError(nameof(CapsForm.WorkspaceCap), "A member's own cap is above this. Lower their cap first, or set the workspace cap higher.");
                break;

            case AiResult.NotAllowed:
                ShowError("Only Admins and the Owner set caps. Nothing changed.");
                break;

            default:
                ShowError("Those caps are out of range. Nothing changed.");
                break;
        }
    }

    private async Task SaveCapsAsync()
    {
        await SaveCaps();
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Saves the dialog's cap once its form is valid; the outcome is announced once the dialog is hidden, a refusal in the dialog.</summary>
    private async Task SaveMemberCapAsync()
    {
        if (Opened is not { } member)
        {
            pendingAnnouncement = "That member is no longer in the workspace, so nothing changed.";
            await capModal.HideAsync();

            return;
        }

        var status = State.SpendOf(member);
        memberForm.Used = status.Used;
        memberForm.WorkspaceCap = State.Policy.WorkspaceMonthlyCap;

        if (!await memberContext.ValidateAsync())
            return;

        var wasBlocked = status.Level == AiSpendLevel.Blocked;
        var result = State.SetMemberCap(member.Id, memberForm.UseDefault ? null : memberForm.Cap);

        switch (result)
        {
            case AiResult.Ok:
                var after = State.SpendOf(member);
                pendingAnnouncement = $"{member.Name}'s cap is {(member.MonthlyCap is null ? "the default, " : null)}{AiDisplay.Tokens(after.Cap)} tokens a month ({PercentText(after)} used)."
                    + (wasBlocked && after.Level != AiSpendLevel.Blocked ? " They can chat again." : after.Level == AiSpendLevel.Blocked ? " They're still blocked: the cap is at or below what they used." : null)
                    + (member.AccountId is null ? null : " They heard about it.");
                await capModal.HideAsync();

                return;

            case AiResult.NoChange:
                pendingAnnouncement = $"{member.Name} already has that cap, so nothing changed.";
                await capModal.HideAsync();

                return;

            case AiResult.BelowUse:
                ShowDialogError($"Set the cap above what {member.Name} used this month ({AiDisplay.Tokens(status.Used)} tokens).");

                return;

            case AiResult.AboveWorkspaceCap:
                ShowDialogError($"A member's cap can't be above the workspace cap ({AiDisplay.Tokens(State.Policy.WorkspaceMonthlyCap)}).");

                return;

            case AiResult.WrongState:
                ShowDialogError($"{member.Name} hasn't joined yet, so there's no usage to cap.");

                return;

            case AiResult.NotAllowed:
                ShowDialogError("Only Admins and the Owner set caps. Nothing changed.");

                return;

            default:
                ShowDialogError($"Use from {AiDisplay.Tokens(AiState.MinQuota)} to {AiDisplay.Tokens(AiState.MaxQuota)} tokens.");

                return;
        }
    }

    /// <summary>Shows a state answer under a caps field, like the form's own messages; the summary and focus follow.</summary>
    private void CapsError(string fieldName, string message)
    {
        capsErrors?.Clear();
        capsErrors?.Add(new FieldIdentifier(capsForm, fieldName), message);
        capsContext.NotifyValidationStateChanged();
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
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

    /// <summary>Pushes the chosen member's usage up by a tenth of their cap (the demo control).</summary>
    private void UseMemberTokens()
    {
        ClearError();

        if (State.FindMember(demoMemberId) is not { } member)
        {
            ShowError("Choose a member first.");

            return;
        }

        var tokens = Math.Max(1, State.CapOf(member) / 10);
        var result = State.AddDemoUsage(member.Id, tokens);

        if (result != AiResult.Ok)
        {
            ShowError(result == AiResult.NotAllowed ? "Only Admins and the Owner use the demo control." : "The demo usage couldn't be added: this tab's storage is full.");

            return;
        }

        var status = State.SpendOf(member);
        SetStatus($"Added {AiDisplay.Tokens(tokens)} demo tokens to {member.Name}: {PercentText(status)} of their cap used, {LevelText(status.Level)}."
            + (status.Level == AiSpendLevel.Blocked ? " Their chat is paused until you raise their cap." : status.Level == AiSpendLevel.Warning ? " Their chat shows a warning." : null));
    }

    /// <summary>Pushes the workspace's usage up by a twentieth of its cap (the demo control).</summary>
    private void UseWorkspaceTokens()
    {
        ClearError();
        var result = State.AddWorkspaceDemoUsage(out var tokens);

        if (result != AiResult.Ok)
        {
            ShowError(result == AiResult.NotAllowed ? "Only Admins and the Owner use the demo control." : "The demo usage couldn't be added: this tab's storage is full.");

            return;
        }

        var status = State.WorkspaceSpend;
        SetStatus($"Added {AiDisplay.Tokens(tokens)} demo tokens to the workspace. {WorkspaceText(status)}");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>The members the demo control can push: the active ones, the demo accounts first.</summary>
    private IReadOnlyList<AiMember> DemoMembers => State.Members.Where(member => member.Status == AiMemberStatus.Active)
        .OrderBy(member => member.AccountId is null).ThenBy(member => member.Name, StringComparer.OrdinalIgnoreCase).ToList();

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || levelFilter.Length > 0 || capFilter.Length > 0;

    private AiMember? Opened => State.Can(AiPermission.ManageSpend) ? State.FindMember(openedId) : null;

    /// <summary>The day the monthly caps start again, like "December 1".</summary>
    private string ResetText => new DateTime(State.Clock.Now.Year, State.Clock.Now.Month, 1).AddMonths(1).ToString("MMMM d", CultureInfo.InvariantCulture);

    /// <summary>The members that pass the filters, as grid rows, the most used share first.</summary>
    private IReadOnlyList<SpendRow> Shown
    {
        get
        {
            var term = search.Trim();
            var spend = State.SpendByMember();

            return State.Members
                .Where(member => spend.ContainsKey(member.Id))
                .Where(member => capFilter.Length == 0 || (member.MonthlyCap is not null) == (capFilter == OwnValue))
                .Where(member => levelFilter.Length == 0 || spend[member.Id].Level.ToString() == levelFilter)
                .Where(member => term.Length == 0 || member.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || member.Email.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(member =>
                {
                    var status = spend[member.Id];

                    return new SpendRow(member.Id, member.Name, member.Email, AiDisplay.RoleLabel(member.Role), status.Used, status.Cap,
                        $"{status.Cap.ToString(CultureInfo.InvariantCulture)} ({(member.MonthlyCap is null ? "default" : "own")})", member.MonthlyCap is not null, status.Percent,
                        PercentText(status), status.Level, LevelText(status.Level));
                })
                .OrderByDescending(row => row.Percent)
                .ToList();
        }
    }

    [Inject] private AiState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The bulk bar's cap: a value (required while setting one) in range and at most the workspace cap, or the default.</summary>
    private sealed class BulkForm : IValidatableObject
    {
        [Range(AiState.MinQuota, AiState.MaxQuota, ErrorMessage = "Use from 1,000 to 10,000,000 tokens.")]
        public int? Cap { get; set; }

        public string Mode { get; set; } = SetValue;

        /// <summary>The workspace cap, set by the page before it validates.</summary>
        public int WorkspaceCap { get; set; }

        /// <summary>Runs on submit: a value while setting one, at most the workspace cap.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Mode == SetValue && Cap is null)
                yield return new ValidationResult("Enter the cap in tokens a month, or choose the default cap.", new[] { nameof(Cap) });
            else if (Mode == SetValue && Cap > WorkspaceCap)
                yield return new ValidationResult(FormattableString.Invariant($"A member's cap can't be above the workspace cap ({WorkspaceCap:#,0} tokens)."), new[] { nameof(Cap) });
        }
    }

    /// <summary>
    /// The workspace and default caps, with the limits of <see cref="AiState.SaveSpendCaps" />: required ranges, the workspace
    /// cap above what is used (when it changes) and at least every member's own cap, and the default at most the workspace cap.
    /// </summary>
    private sealed class CapsForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the default member cap.")]
        [Range(AiState.MinQuota, AiState.MaxQuota, ErrorMessage = "Use from 1,000 to 10,000,000 tokens for the default member cap.")]
        public int? DefaultCap { get; set; }

        /// <summary>The highest member's own cap, set by the page before it validates.</summary>
        public int HighestOwnCap { get; set; }

        /// <summary>The saved workspace cap, set by the page before it validates.</summary>
        public int SavedWorkspaceCap { get; set; }

        [Required(ErrorMessage = "Enter the workspace cap.")]
        [Range(AiState.MinWorkspaceCap, AiState.MaxWorkspaceCap, ErrorMessage = "Use from 10,000 to 100,000,000 tokens for the workspace cap.")]
        public int? WorkspaceCap { get; set; }

        /// <summary>What the workspace used this month, set by the page before it validates.</summary>
        public long WorkspaceUsed { get; set; }

        /// <summary>Runs once the fields are filled in: the cross-field rules between the two caps and the usage.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (WorkspaceCap is not { } workspace)
                yield break;

            if (workspace != SavedWorkspaceCap && workspace <= WorkspaceUsed)
                yield return new ValidationResult(FormattableString.Invariant($"Set the workspace cap above what it used this month ({WorkspaceUsed:#,0} tokens)."), new[] { nameof(WorkspaceCap) });
            else if (workspace < HighestOwnCap)
                yield return new ValidationResult(FormattableString.Invariant($"A member's own cap is {HighestOwnCap:#,0} tokens: the workspace cap must be at least that."), new[] { nameof(WorkspaceCap) });

            if (DefaultCap > workspace)
                yield return new ValidationResult("The default member cap can't be above the workspace cap.", new[] { nameof(DefaultCap) });
        }
    }

    /// <summary>A member's cap: the default, or their own above what they used and at most the workspace cap.</summary>
    private sealed class MemberCapForm : IValidatableObject
    {
        [Range(AiState.MinQuota, AiState.MaxQuota, ErrorMessage = "Use from 1,000 to 10,000,000 tokens.")]
        public int? Cap { get; set; }

        public bool UseDefault { get; set; }

        /// <summary>What the member used this month, set by the page before it validates.</summary>
        public long Used { get; set; }

        /// <summary>The workspace cap, set by the page before it validates.</summary>
        public int WorkspaceCap { get; set; }

        /// <summary>Runs on submit: their own cap above what they used and within the workspace cap.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UseDefault)
                yield break;

            if (Cap is not { } cap)
                yield return new ValidationResult("Enter their cap in tokens a month, or use the default cap.", new[] { nameof(Cap) });
            else if (cap <= Used)
                yield return new ValidationResult(FormattableString.Invariant($"Set the cap above what they used this month ({Used:#,0} tokens)."), new[] { nameof(Cap) });
            else if (cap > WorkspaceCap)
                yield return new ValidationResult(FormattableString.Invariant($"A member's cap can't be above the workspace cap ({WorkspaceCap:#,0} tokens)."), new[] { nameof(Cap) });
        }
    }

    /// <summary>One member's spend as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record SpendRow(
        int Id,
        string Name,
        string Email,
        string Role,
        long Used,
        long Cap,
        string CapText,
        bool HasOwnCap,
        double Percent,
        string PercentText,
        AiSpendLevel LevelValue,
        string Level);

    #endregion
}
