namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// The session of one site, cascaded by <c>ShowcaseSessionHost</c> to everything in the site's layout: the site state,
/// whether its stored snapshot has been restored yet, and the "Reset demo data" dialog the host owns. Guards wait for
/// <see cref="IsRestored" /> before deciding.
/// </summary>
public sealed class ShowcaseSession
{
    #region Constructors and Finalizers

    /// <summary>Creates the session of a site.</summary>
    public ShowcaseSession(IShowcaseSiteState state) => State = state;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Opens the host's "Reset demo data" confirmation. The dialog lives in the host, outside any sticky header, so it
    /// stacks above the page's backdrop.
    /// </summary>
    /// <param name="onCancelled">Runs when the dialog closes without a reset, for example to return focus to the opener.</param>
    public Task RequestResetAsync(Func<Task>? onCancelled = null) => ResetRequested?.Invoke(onCancelled) ?? Task.CompletedTask;

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity => State.Identity;

    /// <summary>Gets whether the session is being reset; saves are skipped so the cleared snapshot stays cleared.</summary>
    public bool IsResetting { get; internal set; }

    /// <summary>Gets whether the stored snapshot was read back (or there was none, or storage can't be read). Before that, screens show seed data.</summary>
    public bool IsRestored { get; internal set; }

    /// <summary>Gets whether the tab's storage could be read; when it can't, the site works in memory and nothing is saved.</summary>
    public bool IsStorageAvailable { get; internal set; } = true;

    /// <summary>Gets or sets the host's handler that opens the reset dialog.</summary>
    internal Func<Func<Task>?, Task>? ResetRequested { get; set; }

    /// <summary>Gets the site state.</summary>
    public IShowcaseSiteState State { get; }

    #endregion
}
