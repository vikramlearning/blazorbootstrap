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

    private readonly Action<CssClassBuilder> buildClasses;

    private List<string> classList = new();

    private string? classNames;

    private bool dirty = true;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new CSS class classList.
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
            classList.Add(value);
    }

    /// <summary>
    /// Appends a string to the class name if the specified condition is true.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <param name="condition">The condition to check.</param>
    public void Append(string value, bool condition)
    {
        if (condition && !string.IsNullOrWhiteSpace(value))
            classList.Add(value);
    }

    /// <summary>
    /// Appends a list of strings to the class name.
    /// </summary>
    /// <param name="values">The list of strings to append.</param>
    public void Append(IEnumerable<string> values)
    {
        if (values is not null && values.Any())
            classList.AddRange(values);
    }

    /// <summary>
    /// Marks the classList as dirty, so that the class names will be rebuilt the next time they are requested.
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
    public string? ClassNames
    {
        get
        {
            if (dirty)
            {
                classList = new List<string>();

                buildClasses(this);

                classNames = classList.Any() ? string.Join(" ", classList) : null;

                dirty = false;
            }

            return classNames;
        }
    }

    #endregion
}