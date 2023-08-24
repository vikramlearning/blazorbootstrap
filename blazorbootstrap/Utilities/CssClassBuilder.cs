namespace BlazorBootstrap;

/// <summary>
/// A helper class for building CSS class names.
/// </summary>
/// <remarks>
/// This class can be used to append strings, lists of strings, and conditions to a CSS class name.
/// </remarks>
public class CssClassBuilder
{
    #region Fields and Constants

    /// <summary>
    /// The delimiter used to separate the class names.
    /// </summary>
    private const char Delimiter = ' ';

    private readonly Action<CssClassBuilder> buildClasses;

    private StringBuilder builder = new();

    private string classNames;

    private bool dirty = true;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new CSS class builder.
    /// </summary>
    /// <param name="buildClasses">The action to be called to build the class names.</param>
    public CssClassBuilder(Action<CssClassBuilder> buildClasses)
    {
        this.buildClasses = buildClasses;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Appends a string to the class name.
    /// </summary>
    /// <param name="value">The string to append.</param>
    public void Append(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            builder.Append(value).Append(Delimiter);
    }

    /// <summary>
    /// Appends a string to the class name if the specified condition is true.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <param name="condition">The condition to check.</param>
    public void Append(string value, bool condition)
    {
        if (condition && !string.IsNullOrWhiteSpace(value))
            builder.Append(value).Append(Delimiter);
    }

    /// <summary>
    /// Appends a list of strings to the class name.
    /// </summary>
    /// <param name="values">The list of strings to append.</param>
    public void Append(IEnumerable<string> values) => builder.Append(string.Join(Delimiter, values)).Append(Delimiter);

    /// <summary>
    /// Marks the builder as dirty, so that the class names will be rebuilt the next time they are requested.
    /// </summary>
    public void Dirty() => dirty = true;

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the class names.
    /// </summary>
    /// <remarks>
    /// The class names are lazily built, so the first time this property is accessed, the `buildClasses` action will be
    /// called.
    /// </remarks>
    public string Class
    {
        get
        {
            if (dirty)
            {
                builder = new StringBuilder();

                buildClasses(this);

                classNames = builder.ToString().TrimEnd()?.EmptyToNull();

                dirty = false;
            }

            return classNames;
        }
    }

    #endregion
}