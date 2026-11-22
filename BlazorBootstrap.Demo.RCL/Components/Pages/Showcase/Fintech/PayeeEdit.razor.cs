using System.ComponentModel.DataAnnotations;
using BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Adds a payee (<c>/payees/new</c>) or changes one (<c>/payees/{Id}</c>) in two steps. Step 1 is an <see cref="EditForm" />
/// (rule WR-002) with the account holder's name (required, up to 60 characters, unique among the payees, checked as you
/// type), the routing number (9 digits with a valid ABA check digit), the account number (4 to 17 digits, and not the
/// routing number again, a cross-field rule; an existing payee's is entered again, since only the last four are kept), and a
/// nickname (optional, up to 30 characters, unique, checked as you type), with a demo note of the four accounts the name
/// check knows. Messages show after a field is touched or on submit, and a failed submit shows a summary that links to each
/// field. "Check name" asks <see cref="FintechState.CheckPayee" /> and explains the result: a match; a close match, with the
/// actual name and "Use {name}"; no match, with a clear warning and a checkbox to go on anyway; or a bank that can't check.
/// Changing a detail clears the result. Step 2 confirms with a one-time code (<see cref="FintechCodeStep" />), which saves
/// through <see cref="FintechState.SavePayee" /> and goes back to the payees list with a status message; the state's refusals
/// are shown on their field or in the alert.
/// </summary>
public partial class PayeeEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    private bool acceptMismatch;

    private FintechPayeeCheck? check;

    /// <summary>The name the result in <see cref="check" /> is about, as it was typed.</summary>
    private string checkedName = string.Empty;

    private ElementReference checkRegion;

    private ElementReference detailsTitle;

    private EditContext editContext = default!;

    private ElementReference errorRegion;

    private FintechPayee? existing;

    /// <summary>Where focus goes after the next render, if anywhere.</summary>
    private FocusTarget focusNext;

    private PayeeForm form = new();

    private ShowcaseUniqueCheck? nameCheck;

    private ShowcaseUniqueCheck? nicknameCheck;

    private ElementReference reviewTitle;

    /// <summary>Set once the payee is saved, while the payees list opens.</summary>
    private bool saved;

    private string? saveError;

    /// <summary>The state's answers that belong to a field (a taken name or nickname, a bad number), shown like the form's own messages.</summary>
    private ValidationMessageStore? serverErrors;

    private EditStep step = EditStep.Details;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var target = focusNext;
        focusNext = FocusTarget.None;

        ElementReference? element = target switch
        {
            FocusTarget.Check => checkRegion,
            FocusTarget.Details => detailsTitle,
            FocusTarget.Error => errorRegion,
            FocusTarget.Review => reviewTitle,
            _ => null,
        };

        await FintechDisplay.FocusAsync(element);
    }

    protected override void OnInitialized()
    {
        State.Changed += OnStateChanged;

        if (Id is not null && State.FindPayee(Id) is { } payee)
        {
            existing = payee;
            form.Name = payee.Name;
            form.Nickname = payee.Nickname;
        }

        editContext = new EditContext(form);
        serverErrors = new ValidationMessageStore(editContext);
        editContext.OnFieldChanged += OnFieldChanged;
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

    #region Internal Methods

    /// <summary>Returns the editor's URL for a payee.</summary>
    internal static string EditUrl(string payeeId) => $"{FintechLayout.Base}/payees/{Uri.EscapeDataString(payeeId)}";

    #endregion

    #region Private Methods

    private static string Digits(string? value) => new((value ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

    private static string LastFour(string? value)
    {
        var digits = Digits(value);

        return digits.Length >= 4 ? digits[^4..] : digits;
    }

    private void BackToDetails()
    {
        step = EditStep.Details;
        focusNext = FocusTarget.Details;
    }

    /// <summary>
    /// Validates the details once the uniqueness checks have finished (a failed submit shows the summary), then asks the bank
    /// whether the name matches the account (nothing is saved).
    /// </summary>
    private async Task CheckNameAsync()
    {
        saveError = null;
        serverErrors?.Clear();
        editContext.NotifyValidationStateChanged();

        var nameFree = nameCheck is null || await nameCheck.CheckNowAsync();
        var nicknameFree = nicknameCheck is null || await nicknameCheck.CheckNowAsync();

        if (!await editContext.ValidateAsync() || !nameFree || !nicknameFree)
            return;

        check = State.CheckPayee(form.ToDraft());
        checkedName = form.Name.Trim();
        acceptMismatch = false;
        focusNext = FocusTarget.Check;
    }

    private void ContinueToCode()
    {
        if (check is null)
        {
            focusNext = FocusTarget.Check;
            return;
        }

        if (check.Match == FintechPayeeMatch.NoMatch && !acceptMismatch)
            return;

        saveError = null;
        step = EditStep.Code;

        // The code step focuses its own field when a code is already waiting; otherwise the summary's heading takes focus.
        focusNext = State.Challenge(FintechCodePurpose.Payee) is null ? FocusTarget.Review : FocusTarget.None;
    }

    private Task<bool> IsNameTakenAsync(string name) => Task.FromResult(State.IsPayeeNameTaken(name, existing?.Id));

    private Task<bool> IsNicknameTakenAsync(string nickname) => Task.FromResult(State.IsNicknameTaken(nickname, existing?.Id));

    private string NameTakenMessage(string name) => $"You already have a payee called {name.Trim()}. Use a different name, or edit that payee.";

    private string NicknameTakenMessage(string nickname) => $"Another payee already has the nickname \"{nickname.Trim()}\". Choose a different one.";

    /// <summary>A changed detail makes the name check out of date, and clears the state's message on that field.</summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        serverErrors?.Clear(args.FieldIdentifier);
        check = null;
        acceptMismatch = false;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Shows a state answer under its field, like the form's own messages; the summary and focus follow.</summary>
    private void ServerError(Expression<Func<object?>> field, string message)
    {
        serverErrors?.Clear();
        serverErrors?.Add(FieldIdentifier.Create(field), message);
        editContext.NotifyValidationStateChanged();
    }

    /// <summary>Saves the payee with the code; the code step shows a wrong code itself, and any other problem is shown here.</summary>
    private FintechResult SubmitCode(string code)
    {
        var result = State.SavePayee(existing?.Id, form.ToDraft(), code, acceptMismatch, out var payee);

        if (result == FintechResult.Ok && payee is not null)
        {
            saved = true;
            Navigation.NavigateTo($"{FintechLayout.Base}/payees?saved={Uri.EscapeDataString(payee.Id)}{(existing is null ? "&added=true" : string.Empty)}");

            return result;
        }

        if (result == FintechResult.WrongCode)
            return result;

        // Back to the details, with a fresh name check, so the problem can be fixed there.
        step = EditStep.Details;
        check = null;
        checkedName = form.Name.Trim();
        StateHasChanged();

        switch (result)
        {
            case FintechResult.Duplicate when State.IsPayeeNameTaken(form.Name, existing?.Id):
                ServerError(() => form.Name, NameTakenMessage(form.Name));
                break;

            case FintechResult.Duplicate:
                ServerError(() => form.Nickname, NicknameTakenMessage(form.Nickname ?? string.Empty));
                break;

            case FintechResult.Invalid:
                ServerError(() => form.RoutingNumber, "Check the routing number, account number, and nickname: one isn't valid.");
                break;

            default:
                saveError = result switch
                {
                    FintechResult.LimitReached => $"You already have {FintechState.MaxPayees} payees, the most you can save. Delete one to add another.",
                    FintechResult.WrongState => "The name doesn't match the account. Confirm that you still want to add this payee, or change the details.",
                    FintechResult.InUse => "A scheduled or held transfer still goes to this payee, so its details can't change. Cancel that transfer first, or add a new payee.",
                    FintechResult.NotFound => "This payee is no longer in your payees, so nothing was saved.",
                    FintechResult.NotAllowed => "Sign in again to change your payees. Nothing was saved.",
                    _ => "That didn't work, and nothing was saved. Try again.",
                };
                check = State.CheckPayee(form.ToDraft());
                focusNext = FocusTarget.Error;
                break;
        }

        return result;
    }

    /// <summary>Uses the account holder's name from a close match, and checks again (now a match).</summary>
    private async Task UseActualNameAsync()
    {
        if (check?.ActualName is not { } actual)
            return;

        form.Name = actual;
        editContext.NotifyFieldChanged(FieldIdentifier.Create(() => form.Name));
        await CheckNameAsync();
    }

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the payee to change from the route; <see langword="null" /> adds a new payee.</summary>
    [Parameter] public string? Id { get; set; }

    private bool IsNew => Id is null;

    private static string ListUrl => $"{FintechLayout.Base}/payees";

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private FintechState State { get; set; } = default!;

    #endregion

    #region Nested Types

    /// <summary>The editor's steps.</summary>
    private enum EditStep
    {
        Details,
        Code,
    }

    /// <summary>What to focus after the next render.</summary>
    private enum FocusTarget
    {
        None,
        Check,
        Details,
        Error,
        Review,
    }

    /// <summary>
    /// The payee's fields, with the bank's limits (<see cref="FintechState.SavePayee" />): a name, a routing number whose ABA
    /// check digit is right, an account number of 4 to 17 digits that isn't the routing number again (a cross-field rule), and
    /// an optional nickname. Spaces and hyphens are allowed between digits, as people copy them from a check.
    /// </summary>
    private sealed class PayeeForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter the account number.")]
        [RegularExpression(@"^[\d\s-]+$", ErrorMessage = "Use only digits in the account number; spaces and hyphens are fine.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the account holder's name.")]
        [StringLength(FintechState.MaxPayeeNameLength, ErrorMessage = "Keep the name to 60 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(FintechState.MaxNicknameLength, ErrorMessage = "Keep the nickname to 30 characters.")]
        public string? Nickname { get; set; }

        [Required(ErrorMessage = "Enter the 9-digit routing number.")]
        [RegularExpression(@"^[\d\s-]+$", ErrorMessage = "Use only digits in the routing number; spaces and hyphens are fine.")]
        public string RoutingNumber { get; set; } = string.Empty;

        /// <summary>Returns the details as the bank's draft.</summary>
        public FintechPayeeDraft ToDraft() => new() { Name = Name, Nickname = Nickname, RoutingNumber = RoutingNumber, AccountNumber = AccountNumber };

        /// <summary>Runs once the fields are filled in: the routing number's length and check digit, and the account number's length and difference from it.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var routing = Digits(RoutingNumber);
            var account = Digits(AccountNumber);

            if (routing.Length != 9)
                yield return new ValidationResult("Enter the 9 digits of the routing number.", new[] { nameof(RoutingNumber) });
            else if (!FintechState.IsRoutingNumber(RoutingNumber))
                yield return new ValidationResult("That isn't a valid routing number: its check digit doesn't match. Check it against a check or their bank's website.", new[] { nameof(RoutingNumber) });

            if (account.Length is < 4 or > 17)
                yield return new ValidationResult("Enter an account number of 4 to 17 digits.", new[] { nameof(AccountNumber) });
            else if (account == routing)
                yield return new ValidationResult("That's the routing number again. Enter their account number here.", new[] { nameof(AccountNumber) });
        }
    }

    #endregion
}
