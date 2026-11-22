using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.AiAssistant;

/// <summary>
/// The sensitive data review of the Parley back office (Admins and the Owner, AC-7i): the policy (each pattern off, redact,
/// or block; <see cref="AiState.SaveDlpPolicy" />), and the events it raised in a <see cref="Grid{TItem}" /> with a saved
/// view: when, member, pattern, action taken, the masked match, the conversation (a link only for one's own: members' chats
/// are private), status (New, Dismissed, Policy changed), and reviewer, with search, status and pattern filters, sorting,
/// paging, the column chooser, and CSV export. A row opens the review dialog (rule WR-002: a decision is required; adjusting
/// needs a choice other than the current policy; a note of up to 200 characters): dismiss it, or adjust the policy for its
/// kind of data from the review (<see cref="AiState.AdjustDlpFromReview" />). The bulk bar dismisses the selection with one
/// note (<see cref="AiState.DismissDlpEvents" />). Outcomes are announced in the status region.
/// </summary>
public partial class AdminSensitiveData : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string AdjustValue = "adjust";

    private const string BlockValue = "block";

    private const int DefaultPageSize = 25;

    private const string DismissValue = "dismiss";

    private const string OffValue = "off";

    private const string RedactValue = "redact";

    /// <summary>The rows' buttons, by event number, so focus returns to one when its dialog closes.</summary>
    private readonly Dictionary<int, ElementReference> openers = new();

    private EditContext bulkContext;

    private NoteForm bulkForm = new();

    private ElementReference? countRegion;

    private ElementReference? dialogCancel;

    private int dialogErrorCount;

    private string? dialogError;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private Grid<EventRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private string kindFilter = string.Empty;

    /// <summary>The event the dialog is about.</summary>
    private int openedId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>The outcome of the dialog, announced once it is hidden.</summary>
    private string? pendingAnnouncement;

    private EditContext policyContext = default!;

    private PolicyForm policyForm = new();

    private EditContext reviewContext;

    private ReviewForm reviewForm = new();

    private Modal reviewModal = default!;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    /// <summary>The events selected in the grid (on its current page).</summary>
    private List<EventRow> selected = new();

    private int statusCount;

    private string statusFilter = string.Empty;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Constructors and Finalizers

    /// <summary>Starts with empty review and bulk forms; the policy form is filled from the policy.</summary>
    public AdminSensitiveData()
    {
        bulkContext = new EditContext(bulkForm);
        reviewContext = new EditContext(reviewForm);
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
        view = GridViews.Get(State.SiteSlug, "sensitive-data", DefaultPageSize, "when", SortDirection.Descending);
        State.Changed += OnStateChanged;
        LoadPolicy();
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private static string ActionText(AiDlpAction action) => action == AiDlpAction.Block ? "Blocked (not sent)" : "Redacted (sent without it)";

    private static string ChoiceOf(AiDlpRule rule) => !rule.Enabled ? OffValue : rule.Action == AiDlpAction.Block ? BlockValue : RedactValue;

    private static string KindHelp(AiDlpKind kind) => kind switch
    {
        AiDlpKind.PaymentCard => "13 to 19 digits, spaces or dashes allowed, that pass the card check (Luhn), like 4242 4242 4242 4242.",
        AiDlpKind.NationalId => "A US Social Security number like 123-45-6789; impossible ranges (000, 666, 900 and up) are left out.",
        AiDlpKind.Iban => "Two letters, two check digits, and the account, like DE89 3704 0044 0532 0130 00, when the check digits are right.",
        _ => "A secret key starting with sk- and 20 or more letters, digits, dashes, or underscores.",
    };

    private static string RuleText(AiDlpRule rule) => !rule.Enabled ? "Off: not checked" : rule.Action == AiDlpAction.Block ? "Block the message" : "Redact the match";

    private static BadgeColor StatusColor(AiDlpStatus status) => status switch
    {
        AiDlpStatus.New => BadgeColor.Warning,
        AiDlpStatus.PolicyChanged => BadgeColor.Info,
        _ => BadgeColor.Secondary,
    };

    private static string StatusText(AiDlpStatus status) => status switch
    {
        AiDlpStatus.New => "New",
        AiDlpStatus.PolicyChanged => "Policy changed",
        _ => "Dismissed",
    };

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        statusFilter = kindFilter = string.Empty;
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

        selected = new List<EventRow>();
        ClearError();
        SetStatus("Selection cleared.");
        focusTarget = () => statusRegion;
    }

    private string CountText(int total)
    {
        var shown = Shown.Count;

        return shown == total ? $"{AdminMembers.Plural(total, "event")}." : $"Showing {shown.ToString(CultureInfo.InvariantCulture)} of {AdminMembers.Plural(total, "event")}.";
    }

    /// <summary>Dismisses the selection's new events in one <see cref="AiState.DismissDlpEvents" /> call once the bar's form is valid.</summary>
    private async Task DismissSelectedAsync()
    {
        if (!await bulkContext.ValidateAsync())
            return;

        ClearError();
        var outcome = State.DismissDlpEvents(selected.Select(row => row.Id).ToList(), bulkForm.Note);

        switch (outcome.Result)
        {
            case AiResult.Ok:
                SetStatus($"Dismissed {AdminMembers.Plural(outcome.Changed, "event")}." + (outcome.Unchanged > 0 ? $" {AdminMembers.Plural(outcome.Unchanged, "was", "were")} reviewed already, and left as they were." : null));
                break;

            case AiResult.NoChange:
                SetStatus("Those events were reviewed already, so nothing changed.");
                break;

            case AiResult.NotAllowed:
                ShowError("Only Admins and the Owner review sensitive data. Nothing changed.");
                break;

            default:
                ShowError($"Keep the note to {AiState.MaxReviewNoteLength} characters. Nothing changed.");
                break;
        }

        bulkForm = new NoteForm();
        bulkContext = new EditContext(bulkForm);

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<EventRow>();
        focusTarget = () => statusRegion;
    }

    /// <summary>A filter changed: back to the first page of the filtered events.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>Fills the policy form from the saved policy.</summary>
    private void LoadPolicy()
    {
        policyForm = new PolicyForm { Rules = State.Policy.DlpRules.Select(rule => new RuleChoice { Kind = rule.Kind, Choice = ChoiceOf(rule) }).ToList() };
        policyContext = new EditContext(policyForm);
    }

    private Task OnDialogShownAsync() => AiDisplay.FocusAsync(dialogCancel);

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: after a review focus moves to the announcement, otherwise back to the row.</summary>
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

        return Task.CompletedTask;
    }

    private async Task OnKindFilterChangedAsync(ChangeEventArgs args)
    {
        kindFilter = Enum.TryParse<AiDlpKind>(args.Value?.ToString(), out var kind) ? kind.ToString() : string.Empty;
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

    private void OnSelectionChanged(HashSet<EventRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            ClearError();
    }

    /// <summary>Another screen or a change here changed the state: the grid reads the events again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStatusFilterChangedAsync(ChangeEventArgs args)
    {
        statusFilter = Enum.TryParse<AiDlpStatus>(args.Value?.ToString(), out var status) ? status.ToString() : string.Empty;
        await FiltersChangedAsync();
    }

    /// <summary>Opens the review dialog of one event: Dismiss chosen for a new one, the current policy chosen for an adjustment.</summary>
    private async Task OpenAsync(int eventId)
    {
        openedId = eventId;
        var item = State.FindDlpEvent(eventId);
        var rule = item is null ? null : State.Policy.DlpRules.First(candidate => candidate.Kind == item.Kind);
        reviewForm = new ReviewForm
        {
            Decision = item is { Status: AiDlpStatus.New } ? DismissValue : AdjustValue,
            Choice = rule is null ? RedactValue : ChoiceOf(rule) == BlockValue ? RedactValue : ChoiceOf(rule) == RedactValue ? BlockValue : RedactValue,
        };
        reviewContext = new EditContext(reviewForm);
        dialogError = null;
        dialogErrorCount = 0;
        pendingAnnouncement = null;
        await reviewModal.ShowAsync();
    }

    /// <summary>The page of events the grid asked for: the newest first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<EventRow>> ProvideRowsAsync(GridDataProviderRequest<EventRow> request) => view.ApplyAsync(request, Shown);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private void ResetPolicy()
    {
        LoadPolicy();
        SetStatus("The policy form is back to the saved policy.");
        focusTarget = () => statusRegion;
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<EventRow>();
        gridVersion++;
        SetStatus("The view is reset: the newest first, 25 a page, and every column shown.");
        StateHasChanged();

        await Task.Yield();
        await AiDisplay.FocusAsync(countRegion);
    }

    /// <summary>Saves the dialog's review once its form is valid; the outcome is announced once the dialog is hidden, a refusal in the dialog.</summary>
    private async Task ReviewAsync()
    {
        if (Opened is not { } item)
        {
            pendingAnnouncement = "That event is no longer in the queue, so nothing changed.";
            await reviewModal.HideAsync();

            return;
        }

        reviewForm.CurrentChoice = ChoiceOf(State.Policy.DlpRules.First(rule => rule.Kind == item.Kind));

        if (!await reviewContext.ValidateAsync())
            return;

        var kind = AiDlp.KindLabel(item.Kind);

        if (reviewForm.Decision == DismissValue)
        {
            var outcome = State.DismissDlpEvents(new[] { item.Id }, reviewForm.Note);

            if (outcome.Result is AiResult.Ok or AiResult.NoChange)
            {
                pendingAnnouncement = outcome.Result == AiResult.Ok ? $"Dismissed event {item.Id.ToString(CultureInfo.InvariantCulture)} ({kind.ToLowerInvariant()} from {MemberName(item)})." : "That event was reviewed already, so nothing changed.";
                await reviewModal.HideAsync();

                return;
            }

            ShowDialogError(outcome.Result == AiResult.NotAllowed ? "Only Admins and the Owner review sensitive data." : $"Keep the note to {AiState.MaxReviewNoteLength} characters.");

            return;
        }

        var enabled = reviewForm.Choice != OffValue;
        var action = reviewForm.Choice == BlockValue ? AiDlpAction.Block : AiDlpAction.Redact;
        var result = State.AdjustDlpFromReview(item.Id, enabled, action, reviewForm.Note);

        switch (result)
        {
            case AiResult.Ok:
                LoadPolicy();
                pendingAnnouncement = $"The policy for {kind.ToLowerInvariant()} now says: {RuleText(State.Policy.DlpRules.First(rule => rule.Kind == item.Kind)).ToLowerInvariant()}. Event {item.Id.ToString(CultureInfo.InvariantCulture)} is marked \"Policy changed\".";
                await reviewModal.HideAsync();

                return;

            case AiResult.NoChange:
                ShowDialogError("That's the policy already. Choose another option, or dismiss the event.");

                return;

            case AiResult.NotAllowed:
                ShowDialogError("Only Admins and the Owner change the sensitive data policy.");

                return;

            default:
                ShowDialogError($"Keep the note to {AiState.MaxReviewNoteLength} characters.");

                return;
        }
    }

    private string MemberName(AiDlpEvent item) => State.FindMember(item.MemberId)?.Name ?? "a former member";

    private string RowClass(EventRow row) => selected.Any(item => item.Id == row.Id) ? "pa-row-selected" : string.Empty;

    /// <summary>Saves the policy form; the state's answer goes to the status line or the alert, which take focus.</summary>
    private async Task SavePolicy()
    {
        if (!await policyContext.ValidateAsync())
            return;

        ClearError();
        var rules = policyForm.Rules.Select(rule => new AiDlpRule
        {
            Kind = rule.Kind, Enabled = rule.Choice != OffValue,
            Action = rule.Choice == BlockValue ? AiDlpAction.Block : rule.Choice == RedactValue ? AiDlpAction.Redact : State.Policy.DlpRules.First(saved => saved.Kind == rule.Kind).Action,
        }).ToList();
        var result = State.SaveDlpPolicy(rules);

        switch (result)
        {
            case AiResult.Ok:
                SetStatus("The policy is saved: " + string.Join("; ", State.Policy.DlpRules.Select(rule => $"{AiDlp.KindLabel(rule.Kind)}: {RuleText(rule).ToLowerInvariant()}")) + ".");
                LoadPolicy();
                break;

            case AiResult.NoChange:
                SetStatus("Nothing changed: this is already the policy.");
                break;

            case AiResult.NotAllowed:
                ShowError("Only Admins and the Owner change the sensitive data policy. Nothing changed.");
                break;

            default:
                ShowError("The policy couldn't be saved. Nothing changed.");
                break;
        }

        focusTarget = () => statusRegion;
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
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

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>What a pattern can do: redact, block, or nothing.</summary>
    private static IReadOnlyList<(string Value, string Text)> Choices { get; } = new[] { (RedactValue, "Redact"), (BlockValue, "Block"), (OffValue, "Off") };

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || statusFilter.Length > 0 || kindFilter.Length > 0;

    private AiDlpEvent? Opened => State.FindDlpEvent(openedId);

    /// <summary>The events that pass the filters, as grid rows, the newest first.</summary>
    private IReadOnlyList<EventRow> Shown
    {
        get
        {
            var term = search.Trim();
            var me = State.Identity.Current?.Id;

            return State.DlpEvents
                .Where(item => statusFilter.Length == 0 || item.Status.ToString() == statusFilter)
                .Where(item => kindFilter.Length == 0 || item.Kind.ToString() == kindFilter)
                .Select(item => (Event: item, Member: State.FindMember(item.MemberId)))
                .Where(pair => term.Length == 0 || (pair.Member?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                    || pair.Event.ConversationTitle.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(pair => new EventRow(
                    pair.Event.Id,
                    pair.Event.At,
                    AiDisplay.Stamp(pair.Event.At),
                    pair.Member?.Name ?? "A former member",
                    AiDlp.KindLabel(pair.Event.Kind),
                    ActionText(pair.Event.ActionTaken),
                    pair.Event.Masked,
                    pair.Event.ConversationTitle.Length == 0 ? "None (not sent)" : pair.Event.ConversationTitle,
                    pair.Event.ConversationId is { } conversationId && pair.Member?.AccountId == me && State.FindConversation(conversationId) is not null ? AiDisplay.ConversationUrl(conversationId) : null,
                    pair.Event.Status,
                    StatusText(pair.Event.Status),
                    pair.Event.ReviewedBy ?? "Not yet"))
                .ToList();
        }
    }

    [Inject] private AiState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One sensitive data event as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record EventRow(
        int Id,
        DateTime When,
        string WhenText,
        string Member,
        string Kind,
        string Action,
        string Masked,
        string Conversation,
        string? ConversationUrl,
        AiDlpStatus StatusValue,
        string Status,
        string Reviewer);

    /// <summary>A note kept with the reviewed events.</summary>
    private sealed class NoteForm
    {
        [StringLength(AiState.MaxReviewNoteLength, ErrorMessage = "Use 200 characters or fewer for the note.")]
        public string? Note { get; set; }
    }

    /// <summary>The policy form: one choice per kind of data (redact, block, or off).</summary>
    private sealed class PolicyForm
    {
        public List<RuleChoice> Rules { get; set; } = new();
    }

    /// <summary>
    /// A review: a decision (required), the new choice for the pattern when adjusting it (other than the current one: a
    /// cross-field rule against the policy), and a note.
    /// </summary>
    private sealed class ReviewForm : IValidatableObject
    {
        public string Choice { get; set; } = RedactValue;

        /// <summary>The pattern's current choice, set by the page before it validates.</summary>
        public string CurrentChoice { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose whether to dismiss the event or adjust the policy.")]
        public string Decision { get; set; } = string.Empty;

        [StringLength(AiState.MaxReviewNoteLength, ErrorMessage = "Use 200 characters or fewer for the note.")]
        public string? Note { get; set; }

        /// <summary>Runs on submit: an adjustment must change the policy.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Decision == AdjustValue && Choice == CurrentChoice)
                yield return new ValidationResult("That's the policy already. Choose another option, or dismiss the event.", new[] { nameof(Choice) });
        }
    }

    /// <summary>One pattern's choice in the policy form.</summary>
    private sealed class RuleChoice
    {
        public string Choice { get; set; } = RedactValue;

        public AiDlpKind Kind { get; set; }
    }

    #endregion
}
