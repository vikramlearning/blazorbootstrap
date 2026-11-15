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
/// Keeps each site's snapshot in the browser tab's <c>sessionStorage</c> under <c>bb-showcase:&lt;slug&gt;</c>, so a site
/// survives a reload in that tab and is gone when the tab closes. Nothing is written to <c>localStorage</c> or sent anywhere.
/// <para>
/// The calls go through <c>window.showcaseStore</c> in the demo RCL's script. A save is one atomic <c>setItem</c>. A read asks
/// for the length first and then fetches slices of at most <see cref="SliceLength" /> characters: on the Server host every
/// value returned to .NET travels as one SignalR message, which the default hub options cap at 32 KB. Because the length
/// comes first, an oversized value is removed without ever being sent.
/// </para>
/// <para>
/// Reading back is defensive: an oversized value, another format version, or any invalid part removes the snapshot and the
/// site keeps its seed state. A read that fails (storage unavailable, a dropped circuit) is reported as
/// <see cref="ShowcaseRestoreResult.Unavailable" /> and never clears anything.
/// </para>
/// </summary>
public sealed class ShowcaseSessionStore
{
    #region Fields and Constants

    /// <summary>The largest snapshot, in characters, a site may store or read back.</summary>
    public const int MaxSnapshotLength = 512 * 1024;

    /// <summary>The most characters read back in one call (worst-case JSON escaping stays under the 32 KB hub limit).</summary>
    public const int SliceLength = 4000;

    private const string KeyPrefix = "bb-showcase:";

    private static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web) { MaxDepth = 32 };

    private readonly IJSRuntime js;

    private readonly HashSet<string> restored = new(StringComparer.Ordinal);

    #endregion

    #region Constructors and Finalizers

    /// <summary>Creates the store for the current circuit or app.</summary>
    public ShowcaseSessionStore(IJSRuntime js) => this.js = js;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the storage key of a site.</summary>
    public static string KeyFor(string siteSlug) => KeyPrefix + siteSlug;

    /// <summary>Removes a site's snapshot; the next load starts from seed data.</summary>
    public async Task ClearAsync(string siteSlug)
    {
        await TryInvokeVoidAsync("showcaseStore.remove", KeyFor(siteSlug));
        restored.Remove(siteSlug);
    }

    /// <summary>Reads the site's snapshot back and applies it when every part is valid.</summary>
    public async Task<ShowcaseRestoreResult> RestoreAsync(IShowcaseSiteState state)
    {
        var slug = state.SiteSlug;
        var key = KeyFor(slug);

        // Re-entering the site later in the same circuit or app: the scoped state in memory is newer than storage.
        if (restored.Contains(slug))
            return ShowcaseRestoreResult.AlreadyInMemory;

        var (lengthRead, length) = await TryInvokeAsync<int>("showcaseStore.length", key);

        // -2: the script reached storage but the browser blocked it.
        if (!lengthRead || length == -2)
            return ShowcaseRestoreResult.Unavailable;

        if (length < 0)
        {
            restored.Add(slug);
            return ShowcaseRestoreResult.None;
        }

        if (length == 0 || length > MaxSnapshotLength)
            return await DiscardAsync(slug);

        var json = new StringBuilder(length);

        for (var start = 0; start < length; start += SliceLength)
        {
            var (sliceRead, slice) = await TryInvokeAsync<string?>("showcaseStore.slice", key, start, SliceLength);

            if (!sliceRead)
                return ShowcaseRestoreResult.Unavailable;

            // The value changed or vanished while it was being read.
            if (slice is null || slice.Length == 0 || slice.Length > SliceLength)
                return await DiscardAsync(slug);

            json.Append(slice);
        }

        if (json.Length != length || !TryApply(state, json.ToString()))
            return await DiscardAsync(slug);

        restored.Add(slug);

        return ShowcaseRestoreResult.Restored;
    }

    /// <summary>Writes the site's snapshot in one call; a snapshot over the size limit, or a refused write, leaves the previous one.</summary>
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

        var (written, accepted) = await TryInvokeAsync<bool>("showcaseStore.set", KeyFor(state.SiteSlug), json);

        if (written && accepted)
            restored.Add(state.SiteSlug);
    }

    #endregion

    #region Private Methods

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

    /// <summary>Calls a <c>showcaseStore</c> function. <c>Ok</c> is <see langword="false" /> when storage couldn't be reached.</summary>
    private async Task<(bool Ok, T? Value)> TryInvokeAsync<T>(string identifier, params object?[] args)
    {
        try
        {
            return (true, await js.InvokeAsync<T>(identifier, args));
        }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            return (false, default);
        }
    }

    private async Task TryInvokeVoidAsync(string identifier, params object?[] args)
    {
        try
        {
            await js.InvokeVoidAsync(identifier, args);
        }
        catch (Exception ex) when (IsUnavailable(ex))
        {
            // Storage is optional: the site keeps working in memory.
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
