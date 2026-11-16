using System.Text.RegularExpressions;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Learning;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;
using BlazorBootstrap.Demo.RCL.Services.Showcase.Learning;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Learning;

/// <summary>
/// Public certificate verification: one labelled field for the certificate ID (case-insensitive, spaces ignored) and Check.
/// The result (valid with the learner, course, and issue date, or not found with what to check) is announced in a status
/// region, and the checked ID is kept in the URL (<c>?code=</c>) so the result can be shared; a <c>?code=</c> link fills
/// the field and checks it.
/// </summary>
public partial class Verify : ComponentBase, IDisposable
{
    #region Fields and Constants

    private string? appliedQuery;

    private string? checkedCode;

    private ElementReference? codeInput;

    /// <summary>Whether the field takes focus after the next render (an empty Check).</summary>
    private bool focusInput;

    private string input = string.Empty;

    private string? inputError;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (focusInput)
        {
            focusInput = false;
            await LearningDisplay.FocusAsync(codeInput);
        }
    }

    protected override void OnInitialized() => State.Changed += OnStateChanged;

    protected override void OnParametersSet()
    {
        // The page's own navigation after Check sets the same value again; only a new ?code= (a shared link) refills the field.
        if (CodeQuery == appliedQuery)
            return;

        appliedQuery = CodeQuery;
        input = CodeQuery ?? string.Empty;
        inputError = null;

        var normalized = Normalize(CodeQuery);
        checkedCode = normalized.Length == 0 ? null : normalized;
    }

    #endregion

    #region Public Methods

    /// <summary>Stops listening to state changes.</summary>
    public void Dispose() => State.Changed -= OnStateChanged;

    /// <summary>Returns the route of the verify page, checking <paramref name="code" /> when one is given.</summary>
    public static string PathFor(string? code) => string.IsNullOrEmpty(code)
        ? $"{LearningLayout.Base}/verify"
        : $"{LearningLayout.Base}/verify?code={Uri.EscapeDataString(code)}";

    #endregion

    #region Private Methods

    [GeneratedRegex(@"^QP[A-Z0-9]{8}$")]
    private static partial Regex CompactCodePattern();

    [GeneratedRegex(@"^QP-[A-Z0-9]{4}-[A-Z0-9]{4}$")]
    private static partial Regex CodePattern();

    private static bool LooksLikeCode(string? code) => code is not null && CodePattern().IsMatch(code);

    /// <summary>Upper-cases the ID and drops whitespace; an ID typed without its dashes gets them back.</summary>
    private static string Normalize(string? value)
    {
        var compact = string.Concat((value ?? string.Empty).Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();

        return CompactCodePattern().IsMatch(compact) ? $"QP-{compact[2..6]}-{compact[6..]}" : compact;
    }

    private Task CheckAsync()
    {
        var normalized = Normalize(input);

        if (normalized.Length == 0)
        {
            inputError = "Enter a certificate ID to check, like QP-7K4M-R2XD.";
            checkedCode = null;

            // The previous result is gone, so its ?code= goes too (a reload or shared link would show it again).
            appliedQuery = null;
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("code", (string?)null), replace: true);

            // Focus after the render that adds the error and aria-invalid, so they're read with the field.
            focusInput = true;

            return Task.CompletedTask;
        }

        inputError = null;
        input = normalized;
        checkedCode = normalized;
        appliedQuery = normalized;
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("code", normalized), replace: true);

        return Task.CompletedTask;
    }

    private void OnStateChanged() => InvokeAsync(StateHasChanged);

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets or sets the certificate ID to check, from <c>?code=</c>.</summary>
    [SupplyParameterFromQuery(Name = "code")] public string? CodeQuery { get; set; }

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private LearningCertificate? Result => State.VerifyCertificate(checkedCode);

    [CascadingParameter] private ShowcaseSession Session { get; set; } = default!;

    [Inject] private LearningState State { get; set; } = default!;

    #endregion
}
