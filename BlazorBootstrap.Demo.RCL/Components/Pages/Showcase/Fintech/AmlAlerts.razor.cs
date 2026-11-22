using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// The AML alert queue in a <see cref="Grid{TItem}" />: every alert the monitoring rules raised, newest first, with the
/// number (linking to the alert; the whole row opens it), the customer (with a Watch badge), the rule and what it saw, the
/// risk and score, the status (with its case), who it's assigned to, and when it was raised, with paging, sorting, the
/// column chooser, and CSV export. It is searched by number or customer and filtered by status (open, the default), risk,
/// rule, and assignee; the grid's view is saved for the visit and can be reset. Selected alerts are triaged together from a
/// bulk bar, an <see cref="EditForm" /> (rule WR-002): assign them to someone, dismiss them as false positives (a reason is
/// required), or open one case from alerts on the same customer (a cross-field rule), assigned to an analyst.
/// </summary>
public partial class AmlAlerts : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string AssignAction = "assign";

    private const string CaseAction = "case";

    private const int DefaultPageSize = 25;

    private const string DismissAction = "dismiss";

    private const string OpenValue = "open";

    private const string UnassignedValue = "none";

    private string assigneeFilter = string.Empty;

    private EditContext bulkContext = default!;

    private BulkForm bulkForm = new();

    private ElementReference? countRegion;

    private bool focusStatus;

    private Grid<AlertRow>? grid;

    private int gridVersion;

    private int pageSize = DefaultPageSize;

    private string riskFilter = string.Empty;

    private string ruleFilter = string.Empty;

    private string search = string.Empty;

    private string searchBoxValue = string.Empty;

    private List<AlertRow> selected = new();

    private int statusCount;

    private string statusFilter = OpenValue;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    private string? statusText;

    private ShowcaseGridView view = default!;

    private bool viewLoaded;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await view.LoadAsync();
            pageSize = (await view.GetSettingsAsync()).PageSize;
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusStatus)
        {
            focusStatus = false;
            await FintechDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "aml-alerts", DefaultPageSize, "raised", SortDirection.Descending);
        ResetBulkForm();
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Says how long ago something happened, like "45 min", "3 h", or "2 d 4 h".</summary>
    internal static string AgeText(DateTime since, DateTime now) => OpsQueue.WaitText(since, now);

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>Applies the bulk bar's action once the form is valid; opening a case goes to the new case.</summary>
    private async Task ApplyBulkAsync()
    {
        bulkForm.CustomerCount = SelectedCustomers;

        if (!await bulkContext.ValidateAsync())
            return;

        var ids = selected.Select(row => row.Id).ToList();
        string message;
        bool isError;

        switch (bulkForm.Action)
        {
            case CaseAction:
                var (result, amlCase) = State.OpenCase(ids, bulkForm.AssigneeId);

                if (result == FintechResult.Ok && amlCase is not null)
                {
                    Navigation.NavigateTo(AmlDisplay.CasePath(amlCase.Id));

                    return;
                }

                message = result == FintechResult.Invalid ? "A case is opened from alerts on one customer. Select alerts on the same customer, and nothing else." : AmlDisplay.ResultText(result);
                isError = true;
                break;

            case AssignAction:
                var assigned = State.AssignAlerts(ids, bulkForm.AssigneeId);
                message = assigned.Result switch
                {
                    FintechResult.Ok => $"Assigned {FintechDisplay.Plural(assigned.Changed, "alert", "alerts")} to {AmlDisplay.StaffText(bulkForm.AssigneeId)}."
                        + (assigned.Unchanged > 0 ? $" {FintechDisplay.Plural(assigned.Unchanged, "alert was", "alerts were")} left as they were: already dismissed, in a case, or with them." : string.Empty),
                    FintechResult.NoChange => "No alerts changed: they're already dismissed, in a case, or assigned to that person.",
                    _ => AmlDisplay.ResultText(assigned.Result),
                };
                isError = assigned.Result is not (FintechResult.Ok or FintechResult.NoChange);
                break;

            default:
                var dismissed = State.DismissAlerts(ids, bulkForm.Reason ?? string.Empty);
                message = dismissed.Result switch
                {
                    FintechResult.Ok => $"Dismissed {FintechDisplay.Plural(dismissed.Changed, "alert", "alerts")} as false positives."
                        + (dismissed.Unchanged > 0 ? $" {FintechDisplay.Plural(dismissed.Unchanged, "alert was", "alerts were")} left as they were: already dismissed or in a case." : string.Empty),
                    FintechResult.NoChange => "No alerts changed: they're already dismissed or in a case.",
                    _ => AmlDisplay.ResultText(dismissed.Result),
                };
                isError = dismissed.Result is not (FintechResult.Ok or FintechResult.NoChange);
                break;
        }

        SetStatus(message, isError);
        ResetBulkForm();

        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<AlertRow>();
        focusStatus = true;
    }

    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = riskFilter = ruleFilter = assigneeFilter = string.Empty;
        statusFilter = OpenValue;
        await FiltersChangedAsync();
        await FintechDisplay.FocusAsync(countRegion);
    }

    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<AlertRow>();
        ResetBulkForm();
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<AlertRow> items) => selected = items.ToList();

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private Task<GridDataProviderResult<AlertRow>> ProvideRowsAsync(GridDataProviderRequest<AlertRow> request) => view.ApplyAsync(request, Rows);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private void ResetBulkForm()
    {
        bulkForm = new BulkForm { AssigneeId = State.CurrentStaffId ?? string.Empty };
        bulkContext = new EditContext(bulkForm);
    }

    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<AlertRow>();
        gridVersion++;
        SetStatus("The view is reset: newest first, 25 alerts a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await FintechDisplay.FocusAsync(countRegion);
    }

    private string RowClass(AlertRow row) => selected.Any(item => item.Id == row.Id) ? "tm-row-selected" : string.Empty;

    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || riskFilter.Length > 0 || ruleFilter.Length > 0 || assigneeFilter.Length > 0 || statusFilter != OpenValue;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private int OpenCount => State.AmlAlerts.Count(alert => alert.Status is FintechAmlAlertStatus.New or FintechAmlAlertStatus.InTriage);

    /// <summary>Gets the alerts that pass every filter, as grid rows.</summary>
    private IReadOnlyList<AlertRow> Rows
    {
        get
        {
            var term = search.Trim();
            var now = State.Clock.Now;
            var customers = State.Customers.ToDictionary(customer => customer.Id, StringComparer.Ordinal);

            return State.AmlAlerts
                .Where(alert => (statusFilter.Length == 0 || (statusFilter == OpenValue ? alert.Status is FintechAmlAlertStatus.New or FintechAmlAlertStatus.InTriage : alert.Status.ToString() == statusFilter))
                    && (riskFilter.Length == 0 || alert.Risk.ToString() == riskFilter) && (ruleFilter.Length == 0 || alert.Rule.ToString() == ruleFilter)
                    && (assigneeFilter.Length == 0 || (assigneeFilter == UnassignedValue ? alert.AssigneeId is null : alert.AssigneeId == assigneeFilter)))
                .Select(alert => (Alert: alert, Customer: customers.GetValueOrDefault(alert.CustomerId)))
                .Where(item => term.Length == 0 || item.Alert.Id.ToString(CultureInfo.InvariantCulture).Contains(term, StringComparison.Ordinal)
                    || (item.Customer?.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                .Select(item => new AlertRow(item.Alert.Id, item.Customer?.Name ?? item.Alert.CustomerId, item.Customer?.Status == FintechCustomerStatus.Watch,
                    FintechSeedData.RuleName(item.Alert.Rule), item.Alert.Summary, AmlDisplay.RiskText(item.Alert.Risk), item.Alert.Risk, item.Alert.Score,
                    AmlDisplay.AlertStatusText(item.Alert.Status), item.Alert.Status, item.Alert.CaseId, AmlDisplay.StaffText(item.Alert.AssigneeId), item.Alert.CreatedAt,
                    AgeText(item.Alert.CreatedAt, now)))
                .ToList();
        }
    }

    /// <summary>Gets how many customers the selected alerts are on.</summary>
    private int SelectedCustomers => selected.Select(row => row.Customer).Distinct().Count();

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One alert as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record AlertRow(int Id, string Customer, bool OnWatch, string Rule, string Summary, string Risk, FintechAmlRisk RiskValue, int Score, string Status,
        FintechAmlAlertStatus StatusValue, int? CaseId, string Assignee, DateTime Raised, string Age);

    /// <summary>
    /// The bulk bar's fields: an action; who to assign to (assign and open a case); and a reason (dismiss, 10 to 500
    /// characters). Cross-field: a case is opened from alerts on one customer.
    /// </summary>
    private sealed class BulkForm : IValidatableObject
    {
        [Required(ErrorMessage = "Choose what to do with the selected alerts.")]
        public string Action { get; set; } = string.Empty;

        public string AssigneeId { get; set; } = string.Empty;

        /// <summary>How many customers the selected alerts are on; set by the page.</summary>
        public int CustomerCount { get; set; }

        [StringLength(FintechState.MaxAmlReason, ErrorMessage = "Keep the reason to 500 characters.")]
        public string? Reason { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Action is AssignAction or CaseAction && FintechSeedData.StaffName(AssigneeId) is null)
                yield return new ValidationResult(Action == CaseAction ? "Choose the analyst who works the case." : "Choose who to assign the alerts to.", new[] { nameof(AssigneeId) });

            if (Action == CaseAction && CustomerCount != 1)
                yield return new ValidationResult("A case is about one customer: select alerts on the same customer only.", new[] { nameof(Action) });

            if (Action == DismissAction && (Reason?.Trim().Length ?? 0) < FintechState.MinAmlNote)
                yield return new ValidationResult("Write why these are false positives, in at least 10 characters.", new[] { nameof(Reason) });
        }
    }

    #endregion
}
