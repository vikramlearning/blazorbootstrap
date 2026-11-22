using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The operator's sales inbox: every lead in a <see cref="Grid{TItem}" /> (name opening the lead, company, team size,
/// status, received, message, note, and "Create tenant") with paging, sorting, the column chooser, and CSV export, filtered
/// by status (each with its count) and searched by name, company, or email. The grid's view is saved for the visit and can
/// be reset. Rows can be selected and given a status together, with a reason added to each note (required to close).
/// Opening a lead shows its full message in a dialog whose status and note form follows rule WR-002 (a closed lead needs a
/// note); "Add lead" opens a validated form for a lead from a call or an event (one open lead per email, checked as you
/// type). Each <see cref="SaasResult" /> becomes a message. <c>?lead={id}</c> opens that lead, for the overview's and the
/// notifications' links. Focus returns to the lead that opened the dialog, or to the status message when its row is gone.
/// </summary>
public partial class AdminLeads : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const int DefaultPageSize = 25;

    /// <summary>The filter buttons in order; <see langword="null" /> is "All".</summary>
    private static readonly SaasLeadStatus?[] filters =
    {
        null, SaasLeadStatus.New, SaasLeadStatus.Contacted, SaasLeadStatus.Qualified, SaasLeadStatus.Closed,
    };

    private readonly Dictionary<int, ElementReference> openButtons = new();

    private Button? addButton;

    private int? appliedLeadQuery;

    private string? bulkError;

    private int bulkErrors;

    private string bulkReason = string.Empty;

    private string bulkStatus = string.Empty;

    private ElementReference? countRegion;

    /// <summary>The lead just added, opened once the add dialog is gone.</summary>
    private int? createdId;

    private EditContext editContext = default!;

    private ShowcaseUniqueCheck? emailCheck;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private SaasLeadStatus? filter;

    private bool focusStatus;

    private LeadForm form = new();

    private Grid<SaasLead>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private Modal leadModal = default!;

    private EditContext newContext = default!;

    private int newErrorCount;

    private string? newError;

    private NewLeadForm newForm = new();

    private Modal newModal = default!;

    private InputText? newNameInput;

    /// <summary>The state's answers that belong to a field of the add form (an email with an open lead).</summary>
    private ValidationMessageStore? newServerErrors;

    private int noteCount;

    /// <summary>The length of the note as typed, before the change event updates the form.</summary>
    private int noteLength;

    private string? noteMessage;

    private string? noteText;

    /// <summary>The ID of the lead shown in the dialog; 0 when it is closed.</summary>
    private int openId;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    /// <summary>A lead asked for in <c>?lead=</c>, opened once the page is interactive.</summary>
    private int? pendingLeadId;

    /// <summary>Whether the dialog closed after a save, so focus goes to the status message when the lead left the list.</summary>
    private bool saved;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types.</summary>
    private string searchBoxValue = string.Empty;

    private ElementReference searchInput;

    /// <summary>The leads selected in the grid (on its current page).</summary>
    private List<SaasLead> selected = new();

    private InputSelect<SaasLeadStatus>? statusSelect;

    private int statusCount;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <summary>Loads the saved view once the page is interactive, then opens a lead asked for in <c>?lead=</c>.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (pendingLeadId is { } leadId)
        {
            pendingLeadId = null;

            // The parent's after-render runs before the modal's own, which wires up its events: let that go first.
            await Task.Yield();
            await OpenAsync(leadId);
        }

        if (focusStatus)
        {
            focusStatus = false;
            await SaasDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "leads", DefaultPageSize, "received", SortDirection.Descending);
        ResetForm(new LeadForm());
        ResetNewForm();
        State.Changed += OnStateChanged;
    }

    /// <summary>A lead in <c>?lead=</c> opens once; a link to another lead opens that one.</summary>
    protected override void OnParametersSet()
    {
        if (LeadQuery == appliedLeadQuery)
            return;

        appliedLeadQuery = LeadQuery;

        if (LeadQuery is { } leadId && State.FindLead(leadId) is not null)
        {
            // Show every lead, so the one asked for is in the list and focus can return to it.
            filter = null;
            search = searchBoxValue = string.Empty;
            pendingLeadId = leadId;
        }
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

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string CountText(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{Count(count)} {many}";

    private static string CreateTenantPath(int leadId) => string.Create(CultureInfo.InvariantCulture, $"{SaasAdminLayout.Base}/tenants/new?lead={leadId}");

    private static string FilterLabel(SaasLeadStatus? status) => status?.ToString() ?? "All";

    /// <summary>The message's first line; the grid cuts it with an ellipsis.</summary>
    private static string FirstLine(string message) =>
        message.Split('\n', 2)[0].Trim() is { Length: > 0 } line ? line : message.Trim();

    private static string StatusHint(SaasLeadStatus status) => status switch
    {
        SaasLeadStatus.New => "Just arrived; nobody from sales has replied yet.",
        SaasLeadStatus.Contacted => "Someone from sales replied.",
        SaasLeadStatus.Qualified => "A real opportunity.",
        _ => "Won, lost, or not a fit; the note says which. Creating a tenant from a lead closes it.",
    };

    private static int TeamSizeIndex(string teamSize) => SaasSeedData.TeamSizes.ToList().IndexOf(teamSize);

    /// <summary>
    /// Applies the bulk bar's status and reason to the selected leads in one <see cref="SaasState.BulkUpdateLeads" /> call,
    /// then refreshes the grid and clears the selection; the bar closes, so focus moves to the announcement.
    /// </summary>
    private async Task ApplyBulkAsync()
    {
        var reason = bulkReason.Trim();
        SaasLeadStatus? status = Enum.TryParse<SaasLeadStatus>(bulkStatus, out var parsed) && Enum.IsDefined(parsed) ? parsed : null;
        var problem = status is null ? "Choose the status to give the selected leads."
            : status == SaasLeadStatus.Closed && reason.Length == 0 ? "Give a reason for closing the leads: it goes on each note."
            : reason.Length > SaasState.MaxLeadReasonLength ? string.Create(CultureInfo.InvariantCulture, $"Shorten the reason to {SaasState.MaxLeadReasonLength} characters.")
            : null;

        if (problem is not null)
        {
            bulkErrors++;
            bulkError = bulkErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkErrors})");

            return;
        }

        bulkError = null;
        bulkErrors = 0;

        var outcome = State.BulkUpdateLeads(selected.Select(lead => lead.Id).ToList(), status!.Value, reason);
        SetStatus(outcome.Result switch
        {
            SaasResult.Ok => $"Updated {CountText(outcome.Changed, "lead", "leads")} to {status}."
                + (outcome.Unchanged > 0 ? $" {CountText(outcome.Unchanged, "lead already was", "leads already were")} like that." : string.Empty),
            SaasResult.NoChange => "No leads changed: the selected leads already had that status.",
            SaasResult.NotAllowed => "Sign in as a Lumora operator to change leads. Nothing changed.",
            _ => "Those changes couldn't be saved. Nothing changed.",
        }, outcome.Result is not (SaasResult.Ok or SaasResult.NoChange));

        bulkStatus = bulkReason = string.Empty;

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<SaasLead>();
        focusStatus = true;
        StateHasChanged();
    }

    private void ClearErrorAndNote()
    {
        errorMessage = errorText = noteMessage = noteText = null;
        errorCount = noteCount = 0;
    }

    /// <summary>The link disappears with the filters it clears, so focus moves to the count, which also announces the new total.</summary>
    private async Task ClearFiltersAsync()
    {
        filter = null;
        search = searchBoxValue = string.Empty;
        await FiltersChangedAsync();
        await SaasDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<SaasLead>();
        bulkError = null;
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    private int CountIn(SaasLeadStatus? status) => Matching(byStatus: false).Count(lead => status is null || lead.Status == status);

    /// <summary>
    /// Adds the lead once the form is valid: the email check finishes first, then every rule runs, and a failed submit shows
    /// the summary (rule WR-002). The state checks again; an email with an open lead goes on its field.
    /// </summary>
    private async Task CreateAsync()
    {
        newServerErrors?.Clear();
        newContext.NotifyValidationStateChanged();

        var emailFree = emailCheck is null || await emailCheck.CheckNowAsync();

        if (!await newContext.ValidateAsync() || !emailFree)
            return;

        var (result, lead) = State.CreateLead(newForm.Name, newForm.Email, newForm.Company, newForm.TeamSize, newForm.Message, newForm.Status, newForm.Note);

        switch (result)
        {
            case SaasResult.Ok when lead is not null:
                createdId = lead.Id;
                SetStatus($"Added the lead from {lead.Name}, {lead.Company}.", false);
                await newModal.HideAsync();

                return;

            case SaasResult.Duplicate:
                newServerErrors?.Add(FieldIdentifier.Create(() => newForm.Email), EmailTakenMessage(newForm.Email));
                newContext.NotifyValidationStateChanged();

                return;

            default:
                newErrorCount++;
                var message = result == SaasResult.NotAllowed
                    ? "Only a signed-in Lumora operator can add a lead. Sign in as an operator and try again."
                    : "The lead couldn't be added: a value is out of range, or this tab holds the most leads it can (500).";
                newError = newErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({newErrorCount})");

                return;
        }
    }

    private string EmailTakenMessage(string email)
    {
        var existing = State.Leads.FirstOrDefault(lead => lead.Status != SaasLeadStatus.Closed && string.Equals(lead.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));

        return existing is null
            ? "This email already has an open lead. Update that lead instead."
            : $"{existing.Name} ({existing.Company}) already has an open lead from {SaasDisplay.Date(existing.CreatedOn)}. Update that lead instead.";
    }

    private string ErrorFor(SaasResult result) => result switch
    {
        SaasResult.Invalid => "A closed lead needs a note saying why, and a note can have up to 1,000 characters.",
        SaasResult.NotFound => "That lead is no longer in this tab. Close the dialog and pick it again from the list.",
        SaasResult.NotAllowed => "Only a signed-in Lumora operator can update a lead. Sign in as an operator and try again.",
        _ => "The lead couldn't be saved. Check the status and the note, and try again.",
    };

    /// <summary>A filter changed: back to the first page of the filtered leads.</summary>
    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private Task<bool> IsEmailTakenAsync(string email) => Task.FromResult(State.HasOpenLead(email));

    /// <summary>Returns the leads that pass the search and (unless <paramref name="byStatus" /> is off) the status filter.</summary>
    private IEnumerable<SaasLead> Matching(bool byStatus = true)
    {
        var term = search.Trim();

        return State.Leads.Where(lead => (!byStatus || filter is null || lead.Status == filter)
            && (term.Length == 0 || lead.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || lead.Company.Contains(term, StringComparison.OrdinalIgnoreCase)
                || lead.Email.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args) => ClearErrorAndNote();

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus (WCAG 2.4.3): focus goes back to the lead's name in the
    /// list, or to the status message when a save moved the lead out of the current filter or page.
    /// </summary>
    private async Task OnHiddenAsync()
    {
        var leadId = openId;
        var wasSaved = saved;

        openId = 0;
        saved = false;
        ClearErrorAndNote();
        ResetForm(new LeadForm());
        await RefreshGridAsync();
        StateHasChanged();
        await Task.Yield();

        var stillShown = Matching().Any(lead => lead.Id == leadId);

        if (stillShown && !wasSaved && openButtons.TryGetValue(leadId, out var button))
            await SaasDisplay.FocusAsync(button);
        else if (wasSaved || leadId != 0)
            await SaasDisplay.FocusAsync(statusRegion);
    }

    /// <summary>The add dialog closed: a new lead opens in the lead dialog, otherwise focus returns to "Add lead".</summary>
    private async Task OnNewHiddenAsync()
    {
        ResetNewForm();

        if (createdId is { } id)
        {
            createdId = null;
            filter = null;
            search = searchBoxValue = string.Empty;
            await FiltersChangedAsync();
            await OpenAsync(id);

            return;
        }

        await SaasDisplay.FocusAsync(addButton?.Element);
    }

    private Task OnNewShownAsync() => SaasDisplay.FocusAsync(newNameInput?.Element);

    private void OnNoteInput(ChangeEventArgs args) => noteLength = args.Value?.ToString()?.Length ?? 0;

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

    private void OnSelectionChanged(HashSet<SaasLead> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            bulkError = null;
    }

    private Task OnShownAsync() => SaasDisplay.FocusAsync(statusSelect?.Element);

    /// <summary>Another screen changed the state: the grid reads the leads again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OpenAsync(int leadId)
    {
        if (State.FindLead(leadId) is not { } lead)
            return;

        openId = lead.Id;
        saved = false;
        ClearErrorAndNote();
        ResetForm(new LeadForm { Status = lead.Status, Note = lead.Note ?? string.Empty });

        // Also opened from OnAfterRenderAsync (for ?lead=), where no render follows by itself.
        StateHasChanged();
        await leadModal.ShowAsync();
    }

    private async Task OpenNewAsync()
    {
        ResetNewForm();
        await newModal.ShowAsync();
    }

    /// <summary>The page of leads the grid asked for, from the filtered list, newest first unless a column is sorted.</summary>
    private Task<GridDataProviderResult<SaasLead>> ProvideLeadsAsync(GridDataProviderRequest<SaasLead> request) => view.ApplyAsync(request, Shown);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private void ResetForm(LeadForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        noteLength = next.Note.Length;
        editContext = new EditContext(form);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    private void ResetNewForm()
    {
        newForm = new NewLeadForm();
        newContext = new EditContext(newForm);
        newServerErrors = new ValidationMessageStore(newContext);
        newContext.OnFieldChanged += (_, args) => newServerErrors?.Clear(args.FieldIdentifier);
        newError = null;
        newErrorCount = 0;
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the count.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<SaasLead>();
        gridVersion++;
        SetStatus("The view is reset: newest first, 25 leads a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await SaasDisplay.FocusAsync(countRegion);
    }

    private string RowClass(SaasLead lead) => selected.Any(item => item.Id == lead.Id) ? "sd-row-selected" : string.Empty;

    /// <summary>Saves the status and note once the form is valid (a closed lead needs a note, rule WR-002); the state checks again.</summary>
    private async Task SaveAsync()
    {
        if (OpenLead is not { } lead)
        {
            ShowError(ErrorFor(SaasResult.NotFound));

            return;
        }

        if (!await editContext.ValidateAsync())
            return;

        var before = lead.Status;
        var result = State.UpdateLead(lead.Id, form.Status, form.Note);

        switch (result)
        {
            case SaasResult.Ok:
                SetStatus(before == form.Status
                    ? $"Saved the note on the lead from {lead.Name}, {lead.Company}."
                    : $"Saved the lead from {lead.Name}, {lead.Company}: {before} → {form.Status}.", false);
                saved = true;
                await leadModal.HideAsync();
                break;

            case SaasResult.NoChange:
                ShowNote("Nothing to save: the status and the note are as they were.");
                break;

            default:
                ShowError(ErrorFor(result));
                break;
        }
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private async Task SelectFilterAsync(SaasLeadStatus? status)
    {
        filter = status;
        await FiltersChangedAsync();
    }

    /// <summary>Shows a page status; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Shows an error in the dialog; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void ShowError(string message)
    {
        noteMessage = noteText = null;
        noteCount = 0;
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : $"{message} ({errorCount})";
    }

    /// <summary>Shows a note in the dialog; the same note again gets a counter, so a screen reader announces it again.</summary>
    private void ShowNote(string message)
    {
        errorMessage = errorText = null;
        errorCount = 0;
        noteCount = message == noteText ? noteCount + 1 : 1;
        noteText = message;
        noteMessage = noteCount == 1 ? message : $"{message} ({noteCount})";
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private string EmptyText => (filter, search.Trim().Length > 0) switch
    {
        ({ } status, true) => $"No {status.ToString().ToLowerInvariant()} lead matches \"{search.Trim()}\".",
        ({ } status, false) => $"No lead is {status.ToString().ToLowerInvariant()} right now.",
        _ => $"No lead matches \"{search.Trim()}\". Search by name, company, or email.",
    };

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => filter is not null || search.Trim().Length > 0;

    /// <summary>Gets or sets the lead to open, such as <c>?lead=2</c> from the overview or a notification.</summary>
    [SupplyParameterFromQuery(Name = "lead")] public int? LeadQuery { get; set; }

    private string NoteLengthText => string.Create(CultureInfo.InvariantCulture, $"{noteLength:#,0} of {SaasState.MaxLeadMessageLength:#,0} characters.");

    private SaasLead? OpenLead => openId == 0 ? null : State.FindLead(openId);

    private IReadOnlyList<SaasLead> Shown => Matching().ToList();

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The lead dialog's status and note, with the rules of <see cref="SaasState.UpdateLead" />: a closed lead needs a note.</summary>
    private sealed class LeadForm : IValidatableObject
    {
        [StringLength(SaasState.MaxLeadMessageLength, ErrorMessage = "Use 1,000 characters or fewer for the note.")]
        public string Note { get; set; } = string.Empty;

        public SaasLeadStatus Status { get; set; } = SaasLeadStatus.New;

        /// <summary>A closed lead says why it was closed.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Status == SaasLeadStatus.Closed && string.IsNullOrWhiteSpace(Note))
                yield return new ValidationResult("Say why the lead is closed: won, lost, or not a fit.", new[] { nameof(Note) });
        }
    }

    /// <summary>The add-lead form's fields, with the limits of <see cref="SaasState.CreateLead" />.</summary>
    private sealed class NewLeadForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the company.")]
        [StringLength(SaasState.MaxLeadTextLength, ErrorMessage = "Use 60 characters or fewer for the company.")]
        public string Company { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the email to reply to.")]
        [StringLength(ShowcaseIdentity.MaxEmailLength, ErrorMessage = "Use 120 characters or fewer for the email.")]
        [RegularExpression(@"^\s*[^@\s]+@[^@\s]+\.[^@\s]+\s*$", ErrorMessage = "Enter an email like name@example.com.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Say what they asked about.")]
        [StringLength(SaasState.MaxLeadMessageLength, ErrorMessage = "Use 1,000 characters or fewer.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the person's name.")]
        [StringLength(SaasState.MaxLeadTextLength, ErrorMessage = "Use 60 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(SaasState.MaxLeadMessageLength, ErrorMessage = "Use 1,000 characters or fewer for the note.")]
        public string Note { get; set; } = string.Empty;

        public SaasLeadStatus Status { get; set; } = SaasLeadStatus.New;

        [Required(ErrorMessage = "Choose the team size.")]
        public string TeamSize { get; set; } = string.Empty;

        /// <summary>A lead added as closed says why.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Status == SaasLeadStatus.Closed && string.IsNullOrWhiteSpace(Note))
                yield return new ValidationResult("Say why the lead is closed: won, lost, or not a fit. Or choose another status.", new[] { nameof(Note) });
        }
    }

    #endregion
}
