using System.Globalization;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Saas;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Saas;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Saas;

/// <summary>
/// Plans and prices in the operator console: a card per plan with its fields, the workspaces on it, their MRR, and the
/// largest seat count among them (the lowest seat limit allowed), and a dialog that edits the plan in an
/// <see cref="EditForm" /> (rule WR-002). The dialog checks the rules of <see cref="SaasState.UpdatePlan" /> field by field
/// (<see cref="SaasPlanDraft" />: lengths, the price range, whole-number limits, a seat limit no lower than the largest
/// workspace on the plan, at least one self-serve plan, and Most popular only on a self-serve plan), checks as you type that
/// no other plan has the name, and on a failed save shows a summary that links to each field. It previews the public price
/// and the MRR change, and turns each <see cref="SaasResult" /> into a message, a taken name or a seat limit on its field.
/// Plans can't be added or removed.
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
        [nameof(SaasPlanDraft.IsPopular)] = "Most popular",
    };

    private readonly Dictionary<string, ElementReference> editButtons = new();

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

    /// <summary>The dialog's edit context: the draft's rule messages, the name check's, and the state's answers live here, so the summary lists them.</summary>
    private EditContext planContext = new(new SaasPlanDraft());

    /// <summary>The draft's rule messages and the state's answers, by field.</summary>
    private ValidationMessageStore? planErrors;

    private ShowcaseUniqueCheck? nameCheck;

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
        : $"1 to {Count(SaasSeedData.MaxSeats)}, and at least {Count(summary.LargestSeats)}: the largest of the {SaasState.Plural(summary.Workspaces, "workspace", "workspaces")} on {plan.Name}.";

    private static string SeatsText(int limit) => limit == 0 ? "Unlimited" : Count(limit);

    /// <summary>A change in MRR, such as "+$120" or "−$45".</summary>
    private static string SignedMoney(decimal value) => value switch
    {
        > 0 => "+" + SaasDisplay.Money(value),
        < 0 => "−" + SaasDisplay.Money(-value),
        _ => "no change",
    };

    private string? AriaInvalid(string field) => ErrorOf(field) is null ? null : "true";

    private void ClearMessages()
    {
        errorMessage = errorText = noteMessage = noteText = null;
        errorCount = noteCount = 0;
    }

    /// <summary>The <c>aria-describedby</c> of a field: its help text; the form's script adds its message while it has one.</summary>
    private static string Describe(string field, string helpId) => helpId;

    private async Task EditAsync(string planId)
    {
        if (State.FindPlan(planId) is not { } plan)
            return;

        editingId = plan.Id;
        returnFocusId = plan.Id;
        draft = SaasPlanDraft.From(plan);
        planContext = new EditContext(draft);
        planErrors = new ValidationMessageStore(planContext);
        planContext.OnValidationStateChanged += (_, _) => InvokeAsync(StateHasChanged);
        ClearMessages();

        await modal.ShowAsync();
    }

    /// <summary>A field changed: its message and the dialog's messages no longer apply, and the name check runs again for a new name.</summary>
    private void Edited(string field)
    {
        if (draft is null)
            return;

        var identifier = new FieldIdentifier(draft, field);
        planErrors?.Clear(identifier);
        planContext.NotifyFieldChanged(identifier);
        planContext.NotifyValidationStateChanged();
        ClearMessages();
    }

    private string? ErrorOf(string field) => draft is null ? null : planContext.GetValidationMessages(new FieldIdentifier(draft, field)).FirstOrDefault();

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

    private string InvalidClass(string field) => ErrorOf(field) is null ? string.Empty : "is-invalid";

    /// <summary>Whether another plan has this name; the plan being edited doesn't count.</summary>
    private Task<bool> IsNameTakenAsync(string name) => Task.FromResult(State.IsPlanNameTaken(name, editingId));

    private string NameTakenMessage(string name) => $"Another plan is already called \"{name.Trim()}\". Plan names show side by side on the pricing page, so choose a different name.";

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
        planErrors?.Clear();
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
            message += onPlan == 0 ? "." : $"; the {SaasState.Plural(onPlan, "workspace", "workspaces")} on it pay the new price from their next invoice.";
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
        planErrors?.Clear();

        // The name check finishes first; then every rule runs, and a failed save shows the summary, which takes focus (rule WR-002).
        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();
        planContext.Validate();

        foreach (var (field, message) in draft.Validate(current, State.Plans, summary.Workspaces, summary.LargestSeats))
            planErrors?.Add(new FieldIdentifier(draft, field), message);

        planContext.NotifyValidationStateChanged();

        if (!nameFree || planContext.GetValidationMessages().Any())
        {
            var names = fieldNames.Keys.Where(field => ErrorOf(field) is not null).Select(field => fieldNames[field]).ToList();
            ShowError(names.Count == 0 ? "Check the highlighted fields." : $"Check {(names.Count == 1 ? "this field" : $"these {names.Count} fields")}: {JoinNames(names)}.");

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
                planErrors?.Add(new FieldIdentifier(draft, nameof(SaasPlanDraft.SeatLimit)), SaasPlanDraft.SeatLimitMessage(current.Name, now.Workspaces, now.LargestSeats));
                planContext.NotifyValidationStateChanged();
                ShowError(SaasPlanDraft.SeatLimitMessage(current.Name, now.Workspaces, now.LargestSeats));
                StateHasChanged();
                await Task.Yield();
                await SaasDisplay.FocusAsync(seatInput);
                break;

            case SaasResult.Duplicate:
                planErrors?.Add(new FieldIdentifier(draft, nameof(SaasPlanDraft.Name)), NameTakenMessage(draft.Name));
                planContext.NotifyValidationStateChanged();
                ShowError("That name is taken by another plan. Nothing was saved.");
                break;

            case SaasResult.NotAllowed:
                ShowError("Only a signed-in Lumora operator can change plans. Sign in as an operator and try again.");
                break;

            case SaasResult.NotFound:
                ShowError("That plan no longer exists in this tab. Close the dialog and reload the page.");
                break;

            default:
                ShowError($"{current.Name} couldn't be saved: a value is out of range. A free plan stays free, a paid plan stays paid, at least one plan stays self-serve, and only a self-serve plan can be Most popular.");
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
