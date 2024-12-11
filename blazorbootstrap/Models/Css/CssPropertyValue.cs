using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlazorBootstrap;

/// <summary>
/// Contains a number value and its unit for CSS rendering purposes: to be applied to properties in <see cref="BootstrapCssSettings"/>
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct CssPropertyValue
{
    [FieldOffset(0)]
    internal readonly float Value;
    [FieldOffset(4)]
    internal readonly byte Unit;
    [FieldOffset(5)]
    internal readonly bool IsInteger;
    [FieldOffset(6)]
    internal readonly byte cssStyle;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="value">Value of the property</param>
    /// <param name="unit">Unit type</param>
    /// <param name="isInteger">Whether the supplied value is a whole number.</param> 
    private CssPropertyValue(float value, byte unit, bool isInteger )
    {
        Value = value;
        Unit = unit;

        IsInteger = isInteger || IsNumberAWholeNumber(value);
        cssStyle = 0;
    }

    private CssPropertyValue(CssStyleEnum cssStyle)
    {
        Value = 0;
        Unit = 0;
        IsInteger = false;
        this.cssStyle = (byte)cssStyle;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (cssStyle != 0)
        {
            return CssStyleEnumToString[(CssStyleEnum)cssStyle];
        }

        return $"{Value.ToString(IsInteger ? "G0" : "G5", CultureInfo.InvariantCulture)}{CssNumberTypeToUnit[Unit]}";
    }

    /// <summary>
    /// Converts <see cref="CssPropertyValue"/> to a value string, or an empty string if null.
    /// </summary>
    public static implicit operator string(CssPropertyValue? value) => value?.ToString() ?? String.Empty;
    /// <summary>
    /// Converts <see cref="CssPropertyValue"/> to a CSS value string.
    /// </summary> 
    public static implicit operator string(CssPropertyValue value) => value.ToString();
    /// <summary>
    /// Generates a raw CSS number value from input.
    /// </summary>
    public static implicit operator CssPropertyValue(float value) => new(value, NumberUnit, false);
    /// <summary>
    /// Generates a raw CSS number value from input.
    /// </summary>
    public static implicit operator CssPropertyValue(int value) => new(value, NumberUnit, true);
    /// <summary>
    /// Sets a default/pre-set CSS value from input.
    /// </summary>
    public static implicit operator CssPropertyValue(CssStyleEnum value) => new(value);

    private static readonly Dictionary<CssStyleEnum, string> CssStyleEnumToString = new()
    {
        {CssStyleEnum.Absolute, "absolute"},
        {CssStyleEnum.After, "after"},
        {CssStyleEnum.Auto, "auto"},
        {CssStyleEnum.AutoPhrase, "auto-phrase"},
        {CssStyleEnum.Before, "before"},
        {CssStyleEnum.Block, "block"},
        {CssStyleEnum.BorderBox, "border-box"},
        {CssStyleEnum.BreakAll, "break-all"},
        {CssStyleEnum.BreakWord, "break-word"},
        {CssStyleEnum.ContentBox, "content-box"},
        {CssStyleEnum.Column, "column"},
        {CssStyleEnum.ColumnReverse, "column-reverse"},
        {CssStyleEnum.Dashed, "dashed"},
        {CssStyleEnum.Dotted, "dotted"},
        {CssStyleEnum.Double, "double"},
        {CssStyleEnum.Fixed, "fixed"},
        {CssStyleEnum.Flex, "flex"},
        {CssStyleEnum.FlowRoot, "flow-root"},
        {CssStyleEnum.Grid, "grid"},
        {CssStyleEnum.Groove, "groove"},
        {CssStyleEnum.Inherit, "inherit"},
        {CssStyleEnum.Initial, "initial"},
        {CssStyleEnum.Inline, "inline"},
        {CssStyleEnum.InlineBlock, "inline-block"},
        {CssStyleEnum.InlineFlex, "inline-flex"},
        {CssStyleEnum.InlineGrid, "inline-grid"},
        {CssStyleEnum.InlineTable, "inline-table"},
        {CssStyleEnum.Inset, "inset"},
        {CssStyleEnum.KeepAll, "keep-all"},
        {CssStyleEnum.LineThrough, "line-through"},
        {CssStyleEnum.ListItem, "list-item"},
        {CssStyleEnum.None, "none"},
        {CssStyleEnum.Normal, "normal"},
        {CssStyleEnum.NotApplied, ""},
        {CssStyleEnum.Outset, "outset"},
        {CssStyleEnum.Overline, "overline"},
        {CssStyleEnum.Relative, "relative"},
        {CssStyleEnum.Revert, "revert"},
        {CssStyleEnum.RevertLayer, "revert-layer"},
        {CssStyleEnum.Ridge, "ridge"},
        {CssStyleEnum.Row, "row"},
        {CssStyleEnum.RowReverse, "row-reverse"},
        {CssStyleEnum.Solid, "solid"},
        {CssStyleEnum.Static, "static"},
        {CssStyleEnum.Sticky, "sticky"},
        {CssStyleEnum.Table, "table"},
        {CssStyleEnum.TableRow, "table-row"},
        {CssStyleEnum.Text, "text"},
        {CssStyleEnum.Underline, "underline"},
        {CssStyleEnum.Unset, "unset"},
        {CssStyleEnum.Left, "left"},
        {CssStyleEnum.Right, "right"},
        {CssStyleEnum.Center, "center"},
        {CssStyleEnum.Justify, "justify"},
        {CssStyleEnum.JustifyAll, "justify-all"},
        {CssStyleEnum.MatchParent, "match-parent"},
        {CssStyleEnum.Start, "start"},
        {CssStyleEnum.End, "end"},

    };

    /// <summary>
    /// Angle value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Angle(float value) => new(value, AngleUnit, false);

    /// <summary>
    /// Angle value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Angle(int value) => new(value, AngleUnit, true);

    /// <summary>
    /// Raw number value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue RawNumber(float value) => new(value, NumberUnit, false);

    /// <summary>
    /// Raw number value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue RawNumber(int value) => new(value, NumberUnit, true);

    /// <summary>
    /// Creates value based on percentage (%) value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Percentage(float value) => new(value, PercentUnit, false);

    /// <summary>
    /// Creates value based on percentage (%) value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Percentage(int value) => new(value, PercentUnit, true);

    /// <summary>
    /// Creates value based on pixels (px)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Pixels(float value) => new(value, PixelsUnit, false);

    /// <summary>
    /// Creates value based on pixels (px)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Pixels(int value) => new(value, PixelsUnit, true);
    
    /// <summary>
    /// rem value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Rem(float value) => new(value, RootUnit, false);

    /// <summary>
    /// rem value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Rem(int value) => new(value, RootUnit, true);
    
    /// <summary>
    /// dots per inch (dpi) value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Resolution(float value) => new(value, ResolutionUnit, false);
    
    /// <summary>
    /// dots per inch (dpi) value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Resolution(int value) => new(value, ResolutionUnit, true);

    /// <summary>
    /// Time value, in seconds
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Time(float value) => new(value, TimeUnit, false);

    /// <summary>
    /// Time value, in seconds
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static CssPropertyValue Time(int value) => new(value, TimeUnit, true);

    private static readonly Dictionary<byte, string> CssNumberTypeToUnit = new()
    {
        {AngleUnit, "deg"},
                      {NumberUnit, ""},
                             {PercentUnit, "%"},
                                    {PixelsUnit, "px"},
                                           {ResolutionUnit, "dpi"},
                                                  {RootUnit, "rem"},
                                                         {TimeUnit, "s"}
                                                            };

    #region Unit Type

    /// <summary>
    /// Angle (degrees)
    /// </summary>
    private const byte AngleUnit = 0;

    /// <summary>
    /// Raw number
    /// </summary>
    private const byte NumberUnit = 1;

    /// <summary>
    /// % value
    /// </summary>
    private const byte PercentUnit = 2;

    /// <summary>
    /// px value
    /// </summary>
    private const byte PixelsUnit = 3;

    /// <summary>
    /// dpi (dots per inch) value
    /// </summary>
    private const byte ResolutionUnit = 4;

    /// <summary>
    /// rem value
    /// </summary>
    private const byte RootUnit = 5;

    /// <summary>
    /// Time value, in seconds (for animations and transitions)
    /// </summary>
    private const byte TimeUnit = 6;

    #endregion

    /// <summary>
    /// Checks if <paramref name="value"/> is a whole number
    /// </summary>
    /// <param name="value">Value to check</param>
    /// <returns><see langword="true"/> if correct.</returns>
    private static bool IsNumberAWholeNumber(float value) => Math.Abs(value - MathF.Floor(value)) < Single.Epsilon;
}
