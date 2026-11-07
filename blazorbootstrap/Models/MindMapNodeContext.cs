namespace BlazorBootstrap;

/// <summary>
/// The context passed to the <see cref="MindMap{TItem}.NodeTemplate" /> of a <see cref="MindMap{TItem}" /> node.
/// </summary>
/// <typeparam name="TItem">The type of the items in the mind map.</typeparam>
public sealed class MindMapNodeContext<TItem>
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="MindMapNodeContext{TItem}" /> class.
    /// </summary>
    /// <param name="item">The item the node represents.</param>
    /// <param name="isMatch">Whether the node's label matches the current search text.</param>
    /// <param name="searchText">The current search text, or <see langword="null" /> when nothing is searched.</param>
    public MindMapNodeContext(TItem item, bool isMatch, string? searchText)
    {
        Item = item;
        IsMatch = isMatch;
        SearchText = searchText;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets whether the node's label matches the current search text.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets whether the node's label matches the current search text.")]
    public bool IsMatch { get; }

    /// <summary>
    /// Gets the item the node represents.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the item the node represents.")]
    public TItem Item { get; }

    /// <summary>
    /// Gets the current search text, or <see langword="null" /> when nothing is searched.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the current search text, or <b>null</b> when nothing is searched.")]
    public string? SearchText { get; }

    #endregion
}
