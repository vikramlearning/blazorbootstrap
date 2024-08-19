namespace BlazorBootstrap;

public class PinElement
{
    #region Properties, Indexers

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public string? Background { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public string? BorderColor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public object? Glyph { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public string? GlyphColor { get; set; }

    public double Scale { get; set; } = 1.0;

    public bool UseIconFonts { get; set; }

    #endregion
}
