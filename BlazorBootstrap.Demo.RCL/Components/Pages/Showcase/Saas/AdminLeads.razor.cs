using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The operator's sales inbox: every message from the public "Contact sales" form, filtered by status and searched by
/// name, company, or email. Opening a lead shows its full message in a dialog where the operator sets its status and the
/// sales team's note (<see cref="SaasState.UpdateLead" />), with a message for each <see cref="SaasResult" />. A lead that
/// isn't closed links to the tenant form, prefilled from the lead. <c>?lead={id}</c> opens that lead, for the overview's
/// links. Focus returns to the lead that opened the dialog, or to the status message when its row was filtered away.
/// </summary>
public partial class AdminLeads : ComponentBase, IDisposable
{
    #region Fields and Constants

    /// <summary>The longest sales note <see cref="SaasState.UpdateLead" /> accepts.</summary>
    private const int MaxNoteLength = 1000;

    /// <summary>The filter buttons in order; <see langword="null" /> is "All".</summary>
    private static readonly SaasLeadStatus?[] filters =
    {
        null, SaasLeadStatus.New, SaasLeadStatus.Contacted, SaasLeadStatus.Qualified, SaasLeadStatus.Closed,
    };

    private readonly Dictionary<int, ElementReference> openButtons = new();

    private int? appliedLeadQuery;

    private EditContext editContext = default!;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private SaasLeadStatus? filter;

    private LeadForm form = new();

    private Modal leadModal = default!;

    private int noteCount;

    /// <summary>The length of the note as typed, before the change event updates the form.</summary>
    private int noteLength;

    private string? noteMessage;

    private string? noteText;

    /// <summary>The ID of the lead shown in the dialog; 0 when it is closed.</summary>
    private int openId;

    /// <summary>A lead asked for in <c>?lead=</c>, opened once the page is interactive.</summary>
    private int? pendingLeadId;

    /// <summary>Whether the dialog closed after a save, so focus goes to the status message when the lead left the list.</summary>
    private bool saved;

    private string search = string.Empty;

    /// <summary>The value bound to the search box; it trails <see cref="search" /> while the user types (see <see cref="OnSearchInput" />).</summary>
    private string searchBoxValue = string.Empty;

    private ElementReference searchInput;

    private InputSelect<SaasLeadStatus>? statusSelect;

    private int statusCount;

    private string? statusMessage;

    private ElementReference statusRegion;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (pendingLeadId is { } leadId)
        {
            pendingLeadId = null;

            // The parent's after-render runs before the modal's own, which wires up its events: let that go first.
            await Task.Yield();
            await OpenAsync(leadId);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnInitialized()
    {
        ResetForm(new LeadForm());
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

    private static int CountIn(IReadOnlyList<SaasLead> leads, SaasLeadStatus? status) =>
        status is { } value ? leads.Count(lead => lead.Status == value) : leads.Count;

    private static string CountText(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{Count(count)} {many}";

    private static string CreateTenantPath(int leadId) => string.Create(CultureInfo.InvariantCulture, $"{SaasAdminLayout.Base}/tenants/new?lead={leadId}");

    private static string FilterLabel(SaasLeadStatus? status) => status?.ToString() ?? "All";

    /// <summary>The message's first line; the list cuts it to one line with an ellipsis.</summary>
    private static string FirstLine(string message) =>
        message.Split('\n', 2)[0].Trim() is { Length: > 0 } line ? line : message.Trim();

    private static string StatusHint(SaasLeadStatus status) => status switch
    {
        SaasLeadStatus.New => "Just arrived; nobody from sales has replied yet.",
        SaasLeadStatus.Contacted => "Someone from sales replied.",
        SaasLeadStatus.Qualified => "A real opportunity.",
        _ => "Won, lost, or not a fit. Creating a tenant from a lead closes it.",
    };

    private static string SummaryText(int shown, int total) => total == 0
        ? "No leads yet."
        : shown == total
            ? $"Showing all {CountText(total, "lead", "leads")}."
            : $"Showing {Count(shown)} of {CountText(total, "lead", "leads")}.";

    private void ClearErrorAndNote()
    {
        errorMessage = errorText = noteMessage = noteText = null;
        errorCount = noteCount = 0;
    }

    private async Task ClearFiltersAsync()
    {
        filter = null;
        search = searchBoxValue = string.Empty;
        StateHasChanged();

        // The button is gone once the list is back, so focus moves to the search box.
        await Task.Yield();
        await SaasDisplay.FocusAsync(searchInput);
    }

    private IReadOnlyList<SaasLead> FilteredLeads(IReadOnlyList<SaasLead> leads)
    {
        IEnumerable<SaasLead> rows = leads;

        if (filter is { } status)
            rows = rows.Where(lead => lead.Status == status);

        var term = search.Trim();

        if (term.Length > 0)
        {
            rows = rows.Where(lead => lead.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || lead.Company.Contains(term, StringComparison.OrdinalIgnoreCase)
                || lead.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return rows.ToList();
    }

    private string ErrorFor(SaasResult result) => result switch
    {
        SaasResult.Invalid => $"The note is too long. Keep it to {Count(MaxNoteLength)} characters or fewer.",
        SaasResult.NotFound => "That lead is no longer in this tab. Close the dialog and pick it again from the list.",
        SaasResult.NotAllowed => "Only a signed-in Lumora operator can update a lead. Sign in as an operator and try again.",
        _ => "The lead couldn't be saved. Check the status and the note, and try again.",
    };

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args) => ClearErrorAndNote();

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus (WCAG 2.4.3): focus goes back to the lead's name in the
    /// list, or to the status message when a save moved the lead out of the current filter.
    /// </summary>
    private async Task OnHiddenAsync()
    {
        var leadId = openId;
        var wasSaved = saved;

        openId = 0;
        saved = false;
        ClearErrorAndNote();
        ResetForm(new LeadForm());
        StateHasChanged();
        await Task.Yield();

        var stillShown = FilteredLeads(State.Leads).Any(lead => lead.Id == leadId);

        if (stillShown && openButtons.TryGetValue(leadId, out var button))
            await SaasDisplay.FocusAsync(button);
        else if (wasSaved || leadId != 0)
            await SaasDisplay.FocusAsync(statusRegion); // The lead left the current filter, or the list.
    }

    private void OnNoteInput(ChangeEventArgs args) => noteLength = args.Value?.ToString()?.Length ?? 0;

    /// <summary>Filters on every keystroke without changing the box's bound value, so a slower render never overwrites newer typing.</summary>
    private void OnSearchInput(ChangeEventArgs args) => search = args.Value?.ToString() ?? string.Empty;

    private void OnSearchChanged(ChangeEventArgs args) => search = searchBoxValue = args.Value?.ToString() ?? string.Empty;

    private Task OnShownAsync() => SaasDisplay.FocusAsync(statusSelect?.Element);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

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

    private void ResetForm(LeadForm next)
    {
        if (editContext is not null)
            editContext.OnFieldChanged -= OnFieldChanged;

        form = next;
        noteLength = next.Note.Length;
        editContext = new EditContext(form);
        editContext.OnFieldChanged += OnFieldChanged;
    }

    private async Task SaveAsync()
    {
        if (OpenLead is not { } lead)
        {
            ShowError(ErrorFor(SaasResult.NotFound));

            return;
        }

        var before = lead.Status;
        var result = State.UpdateLead(lead.Id, form.Status, form.Note);

        switch (result)
        {
            case SaasResult.Ok:
                SetStatus(before == form.Status
                    ? $"Saved the note on the lead from {lead.Name}, {lead.Company}."
                    : $"Saved the lead from {lead.Name}, {lead.Company}: {before} → {form.Status}.");
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

    private void SelectFilter(SaasLeadStatus? status) => filter = status;

    /// <summary>Shows a page status; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
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

    /// <summary>Gets or sets the lead to open, such as <c>?lead=2</c> from the overview.</summary>
    [SupplyParameterFromQuery(Name = "lead")] public int? LeadQuery { get; set; }

    private string NoteLengthText => string.Create(CultureInfo.InvariantCulture, $"{noteLength:#,0} of {MaxNoteLength:#,0} characters.");

    private SaasLead? OpenLead => openId == 0 ? null : State.FindLead(openId);

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    private sealed class LeadForm
    {
        public string Note { get; set; } = string.Empty;

        public SaasLeadStatus Status { get; set; } = SaasLeadStatus.New;
    }

    #endregion
}
