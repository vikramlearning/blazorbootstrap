namespace BlazorBootstrap;

/// <summary>
/// Implemented by a <see cref="ListGroup{TItem}" /> in tab mode, so its <see cref="ListGroupTabPane" /> children can register without knowing <c>TItem</c>.
/// </summary>
internal interface IListGroupTabHost
{
    #region Methods

    /// <summary>
    /// Registers a pane, so the group renders a tab for it.
    /// </summary>
    void AddPane(ListGroupTabPane pane);

    /// <summary>
    /// Returns whether the pane is the one shown when the group first renders.
    /// </summary>
    bool IsInitiallyActive(ListGroupTabPane pane);

    /// <summary>
    /// Unregisters a pane that is being removed.
    /// </summary>
    void RemovePane(ListGroupTabPane pane);

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets whether panes fade in and out.
    /// </summary>
    bool Fade { get; }

    #endregion
}
