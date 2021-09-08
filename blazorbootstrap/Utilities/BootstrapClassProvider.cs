namespace BlazorBootstrap.Utilities
{
    public class BootstrapClassProvider
    {
        #region Alert

        public string Alert() => "alert";

        public string AlertColor(Color color) => $"{Alert()}-{ToColor(color)}";

        public string AlertDismisable() => "alert-dismissible";

        public string AlertFade() => Fade();

        public string AlertShow() => Show();

        public string AlertHasMessage() => null;

        public string AlertHasDescription() => null;

        public string AlertMessage() => null;

        public string AlertDescription() => null;

        #endregion

        #region Button

        public string Button() => "btn";

        public string ButtonColor(Color color) => $"{Button()}-{ToColor(color)}";

        public string ButtonOutline(Color color) => color != Color.None ? $"{Button()}-outline-{ToColor(color)}" : $"{Button()}-outline";

        public string ButtonSize(Size size) => $"{Button()}-{ToSize(size)}";

        public string ButtonBlock() => $"{Button()}-block";

        public string ButtonActive() => "active";

        public string ButtonDisabled() => "disabled";

        public string ButtonLoading() => null;

        #endregion

        #region States

        public string Show() => "show";

        public string Fade() => "fade";

        public string Active() => "active";

        public string Disabled() => "disabled";

        public string Collapsed() => "collapsed";

        #endregion

        #region Tooltip

        //public string Tooltip() => "b-tooltip";

        //public string TooltipPlacement(TooltipPlacement tooltipPlacement) => $"b-tooltip-{ToTooltipPlacement(tooltipPlacement)}";

        //public string TooltipMultiline() => "b-tooltip-multiline";

        //public string TooltipAlwaysActive() => "b-tooltip-active";

        //public string TooltipFade() => "b-tooltip-fade";

        //public string TooltipInline() => "b-tooltip-inline";

        #endregion

        #region Enums

        public string ToColor(Color color)
        {
            return color switch
            {
                Color.Primary => "primary",
                Color.Secondary => "secondary",
                Color.Success => "success",
                Color.Danger => "danger",
                Color.Warning => "warning",
                Color.Info => "info",
                Color.Light => "light",
                Color.Dark => "dark",
                Color.Link => "link",
                _ => null,
            };
        }

        public string ToSize(Size size)
        {
            return size switch
            {
                Size.ExtraSmall => "xs",
                Size.Small => "sm",
                Size.Medium => "md",
                Size.Large => "lg",
                Size.ExtraLarge => "xl",
                _ => null,
            };
        }

        public string ToTooltipPlacement(TooltipPlacement tooltipPlacement)
        {
            return tooltipPlacement switch
            {
                TooltipPlacement.Bottom => "bottom",
                TooltipPlacement.Left => "left",
                TooltipPlacement.Right => "right",
                _ => "top",
            };
        }

        #endregion
    }
}
