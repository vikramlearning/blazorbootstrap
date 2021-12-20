namespace BlazorBootstrap.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Get the background class.
        /// </summary>
        /// <param name="backgroundColor"></param>
        /// <returns>string</returns>
        public static string ToBackgroundClass(this BackgroundColor backgroundColor)
        {
            return backgroundColor switch
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
        }
        /// <summary>
        /// Get the background and text classes.
        /// </summary>
        /// <param name="backgroundColor"></param>
        /// <returns>string</returns>
        public static string ToBackgroundAndTextClass(this BackgroundColor backgroundColor)
        {
            return backgroundColor switch
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
        }

        /// <summary>
        /// Gets the button class.
        /// </summary>
        /// <param name="buttonColor"></param>
        /// <returns>string</returns>
        public static string ToButtonClass(this ButtonColor buttonColor)
        {
            return buttonColor switch
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
        }

        /// <summary>
        /// Gets the button tag name.
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns>string</returns>
        public static string ToButtonTagName(this ButtonType buttonType)
        {
            return buttonType switch
            {
                ButtonType.Link => "a",
                _ => "button"
            };
        }

        /// <summary>
        /// Gets the button type.
        /// </summary>
        /// <param name="buttonType"></param>
        /// <returns>string</returns>
        public static string ToButtonTypeString(this ButtonType buttonType)
        {
            return buttonType switch
            {
                ButtonType.Button => "button",
                ButtonType.Submit => "submit",
                ButtonType.Reset => "reset",
                _ => null,
            };
        }

        /// <summary>
        /// Gets the callout color.
        /// </summary>
        /// <param name="color"></param>
        /// <returns>string</returns>
        public static string ToCalloutColor(this CalloutColor calloutColor)
        {
            return calloutColor switch
            {
                CalloutColor.None => "",
                CalloutColor.Danger => "bb-callout-danger",
                CalloutColor.Warning => "bb-callout-warning",
                CalloutColor.Info => "bb-callout-info",
                _ => ""
            };
        }

        /// <summary>
        /// Gets the spinner color.
        /// </summary>
        /// <param name="color"></param>
        /// <returns>string</returns>
        public static string ToSpinnerColor(this SpinnerColor color)
        {
            return color switch
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
        }

        /// <summary>
        /// Gets the link target name.
        /// </summary>
        /// <param name="target"></param>
        /// <returns>string</returns>
        public static string ToTargetString(this Target target) => target switch
        {
            Target.Blank => "_blank",
            Target.Parent => "_parent",
            Target.Top => "_top",
            Target.Self => "_self",
            _ => null,
        };

        /// <summary>
        /// Gets the tooltip placement.
        /// </summary>
        /// <param name="tooltipPlacement"></param>
        /// <returns>string</returns>
        public static string ToTooltipPlacementName(this TooltipPlacement tooltipPlacement)
        {
            return tooltipPlacement switch
            {
                TooltipPlacement.Auto => "auto",
                TooltipPlacement.Right => "right",
                TooltipPlacement.Bottom => "bottom",
                TooltipPlacement.Left => "left",
                _ => "top"
            };
        }
    }
}
