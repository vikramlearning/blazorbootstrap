using BlazorBootstrap.Demo.RCL.Models.Showcase.Fintech;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Fintech;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Fintech;

/// <summary>
/// Adds a payee (<c>/payees/new</c>) or changes one (<c>/payees/{Id}</c>) in two steps. Step 1 takes the account holder's
/// name, the routing number (9 digits), the account number (4 to 17 digits; an existing payee's is entered again, since
/// only the last four are kept), and a nickname, with a demo note of the four accounts the name check knows. "Check name"
/// asks <see cref="FintechState.CheckPayee" /> and explains the result: a match; a close match, with the actual name and
/// "Use {name}"; no match, with a clear warning and a checkbox to go on anyway; or a bank that can't check. Changing a
/// detail clears the result. Step 2 confirms with a one-time code (<see cref="FintechCodeStep" />), which saves through
/// <see cref="FintechState.SavePayee" /> and goes back to the payees list with a status message.
/// </summary>
public partial class PayeeEdit : ComponentBase, IDisposable
{
    #region Fields and Constants

    private readonly FintechPayeeDraft draft = new();

    private readonly Dictionary<string, string> errors = new(StringComparer.Ordinal);

    private bool acceptMismatch;

    private ElementReference accountInput;

    private FintechPayeeCheck? check;

    /// <summary>The name the result in <see cref="check" /> is about, as it was typed.</summary>
    private string checkedName = string.Empty;

    private ElementReference checkRegion;

    private ElementReference detailsTitle;

    private ElementReference errorRegion;

    private FintechPayee? existing;

    /// <summary>Where focus goes after the next render, if anywhere.</summary>
    private FocusTarget focusNext;

    private ElementReference nameInput;

    private ElementReference nicknameInput;

    private ElementReference reviewTitle;

    private ElementReference routingInput;

    /// <summary>Set once the payee is saved, while the payees list opens.</summary>
    private bool saved;

    private string? saveError;

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
            FocusTarget.Account => accountInput,
            FocusTarget.Check => checkRegion,
            FocusTarget.Details => detailsTitle,
            FocusTarget.Error => errorRegion,
            FocusTarget.Name => nameInput,
            FocusTarget.Nickname => nicknameInput,
            FocusTarget.Review => reviewTitle,
            FocusTarget.Routing => routingInput,
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
            draft.Name = payee.Name;
            draft.Nickname = payee.Nickname;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    #endregion

    #region Internal Methods

    /// <summary>Returns the editor's URL for a payee.</summary>
    internal static string EditUrl(string payeeId) => $"{FintechLayout.Base}/payees/{Uri.EscapeDataString(payeeId)}";

    #endregion

    #region Private Methods

    private static string Digits(string? value) => new((value ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

    private static bool HasOnlyNumberCharacters(string? value) => (value ?? string.Empty).All(c => char.IsAsciiDigit(c) || c is ' ' or '-');

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

    /// <summary>Validates the details, then asks the bank whether the name matches the account (nothing is saved).</summary>
    private void CheckName()
    {
        saveError = null;

        if (!Validate())
            return;

        check = State.CheckPayee(draft);
        checkedName = draft.Name.Trim();
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

    /// <summary>A changed detail makes the name check out of date.</summary>
    private void DetailsChanged()
    {
        check = null;
        acceptMismatch = false;
    }

    private string? Error(string field) => errors.TryGetValue(field, out var message) ? message : null;

    private string? Invalid(string field) => errors.ContainsKey(field) ? "is-invalid" : null;

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Saves the payee with the code; the code step shows a wrong code itself, and any other problem is shown here.</summary>
    private FintechResult SubmitCode(string code)
    {
        var result = State.SavePayee(existing?.Id, draft, code, acceptMismatch, out var payee);

        if (result == FintechResult.Ok && payee is not null)
        {
            saved = true;
            Navigation.NavigateTo($"{FintechLayout.Base}/payees?saved={Uri.EscapeDataString(payee.Id)}{(existing is null ? "&added=true" : string.Empty)}");

            return result;
        }

        if (result == FintechResult.WrongCode)
            return result;

        saveError = result switch
        {
            FintechResult.Invalid => "Some details aren't valid. Check the name, routing number, account number, and nickname.",
            FintechResult.Duplicate => $"You already have a payee called {draft.Name.Trim()}. Use a different name, or edit that payee.",
            FintechResult.LimitReached => $"You already have {FintechState.MaxPayees} payees, the most you can save. Delete one to add another.",
            FintechResult.WrongState => "The name doesn't match the account. Confirm that you still want to add this payee, or change the details.",
            FintechResult.InUse => "A scheduled or held transfer still goes to this payee, so its details can't change. Cancel that transfer first, or add a new payee.",
            FintechResult.NotFound => "This payee is no longer in your payees, so nothing was saved.",
            FintechResult.NotAllowed => "Sign in again to change your payees. Nothing was saved.",
            _ => "That didn't work, and nothing was saved. Try again.",
        };

        // Back to the details, with a fresh name check, so the problem can be fixed there.
        step = EditStep.Details;
        check = Validate() ? State.CheckPayee(draft) : null;
        checkedName = draft.Name.Trim();
        focusNext = FocusTarget.Error;
        StateHasChanged();

        return result;
    }

    /// <summary>Uses the account holder's name from a close match, and checks again (now a match).</summary>
    private void UseActualName()
    {
        if (check?.ActualName is not { } actual)
            return;

        draft.Name = actual;
        CheckName();
    }

    /// <summary>Checks every field, records the messages, and focuses the first field with a problem.</summary>
    private bool Validate()
    {
        errors.Clear();
        var name = draft.Name?.Trim() ?? string.Empty;
        var routing = Digits(draft.RoutingNumber);
        var account = Digits(draft.AccountNumber);

        if (name.Length == 0)
            errors["name"] = "Enter the account holder's name.";
        else if (name.Length > 60)
            errors["name"] = "Keep the name to 60 characters.";
        else if (State.Payees.Any(other => other.Id != existing?.Id && string.Equals(other.Name, name, StringComparison.OrdinalIgnoreCase)))
            errors["name"] = $"You already have a payee called {name}. Use a different name, or edit that payee.";

        if (!HasOnlyNumberCharacters(draft.RoutingNumber) || routing.Length != 9)
            errors["routing"] = "Enter the 9 digits of the routing number.";

        if (!HasOnlyNumberCharacters(draft.AccountNumber) || account.Length is < 4 or > 17)
            errors["account"] = "Enter an account number of 4 to 17 digits.";

        if (draft.Nickname is { } nickname && nickname.Trim().Length > 30)
            errors["nickname"] = "Keep the nickname to 30 characters.";

        focusNext = errors.ContainsKey("name") ? FocusTarget.Name
            : errors.ContainsKey("routing") ? FocusTarget.Routing
            : errors.ContainsKey("account") ? FocusTarget.Account
            : errors.ContainsKey("nickname") ? FocusTarget.Nickname
            : focusNext;

        return errors.Count == 0;
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
        Account,
        Check,
        Details,
        Error,
        Name,
        Nickname,
        Review,
        Routing,
    }

    #endregion
}
