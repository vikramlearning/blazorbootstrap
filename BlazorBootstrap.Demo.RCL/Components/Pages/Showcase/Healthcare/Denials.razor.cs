using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Healthcare;

/// <summary>
/// Billing's denied claims in a <see cref="Grid{TItem}" />: claim number (the row opens the claim), patient, plan, the plan's
/// reasons in words, when it was denied, the appeal deadline with "overdue" or "due soon" marked (or "after appeal" for a
/// final denial), the visit's doctor, and the billed amount, soonest deadline first, with paging, sorting, the column
/// chooser, and CSV export. A filter shows the denials that can still be appealed, those due within 14 days, or the final
/// ones; the grid's view is saved for the visit and can be reset.
/// </summary>
public partial class Denials : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string AppealableFilter = "appealable";

    private const int DefaultPageSize = 25;

    private const string FinalFilter = "final";

    private const string SoonFilter = "soon";

    private ElementReference? countRegion;

    private Grid<DenialRow>? grid;

    /// <summary>Changes when the view is reset, so the grid is built again with its defaults.</summary>
    private int gridVersion;

    /// <summary>The grid's page size: the saved one once the view is loaded, then whatever the page size selector chose.</summary>
    private int pageSize = DefaultPageSize;

    private string stateFilter = string.Empty;

    private int statusCount;

    private string? statusMessage;

    private ElementReference? statusRegion;

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
    }

    protected override void OnInitialized()
    {
        view = GridViews.Get(State.SiteSlug, "denials", DefaultPageSize, "deadline", SortDirection.Ascending);
        State.Changed += OnStateChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    private void OnStateChanged() => InvokeAsync(async () =>
    {
        await RefreshGridAsync();
        StateHasChanged();
    });

    private async Task OnStateFilterChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        stateFilter = value is AppealableFilter or SoonFilter or FinalFilter ? value : string.Empty;

        if (grid is not null)
            await grid.ResetPageNumber();
    }

    /// <summary>The page of denials the grid asked for: soonest deadline first, unless a column is sorted.</summary>
    private Task<GridDataProviderResult<DenialRow>> ProvideDenialsAsync(GridDataProviderRequest<DenialRow> request) => view.ApplyAsync(request, Shown);

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
        gridVersion++;
        statusCount++;
        statusMessage = "The view is reset: soonest deadline first, 25 a page, and every column shown." + (statusCount == 1 ? null : string.Create(CultureInfo.InvariantCulture, $" ({statusCount})"));
        StateHasChanged();

        await Task.Yield();
        await HcDisplay.FocusAsync(countRegion);
    }

    /// <summary>Keeps the grid's settings in the saved view, and follows the page size the selector chose.</summary>
    private async Task SaveSettingsAsync(GridSettings settings)
    {
        if (settings.PageSize > 0)
            pageSize = settings.PageSize;

        await view.SaveSettingsAsync(settings);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private ShowcaseGridViewStore GridViews { get; set; } = default!;

    /// <summary>The denied claims that pass the filter, as grid rows.</summary>
    private IReadOnlyList<DenialRow> Shown
    {
        get
        {
            var today = State.Clock.Today;
            var visits = State.AllAppointments.ToDictionary(item => item.Id, item => item.DoctorId);

            return State.Claims
                .Where(claim => claim.Status == HcClaimStatus.Denied)
                .Where(claim => stateFilter switch
                {
                    AppealableFilter => !HcState.IsFinalDenial(claim),
                    SoonFilter => claim.AppealDeadline is { } deadline && deadline <= today.AddDays(14),
                    FinalFilter => HcState.IsFinalDenial(claim),
                    _ => true,
                })
                .Select(claim =>
                {
                    var answer = claim.Responses.LastOrDefault();
                    var deadline = claim.AppealDeadline;
                    var days = deadline is { } date ? (date - today).Days : (int?)null;

                    return new DenialRow(
                        claim.Id,
                        claim.Number,
                        State.FindPatient(claim.PatientId)?.Name ?? "A former patient",
                        State.FindPayer(claim.PayerId)?.Name ?? claim.PayerId,
                        answer is null ? string.Empty : ClaimDetail.CodesText(State, answer.DenialCodes),
                        answer?.At ?? claim.CreatedAt,
                        HcDisplay.Date(answer?.At ?? claim.CreatedAt),
                        deadline,
                        deadline is { } by ? $"{HcDisplay.Date(by)} ({ClaimDetail.DaysLeftText(by, today)})" : "After appeal: close it",
                        days is < 0 ? "Overdue" : days is <= 14 ? "Due soon" : string.Empty,
                        days is < 0,
                        State.FindDoctor(visits.GetValueOrDefault(claim.AppointmentId))?.Name ?? string.Empty,
                        claim.BilledCents);
                })
                .ToList();
        }
    }

    [Inject] private HcState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>One denied claim as the grid shows and exports it: every column is a property, so CSV export includes it.</summary>
    private sealed record DenialRow(
        int Id,
        string Number,
        string Patient,
        string Payer,
        string Reason,
        DateTime Denied,
        string DeniedText,
        DateTime? Deadline,
        string DeadlineText,
        string Urgency,
        bool IsOverdue,
        string Doctor,
        int Billed)
    {
        /// <summary>Gets the billed amount in words.</summary>
        public string BilledText => HcDisplay.Money(Billed);
    }

    #endregion
}
