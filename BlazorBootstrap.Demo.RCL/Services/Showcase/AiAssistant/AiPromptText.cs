using System.Globalization;
using System.Text.RegularExpressions;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.AiAssistant;

/// <summary>
/// The rules of a prompt's variables (shared by the state, the seed, and the prompt forms): a variable is written
/// <c>{{name}}</c> in the text, its name is lower-case letters, digits, and underscores starting with a letter, and every
/// variable the text uses is declared, and every declared one is used.
/// </summary>
public static partial class AiPromptText
{
    #region Fields and Constants

    /// <summary>The longest variable name.</summary>
    public const int MaxVariableLength = 24;

    /// <summary>The most variables a prompt declares.</summary>
    public const int MaxVariables = 8;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns whether the text's variables and the declared ones agree (no problem with either).</summary>
    public static bool IsConsistent(string text, IReadOnlyList<string> declared) => TextProblem(text, declared) is null && VariablesProblem(text, declared) is null;

    /// <summary>Returns whether a declared variable name is well formed.</summary>
    public static bool IsVariableName(string? name) => name is { Length: > 0 and <= MaxVariableLength } && NamePattern().IsMatch(name);

    /// <summary>Splits a comma-separated list of variables the way they are kept: trimmed, braces dropped, lower case, no empty ones, each once.</summary>
    public static List<string> ParseVariables(string? value) =>
        (value ?? string.Empty).Split(',').Select(item => item.Trim().Trim('{', '}').Trim().ToLowerInvariant()).Where(item => item.Length > 0).Distinct(StringComparer.Ordinal).ToList();

    /// <summary>
    /// Returns what is wrong with the placeholders in the text, in words, or <see langword="null" />: braces that don't
    /// close, a placeholder that isn't a variable name, or a variable that isn't declared.
    /// </summary>
    public static string? TextProblem(string text, IReadOnlyList<string> declared)
    {
        text ??= string.Empty;
        var opens = Count(text, "{{");
        var closes = Count(text, "}}");

        if (opens != closes)
            return "Close every variable with two braces on each side, like {{audience}}.";

        foreach (Match match in PlaceholderPattern().Matches(text))
        {
            var name = match.Groups[1].Value.Trim();

            if (!IsVariableName(name))
                return $"\"{match.Value}\" isn't a variable: use lower-case letters, digits, and underscores, starting with a letter, like {{{{audience}}}}.";
        }

        var undeclared = VariablesIn(text).Where(name => !declared.Contains(name)).ToList();

        return undeclared.Count == 0 ? null
            : undeclared.Count == 1 ? $"The text uses {{{{{undeclared[0]}}}}}, which isn't declared. Add {undeclared[0]} to the variables, or take it out of the text."
            : $"The text uses {string.Join(", ", undeclared.Select(name => "{{" + name + "}}"))}, which aren't declared. Add them to the variables, or take them out of the text.";
    }

    /// <summary>
    /// Returns what is wrong with the declared variables, in words, or <see langword="null" />: too many, a name that isn't
    /// well formed, or one the text doesn't use.
    /// </summary>
    public static string? VariablesProblem(string text, IReadOnlyList<string> declared)
    {
        if (declared.Count > MaxVariables)
            return string.Create(CultureInfo.InvariantCulture, $"Declare up to {MaxVariables} variables (now {declared.Count}).");

        if (declared.FirstOrDefault(name => !IsVariableName(name)) is { } bad)
            return $"\"{bad}\" can't be a variable: use up to {MaxVariableLength} lower-case letters, digits, and underscores, starting with a letter.";

        var used = VariablesIn(text ?? string.Empty);
        var unused = declared.Where(name => !used.Contains(name)).ToList();

        return unused.Count == 0 ? null
            : unused.Count == 1 ? $"The text doesn't use {unused[0]}. Put {{{{{unused[0]}}}}} where it goes, or remove it from the variables."
            : $"The text doesn't use {string.Join(", ", unused)}. Put each where it goes, like {{{{{unused[0]}}}}}, or remove them from the variables.";
    }

    /// <summary>Returns the well-formed variables the text uses, in the order they first appear, each once.</summary>
    public static List<string> VariablesIn(string text) =>
        PlaceholderPattern().Matches(text ?? string.Empty).Select(match => match.Groups[1].Value.Trim()).Where(IsVariableName).Distinct(StringComparer.Ordinal).ToList();

    #endregion

    #region Private Methods

    private static int Count(string text, string part)
    {
        var count = 0;

        for (var index = text.IndexOf(part, StringComparison.Ordinal); index >= 0; index = text.IndexOf(part, index + part.Length, StringComparison.Ordinal))
            count++;

        return count;
    }

    [GeneratedRegex("^[a-z][a-z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"\{\{([^{}]*)\}\}", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderPattern();

    #endregion

    #endregion
}
