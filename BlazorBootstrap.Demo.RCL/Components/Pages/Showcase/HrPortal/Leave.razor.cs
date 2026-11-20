using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BlazorBootstrap.Demo.RCL.Models.Showcase.HrPortal;
using BlazorBootstrap.Demo.RCL.Services.Showcase.HrPortal;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.HrPortal;

/// <summary>
/// My leave: the signed-in employee's balances for this year (<see cref="HrState.Balances" />: allowance, used, waiting, and
/// available in words with a stacked <see cref="Progress" />; unlimited types show only the days used), a validated request
/// form (rule WR-002) that also edits a waiting request (<see cref="HrState.RequestLeave" />,
/// <see cref="HrState.UpdateLeave" />): the type (required) with its description and notice rule; the first and last day
/// (required, the last on or after the first, in the same year, at most <see cref="HrState.MaxLeaveSpanDays" /> calendar
/// days, with the notice the type needs, and with a working day in it: cross-field rules), with a live "That's N working
/// days" naming the holidays it skips; and an optional note of up to 300 characters. A failed submit shows a summary that
/// links to each field; an overlap or a short balance from the state is shown under the dates the same way. Then their
/// requests, upcoming and past, with the status in words, who decided and when, and the manager's note. Cancel (waiting, or
/// approved and not started) is confirmed in a <see cref="Modal" /> that names the days going back to the balance and runs
/// once the dialog is hidden (<see cref="HrState.CancelLeave" />). Company holidays are listed in a <c>details</c>. Every
/// outcome is announced in an always-rendered status region.
/// </summary>
public partial class Leave : ComponentBase, IDisposable
{
    #region Fields and Constants

    private Modal cancelModal = default!;

    /// <summary>The key of the Cancel button that opened the dialog, so Keep the request returns focus to it.</summary>
    private string? cancelOpenerKey;

    /// <summary>The request the cancel dialog is about.</summary>
    private HrLeaveRequest? cancelRequest;

    private ElementReference? dialogDismiss;

    private EditContext editContext = new(new LeaveForm());

    /// <summary>The request being edited; 0 while requesting new leave.</summary>
    private int editingId;

    private string editingLabel = string.Empty;

    /// <summary>Returns the element to focus after the next render.</summary>
    private Func<ElementReference?>? focusTarget;

    private LeaveForm form = new();

    private ElementReference? formHeading;

    /// <summary>The note as typed, for the live counter (the bound value changes only on change).</summary>
    private string noteText = string.Empty;

    /// <summary>The Edit and Cancel buttons, by key.</summary>
    private readonly Dictionary<string, ElementReference> openers = new();

    /// <summary>The confirmed cancellation, run once the dialog is hidden.</summary>
    private int? pendingCancelId;

    /// <summary>The state's answers that belong to a field (an overlap, a short balance), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private bool statusIsError;

    private string? statusMessage;

    private ElementReference? statusRegion;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusTarget is { } target)
        {
            focusTarget = null;
            await HrDisplay.FocusAsync(target());
        }
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;
        NewForm(State.Policies.FirstOrDefault()?.Id ?? 0);
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Private Methods

    /// <summary>"5 days used and 3 days waiting for approval, of 25 days a year."</summary>
    private static string BalanceSummary(HrBalance balance) =>
        $"{HrDisplay.Days(balance.Available)} available: {HrDisplay.Days(balance.Used)} used and {HrDisplay.Days(balance.Pending)} waiting for approval, of {HrDisplay.Days(balance.Allowance)} a year.";

    /// <summary>Whether a request can still be cancelled: waiting, or approved and not started.</summary>
    private static bool CanCancel(HrLeaveRequest request, DateTime today) =>
        request.Status == HrLeaveStatus.Pending || (request.Status == HrLeaveStatus.Approved && request.From > today);

    private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    /// <summary>The first day a type allows: its notice from today, or up to 30 days back for unlimited types.</summary>
    private static DateTime Earliest(HrLeavePolicy policy, DateTime today) =>
        policy.IsUnlimited ? today.AddDays(Math.Min(policy.MinNoticeDays, -LeaveText.UnlimitedBackdateDays)) : today.AddDays(policy.MinNoticeDays);

    /// <summary>The form field a rule problem belongs to.</summary>
    private static string MemberOf(FormField field) => field switch
    {
        FormField.Policy => nameof(LeaveForm.PolicyId),
        FormField.From => nameof(LeaveForm.From),
        FormField.Note => nameof(LeaveForm.Note),
        _ => nameof(LeaveForm.To),
    };

    private static string NoticeWords(int days) => days == 1 ? "1 day" : FormattableString.Invariant($"{days} days");

    private static double Percent(int days, int allowance) => allowance <= 0 ? 0 : Math.Min(100, Math.Round(days * 100.0 / allowance, 1));

    private static string StatusWords(HrLeaveStatus status) => status switch
    {
        HrLeaveStatus.Pending => "waiting for approval",
        _ => status.ToString().ToLowerInvariant(),
    };

    /// <summary>"Rafael Costa" (the manager), or "HR" for someone with no manager.</summary>
    private string ApproverName(HrEmployee me) => me.ManagerId is { } managerId && State.FindEmployee(managerId) is { } manager ? manager.Name : "HR";

    private async Task AskCancelAsync(HrLeaveRequest request, string openerKey)
    {
        cancelRequest = request;
        cancelOpenerKey = openerKey;
        pendingCancelId = null;
        StateHasChanged();

        await cancelModal.ShowAsync();
    }

    /// <summary>Names what cancelling does to the balance.</summary>
    private string CancelConsequence(HrLeaveRequest request)
    {
        var policy = State.FindPolicy(request.PolicyId);
        var me = State.CurrentEmployee;
        var approver = me is null ? "your manager" : ApproverName(me);

        if (policy is null || policy.IsUnlimited)
            return $"The {HrDisplay.Plural(request.Days, "working day")} won't be counted as {policy?.Name ?? "leave"} any more{(request.Status == HrLeaveStatus.Approved ? $", and {approver} sees it as cancelled" : null)}.";

        var balance = LeaveText.BalanceFor(me is null ? Array.Empty<HrLeaveRequest>() : State.LeaveOf(me.Id), policy, request.From.Year);
        var after = balance.Left + request.Days;

        return request.Status == HrLeaveStatus.Approved
            ? $"{approver} already approved it. {HrDisplay.Days(request.Days)} go back to your {policy.Name} balance for {request.From.Year}: {HrDisplay.Days(after)} available instead of {HrDisplay.Days(balance.Left)}. Let {approver} know you'll be working those days."
            : $"{HrDisplay.Days(request.Days)} waiting for approval go back to your {policy.Name} balance for {request.From.Year}: {HrDisplay.Days(after)} available instead of {HrDisplay.Days(balance.Left)}. {approver} won't be asked to decide it.";
    }

    /// <summary>Cancels a request after the dialog is hidden, and says what happened to the balance.</summary>
    private void CancelRequest(int requestId)
    {
        var request = State.FindLeave(requestId);
        var policy = request is null ? null : State.FindPolicy(request.PolicyId);
        var label = request is null ? "that request" : $"your request for {LeaveText.RequestLabel(request, policy)}";
        var result = State.CancelLeave(requestId);

        if (result == HrResult.Ok && editingId == requestId)
            ResetForm();

        switch (result)
        {
            case HrResult.Ok when request is not null && policy is { IsUnlimited: false } && State.CurrentEmployee is { } me:
                var left = LeaveText.BalanceFor(State.LeaveOf(me.Id), policy, request.From.Year).Left;
                SetStatus($"Cancelled {label}. {HrDisplay.Days(request.Days)} went back to your {policy.Name} balance: {HrDisplay.Days(left)} available for {request.From.Year}.", false);
                break;

            case HrResult.Ok:
                SetStatus($"Cancelled {label}. Those days aren't counted any more.", false);
                break;

            case HrResult.NotFound:
                SetStatus("That request no longer exists, so nothing was cancelled.", true);
                break;

            case HrResult.WrongState when request is { Status: HrLeaveStatus.Approved }:
                SetStatus($"{Capitalize(label)} has already started, so it can't be cancelled here. Ask your manager or HR to change it. Nothing changed.", true);
                break;

            case HrResult.WrongState when request is not null:
                SetStatus($"{Capitalize(label)} is already {StatusWords(request.Status)}, so nothing changed.", true);
                break;

            case HrResult.NotAllowed:
                SetStatus("Your sign-in ended. Sign in again to cancel leave; nothing changed.", true);
                break;

            default:
                SetStatus("That didn't work, and nothing was cancelled. Try again.", true);
                break;
        }
    }

    private Task ConfirmCancelAsync()
    {
        if (pendingCancelId is not null || cancelRequest is not { } request)
            return Task.CompletedTask;

        pendingCancelId = request.Id;

        return cancelModal.HideAsync();
    }

    /// <summary>"you", the decider's name, or "a former colleague".</summary>
    private string DeciderName(int deciderId, HrEmployee me) => deciderId == me.Id ? "you" : State.FindEmployee(deciderId)?.Name ?? "a former colleague";

    /// <summary>
    /// The live line under the dates: how many working days the range takes and the holidays it skips, what would be left of a
    /// limited balance, or why the range can't be requested.
    /// </summary>
    private string DaysText(HrEmployee me, HrLeavePolicy? policy)
    {
        if (form.From is not { } from || form.To is not { } to)
            return "Pick the first and last day to see how many working days that is.";

        from = from.Date;
        to = to.Date;

        if (to < from)
            return "The last day is before the first.";

        if (from.Year != to.Year)
            return "A request can't run across two years, because balances are per calendar year.";

        if ((to - from).TotalDays >= HrState.MaxLeaveSpanDays)
            return FormattableString.Invariant($"A request can cover at most {HrState.MaxLeaveSpanDays} calendar days.");

        var days = State.WorkingDays(from, to);
        var holidays = LeaveText.WeekdayHolidays(State.Holidays, from, to);
        var weekend = Enumerable.Range(0, (int)(to - from).TotalDays + 1).Any(offset => from.AddDays(offset).DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday);
        var skips = holidays.Count > 0 ? $" It skips {LeaveText.HolidayNames(holidays)}{(weekend ? " and the weekend" : null)}." : weekend ? " Weekends aren't counted." : null;

        if (days == 0)
            return $"Those days are all weekends or company holidays, so there are no working days to take.{skips}";

        var text = $"That's {HrDisplay.Plural(days, "working day")}.{skips}";

        if (policy is { IsUnlimited: false })
        {
            var left = LeaveText.BalanceFor(State.LeaveOf(me.Id), policy, from.Year, editingId == 0 ? null : editingId).Left - days;
            text += left >= 0
                ? $" You'd have {HrDisplay.Days(left)} of {policy.Name} left for {from.Year}."
                : $" That's {HrDisplay.Days(-left)} more than the {policy.Name} you have left for {from.Year}.";
        }

        return text;
    }

    /// <summary>Fills the form with a waiting request, to change it.</summary>
    private void Edit(HrLeaveRequest request)
    {
        editingId = request.Id;
        editingLabel = LeaveText.RequestLabel(request, State.FindPolicy(request.PolicyId));
        NewForm(request.PolicyId, request.From, request.To, request.Note);
        focusTarget = () => formHeading;
    }

    /// <summary>Starts a fresh form (and its validation), wired to the page's rules and the state's field answers.</summary>
    private void NewForm(int policyId, DateTime? from = null, DateTime? to = null, string note = "")
    {
        form = new LeaveForm { PolicyId = policyId, From = from, To = to, Note = note, Rules = RulesFor };
        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
        noteText = note;
    }

    private Task OnDialogShownAsync() => HrDisplay.FocusAsync(dialogDismiss);

    /// <summary>
    /// The dialog opens from code, so Bootstrap doesn't return focus. A confirmed cancellation runs now that the dialog is
    /// gone and focus moves to its outcome; Keep the request or Escape returns focus to the Cancel button.
    /// </summary>
    private void OnDialogHidden()
    {
        var requestId = pendingCancelId;
        var openerKey = cancelOpenerKey;
        pendingCancelId = null;
        cancelRequest = null;
        cancelOpenerKey = null;

        if (requestId is { } id)
            CancelRequest(id);
        else if (openerKey is not null && openers.TryGetValue(openerKey, out var opener))
            focusTarget = () => opener;
    }

    /// <summary>
    /// A changed field clears the state's answers (they were about the old values). A first day without a last day gets the
    /// same day, which the employee can change.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear();

        if (args.FieldIdentifier.FieldName == nameof(LeaveForm.From) && form.To is null && form.From is { } from)
            form.To = from.Date;
    }

    private void OnNoteInput(ChangeEventArgs args) => noteText = args.Value?.ToString() ?? string.Empty;

    /// <summary>Re-renders; a new request's type falls back to the first offered one when its type is gone (a restored tab, or HR removed it).</summary>
    private void OnStateChanged() => InvokeAsync(() =>
    {
        if (editingId == 0 && State.FindPolicy(form.PolicyId) is not { IsActive: true })
            form.PolicyId = State.Policies.FirstOrDefault()?.Id ?? 0;

        StateHasChanged();
    });

    /// <summary>The offered types, plus the edited request's type when it is no longer offered.</summary>
    private IReadOnlyList<HrLeavePolicy> PolicyOptions()
    {
        var options = State.Policies.ToList();

        if (State.FindPolicy(form.PolicyId) is { IsActive: false } current)
            options.Add(current);

        return options;
    }

    /// <summary>The type's description, yearly days, and notice rule.</summary>
    private string PolicyHelp(HrLeavePolicy? policy)
    {
        if (policy is null)
            return "Choose the type of leave.";

        var today = State.Clock.Today;
        var description = policy.Description.Length > 0 ? policy.Description.TrimEnd('.') + ". " : string.Empty;
        var days = policy.IsUnlimited ? "No yearly limit. " : $"{HrDisplay.Days(policy.AnnualDays)} a year. ";
        var notice = policy.IsUnlimited
            ? $"You can also record days up to {LeaveText.UnlimitedBackdateDays} days back (from {HrDisplay.Date(Earliest(policy, today))})."
            : policy.MinNoticeDays == 0 ? "No notice needed: start today or later."
            : $"Request at least {NoticeWords(policy.MinNoticeDays)} ahead: start on {HrDisplay.Date(Earliest(policy, today))} or later.";

        return policy.IsActive ? description + days + notice : $"{policy.Name} is no longer offered; pick another type to save this request.";
    }

    private void ResetForm()
    {
        editingId = 0;
        editingLabel = string.Empty;
        NewForm(State.FindPolicy(form.PolicyId) is { IsActive: true } kept ? kept.Id : State.Policies.FirstOrDefault()?.Id ?? 0);
    }

    /// <summary>
    /// The rules the portal checks before saving, in the same order, as the field to point at and what to fix; or
    /// <see langword="null" /> when the range can be requested.
    /// </summary>
    private (FormField Field, string Message)? RuleProblem(HrLeavePolicy policy, DateTime from, DateTime to)
    {
        var today = State.Clock.Today;
        var earliest = Earliest(policy, today);

        if (!policy.IsActive)
            return (FormField.Policy, $"{policy.Name} is no longer offered for new requests or changes. Pick another type.");

        if (to < from)
            return (FormField.To, $"The last day must be on or after the first ({HrDisplay.Date(from)}).");

        if (from.Year != to.Year)
            return (FormField.To, FormattableString.Invariant($"Leave can't run across two years, because balances are per calendar year. End on Dec 31, {from.Year}, and request the days from Jan 1 separately."));

        if ((to - from).TotalDays >= HrState.MaxLeaveSpanDays)
            return (FormField.To, FormattableString.Invariant($"A request can cover at most {HrState.MaxLeaveSpanDays} calendar days: end by {HrDisplay.Date(from.AddDays(HrState.MaxLeaveSpanDays - 1))}, and request the rest separately."));

        if (from < earliest)
        {
            return (FormField.From, policy.IsUnlimited ? $"{policy.Name} can be recorded up to {LeaveText.UnlimitedBackdateDays} days back: start on {HrDisplay.Date(earliest)} or later."
                : policy.MinNoticeDays == 0 ? $"{policy.Name} can't start in the past: start today ({HrDisplay.Date(today)}) or later."
                : $"{policy.Name} needs a notice of {NoticeWords(policy.MinNoticeDays)}: start on {HrDisplay.Date(earliest)} or later.");
        }

        if (from > HrSeedData.Now.AddYears(3).Date)
            return (FormField.From, $"That's too far ahead: start before {HrDisplay.Date(HrSeedData.Now.AddYears(3))}.");

        if (State.WorkingDays(from, to) == 0)
            return (FormField.To, "Those days are all weekends or company holidays, so there are no working days to take. Pick a range with a working day in it.");

        return null;
    }

    /// <summary>The form's cross-field rules (the type, the dates, and the notice), as validation results on their fields.</summary>
    private IEnumerable<ValidationResult> RulesFor(LeaveForm values)
    {
        if (State.FindPolicy(values.PolicyId) is not { } policy)
        {
            yield return new ValidationResult("That type of leave was removed. Pick another type.", new[] { nameof(LeaveForm.PolicyId) });

            yield break;
        }

        if (values.From is { } from && values.To is { } to && RuleProblem(policy, from.Date, to.Date) is { } problem)
            yield return new ValidationResult(problem.Message, new[] { MemberOf(problem.Field) });
    }

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(System.Linq.Expressions.Expression<Func<object>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(field, message);
        editContext.NotifyValidationStateChanged();
    }

    private void SetStatus(string message, bool isError)
    {
        statusMessage = message;
        statusIsError = isError;
        focusTarget = () => statusRegion;
    }

    /// <summary>Leaves editing without saving, and returns focus to the Edit button (or the form's heading when it is gone).</summary>
    private void StopEditing()
    {
        var key = $"edit-{editingId}";
        ResetForm();
        focusTarget = openers.TryGetValue(key, out var opener) ? () => opener : () => formHeading;
    }

    /// <summary>
    /// Requests the leave (or saves the edited request) once the form is valid; a failed submit shows the summary (rule
    /// WR-002). The state checks again: an overlap or a short balance is shown under the dates, anything else is announced.
    /// </summary>
    private Task SubmitAsync()
    {
        serverErrors?.Clear();

        if (State.CurrentEmployee is not { } me)
        {
            SetStatus("Your sign-in ended. Sign in again to request leave; nothing was saved.", true);

            return Task.CompletedTask;
        }

        if (!editContext.Validate() || State.FindPolicy(form.PolicyId) is not { } policy || form.From is not { } fromValue || form.To is not { } toValue)
            return Task.CompletedTask;

        var first = fromValue.Date;
        var last = toValue.Date;
        var note = form.Note.Trim();
        var requestId = editingId;
        var verb = requestId == 0 ? "requested" : "saved";
        var result = requestId == 0 ? State.RequestLeave(policy.Id, first, last, note, out _) : State.UpdateLeave(requestId, policy.Id, first, last, note);
        var approver = ApproverName(me);
        var days = State.WorkingDays(first, last);
        var range = HrDisplay.Range(first, last);

        switch (result)
        {
            case HrResult.Ok:
                ResetForm();
                SetStatus(requestId == 0
                    ? $"Requested {HrDisplay.Days(days)} of {policy.Name}, {range}. It's waiting for {approver}'s decision; until then the days show as waiting in your balance."
                    : $"Saved your {policy.Name} request, {range} ({HrDisplay.Days(days)}). It's still waiting for {approver}'s decision.", false);

                break;

            case HrResult.NoChange:
                SetStatus($"You didn't change your request for {editingLabel}, so nothing was saved.", false);

                break;

            case HrResult.Overlap:
                var other = State.LeaveOf(me.Id).FirstOrDefault(item => item.Id != requestId && item.Status is HrLeaveStatus.Pending or HrLeaveStatus.Approved && LeaveText.Overlaps(item, first, last));
                ServerError(() => form.From, other is null
                    ? $"Those days overlap another of your requests. Change the dates; nothing was {verb}."
                    : $"Those days overlap your request for {LeaveText.RequestLabel(other, State.FindPolicy(other.PolicyId))} ({HrDisplay.LeaveLabel(other.Status).ToLowerInvariant()}). Change the dates, or {(other.Status == HrLeaveStatus.Pending ? "edit or cancel" : "cancel")} that request first; nothing was {verb}.");

                break;

            case HrResult.InsufficientBalance:
                var left = LeaveText.BalanceFor(State.LeaveOf(me.Id), policy, first.Year, requestId == 0 ? null : requestId).Left;
                ServerError(() => form.To, $"You have {HrDisplay.Days(left)} of {policy.Name} left for {first.Year}, and this request takes {HrDisplay.Days(days)}. Shorten it, or pick another type; nothing was {verb}.");

                break;

            case HrResult.Invalid when RuleProblem(policy, first, last) is { } rule:
                serverErrors?.Add(new FieldIdentifier(form, MemberOf(rule.Field)), rule.Message);
                editContext.NotifyValidationStateChanged();

                break;

            case HrResult.Invalid:
                SetStatus($"This demo tab's storage is full, so nothing was {verb}. Cancel an old request, or start a fresh tab.", true);

                break;

            case HrResult.NotFound when requestId != 0 && State.FindLeave(requestId) is null:
                ResetForm();
                SetStatus("That request no longer exists, so nothing was saved. Request the leave again if you still need it.", true);

                break;

            case HrResult.NotFound:
                ServerError(() => form.PolicyId, "That type of leave was removed. Pick another type.");

                break;

            case HrResult.WrongState when State.FindLeave(requestId) is { } current:
                ResetForm();
                SetStatus($"Your request for {LeaveText.RequestLabel(current, State.FindPolicy(current.PolicyId))} was {StatusWords(current.Status)} while you were editing it, so it can't be changed any more. Nothing was saved.", true);

                break;

            case HrResult.NotAllowed:
                SetStatus($"Your sign-in ended. Sign in again to request leave; nothing was {verb}.", true);

                break;

            default:
                SetStatus($"That didn't work, and nothing was {verb}. Try again.", true);

                break;
        }

        return Task.CompletedTask;
    }

    #endregion

    #endregion

    #region Properties, Indexers

    [Inject] private HrState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>Where a rule problem belongs.</summary>
    private enum FormField
    {
        Policy,
        From,
        To,
        Note,
    }

    /// <summary>
    /// The request form's fields: a type, the first and last day (required), and a note of up to 300 characters. The
    /// cross-field rules (dates in order, one year, the span, the notice, a working day) come from the page through
    /// <see cref="Rules" />, since they need the type and today.
    /// </summary>
    private sealed class LeaveForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the first day of leave as a full date.")]
        public DateTime? From { get; set; }

        [StringLength(HrState.MaxNoteLength, ErrorMessage = "Shorten the note to 300 characters or fewer.")]
        public string Note { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Choose the type of leave.")]
        public int PolicyId { get; set; }

        /// <summary>Gets or sets the page's cross-field rules; not a field.</summary>
        public Func<LeaveForm, IEnumerable<ValidationResult>>? Rules { get; set; }

        [Required(ErrorMessage = "Enter the last day of leave as a full date.")]
        public DateTime? To { get; set; }

        /// <summary>Runs once every field is filled in: the page's rules for the type and the dates.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => Rules?.Invoke(this) ?? Enumerable.Empty<ValidationResult>();
    }

    #endregion
}
