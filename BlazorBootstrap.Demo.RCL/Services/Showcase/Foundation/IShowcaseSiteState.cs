using System.Text.Json;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// The contract between a site's state service and the shared showcase foundation: its clock, its demo sign-in, and how
/// its data is captured into and restored from the tab's snapshot.
/// </summary>
public interface IShowcaseSiteState
{
    #region Methods

    /// <summary>Returns the site's own data as a serializable object (a DTO, not live state).</summary>
    object CaptureData();

    /// <summary>Raises <see cref="Changed" /> after a snapshot was restored, so screens redraw with it.</summary>
    void NotifyRestored();

    /// <summary>
    /// Validates the site's data read back from storage and applies it only when every part is valid: known IDs, defined
    /// enum values, quantities and amounts in range, bounded text. Must not throw on bad input.
    /// </summary>
    /// <returns><see langword="false" /> to discard the snapshot and keep the seed state.</returns>
    bool TryRestoreData(JsonElement data);

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the site's clock.</summary>
    ShowcaseClock Clock { get; }

    /// <summary>Gets the site's demo sign-in.</summary>
    ShowcaseIdentity Identity { get; }

    /// <summary>Gets the site's slug, used in the storage key.</summary>
    string SiteSlug { get; }

    /// <summary>Gets the snapshot format version; a stored snapshot with another version is discarded.</summary>
    int SnapshotVersion { get; }

    #endregion

    #region Events

    /// <summary>Raised when anything in the site's state changes, including sign-in.</summary>
    event Action? Changed;

    #endregion
}
