namespace BlazorBootstrap;

/// <summary>
/// Describes the nodes a <see cref="MindMap{TItem}" /> opened and closed in one change.
/// </summary>
/// <typeparam name="TItem">The type of the items in the mind map.</typeparam>
public sealed class MindMapExpandedChangedEventArgs<TItem>
{
    #region Constructors and Finalizers

    /// <summary>
    /// Initializes a new instance of the <see cref="MindMapExpandedChangedEventArgs{TItem}" /> class.
    /// </summary>
    /// <param name="opened">The items whose nodes opened.</param>
    /// <param name="closed">The items whose nodes closed.</param>
    public MindMapExpandedChangedEventArgs(IReadOnlyList<TItem> opened, IReadOnlyList<TItem> closed)
    {
        Opened = opened;
        Closed = closed;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the items whose nodes closed.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the items whose nodes closed.")]
    public IReadOnlyList<TItem> Closed { get; }

    /// <summary>
    /// Gets the items whose nodes opened.
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets the items whose nodes opened.")]
    public IReadOnlyList<TItem> Opened { get; }

    #endregion
}
