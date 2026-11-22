using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// Performance reviews (HR admin): a <see cref="Grid{TItem}" /> of the review cycles (name and scope, status, people,
/// progress of the current stage in words and as a bar, due dates, and when it opened), with sorting, the column chooser,
/// CSV export, and a saved view that can be reset; a row opens the cycle. "Open a review cycle" opens a validated dialog
/// (rule WR-002): a unique name, checked as you type; the whole company or some departments (at least one); the self-review
/// due date (after today) and the manager-review due date (after it, within <see cref="HrState.MaxReviewDueDays" /> days):
/// cross-field rules. A live preview says how many employees will be included, who isn't (joining, or without a manager),
/// and whether a cycle that isn't closed already covers them (<see cref="HrState.OpenCycle" />, whose answer maps to a
/// message). Opening a cycle notifies everyone in it who can sign in.
/// </summary>
public partial class AdminReviews : ComponentBase, IDisposable
{
    #region Fields and Constants

    private EditContext editContext = new(new CycleForm());

    private Modal editorModal = default!;

    private bool focusStatus;

    private CycleForm form = new();

    private string? formError;

    private int formErrorCount;

    private Grid<CycleRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    private ShowcaseUniqueCheck? nameCheck;

    private InputText? nameInput;

    private ElementReference? openButton;

    /// <summary>The cycle the dialog opened, so focus goes to the status once it closes.</summary>
    private HrReviewCycle? opened;

    private int statusCount;

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
        // The saved view lives in the tab's session storage, so it loads once the page is interactive; then the grid shows.
        if (firstRender)
        {
            await view.LoadAsync();
            viewLoaded = true;
            StateHasChanged();
        }

        if (focusStatus)
        {
            focusStatus = false;
            await HrDisplay.FocusAsync(statusRegion);
        }
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "review-cycles", HrState.MaxCycles);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>Whether another cycle already has this name.</summary>
    private Task<bool> IsNameTakenAsync(string value) =>
        Task.FromResult(State.ReviewCycles().Any(cycle => string.Equals(cycle.Name, value, StringComparison.OrdinalIgnoreCase)));

    /// <summary>The dialog opens from code, so Bootstrap doesn't return focus: after opening a cycle focus goes to the status, otherwise back to the button.</summary>
    private async Task OnEditorHiddenAsync()
    {
        if (opened is not null)
        {
            opened = null;
            await HrDisplay.FocusAsync(statusRegion);
        }
        else
        {
            await HrDisplay.FocusAsync(openButton);
        }
    }

    private Task OnEditorShownAsync() => HrDisplay.FocusAsync(nameInput?.Element);

    /// <summary>A cycle or a review changed (here or elsewhere): the grid reads the cycles again.</summary>
    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    /// <summary>Fills the dialog with a new cycle's defaults (self reviews due in two weeks, manager reviews two weeks later) and opens it.</summary>
    private async Task OpenEditorAsync()
    {
        var today = State.Clock.Today;
        form = new CycleForm { SelfReviewDue = today.AddDays(14), ManagerReviewDue = today.AddDays(28), Rules = RulesFor };
        editContext = new EditContext(form);
        formError = null;
        formErrorCount = 0;
        opened = null;

        await editorModal.ShowAsync();
    }

    /// <summary>The cycles for the grid, the newest first unless a column is sorted, with the current stage's progress in words.</summary>
    private Task<GridDataProviderResult<CycleRow>> ProvideCyclesAsync(GridDataProviderRequest<CycleRow> request)
    {
        var rows = State.ReviewCycles().Select(cycle =>
        {
            var progress = State.CycleProgress(cycle.Id);
            var included = progress.Total - progress.LeftOut;
            var (done, text) = cycle.Status switch
            {
                HrCycleStatus.Open => (progress.ManagerDone, FormattableString.Invariant($"{progress.SelfDone} of {progress.Total} self reviews, {progress.ManagerDone} manager reviews")),
                HrCycleStatus.Calibration => (progress.Calibrated, FormattableString.Invariant($"{progress.Calibrated} of {included} ratings calibrated")),
                HrCycleStatus.SignOff => (progress.Signed, FormattableString.Invariant($"{progress.Signed} of {included} signed off by both")),
                _ => (included, FormattableString.Invariant($"Closed: {progress.Signed} of {included} signed off by both")),
            };

            return new CycleRow(cycle.Id, cycle.Name, State.ScopeText(cycle), ReviewDisplay.StatusShort(cycle.Status), cycle.Status, progress.Total,
                text, included == 0 ? 0 : (double)done / (cycle.Status == HrCycleStatus.Open ? progress.Total : included),
                HrDisplay.Date(cycle.SelfReviewDue), cycle.SelfReviewDue, HrDisplay.Date(cycle.ManagerReviewDue), cycle.ManagerReviewDue, HrDisplay.Date(cycle.CreatedAt), cycle.CreatedAt);
        }).ToList();

        return view.ApplyAsync(request, rows);
    }

    private async Task RefreshGridAsync()
    {
        if (grid is not null)
            await grid.RefreshDataAsync();
    }

    /// <summary>Forgets the saved view and builds the grid again with its defaults; focus moves to the status.</summary>
    private async Task ResetViewAsync()
    {
        await view.ResetAsync();
        gridVersion++;
        SetStatus("The view is reset: the newest cycle first, and every column shown.");
        focusStatus = true;
    }

    /// <summary>The dialog's cross-field rules: the due dates in order and in range, a department chosen, and nobody already in a cycle that isn't closed.</summary>
    private IEnumerable<ValidationResult> RulesFor(CycleForm values)
    {
        var today = State.Clock.Today;

        if (values.SelfReviewDue is { } self && self.Date <= today)
            yield return new ValidationResult($"Pick a self-review due date after today ({HrDisplay.Date(today)}).", new[] { nameof(CycleForm.SelfReviewDue) });

        if (values.SelfReviewDue is { } first && values.ManagerReviewDue is { } manager)
        {
            if (manager.Date <= first.Date)
                yield return new ValidationResult($"Manager reviews must be due after the self reviews ({HrDisplay.Date(first)}), so managers can read them first.", new[] { nameof(CycleForm.ManagerReviewDue) });
            else if (manager.Date > today.AddDays(HrState.MaxReviewDueDays))
                yield return new ValidationResult(string.Create(CultureInfo.InvariantCulture, $"Pick a manager-review due date within {HrState.MaxReviewDueDays} days, by {HrDisplay.Date(today.AddDays(HrState.MaxReviewDueDays))}."),
                    new[] { nameof(CycleForm.ManagerReviewDue) });
        }

        if (!values.WholeCompany && values.DepartmentIds.Count == 0)
        {
            yield return new ValidationResult("Choose at least one department, or the whole company.", new[] { nameof(CycleForm.DepartmentIds) });

            yield break;
        }

        var scope = values.WholeCompany ? Array.Empty<string>() : values.DepartmentIds.ToArray();

        if (State.CycleOverlap(scope) is { } overlap)
            yield return new ValidationResult($"“{overlap.Name}” isn't closed yet and covers some of these people. Close it first, or choose other departments.", new[] { nameof(CycleForm.DepartmentIds) });
        else if (State.CycleParticipants(scope).Count == 0)
            yield return new ValidationResult("Nobody in these departments can take part: they're joining or have no manager. Choose other departments.", new[] { nameof(CycleForm.DepartmentIds) });
    }

    /// <summary>
    /// Opens the cycle once the form is valid: the name check finishes first, then every rule runs, and a failed submit
    /// shows the summary (rule WR-002). The state checks again; its answer maps to a message.
    /// </summary>
    private async Task SaveAsync()
    {
        formError = null;

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();

        if (!await editContext.ValidateAsync() || !nameFree)
            return;

        var scope = form.WholeCompany ? Array.Empty<string>() : form.DepartmentIds.ToArray();
        var result = State.OpenCycle(form.Name, scope, form.SelfReviewDue!.Value, form.ManagerReviewDue!.Value, out var cycle);

        switch (result)
        {
            case HrResult.Ok when cycle is not null:
                SetStatus($"Opened “{cycle.Name}” for {HrDisplay.Plural(State.CycleReviews(cycle.Id).Count, "person", "people")} in {State.ScopeText(cycle)}. "
                          + $"Everyone in it who can sign in is told their self review is waiting, due {HrDisplay.Date(cycle.SelfReviewDue)}.");
                opened = cycle;
                await editorModal.HideAsync();

                return;

            case HrResult.Duplicate:
                SetFormError("Another cycle already has that name. Choose another; nothing was opened.");

                return;

            case HrResult.Overlap:
                SetFormError("A cycle that isn't closed already covers some of these people. Close it first, or choose other departments; nothing was opened.");

                return;

            case HrResult.WrongState:
                SetFormError("Nobody in these departments can take part. Choose other departments; nothing was opened.");

                return;

            case HrResult.NotAllowed:
                SetFormError("Only the HR admin can open review cycles. Nothing was opened.");

                return;

            default:
                SetFormError(State.ReviewCycles().Count >= HrState.MaxCycles
                    ? $"There are {HrState.MaxCycles} cycles, the most the portal keeps. Nothing was opened."
                    : "The cycle couldn't be opened: a value is out of range, or this demo tab's storage is full. Nothing was opened.");

                return;
        }
    }

    /// <summary>Shows a dialog error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void SetFormError(string message)
    {
        formErrorCount++;
        formError = formErrorCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({formErrorCount})");
    }

    /// <summary>Shows a status message; the same message again gets a counter, so a screen reader announces it again.</summary>
    private void SetStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : string.Create(CultureInfo.InvariantCulture, $"{message} ({statusCount})");
    }

    /// <summary>Adds or removes a department from the scope, and tells the form the field changed (so its message follows).</summary>
    private void ToggleDepartment(string departmentId, bool chosen)
    {
        if (chosen && !form.DepartmentIds.Contains(departmentId))
            form.DepartmentIds.Add(departmentId);
        else if (!chosen)
            form.DepartmentIds.Remove(departmentId);

        editContext.NotifyFieldChanged(editContext.Field(nameof(CycleForm.DepartmentIds)));
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>
    /// The dialog's fields, with the limits of <see cref="HrState.OpenCycle" />: a name of 1 to 80 characters and both due
    /// dates required; the cross-field rules (dates in order and in range, a department, no overlap) come from the page
    /// through <see cref="Rules" />, since they need today and the other cycles.
    /// </summary>
    private sealed class CycleForm : IValidatableObject
    {
        /// <summary>Gets the departments chosen when it's not the whole company.</summary>
        public List<string> DepartmentIds { get; } = new();

        [Required(ErrorMessage = "Pick the day manager reviews are due.")]
        public DateTime? ManagerReviewDue { get; set; }

        [Required(ErrorMessage = "Give the cycle a name.")]
        [StringLength(HrState.MaxCycleNameLength, ErrorMessage = "Use 80 characters or fewer for the name.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the page's cross-field rules; not a field.</summary>
        public Func<CycleForm, IEnumerable<ValidationResult>>? Rules { get; set; }

        [Required(ErrorMessage = "Pick the day self reviews are due.")]
        public DateTime? SelfReviewDue { get; set; }

        public bool WholeCompany { get; set; } = true;

        /// <summary>Runs once the fields are filled in: the page's rules.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => Rules?.Invoke(this) ?? Enumerable.Empty<ValidationResult>();
    }

    /// <summary>One cycle as a grid row: the words each column shows and exports, and the values it sorts by.</summary>
    private sealed record CycleRow(int Id, string Name, string Scope, string Status, HrCycleStatus StatusKind, int People, string Progress, double Share, string SelfDue,
        DateTime SelfDueDate, string ManagerDue, DateTime ManagerDueDate, string Opened, DateTime OpenedAt);

    #endregion
}
