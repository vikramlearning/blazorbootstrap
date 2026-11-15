using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Plans and prices in the operator console: a card per plan with its fields, the workspaces on it, their MRR, and the
/// largest seat count among them (the lowest seat limit allowed), and a dialog that edits the plan. The dialog checks the
/// rules of <see cref="SaasState.UpdatePlan" /> field by field (<see cref="SaasPlanDraft" />), previews the public price
/// and the MRR change, and turns each <see cref="SaasResult" /> into a message. Plans can't be added or removed.
/// </summary>
public partial class AdminPlans : ComponentBase, IDisposable
{
    #region Fields and Constants

    private const string PricingPath = SaasLayout.Base + "/pricing";

    /// <summary>The visible names of the form's fields, for the error summary.</summary>
    private static readonly Dictionary<string, string> fieldNames = new()
    {
        [nameof(SaasPlanDraft.Name)] = "Name",
        [nameof(SaasPlanDraft.Description)] = "Description",
        [nameof(SaasPlanDraft.Price)] = "Price",
        [nameof(SaasPlanDraft.Highlights)] = "Highlights",
        [nameof(SaasPlanDraft.CallToAction)] = "Button text",
        [nameof(SaasPlanDraft.SeatLimit)] = "Seat limit",
        [nameof(SaasPlanDraft.EventLimit)] = "Event limit",
        [nameof(SaasPlanDraft.DashboardLimit)] = "Dashboard limit",
        [nameof(SaasPlanDraft.IsSelfServe)] = "Self-serve",
    };

    private readonly Dictionary<string, ElementReference> editButtons = new();

    private readonly Dictionary<string, string> errors = new();

    private ElementReference callToActionInput;

    private ElementReference dashboardInput;

    private ElementReference descriptionInput;

    private SaasPlanDraft? draft;

    private string? editingId;

    private int errorCount;

    private string? errorMessage;

    private string? errorText;

    private ElementReference eventInput;

    private ElementReference highlightsInput;

    private Modal modal = default!;

    private ElementReference nameInput;

    private int noteCount;

    private string? noteMessage;

    private string? noteText;

    private ElementReference priceInput;

    /// <summary>The plan whose edit button gets focus back when the dialog closes.</summary>
    private string? returnFocusId;

    private ElementReference seatInput;

    private ElementReference selfServeInput;

    private int statusCount;

    private string? statusMessage;

    private string? statusText;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>What the plan costs on annual billing, as the pricing page computes it.</summary>
    private static string AnnualText(SaasPlan plan)
    {
        if (plan.MonthlyPricePerSeat == 0)
            return "Free on monthly and annual billing.";

        var annual = SaasPricing.MonthlySeatPrice(plan, SaasBillingPeriod.Annual);

        return $"Annual billing: {SaasDisplay.Money(annual)} per seat a month ({SaasDisplay.Money(annual * 12)} a year).";
    }

    /// <summary>The annual price a seat costs a year, and what it costs a month now when the price changes.</summary>
    private static string AnnualPreviewNote(decimal annual, decimal? currentAnnual) => currentAnnual is { } now
        ? $"{SaasDisplay.Money(annual * 12)} per seat a year; now {SaasDisplay.Money(now)} a month"
        : $"{SaasDisplay.Money(annual * 12)} per seat a year";

    private static string Count(long value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    private static string DashboardsText(int limit) => limit == 0 ? "Unlimited" : Count(limit);

    private static string EventsText(long limit) => limit == 0 ? "Unlimited" : $"{Count(limit)} ({SaasDisplay.Events(limit)})";

    private static string JoinNames(IReadOnlyList<string> names) =>
        names.Count <= 2 ? string.Join(" and ", names) : $"{string.Join(", ", names.Take(names.Count - 1))}, and {names[^1]}";

    private static string Plural(int count, string one, string many) => count == 1 ? $"1 {one}" : $"{Count(count)} {many}";

    /// <summary>The help of the "Most popular" switch: only one plan has the badge.</summary>
    private static string PopularHelp(SaasPlan? otherPopular) => otherPopular is null
        ? "Shows the \"Most popular\" badge on the pricing page. Only one plan has it."
        : $"Shows the \"Most popular\" badge on the pricing page. Only one plan has it: turning it on here removes it from {otherPopular.Name}.";

    /// <summary>The draft's price when it is valid for the plan, for the preview; <see langword="null" /> otherwise.</summary>
    private static decimal? PreviewPrice(SaasPlan plan, SaasPlanDraft form)
    {
        if (plan.MonthlyPricePerSeat == 0)
            return 0m;

        return form.TryGetPrice(out var price) && price > 0 && price <= SaasPlanDraft.MaxPrice && price == Math.Round(price, 2) ? price : null;
    }

    /// <summary>A price per seat: whole dollars when it has no cents ("$49"), otherwise with cents ("$49.50").</summary>
    private static string SeatPrice(decimal price) => price == Math.Round(price, 0) ? SaasDisplay.Money(price) : SaasDisplay.MoneyCents(price);

    private static string SeatsHelp(SaasPlan plan, PlanSummary summary) => summary.Workspaces == 0
        ? $"1 to {Count(SaasSeedData.MaxSeats)}. No workspace is on {plan.Name} yet."
        : $"1 to {Count(SaasSeedData.MaxSeats)}, and at least {Count(summary.LargestSeats)}: the largest of the {Plural(summary.Workspaces, "workspace", "workspaces")} on {plan.Name}.";

    private static string SeatsText(int limit) => limit == 0 ? "Unlimited" : Count(limit);

    /// <summary>A change in MRR, such as "+$120" or "−$45".</summary>
    private static string SignedMoney(decimal value) => value switch
    {
        > 0 => "+" + SaasDisplay.Money(value),
        < 0 => "−" + SaasDisplay.Money(-value),
        _ => "no change",
    };

    private string? AriaInvalid(string field) => errors.ContainsKey(field) ? "true" : null;

    private void ClearMessages()
    {
        errorMessage = errorText = noteMessage = noteText = null;
        errorCount = noteCount = 0;
    }

    /// <summary>The <c>aria-describedby</c> of a field: its help text, and its error while it has one.</summary>
    private string Describe(string field, string helpId) => errors.ContainsKey(field) ? $"{helpId} {helpId}-error" : helpId;

    private async Task EditAsync(string planId)
    {
        if (State.FindPlan(planId) is not { } plan)
            return;

        editingId = plan.Id;
        returnFocusId = plan.Id;
        draft = SaasPlanDraft.From(plan);
        errors.Clear();
        ClearMessages();

        await modal.ShowAsync();
    }

    /// <summary>A field changed: its error and the dialog's messages no longer apply.</summary>
    private void Edited(string field)
    {
        errors.Remove(field);
        ClearMessages();
    }

    private string? ErrorOf(string field) => errors.TryGetValue(field, out var message) ? message : null;

    private ElementReference? FieldElement(string field) => field switch
    {
        nameof(SaasPlanDraft.Name) => nameInput,
        nameof(SaasPlanDraft.Description) => descriptionInput,
        nameof(SaasPlanDraft.Price) => priceInput,
        nameof(SaasPlanDraft.Highlights) => highlightsInput,
        nameof(SaasPlanDraft.CallToAction) => callToActionInput,
        nameof(SaasPlanDraft.SeatLimit) => seatInput,
        nameof(SaasPlanDraft.EventLimit) => eventInput,
        nameof(SaasPlanDraft.DashboardLimit) => dashboardInput,
        nameof(SaasPlanDraft.IsSelfServe) => selfServeInput,
        _ => null,
    };

    private Task HideAsync() => modal.HideAsync();

    private string InvalidClass(string field) => errors.ContainsKey(field) ? "is-invalid" : string.Empty;

    /// <summary>The MRR of the plan's paying workspaces now and at a price, and how many pay.</summary>
    private (decimal Before, decimal After, int Paying) MrrAt(SaasPlan plan, decimal price)
    {
        var priced = plan with { MonthlyPricePerSeat = price };
        var paying = State.Workspaces.Where(workspace => workspace.PlanId == plan.Id && State.MonthlyRevenueFor(workspace) > 0).ToList();

        return (paying.Sum(State.MonthlyRevenueFor), paying.Sum(workspace => SaasPricing.MonthlyAmount(priced, workspace.Period, workspace.Seats)), paying.Count);
    }

    /// <summary>The modal opens from code, so Bootstrap doesn't move focus back to what opened it; this does (WCAG 2.4.3).</summary>
    private async Task OnHiddenAsync()
    {
        var target = returnFocusId is { } id && editButtons.TryGetValue(id, out var button) ? button : (ElementReference?)null;

        returnFocusId = null;
        editingId = null;
        draft = null;
        errors.Clear();
        ClearMessages();

        await SaasDisplay.FocusAsync(target);
    }

    private Task OnShownAsync() => SaasDisplay.FocusAsync(nameInput);

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>The plan currently marked "Most popular" other than this one, or <see langword="null" />.</summary>
    private SaasPlan? OtherPopular(string planId) => State.Plans.FirstOrDefault(plan => plan.Id != planId && plan.IsPopular);

    private static string PriceText(SaasPlan plan) => plan.MonthlyPricePerSeat == 0 ? "Free" : SeatPrice(plan.MonthlyPricePerSeat);

    private string SavedMessage(SaasPlan before, SaasPlan after, SaasPlan? popularBefore)
    {
        var message = $"Saved {after.Name}. The pricing page, sign-up, and billing show it now.";

        if (after.MonthlyPricePerSeat != before.MonthlyPricePerSeat)
        {
            var onPlan = Summary(after).Workspaces;
            message += $" The price went from {SeatPrice(before.MonthlyPricePerSeat)} to {SeatPrice(after.MonthlyPricePerSeat)} per seat a month";
            message += onPlan == 0 ? "." : $"; the {Plural(onPlan, "workspace", "workspaces")} on it pay the new price from their next invoice.";
        }

        if (after.IsPopular && popularBefore is { } other)
            message += $" {other.Name} is no longer marked Most popular.";

        return message;
    }

    private async Task SaveAsync()
    {
        if (draft is null || Editing is not { } current)
        {
            ShowError("That plan no longer exists in this tab. Close the dialog and open the plan again.");

            return;
        }

        var summary = Summary(current);
        errors.Clear();

        foreach (var (field, message) in draft.Validate(current, State.Plans, summary.Workspaces, summary.LargestSeats))
            errors[field] = message;

        if (errors.Count > 0)
        {
            var names = errors.Keys.Select(field => fieldNames.TryGetValue(field, out var name) ? name : field).ToList();
            ShowError($"Check {(errors.Count == 1 ? "this field" : $"these {errors.Count} fields")}: {JoinNames(names)}.");
            StateHasChanged();
            await Task.Yield();
            await SaasDisplay.FocusAsync(FieldElement(errors.Keys.First()));

            return;
        }

        var edited = draft.ToPlan(current);
        var popularBefore = OtherPopular(current.Id);
        var result = State.UpdatePlan(edited);

        switch (result)
        {
            case SaasResult.Ok:
                ShowStatus(SavedMessage(current, State.FindPlan(current.Id) ?? edited, popularBefore));
                await modal.HideAsync();
                break;

            case SaasResult.NoChange:
                ShowNote($"Nothing to save: {current.Name} already has these details.");
                break;

            case SaasResult.SeatsOutOfRange:
                // A workspace may have grown since the dialog opened: read the seats again.
                var now = Summary(current);
                errors[nameof(SaasPlanDraft.SeatLimit)] = SaasPlanDraft.SeatLimitMessage(current.Name, now.Workspaces, now.LargestSeats);
                ShowError(SaasPlanDraft.SeatLimitMessage(current.Name, now.Workspaces, now.LargestSeats));
                StateHasChanged();
                await Task.Yield();
                await SaasDisplay.FocusAsync(seatInput);
                break;

            case SaasResult.NotAllowed:
                ShowError("Only a signed-in Lumora operator can change plans. Sign in as an operator and try again.");
                break;

            case SaasResult.NotFound:
                ShowError("That plan no longer exists in this tab. Close the dialog and reload the page.");
                break;

            default:
                ShowError($"{current.Name} couldn't be saved: a value is out of range. A free plan stays free, a paid plan stays paid, and at least one plan stays self-serve.");
                break;
        }
    }

    /// <summary>Shows an error; the same error again gets a counter, so a screen reader announces it again.</summary>
    private void ShowError(string message)
    {
        noteMessage = noteText = null;
        noteCount = 0;
        errorCount = message == errorText ? errorCount + 1 : 1;
        errorText = message;
        errorMessage = errorCount == 1 ? message : $"{message} ({errorCount})";
    }

    /// <summary>Shows a note in the dialog; the same note again gets a counter.</summary>
    private void ShowNote(string message)
    {
        errorMessage = errorText = null;
        errorCount = 0;
        noteCount = message == noteText ? noteCount + 1 : 1;
        noteText = message;
        noteMessage = noteCount == 1 ? message : $"{message} ({noteCount})";
    }

    /// <summary>Shows a page message after a save; the same message again gets a counter.</summary>
    private void ShowStatus(string message)
    {
        statusCount = message == statusText ? statusCount + 1 : 1;
        statusText = message;
        statusMessage = statusCount == 1 ? message : $"{message} ({statusCount})";
    }

    /// <summary>The workspaces on a plan: how many, how many pay and are on trial, their MRR, and the largest seat count.</summary>
    private PlanSummary Summary(SaasPlan plan)
    {
        var onPlan = State.Workspaces.Where(workspace => workspace.PlanId == plan.Id).ToList();

        return new PlanSummary(
            onPlan.Count,
            onPlan.Count(workspace => State.MonthlyRevenueFor(workspace) > 0),
            onPlan.Count(workspace => workspace.Status == SaasWorkspaceStatus.Trial),
            onPlan.Sum(State.MonthlyRevenueFor),
            onPlan.Count == 0 ? 0 : onPlan.Max(workspace => workspace.Seats));
    }

    private static string WorkspacesNote(SaasPlan plan, PlanSummary summary)
    {
        if (summary.Workspaces == 0)
            return "No workspaces yet";

        if (plan.MonthlyPricePerSeat == 0)
            return "None billed: the plan is free";

        var parts = new List<string>();

        if (summary.Paying > 0)
            parts.Add($"{Count(summary.Paying)} paying");

        if (summary.Trials > 0)
            parts.Add($"{Count(summary.Trials)} on trial");

        var others = summary.Workspaces - summary.Paying - summary.Trials;

        if (others > 0)
            parts.Add($"{Count(others)} not billed");

        return string.Join(", ", parts);
    }

    #endregion

    #endregion

    #region Properties, Indexers

    private static int AnnualDiscountPercent => (int)Math.Round(SaasPricing.AnnualDiscount * 100, MidpointRounding.AwayFromZero);

    private SaasPlan? Editing => editingId is null ? null : State.FindPlan(editingId);

    [Inject] private SaasState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The workspaces on one plan.</summary>
    /// <param name="Workspaces">How many workspaces are on the plan, in any status.</param>
    /// <param name="Paying">How many count toward MRR (active or past due on a paid plan).</param>
    /// <param name="Trials">How many are on trial.</param>
    /// <param name="Mrr">Their monthly recurring revenue.</param>
    /// <param name="LargestSeats">The largest seat count among them: the lowest seat limit allowed.</param>
    private sealed record PlanSummary(int Workspaces, int Paying, int Trials, decimal Mrr, int LargestSeats);

    #endregion
}
