namespace BlazorBootstrap;

public static class EnumExtensions
{
    #region Methods

    /// <summary>
    /// Get the background and text classes.
    /// </summary>
    /// <param name="backgroundColor"></param>
    /// <returns>string</returns>
    public static string ToBackgroundAndTextClass(this BackgroundColor backgroundColor) =>
        backgroundColor switch
        {
            BackgroundColor.Primary => "bg-primary text-white",
            BackgroundColor.Secondary => "bg-secondary text-white",
            BackgroundColor.Success => "bg-success text-white",
            BackgroundColor.Danger => "bg-danger text-white",
            BackgroundColor.Warning => "bg-warning text-dark",
            BackgroundColor.Info => "bg-info text-dark",
            BackgroundColor.Light => "bg-light text-dark",
            BackgroundColor.Dark => "bg-dark text-white",
            BackgroundColor.Body => "bg-body text-dark",
            BackgroundColor.White => "bg-white text-dark",
            BackgroundColor.Transparent => "bg-transparent text-dark",
            _ => ""
        };

    /// <summary>
    /// Get the background class.
    /// </summary>
    /// <param name="backgroundColor"></param>
    /// <returns>string</returns>
    public static string ToBackgroundClass(this BackgroundColor backgroundColor) =>
        backgroundColor switch
        {
            BackgroundColor.Primary => "bg-primary",
            BackgroundColor.Secondary => "bg-secondary",
            BackgroundColor.Success => "bg-success",
            BackgroundColor.Danger => "bg-danger",
            BackgroundColor.Warning => "bg-warning",
            BackgroundColor.Info => "bg-info",
            BackgroundColor.Light => "bg-light",
            BackgroundColor.Dark => "bg-dark",
            BackgroundColor.Body => "bg-body",
            BackgroundColor.White => "bg-white",
            BackgroundColor.Transparent => "bg-transparent",
            _ => ""
        };

    /// <summary>
    /// Gets the button class.
    /// </summary>
    /// <param name="buttonColor"></param>
    /// <returns>string</returns>
    public static string ToButtonClass(this ButtonColor buttonColor) =>
        buttonColor switch
        {
            ButtonColor.Primary => "btn btn-primary",
            ButtonColor.Secondary => "btn btn-secondary",
            ButtonColor.Success => "btn btn-success",
            ButtonColor.Danger => "btn btn-danger",
            ButtonColor.Warning => "btn btn-warning",
            ButtonColor.Info => "btn btn-info",
            ButtonColor.Light => "btn btn-light",
            ButtonColor.Dark => "btn btn-dark",
            ButtonColor.Link => "btn btn-link",
            _ => "btn btn-primary"
        };

    /// <summary>
    /// Gets the button tag name.
    /// </summary>
    /// <param name="buttonType"></param>
    /// <returns>string</returns>
    public static string ToButtonTagName(this ButtonType buttonType) =>
        buttonType switch
        {
            ButtonType.Link => "a",
            _ => "button"
        };

    /// <summary>
    /// Gets the button type.
    /// </summary>
    /// <param name="buttonType"></param>
    /// <returns>string</returns>
    public static string? ToButtonTypeString(this ButtonType buttonType) =>
        buttonType switch
        {
            ButtonType.Button => "button",
            ButtonType.Submit => "submit",
            ButtonType.Reset => "reset",
            _ => null
        };

    /// <summary>
    /// Gets the callout color.
    /// </summary>
    /// <param name="color"></param>
    /// <returns>string</returns>
    public static string ToCalloutColor(this CalloutType calloutColor) =>
        calloutColor switch
        {
            CalloutType.Default => "",
            CalloutType.Danger => "bb-callout-danger",
            CalloutType.Warning => "bb-callout-warning",
            CalloutType.Info => "bb-callout-info",
            _ => ""
        };

    /// <summary>
    /// Converts value into css valid string.
    /// </summary>
    public static string ToCssString(this Unit value) =>
        value switch
        {
            Unit.Em => "em",
            Unit.Percentage => "%",
            Unit.Pt => "pt",
            Unit.Px => "px",
            Unit.Rem => "rem",
            Unit.Vh => "vh",
            Unit.VMax => "vmax",
            Unit.VMin => "vmin",
            Unit.Vw => "vw",
            _ => string.Empty
        };

    public static object ToSortableListPullMode(this SortableListPullMode mode) =>
        mode switch
        {
            SortableListPullMode.True => true,
            SortableListPullMode.False => false,
            SortableListPullMode.Clone => "clone",
            //SortableListPullMode.Array => "array"
        };

    public static object ToSortableListPutMode(this SortableListPutMode mode) =>
        mode switch
        {
            SortableListPutMode.True => true,
            SortableListPutMode.False => false,
            //SortableListPullMode.Array => "array"
        };

    /// <summary>
    /// Gets the spinner color.
    /// </summary>
    /// <param name="color"></param>
    /// <returns>string</returns>
    public static string ToSpinnerColor(this SpinnerColor color) =>
        color switch
        {
            SpinnerColor.Primary => "text-primary",
            SpinnerColor.Secondary => "text-secondary",
            SpinnerColor.Success => "text-success",
            SpinnerColor.Danger => "text-danger",
            SpinnerColor.Warning => "text-warning",
            SpinnerColor.Info => "text-info",
            SpinnerColor.Light => "text-light",
            SpinnerColor.Dark => "text-dark",
            _ => ""
        };

    /// <summary>
    /// Gets the link target name.
    /// </summary>
    /// <param name="target"></param>
    /// <returns>string</returns>
    public static string? ToTargetString(this Target target) =>
        target switch
        {
            Target.Blank => "_blank",
            Target.Parent => "_parent",
            Target.Top => "_top",
            Target.Self => "_self",
            _ => null
        };

    /// <summary>
    /// Gets the tooltip placement.
    /// </summary>
    /// <param name="tooltipPlacement"></param>
    /// <returns>string</returns>
    public static string ToTooltipPlacementName(this TooltipPlacement tooltipPlacement) =>
        tooltipPlacement switch
        {
            TooltipPlacement.Auto => "auto",
            TooltipPlacement.Right => "right",
            TooltipPlacement.Bottom => "bottom",
            TooltipPlacement.Left => "left",
            _ => "top"
        };

    #endregion
}
