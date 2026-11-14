using System.Globalization;
using System.Text;
using System.Text.Json;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// Keeps each site's snapshot in the browser tab's <c>sessionStorage</c>, so a site survives a reload in that tab and is
/// gone when the tab closes. Nothing is written to <c>localStorage</c> or sent anywhere.
/// <para>
/// A snapshot is stored in chunks of at most <see cref="ChunkLength" /> characters under <c>bb-showcase:&lt;slug&gt;:&lt;n&gt;</c>,
/// with the chunk count under <c>bb-showcase:&lt;slug&gt;</c>. On the Server host every value read back through JS interop
/// travels to the server as one SignalR message, which the default hub options cap at 32 KB; small chunks keep each read
/// far below that cap without changing the host.
/// </para>
/// <para>
/// Reading back is defensive: a snapshot with too many chunks, a missing chunk, another format version, or any invalid
/// part is removed and the site keeps its seed state. Storage that is unavailable (private mode, quota, a closed
/// circuit) leaves the site working in memory.
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

    private readonly Dictionary<string, int> storedChunks = new(StringComparer.Ordinal);

    #endregion

    #region Constructors and Finalizers

    /// <summary>Creates the store for the current circuit or app.</summary>
    public ShowcaseSessionStore(IJSRuntime js) => this.js = js;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the storage key that holds a site's chunk count.</summary>
    public static string KeyFor(string siteSlug) => KeyPrefix + siteSlug;

    /// <summary>Removes a site's snapshot; the next load starts from seed data.</summary>
    public async Task ClearAsync(string siteSlug)
    {
        var count = storedChunks.TryGetValue(siteSlug, out var known) ? known : maxChunks;

        for (var index = 0; index < count; index++)
            await TryRemoveAsync(ChunkKey(siteSlug, index));

        await TryRemoveAsync(KeyFor(siteSlug));
        storedChunks[siteSlug] = 0;
    }

    /// <summary>Reads the site's snapshot back and applies it when every part is valid.</summary>
    /// <returns><see langword="true" /> when a snapshot was applied.</returns>
    public async Task<bool> RestoreAsync(IShowcaseSiteState state)
    {
        var slug = state.SiteSlug;
        var countText = await TryGetAsync(KeyFor(slug));

        if (countText is null)
        {
            storedChunks[slug] = 0;
            return false;
        }

        if (countText.Length > 4 || !int.TryParse(countText, NumberStyles.None, CultureInfo.InvariantCulture, out var count) || count is < 1 || count > maxChunks)
        {
            await ClearAsync(slug);
            return false;
        }

        storedChunks[slug] = count;
        var json = new StringBuilder();

        for (var index = 0; index < count; index++)
        {
            var chunk = await TryGetAsync(ChunkKey(slug, index));

            if (chunk is null || chunk.Length > ChunkLength)
            {
                await ClearAsync(slug);
                return false;
            }

            json.Append(chunk);
        }

        if (TryApply(state, json.ToString()))
            return true;

        await ClearAsync(slug);

        return false;
    }

    /// <summary>Writes the site's snapshot; a snapshot over the size limit, or a failed write, is skipped.</summary>
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
        var count = (json.Length + ChunkLength - 1) / ChunkLength;

        // Chunks first, then the count, so a reload in between never reads a count ahead of its chunks.
        for (var index = 0; index < count; index++)
        {
            if (!await TrySetAsync(ChunkKey(slug, index), json.Substring(index * ChunkLength, Math.Min(ChunkLength, json.Length - (index * ChunkLength)))))
                return;
        }

        if (!await TrySetAsync(KeyFor(slug), count.ToString(CultureInfo.InvariantCulture)))
            return;

        var previous = storedChunks.TryGetValue(slug, out var known) ? known : 0;

        for (var index = count; index < previous; index++)
            await TryRemoveAsync(ChunkKey(slug, index));

        storedChunks[slug] = count;
    }

    #endregion

    #region Private Methods

    private static string ChunkKey(string siteSlug, int index) => $"{KeyPrefix}{siteSlug}:{index.ToString(CultureInfo.InvariantCulture)}";

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

    private async Task<string?> TryGetAsync(string key)
    {
        try
        {
            return await js.InvokeAsync<string?>("sessionStorage.getItem", key);
        }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            return null;
        }
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
            // Quota or unavailable storage: the site keeps working in memory.
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
