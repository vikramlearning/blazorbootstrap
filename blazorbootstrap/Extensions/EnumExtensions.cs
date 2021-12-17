namespace BlazorBootstrap.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the button class.
        /// </summary>
        /// <param name="buttonType"></param>
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
    }
}
