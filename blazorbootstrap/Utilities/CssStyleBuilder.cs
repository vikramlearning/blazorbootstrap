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

    /// <summary>
    /// The delimiter used to separate the styles.
    /// </summary>
    private const char Delimiter = ';';

    private readonly Action<CssStyleBuilder> buildStyles;

    private StringBuilder builder = new();

    private bool dirty = true;

    private string styles;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new CSS style builder.
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
            builder.Append(value).Append(Delimiter);
    }

    /// <summary>
    /// Appends a string to the style if the specified condition is true.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <param name="condition">The condition to check.</param>
    public void Append(string value, bool condition)
    {
        if (condition && !string.IsNullOrWhiteSpace(value))
            builder.Append(value).Append(Delimiter);
    }

    /// <summary>
    /// Marks the builder as dirty, so that the styles will be rebuilt the next time they are requested.
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
    public string Styles
    {
        get
        {
            if (dirty)
            {
                builder = new StringBuilder();

                buildStyles(this);

                styles = builder.ToString().TrimEnd(' ', Delimiter)?.EmptyToNull();

                dirty = false;
            }

            return styles;
        }
    }

    #endregion
}