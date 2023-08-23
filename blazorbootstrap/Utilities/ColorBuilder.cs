namespace BlazorBootstrap.Utilities;

public static class ColorBuilder
{
    #region Properties, Indexers

    /// <summary>
    /// Returns 6 categorical #RRGGBB colors.
    /// <seealso cref="https://spectrum.adobe.com/page/color-for-data-visualization/" />
    /// </summary>
    public static string[] CategoricalSixColors => new[] { "#0fb5ae", "#4046ca", "#f68511", "#de3d82", "#7e84fa", "#72e06a", "#147af3", "#7326d3", "#e8c600", "#e8c600", "#e8c600", "#e8c600" };

    /// <summary>
    /// Returns 12 categorical #RRGGBB colors.
    /// <seealso cref="https://spectrum.adobe.com/page/color-for-data-visualization/" />
    /// </summary>
    public static string[] CategoricalTwelveColors => new[] { "#0fb5ae", "#4046ca", "#f68511", "#de3d82", "#7e84fa", "#72e06a", "#147af3", "#7326d3", "#e8c600", "#cb5d00", "#008f5d", "#bce931" };

    #endregion
}