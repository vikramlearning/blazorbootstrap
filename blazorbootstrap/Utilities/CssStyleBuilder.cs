namespace BlazorBootstrap;

/// <summary>
/// A helper class for building CSS styles.
/// </summary>
/// <remarks>
/// This class can be used to append strings, lists of strings, and conditions to a CSS style.
/// </remarks>
public class CssStyleBuilder
{
    #region Fields and Constants

    private readonly Action<CssStyleBuilder> buildStyles;

    private bool dirty = true;

    private List<string> styleList = new();

    private string? styles;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new CSS style styleList.
    /// </summary>
    /// <param name="buildStyles">The action to be called to build the styles.</param>
    public CssStyleBuilder(Action<CssStyleBuilder> buildStyles)
    {
        this.buildStyles = buildStyles;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Appends a string to the style.
    /// </summary>
    /// <param name="value">The string to append.</param>
    public void Append(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            styleList.Add(value);
    }

    /// <summary>
    /// Appends a string to the style if the specified condition is true.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <param name="condition">The condition to check.</param>
    public void Append(string value, bool condition)
    {
        if (condition && !string.IsNullOrWhiteSpace(value))
            styleList.Add(value);
    }

    /// <summary>
    /// Appends a list of strings to the styles.
    /// </summary>
    /// <param name="values">The list of strings to append.</param>
    public void Append(IEnumerable<string> values)
    {
        if (values is not null && values.Any())
            styleList.AddRange(values);
    }

    /// <summary>
    /// Marks the styleList as dirty, so that the styles will be rebuilt the next time they are requested.
    /// </summary>
    public void Dirty() => dirty = true;

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the styles.
    /// </summary>
    /// <remarks>
    /// The styles are lazily built, so the first time this property is accessed, the `buildStyles` action will be called.
    /// </remarks>
    public string? Styles
    {
        get
        {
            if (dirty)
            {
                styleList = new List<string>();

                buildStyles(this);

                styles = styleList.Any() ? string.Join(";", styleList) : null;

                dirty = false;
            }

            return styles;
        }
    }

    #endregion
}