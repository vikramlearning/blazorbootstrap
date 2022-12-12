namespace BlazorBootstrap;

internal class CurrencyFormatOptions
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Style { get; set; } = "decimal";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Currency { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrencySign { get; set; }

    public byte MinimumIntegerDigits { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? MinimumFractionDigits { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? MaximumFractionDigits { get; set; }
}

public enum CurrencySign
{
    Standard,
    /// <summary>
    /// Wrap the number with parentheses instead of appending a minus sign.
    /// </summary>
    Accounting
}