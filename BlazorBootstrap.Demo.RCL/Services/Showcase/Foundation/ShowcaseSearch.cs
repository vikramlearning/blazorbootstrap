namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// How sites rank a global search match.
/// </summary>
public static class ShowcaseSearch
{
    #region Methods

    /// <summary>
    /// Ranks how well <paramref name="term" /> matches the best of <paramref name="values" />: 0 when a value equals it,
    /// 1 when a value starts with it, 2 when a value contains it (ignoring case); <see langword="null" /> when none matches.
    /// </summary>
    public static int? Rank(string term, params string?[] values) =>
        values.Select(value => value is null ? (int?)null
                                 : string.Equals(value, term, StringComparison.OrdinalIgnoreCase) ? 0
                                 : value.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 1
                                 : value.Contains(term, StringComparison.OrdinalIgnoreCase) ? 2
                                 : null)
              .Where(rank => rank is not null)
              .DefaultIfEmpty(null)
              .Min();

    #endregion
}
