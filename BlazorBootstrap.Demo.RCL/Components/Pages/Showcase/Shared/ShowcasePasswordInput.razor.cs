using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBootstrap.Demo.RCL.Components.Pages.Showcase.Shared;

/// <summary>
/// A password field for the showcase's demo sign-in and account forms: an <see cref="InputBase{TValue}" /> (so it
/// validates like other form fields) with a show/hide toggle button whose name and pressed state are announced.
/// </summary>
public partial class ShowcasePasswordInput : InputBase<string?>
{
    #region Fields and Constants

    private bool visible;

    #endregion

    #region Methods

    #region Protected Override Methods

    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;

        return true;
    }

    #endregion

    #region Private Methods

    private void OnChange(ChangeEventArgs args) => CurrentValueAsString = args.Value?.ToString();

    private void Toggle() => visible = !visible;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the <c>autocomplete</c> value. Defaults to <c>off</c>: these are demo passwords, and the showcase doesn't
    /// invite the browser to fill in or save real ones.
    /// </summary>
    [Parameter] public string AutoComplete { get; set; } = "off";

    /// <summary>Gets or sets the input's ID, which its label's <c>for</c> points to.</summary>
    [Parameter, EditorRequired] public string Id { get; set; } = default!;

    #endregion
}
