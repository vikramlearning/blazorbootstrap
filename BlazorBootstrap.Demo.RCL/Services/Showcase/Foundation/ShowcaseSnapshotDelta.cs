using System.Text.Json;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// Helpers for a site snapshot that stores only what differs from a fresh tab: the records changed or added, and the keys
/// of seeded records removed. Records are compared by their JSON, so any change to a record is caught without per-type code.
/// </summary>
public static class ShowcaseSnapshotDelta
{
    #region Fields and Constants

    /// <summary>The most changed records or removed keys one list may store.</summary>
    public const int MaxEntries = 20_000;

    #endregion

    #region Methods

    /// <summary>
    /// Returns the records of <paramref name="current" /> that differ from the seed (changed or added, in list order) and the keys
    /// of seeded records no longer there; <see langword="null" /> for an empty list, so it is left out of the snapshot.
    /// </summary>
    public static (List<T>? Changed, List<TKey>? Removed) Diff<T, TKey>(IEnumerable<T> current, IReadOnlyDictionary<TKey, string> seeded, Func<T, TKey> key, JsonSerializerOptions options)
        where TKey : notnull
    {
        var currentKeys = new HashSet<TKey>();
        var changed = new List<T>();

        foreach (var item in current)
        {
            var itemKey = key(item);
            currentKeys.Add(itemKey);

            if (!seeded.TryGetValue(itemKey, out var original) || original != JsonSerializer.Serialize(item, options))
                changed.Add(item);
        }

        var removed = seeded.Keys.Where(seededKey => !currentKeys.Contains(seededKey)).ToList();

        return (changed.Count == 0 ? null : changed, removed.Count == 0 ? null : removed);
    }

    /// <summary>
    /// Indexes a fresh tab's records by key, with each record's JSON, for <see cref="Diff{T, TKey}" />.
    /// </summary>
    public static Dictionary<TKey, string> Index<T, TKey>(IEnumerable<T> seeded, Func<T, TKey> key, JsonSerializerOptions options)
        where TKey : notnull =>
        seeded.ToDictionary(key, item => JsonSerializer.Serialize(item, options));

    /// <summary>
    /// Rebuilds a list from a fresh tab's records and a snapshot's changes: the seeded records in their order (replaced by a
    /// stored record with the same key, left out when removed), then the added records in their stored order. Returns
    /// <see langword="null" /> for an inconsistent change list (a missing record, a repeated key, too many entries, or a
    /// removed key that isn't seeded or is also changed), which discards the snapshot.
    /// </summary>
    public static List<T>? Overlay<T, TKey>(IReadOnlyList<T> seeded, List<T>? changed, List<TKey>? removed, Func<T, TKey> key)
        where TKey : notnull
    {
        changed ??= new List<T>();
        removed ??= new List<TKey>();

        if (changed.Count > MaxEntries || removed.Count > MaxEntries || changed.Any(item => item is null) || removed.Any(item => item is null))
            return null;

        var replacements = new Dictionary<TKey, T>();

        foreach (var item in changed)
            if (key(item) is not { } itemKey || !replacements.TryAdd(itemKey, item))
                return null;

        var seededKeys = seeded.Select(key).ToHashSet();
        var removedKeys = removed.ToHashSet();

        if (removedKeys.Count != removed.Count || removedKeys.Any(item => !seededKeys.Contains(item) || replacements.ContainsKey(item)))
            return null;

        var result = new List<T>(seeded.Count + changed.Count);

        foreach (var item in seeded)
        {
            var itemKey = key(item);

            if (replacements.Remove(itemKey, out var replacement))
                result.Add(replacement);
            else if (!removedKeys.Contains(itemKey))
                result.Add(item);
        }

        // What is left was added after the seed, in the order it was stored.
        result.AddRange(changed.Where(item => replacements.ContainsKey(key(item))));

        return result;
    }

    #endregion
}
