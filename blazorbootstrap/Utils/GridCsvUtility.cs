using System.Text;

namespace BlazorBootstrap;

/// <summary>
/// Writes grid rows as CSV text for <see cref="Grid{TItem}" /> export.
/// </summary>
internal static class GridCsvUtility
{
    #region Fields and Constants

    private const string LineBreak = "\r\n";

    private const char Separator = ',';

    /// <summary>
    /// Characters that make a spreadsheet app treat a text cell as a formula (CSV injection).
    /// </summary>
    private static readonly char[] formulaPrefixes = { '=', '+', '-', '@', '\t', '\r' };

    #endregion

    #region Methods

    #region Internal Methods

    /// <summary>
    /// Writes a header row of the column labels, then one row per item. Every value is quoted, inner quotes are doubled,
    /// and a text value that starts with a formula character is prefixed with an apostrophe so it stays text.
    /// </summary>
    internal static string Write<TItem>(IEnumerable<TItem> items, IReadOnlyList<(string Label, string PropertyName)> columns, CultureInfo culture)
    {
        var builder = new StringBuilder();

        builder.Append(string.Join(Separator, columns.Select(column => Quote(Neutralize(column.Label)))));
        builder.Append(LineBreak);

        foreach (var item in items)
        {
            builder.Append(string.Join(Separator, columns.Select(column => Quote(FormatValue(GetPropertyValue(item, column.PropertyName), culture)))));
            builder.Append(LineBreak);
        }

        return builder.ToString();
    }

    #endregion

    #region Private Methods

    private static string FormatValue(object? value, CultureInfo culture) =>
        value switch
        {
            null => string.Empty,
            string text => Neutralize(text),
            IFormattable formattable => formattable.ToString(null, culture),
            _ => Neutralize(value.ToString() ?? string.Empty)
        };

    /// <summary>
    /// Gets a property value by name, following dots for nested properties (for example <c>Address.City</c>).
    /// </summary>
    private static object? GetPropertyValue(object? item, string propertyName)
    {
        var value = item;

        foreach (var name in propertyName.Split('.'))
        {
            if (value is null)
                return null;

            value = value.GetType().GetProperty(name)?.GetValue(value);
        }

        return value;
    }

    private static string Neutralize(string text) =>
        text.Length > 0 && Array.IndexOf(formulaPrefixes, text[0]) >= 0 ? $"'{text}" : text;

    private static string Quote(string text) => $"\"{text.Replace("\"", "\"\"")}\"";

    #endregion

    #endregion
}
