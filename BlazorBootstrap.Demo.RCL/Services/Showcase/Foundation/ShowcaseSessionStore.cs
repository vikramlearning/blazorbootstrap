using System.Globalization;
using System.Text;
using System.Text.Json;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>The outcome of reading a site's snapshot back from the tab.</summary>
public enum ShowcaseRestoreResult
{
    /// <summary>Nothing was stored; the site starts from seed data.</summary>
    None,

    /// <summary>The stored snapshot was valid and applied.</summary>
    Restored,

    /// <summary>The stored snapshot was invalid, removed, and the site starts from seed data.</summary>
    Discarded,

    /// <summary>The site was already restored in this circuit or app; the in-memory state is newer, so nothing was read.</summary>
    AlreadyInMemory,

    /// <summary>Storage could not be read (private mode, a dropped connection); nothing was changed, and saving should stay off.</summary>
    Unavailable,
}

/// <summary>
/// Keeps each site's snapshot in the browser tab's <c>sessionStorage</c>, so a site survives a reload in that tab and is
/// gone when the tab closes. Nothing is written to <c>localStorage</c> or sent anywhere.
/// <para>
/// A snapshot is stored in chunks of at most <see cref="ChunkLength" /> characters: on the Server host every value read
/// back through JS interop travels to the server as one SignalR message, which the default hub options cap at 32 KB.
/// Chunks alternate between two key sets (<c>bb-showcase:&lt;slug&gt;:a:&lt;n&gt;</c> and <c>…:b:&lt;n&gt;</c>), and the
/// pointer key <c>bb-showcase:&lt;slug&gt;</c> (set letter plus chunk count, such as <c>b3</c>) is written last. A save that
/// is interrupted (a reload, a quota error) therefore leaves the previous snapshot intact.
/// </para>
/// <para>
/// Reading back is defensive: a bad pointer, a missing or oversized chunk, another format version, or any invalid part
/// removes the snapshot and the site keeps its seed state. A read that fails (storage unavailable, a dropped circuit) is
/// reported as <see cref="ShowcaseRestoreResult.Unavailable" /> and never clears anything.
/// </para>
/// </summary>
public sealed class ShowcaseSessionStore
{
    #region Fields and Constants

    /// <summary>The most characters one stored chunk holds (worst-case JSON escaping stays under the 32 KB hub limit).</summary>
    public const int ChunkLength = 4000;

    /// <summary>The largest snapshot, in characters, a site may store or read back.</summary>
    public const int MaxSnapshotLength = 512 * 1024;

    private const string KeyPrefix = "bb-showcase:";

    private static readonly int maxChunks = MaxSnapshotLength / ChunkLength;

    private static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly IJSRuntime js;

    private readonly HashSet<string> restored = new(StringComparer.Ordinal);

    private readonly Dictionary<string, (char Set, int Count)> stored = new(StringComparer.Ordinal);

    #endregion

    #region Constructors and Finalizers

    /// <summary>Creates the store for the current circuit or app.</summary>
    public ShowcaseSessionStore(IJSRuntime js) => this.js = js;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the storage key that points at a site's current chunk set.</summary>
    public static string KeyFor(string siteSlug) => KeyPrefix + siteSlug;

    /// <summary>Removes a site's snapshot (pointer first, then both chunk sets); the next load starts from seed data.</summary>
    public async Task ClearAsync(string siteSlug)
    {
        await TryRemoveAsync(KeyFor(siteSlug));

        // Without a known pointer, both sets are cleared to their limit, which also removes chunks orphaned by a failed save.
        var known = stored.TryGetValue(siteSlug, out var pointer) ? pointer : ((char Set, int Count)?)null;

        foreach (var set in new[] { 'a', 'b' })
        {
            var count = known is { } current && current.Set == set ? current.Count : maxChunks;

            for (var index = 0; index < count; index++)
                await TryRemoveAsync(ChunkKey(siteSlug, set, index));
        }

        stored.Remove(siteSlug);
    }

    /// <summary>Reads the site's snapshot back and applies it when every part is valid.</summary>
    public async Task<ShowcaseRestoreResult> RestoreAsync(IShowcaseSiteState state)
    {
        var slug = state.SiteSlug;

        // Re-entering the site later in the same circuit or app: the scoped state in memory is newer than storage.
        if (restored.Contains(slug))
            return ShowcaseRestoreResult.AlreadyInMemory;

        var (pointerRead, pointerText) = await TryGetAsync(KeyFor(slug));

        if (!pointerRead)
            return ShowcaseRestoreResult.Unavailable;

        if (pointerText is null)
        {
            restored.Add(slug);
            return ShowcaseRestoreResult.None;
        }

        if (!TryParsePointer(pointerText, out var set, out var count))
            return await DiscardAsync(slug);

        stored[slug] = (set, count);
        var json = new StringBuilder();

        for (var index = 0; index < count; index++)
        {
            var (chunkRead, chunk) = await TryGetAsync(ChunkKey(slug, set, index));

            if (!chunkRead)
                return ShowcaseRestoreResult.Unavailable;

            if (chunk is null || chunk.Length > ChunkLength)
                return await DiscardAsync(slug);

            json.Append(chunk);
        }

        if (!TryApply(state, json.ToString()))
            return await DiscardAsync(slug);

        restored.Add(slug);

        return ShowcaseRestoreResult.Restored;
    }

    /// <summary>
    /// Writes the site's snapshot into the set not in use, then points at it, then removes the old set. A snapshot over the
    /// size limit, or a failed write, leaves the previous snapshot as it was.
    /// </summary>
    public async Task SaveAsync(IShowcaseSiteState state)
    {
        string json;

        try
        {
            var data = state.CaptureData();
            var envelope = new Envelope
            {
                V = state.SnapshotVersion,
                Elapsed = Math.Round(state.Clock.Elapsed.TotalSeconds, 1),
                Identity = state.Identity.Capture(),
                Data = JsonSerializer.SerializeToElement(data, data.GetType(), options),
            };

            json = JsonSerializer.Serialize(envelope, options);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            return;
        }

        if (json.Length > MaxSnapshotLength)
            return;

        var slug = state.SiteSlug;
        var previous = stored.TryGetValue(slug, out var pointer) ? pointer : ('b', 0);
        var set = previous.Item1 == 'a' ? 'b' : 'a';
        var count = (json.Length + ChunkLength - 1) / ChunkLength;

        for (var index = 0; index < count; index++)
        {
            var start = index * ChunkLength;

            if (!await TrySetAsync(ChunkKey(slug, set, index), json.Substring(start, Math.Min(ChunkLength, json.Length - start))))
                return;
        }

        if (!await TrySetAsync(KeyFor(slug), set + count.ToString(CultureInfo.InvariantCulture)))
            return;

        stored[slug] = (set, count);
        restored.Add(slug);

        for (var index = 0; index < previous.Item2; index++)
            await TryRemoveAsync(ChunkKey(slug, previous.Item1, index));
    }

    #endregion

    #region Private Methods

    private static string ChunkKey(string siteSlug, char set, int index) => $"{KeyPrefix}{siteSlug}:{set}:{index.ToString(CultureInfo.InvariantCulture)}";

    private async Task<ShowcaseRestoreResult> DiscardAsync(string siteSlug)
    {
        await ClearAsync(siteSlug);
        restored.Add(siteSlug);

        return ShowcaseRestoreResult.Discarded;
    }

    private static bool IsUnavailable(Exception ex) => ex is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException;

    private static bool TryApply(IShowcaseSiteState state, string json)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<Envelope>(json, options);

            if (envelope is null
                || envelope.V != state.SnapshotVersion
                || envelope.Data.ValueKind != JsonValueKind.Object
                || !state.Identity.TryValidate(envelope.Identity, out var identity))
                return false;

            // The clock value is checked before anything is applied, so a bad value can't leave the state half restored.
            if (!new ShowcaseClock(state.Clock.Start).TryRestore(envelope.Elapsed) || !state.TryRestoreData(envelope.Data))
                return false;

            state.Identity.Apply(identity);
            state.Clock.TryRestore(envelope.Elapsed);

            return true;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException or FormatException or ArgumentException or OverflowException)
        {
            return false;
        }
    }

    /// <summary>Reads a key. <c>Read</c> is <see langword="false" /> when storage couldn't be reached, which is not the same as a missing key.</summary>
    private async Task<(bool Read, string? Value)> TryGetAsync(string key)
    {
        try
        {
            return (true, await js.InvokeAsync<string?>("sessionStorage.getItem", key));
        }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            return (false, null);
        }
    }

    private static bool TryParsePointer(string text, out char set, out int count)
    {
        set = text.Length > 0 ? text[0] : '\0';
        count = 0;

        return text.Length is >= 2 and <= 4
            && set is 'a' or 'b'
            && int.TryParse(text.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out count)
            && count >= 1 && count <= maxChunks;
    }

    private async Task TryRemoveAsync(string key)
    {
        try
        {
            await js.InvokeVoidAsync("sessionStorage.removeItem", key);
        }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            // Storage is optional: the site keeps working in memory.
        }
    }

    private async Task<bool> TrySetAsync(string key, string value)
    {
        try
        {
            await js.InvokeVoidAsync("sessionStorage.setItem", key, value);

            return true;
        }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            // Quota or unavailable storage: the previous snapshot stays as it was.
            return false;
        }
    }

    #endregion

    #endregion

    #region Nested Types

    /// <summary>The stored snapshot: format version, clock, identity, and the site's own data.</summary>
    private sealed class Envelope
    {
        #region Properties, Indexers

        public JsonElement Data { get; set; }

        public double Elapsed { get; set; }

        public ShowcaseIdentitySnapshot? Identity { get; set; }

        public int V { get; set; }

        #endregion
    }

    #endregion
}
