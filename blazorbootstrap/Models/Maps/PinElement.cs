namespace BlazorBootstrap;

public class PinElement
{
    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the background color of the pin.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the background color of the pin.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Background { get; set; }

    /// <summary>
    /// Gets or sets the border color of the pin.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the border color of the pin.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BorderColor { get; set; }

    /// <summary>
    /// Gets or sets the legacy glyph content for the pin.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the legacy glyph content for the pin.")]
    [Obsolete("Use GlyphText or GlyphSrc instead.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Glyph { get; set; }

    /// <summary>
    /// Gets or sets the color of the pin glyph.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the color of the pin glyph.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GlyphColor { get; set; }

    /// <summary>
    /// Gets or sets the URL of the image displayed in the pin glyph.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the URL of the image displayed in the pin glyph.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GlyphSrc { get; set; }

    /// <summary>
    /// Gets or sets the text displayed in the pin glyph.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.0.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the text displayed in the pin glyph.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GlyphText { get; set; }

    /// <summary>
    /// Gets or sets the scale of the pin.
    /// <para>
    /// Default value is 1.0.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(1.0)]
    [Description("Gets or sets the scale of the pin.")]
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets a value indicating whether the legacy glyph is an icon-font class.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("3.0.0")]
    [DefaultValue(false)]
    [Description("Gets or sets a value indicating whether the legacy glyph is an icon-font class.")]
    public bool UseIconFonts { get; set; }

    #endregion
}
