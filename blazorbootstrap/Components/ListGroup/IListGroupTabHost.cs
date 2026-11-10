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
    /// Returns whether the pane renders as active: only the pane shown at first render (Bootstrap switches tabs in the page after that).
    /// </summary>
    bool IsActive(ListGroupTabPane pane);

    /// <summary>
    /// Unregisters a pane that is being removed.
    /// </summary>
    void RemovePane(ListGroupTabPane pane);

    /// <summary>
    /// Puts Bootstrap's tab state back after a pane re-rendered its class on its own (for example a changed <c>Class</c>).
    /// </summary>
    Task SyncTabsAsync();

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets whether panes fade in and out.
    /// </summary>
    bool Fade { get; }

    #endregion
}
