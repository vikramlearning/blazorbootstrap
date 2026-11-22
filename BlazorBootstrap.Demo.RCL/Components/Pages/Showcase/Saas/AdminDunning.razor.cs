using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// The operator's dunning page (AC-7d): a summary (retrying, in grace, suspended, and the money at risk), the policy in
/// words with a validated editor that previews it (rule WR-002), a demo control that moves the site's clock forward a day,
/// and a <see cref="Grid{TItem}" /> of the dunning cases (tenant linking to its page and timeline, invoice, amount, stage,
/// reason, attempts, the next step, failure date, account manager) with a saved view, paging, sorting, the column chooser,
/// and CSV export, filtered by stage (open by default; each with its count) and searched by tenant or invoice. Selected cases
/// can be retried now or given more grace (1 to 14 days with a reason) together.
/// </summary>
public partial class AdminDunning : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string AllValue = "all";

    private const int DefaultPageSize = 25;

    private const string OpenValue = "open";

    private static readonly (string Value, string Label)[] stageOptions =
    {
        (OpenValue, "Open"), (nameof(SaasDunningStage.Retrying), "Retrying"), (nameof(SaasDunningStage.Grace), "In grace"), (nameof(SaasDunningStage.Suspended), "Suspended"),
        (nameof(SaasDunningStage.Recovered), "Recovered"), (AllValue, "All"),
    };

    private string? bulkError;

    private int bulkErrors;

    private int? bulkDays = 7;

    private string bulkReason = string.Empty;

    private ElementReference? countRegion;

    private InputText? daysInput;

    private bool focusStatus;

    private Grid<DunningRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private Button? policyButton;

    private EditContext policyContext = new(new PolicyForm());

    /// <summary>The retry days as typed, before the field's change event, so the preview follows each keystroke.</summary>
    private string? policyDaysPreview;

    private string? policyError;

    private PolicyForm policyForm = new();

    private Modal policyModal = default!;

    /// <summary>Whether the policy was saved, so the dialog's close announces it.</summary>
    private bool policySaved;

    private string search = string.Empty;

    private string searchBoxValue = string.Empty;

    /// <summary>The cases selected in the grid (on its current page).</summary>
    private List<DunningRow> selected = new();

    private string stageFilter = OpenValue;

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

        if (focusStatus)
        {
            focusStatus = false;
            await SaasDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "dunning", DefaultPageSize, "next");
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns a case's next step in words: the next retry, the end of grace, the suspension, or the recovery.</summary>
    internal static string NextStepText(SaasDunningCase item) => item.Stage switch
    {
        SaasDunningStage.Retrying when item.NextRetryAt is { } next => string.Create(CultureInfo.InvariantCulture,
            $"Retry {item.RetriesDone + 1} of {item.RetryDays.Count} on {SaasDisplay.Date(next)}"),
        SaasDunningStage.Grace when item.GraceEndsAt is { } ends => $"Suspends on {SaasDisplay.Date(ends)}",
        SaasDunningStage.Suspended => "Suspended until paid",
        _ => item.ClosedAt is { } closed ? $"Recovered on {SaasDisplay.Date(closed)}" : "Recovered",
    };

    /// <summary>Parses retry days typed like "1, 3, 7"; <see langword="null" /> when something isn't a whole number.</summary>
    internal static List<int>? ParseDays(string? text)
    {
        var parts = (text ?? string.Empty).Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
        var days = new List<int>();

        foreach (var part in parts)
        {
            if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var day))
                return null;

            days.Add(day);
        }

        return days;
    }

    #endregion

    #region Private Methods

    private static string Count(int value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static bool MatchesStage(SaasDunningCase item, string filter) => filter switch
    {
        OpenValue => item.IsOpen,
        AllValue => true,
        _ => item.Stage.ToString() == filter,
    };

    private static string TenantPath(int workspaceId) => string.Create(CultureInfo.InvariantCulture, $"{SaasAdminLayout.Base}/tenants/{workspaceId}");

    /// <summary>Moves the site's clock a day; the status message says what dunning did (from the audit log's new entries).</summary>
    private async Task AdvanceDayAsync()
    {
        var before = State.AuditEntries.Count == 0 ? 0 : State.AuditEntries.Max(entry => entry.Id);
        var result = State.AdvanceDemoDay();

        if (result != SaasResult.Ok)
        {
            SetStatus(result == SaasResult.Invalid
                ? string.Create(CultureInfo.InvariantCulture, $"The demo clock is already {SaasState.MaxDemoDays} days ahead, the most it goes. Reset the demo data to start again.")
                : "Sign in as a Lumora operator to move the demo clock.", true);
            focusStatus = true;

            return;
        }

        var steps = State.AuditEntries.Where(entry => entry.Id > before).ToList();
        var retries = steps.Count(entry => entry.Action == "Payment retry failed");
        var failed = steps.Count(entry => entry.Action == "Payment failed");
        var graces = steps.Count(entry => entry.Action == "Grace period started");
        var suspended = steps.Count(entry => entry.Action == "Tenant suspended");
        var recovered = steps.Count(entry => entry.Action == "Payment recovered");
        var parts = new List<string>();

        if (failed > 0)
            parts.Add($"{SaasState.Plural(failed, "new charge", "new charges")} failed");

        if (retries > 0)
            parts.Add($"{SaasState.Plural(retries, "retry", "retries")} failed");

        if (graces > 0)
            parts.Add($"{SaasState.Plural(graces, "tenant", "tenants")} entered grace");

        if (suspended > 0)
            parts.Add($"{SaasState.Plural(suspended, "tenant was", "tenants were")} suspended");

        if (recovered > 0)
            parts.Add($"{SaasState.Plural(recovered, "tenant", "tenants")} recovered");

        SetStatus($"It's now {State.Clock.Now.ToString("dddd, MMM d", CultureInfo.InvariantCulture)}. "
            + (parts.Count == 0 ? "No dunning step was due." : $"Dunning: {string.Join(", ", parts)}.") + " Renewals that came due are billed.", false);
        await RefreshGridAsync();
        focusStatus = true;
    }

    private async Task ClearFiltersAsync()
    {
        search = searchBoxValue = string.Empty;
        stageFilter = OpenValue;
        await FiltersChangedAsync();
        await SaasDisplay.FocusAsync(countRegion);
    }

    /// <summary>The bar closes, so focus moves to the announcement.</summary>
    private async Task ClearSelectionAsync()
    {
        if (grid is not null)
            await grid.UnSelectAllItemsAsync();

        selected = new List<DunningRow>();
        bulkError = null;
        SetStatus("Selection cleared.", false);
        focusStatus = true;
    }

    /// <summary>Extends the grace of the selected cases (1 to 14 days with a reason); cases still retrying or recovered are left out, and the message says so.</summary>
    private async Task ExtendSelectedAsync()
    {
        var reason = bulkReason.Trim();
        var problem = bulkDays is not (>= 1 and <= SaasState.MaxExtensionDays)
            ? string.Create(CultureInfo.InvariantCulture, $"Extend by 1 to {SaasState.MaxExtensionDays} days.")
            : reason.Length == 0 ? "Give a reason for the extension: it goes on each case's timeline."
            : null;

        if (problem is not null)
        {
            bulkErrors++;
            bulkError = bulkErrors == 1 ? problem : string.Create(CultureInfo.InvariantCulture, $"{problem} ({bulkErrors})");

            return;
        }

        bulkError = null;
        bulkErrors = 0;
        var outcome = State.BulkExtendGrace(selected.Select(row => row.Id).ToList(), bulkDays!.Value, reason);
        SetStatus(outcome.Result switch
        {
            SaasResult.Ok => $"Gave {SaasState.Plural(outcome.Changed, "case", "cases")} {SaasState.Plural(bulkDays.Value, "more day", "more days")} of grace; suspended ones are usable again."
                + (outcome.Unchanged > 0 ? $" {SaasState.Plural(outcome.Unchanged, "case was", "cases were")} left out: still retrying or already recovered." : string.Empty),
            SaasResult.NoChange => "No case was extended: only cases in grace or suspended can be. Cases still retrying wait for their retries.",
            SaasResult.NotAllowed => "Sign in as a Lumora operator to extend grace. Nothing changed.",
            _ => "The extension couldn't be saved. Nothing changed.",
        }, outcome.Result is not (SaasResult.Ok or SaasResult.NoChange));

        bulkReason = string.Empty;
        await FinishBulkAsync();
    }

    private async Task FiltersChangedAsync()
    {
        if (grid is not null)
            await grid.ResetPageNumber();
    }

    private async Task FinishBulkAsync()
    {
        if (grid is not null)
        {
            await grid.RefreshDataAsync();
            await grid.UnSelectAllItemsAsync();
        }

        selected = new List<DunningRow>();
        focusStatus = true;
        StateHasChanged();
    }

    private IEnumerable<DunningRow> Matching(bool byStage = true)
    {
        var term = search.Trim();

        foreach (var item in State.DunningCases)
        {
            if (State.FindWorkspace(item.WorkspaceId) is not { } workspace)
                continue;

            if (byStage && !MatchesStage(item, stageFilter))
                continue;

            if (term.Length > 0 && !workspace.Name.Contains(term, StringComparison.OrdinalIgnoreCase) && !item.InvoiceNumber.Contains(term, StringComparison.OrdinalIgnoreCase))
                continue;

            DateTime? nextAt = item.Stage switch
            {
                SaasDunningStage.Retrying => item.NextRetryAt,
                SaasDunningStage.Grace => item.GraceEndsAt,
                _ => null,
            };

            yield return new DunningRow(item.Id, workspace.Id, workspace.Name, SaasDisplay.StatusName(workspace.Status), item.InvoiceNumber, item.Amount,
                SaasDisplay.DunningStageName(item.Stage), item.Stage, SaasDisplay.FailureName(item.Reason), item.Attempts, NextStepText(item), nextAt, item.FailedAt,
                SaasSeedData.ManagerName(workspace.ManagerId) ?? string.Empty);
        }
    }

    private void OnDaysInput(ChangeEventArgs args) => policyDaysPreview = args.Value?.ToString();

    /// <summary>The dialog opens from code, so focus returns to "Edit policy" by hand; a save is announced.</summary>
    private async Task OnPolicyHiddenAsync()
    {
        if (policySaved)
        {
            policySaved = false;
            focusStatus = true;

            return;
        }

        await SaasDisplay.FocusAsync(policyButton?.Element);
    }

    private Task OnPolicyShownAsync() => SaasDisplay.FocusAsync(daysInput?.Element);

    private void OnSearchChanged(ChangeEventArgs args) => searchBoxValue = search = args.Value?.ToString() ?? string.Empty;

    private async Task OnSearchInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (value == search)
            return;

        search = value;
        await FiltersChangedAsync();
    }

    private void OnSelectionChanged(HashSet<DunningRow> items)
    {
        selected = items.ToList();

        if (selected.Count > 0)
            bulkError = null;
    }

    /// <summary>Another screen, the engine, or a bulk change changed the state: the grid reads the cases again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private static string OpenCasesNote(IReadOnlyList<SaasDunningCase> cases)
    {
        var open = cases.Count(item => item.IsOpen);

        return open == 0 ? "No case is open now." : $"The {SaasState.Plural(open, "open case keeps", "open cases keep")} the schedule {(open == 1 ? "it" : "they")} started with.";
    }

    private async Task OpenPolicyAsync()
    {
        var policy = State.DunningPolicy;
        policyForm = new PolicyForm
        {
            RetryDays = string.Join(", ", policy.RetryDays.Select(day => day.ToString(CultureInfo.InvariantCulture))),
            GraceDays = policy.GraceDays,
            EmailCustomer = policy.EmailCustomer,
        };
        policyContext = new EditContext(policyForm);
        policyDaysPreview = null;
        policyError = null;
        policySaved = false;
        await policyModal.ShowAsync();
    }

    private Task<GridDataProviderResult<DunningRow>> ProvideCasesAsync(GridDataProviderRequest<DunningRow> request) => view.ApplyAsync(request, Shown);

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        pageSize = DefaultPageSize;
        selected = new List<DunningRow>();
        gridVersion++;
        SetStatus("The view is reset: the soonest next step first, 25 cases a page, and every column shown.", false);
        StateHasChanged();

        await Task.Yield();
        await SaasDisplay.FocusAsync(countRegion);
    }

    /// <summary>Retries the selected open cases now; the message says how many went through and that the others still need a new card.</summary>
    private async Task RetrySelectedAsync()
    {
        bulkError = null;
        var outcome = State.BulkRetryDunning(selected.Select(row => row.Id).ToList());
        SetStatus(outcome.Result switch
        {
            SaasResult.NotAllowed => "Sign in as a Lumora operator to retry payments. Nothing changed.",
            SaasResult.Invalid => "Select the cases to retry.",
            _ => (outcome.Changed == 0 ? "No payment went through." : $"{SaasState.Plural(outcome.Changed, "payment", "payments")} went through, and those tenants are recovered.")
                + (outcome.Unchanged > 0 ? $" {SaasState.Plural(outcome.Unchanged, "case still fails", "cases still fail")} (or {(outcome.Unchanged == 1 ? "was" : "were")} already recovered): the customer needs to replace the card." : string.Empty),
        }, outcome.Changed == 0);

        await FinishBulkAsync();
    }

    private string RowClass(DunningRow row) => selected.Any(item => item.Id == row.Id) ? "sd-row-selected" : string.Empty;

    /// <summary>Saves the policy once the form is valid (rule WR-002); the state checks the same rules again.</summary>
    private async Task SavePolicyAsync()
    {
        if (!await policyContext.ValidateAsync() || ParseDays(policyForm.RetryDays) is not { } days || policyForm.GraceDays is not { } grace)
            return;

        var result = State.UpdateDunningPolicy(days, grace, policyForm.EmailCustomer);

        switch (result)
        {
            case SaasResult.Ok:
                SetStatus($"Policy saved: {SaasState.DescribePolicy(days, grace)} It applies to charges that fail from now on.", false);
                policySaved = true;
                await policyModal.HideAsync();

                return;

            case SaasResult.NoChange:
                policyError = "Nothing changed: this is already the policy.";

                return;

            default:
                policyError = result == SaasResult.NotAllowed ? "Sign in as a Lumora operator to change the policy." : "The policy couldn't be saved. Check the retry days and the grace period.";

                return;
        }
    }

    private async Task SelectStageAsync(string value)
    {
        stageFilter = value;
        await FiltersChangedAsync();
    }

    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message, bool isError)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusIsError = isError;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    private int StageCount(string filter) => Matching(byStage: false).Count(row => filter switch
    {
        OpenValue => row.StageValue != SaasDunningStage.Recovered,
        AllValue => true,
        _ => row.StageValue.ToString() == filter,
    });

    /// <summary>The summary cards: retrying, in grace, suspended, and the money at risk.</summary>
    private IEnumerable<(string Label, string Value, string Note, IconName Icon)> Summary(IReadOnlyList<SaasDunningCase> cases)
    {
        var retrying = cases.Where(item => item.Stage == SaasDunningStage.Retrying).ToList();
        var grace = cases.Where(item => item.Stage == SaasDunningStage.Grace).ToList();
        var suspended = cases.Count(item => item.Stage == SaasDunningStage.Suspended);
        var nextRetry = retrying.Where(item => item.NextRetryAt is not null).Select(item => item.NextRetryAt!.Value).DefaultIfEmpty().Min();
        var nextSuspension = grace.Where(item => item.GraceEndsAt is not null).Select(item => item.GraceEndsAt!.Value).DefaultIfEmpty().Min();

        yield return ("Retrying", Count(retrying.Count), retrying.Count == 0 ? "No retry is scheduled" : $"Next retry {SaasDisplay.Date(nextRetry)}", IconName.ArrowRepeat);
        yield return ("In grace", Count(grace.Count), grace.Count == 0 ? "Nobody is about to be suspended" : $"First suspension {SaasDisplay.Date(nextSuspension)}", IconName.HourglassSplit);
        yield return ("Suspended for non-payment", Count(suspended), suspended == 0 ? "None right now" : "Paying restores them", IconName.SlashCircle);
        yield return ("At risk", SaasDisplay.Money(State.DunningAtRisk), $"in {SaasState.Plural(cases.Count(item => item.IsOpen), "open case", "open cases")}", IconName.CashCoin);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    private bool IsFiltered => search.Trim().Length > 0 || stageFilter != OpenValue;

    /// <summary>The policy as typed, in words, or what is wrong with it.</summary>
    private string PolicyPreview
    {
        get
        {
            var days = ParseDays(policyDaysPreview ?? policyForm.RetryDays);

            if (days is null || days.Count == 0)
                return "Enter the retry days as whole numbers, like 1, 3, 7.";

            if (policyForm.GraceDays is not { } grace)
                return "Enter the grace period in days.";

            return SaasState.PolicyProblem(days, grace) ?? SaasState.DescribePolicy(days, grace);
        }
    }

    private IReadOnlyList<DunningRow> Shown => Matching().ToList();

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One dunning case as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record DunningRow(
        int Id,
        int WorkspaceId,
        string Tenant,
        string Status,
        string Invoice,
        decimal Amount,
        string Stage,
        SaasDunningStage StageValue,
        string Reason,
        int Attempts,
        string NextStep,
        DateTime? NextAt,
        DateTime FailedAt,
        string Manager);

    /// <summary>The policy editor's fields, with the rules of <see cref="SaasState.UpdateDunningPolicy" />.</summary>
    private sealed class PolicyForm : IValidatableObject
    {
        public bool EmailCustomer { get; set; }

        [Required(ErrorMessage = "Enter the grace period in days, like 7 (0 for none).")]
        [Range(0, SaasState.MaxGraceDays, ErrorMessage = "Use 0 to 30 days of grace.")]
        public int? GraceDays { get; set; }

        [Required(ErrorMessage = "Enter the retry days, like 1, 3, 7.")]
        public string RetryDays { get; set; } = string.Empty;

        /// <summary>The retry days are whole numbers, 1 to 5 of them, each from 1 to 30 and later than the one before (a cross-field rule with the count).</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ParseDays(RetryDays) is not { Count: > 0 } days)
            {
                yield return new ValidationResult("Use whole numbers separated by commas, like 1, 3, 7.", new[] { nameof(RetryDays) });
                yield break;
            }

            if (SaasState.PolicyProblem(days, Math.Clamp(GraceDays ?? 0, 0, SaasState.MaxGraceDays)) is { } problem)
                yield return new ValidationResult(problem, new[] { nameof(RetryDays) });
        }
    }

    #endregion
}
