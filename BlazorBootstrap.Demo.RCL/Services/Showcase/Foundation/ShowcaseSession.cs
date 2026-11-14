namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// The session of one site, cascaded by <c>ShowcaseSessionHost</c> to everything in the site's layout: the site state and
/// whether its stored snapshot has been restored yet. Guards wait for <see cref="IsRestored" /> before deciding.
/// </summary>
public sealed class ShowcaseSession
{
    #region Constructors and Finalizers

    /// <summary>Creates the session of a site.</summary>
    public ShowcaseSession(IShowcaseSiteState state) => State = state;

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the site's demo sign-in.</summary>
    public ShowcaseIdentity Identity => State.Identity;

    /// <summary>Gets whether the session is being reset; saves are skipped so the cleared snapshot stays cleared.</summary>
    public bool IsResetting { get; internal set; }

    /// <summary>Gets whether the stored snapshot was read back (or there was none). Before that, screens show seed data.</summary>
    public bool IsRestored { get; internal set; }

    /// <summary>Gets the site state.</summary>
    public IShowcaseSiteState State { get; }

    #endregion
}
