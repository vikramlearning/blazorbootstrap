namespace BlazorBootstrap.Utilities
{
    public class BootstrapClassProvider
    {
        #region Alert

        public string Alert() => "alert";

        public string AlertColor(AlertColor color) => $"{Alert()}-{ToAlertColor(color)}";

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

        public string ButtonColor(ButtonColor color) => $"{Button()}-{ToButtonColor(color)}";

        public string ButtonOutline(ButtonColor color) => color != BlazorBootstrap.ButtonColor.None ? $"{Button()}-outline-{ToButtonColor(color)}" : $"{Button()}-outline";

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

        public string Tooltip() => "b-tooltip";

        public string TooltipPlacement(TooltipPlacement tooltipPlacement) => $"b-tooltip-{ToTooltipPlacement(tooltipPlacement)}";

        public string TooltipMultiline() => "b-tooltip-multiline";

        public string TooltipAlwaysActive() => "b-tooltip-active";

        public string TooltipFade() => "b-tooltip-fade";

        public string TooltipInline() => "b-tooltip-inline";

        #endregion

        #region Heading

        public string HeadingSize(HeadingSize headingSize) => $"h{ToHeadingSize(headingSize)}";

        #endregion

        #region DisplayHeading

        public string DisplayHeadingSize(DisplayHeadingSize displayHeadingSize) => $"display-{ToDisplayHeadingSize(displayHeadingSize)}";

        #endregion

        #region Divider

        public string Divider() => "divider";

        public string DividerType(DividerType dividerType) => $"{Divider()}-{ToDividerType(dividerType)}";

        #endregion

        #region Offcanvas

        public string Offcanvas() => "offcanvas";

        public string Offcanvas(Placement placement) => $"{Offcanvas()}-{ToPlacement(placement)}";

        #endregion Offcanvas

        #region Modal

        public string Modal() => "modal";

        #endregion Modal

        #region Methods

        public string ToButtonColor(ButtonColor color)
        {
            return color switch
            {
                BlazorBootstrap.ButtonColor.Primary => "primary",
                BlazorBootstrap.ButtonColor.Secondary => "secondary",
                BlazorBootstrap.ButtonColor.Success => "success",
                BlazorBootstrap.ButtonColor.Danger => "danger",
                BlazorBootstrap.ButtonColor.Warning => "warning",
                BlazorBootstrap.ButtonColor.Info => "info",
                BlazorBootstrap.ButtonColor.Light => "light",
                BlazorBootstrap.ButtonColor.Dark => "dark",
                BlazorBootstrap.ButtonColor.Link => "link",
                _ => null,
            };
        }

        public string ToTextColor(TextColor color)
        {
            return color switch
            {
                BlazorBootstrap.TextColor.Primary => "primary",
                BlazorBootstrap.TextColor.Secondary => "secondary",
                BlazorBootstrap.TextColor.Success => "success",
                BlazorBootstrap.TextColor.Danger => "danger",
                BlazorBootstrap.TextColor.Warning => "warning",
                BlazorBootstrap.TextColor.Info => "info",
                BlazorBootstrap.TextColor.Light => "light",
                BlazorBootstrap.TextColor.Dark => "dark",
                BlazorBootstrap.TextColor.Body => "body",
                BlazorBootstrap.TextColor.Muted => "muted",
                BlazorBootstrap.TextColor.White => "white",
                _ => null,
            };
        }

        public string ToBackgroundColor(BackgroundColor color)
        {
            return color switch
            {
                BlazorBootstrap.BackgroundColor.Primary => "primary",
                BlazorBootstrap.BackgroundColor.Secondary => "secondary",
                BlazorBootstrap.BackgroundColor.Success => "success",
                BlazorBootstrap.BackgroundColor.Danger => "danger",
                BlazorBootstrap.BackgroundColor.Warning => "warning",
                BlazorBootstrap.BackgroundColor.Info => "info",
                BlazorBootstrap.BackgroundColor.Light => "light",
                BlazorBootstrap.BackgroundColor.Dark => "dark",
                BlazorBootstrap.BackgroundColor.Body => "body",
                BlazorBootstrap.BackgroundColor.White => "white",
                BlazorBootstrap.BackgroundColor.Transparent => "transparent",
                _ => null,
            };
        }

        public string ToAlertColor(AlertColor color)
        {
            return color switch
            {
                BlazorBootstrap.AlertColor.Primary => "primary",
                BlazorBootstrap.AlertColor.Secondary => "secondary",
                BlazorBootstrap.AlertColor.Success => "success",
                BlazorBootstrap.AlertColor.Danger => "danger",
                BlazorBootstrap.AlertColor.Warning => "warning",
                BlazorBootstrap.AlertColor.Info => "info",
                BlazorBootstrap.AlertColor.Light => "light",
                BlazorBootstrap.AlertColor.Dark => "dark",
                _ => null,
            };
        }

        public string ToColor(TextColor color)
        {
            return color switch
            {
                BlazorBootstrap.TextColor.Primary => "primary",
                BlazorBootstrap.TextColor.Secondary => "secondary",
                BlazorBootstrap.TextColor.Success => "success",
                BlazorBootstrap.TextColor.Danger => "danger",
                BlazorBootstrap.TextColor.Warning => "warning",
                BlazorBootstrap.TextColor.Info => "info",
                BlazorBootstrap.TextColor.Light => "light",
                BlazorBootstrap.TextColor.Dark => "dark",
                _ => null,
            };
        }

        public string ToSize(Size size)
        {
            return size switch
            {
                BlazorBootstrap.Size.ExtraSmall => "xs",
                BlazorBootstrap.Size.Small => "sm",
                BlazorBootstrap.Size.Medium => "md",
                BlazorBootstrap.Size.Large => "lg",
                BlazorBootstrap.Size.ExtraLarge => "xl",
                _ => null,
            };
        }

        public string ToTooltipPlacement(TooltipPlacement tooltipPlacement)
        {
            return tooltipPlacement switch
            {
                BlazorBootstrap.TooltipPlacement.Bottom => "bottom",
                BlazorBootstrap.TooltipPlacement.Left => "left",
                BlazorBootstrap.TooltipPlacement.Right => "right",
                _ => "top",
            };
        }

        public string ToHeadingSize(HeadingSize headingSize)
        {
            return headingSize switch
            {
                BlazorBootstrap.HeadingSize.H1 => "1",
                BlazorBootstrap.HeadingSize.H2 => "2",
                BlazorBootstrap.HeadingSize.H3 => "3",
                BlazorBootstrap.HeadingSize.H4 => "4",
                BlazorBootstrap.HeadingSize.H5 => "5",
                BlazorBootstrap.HeadingSize.H6 => "6",
                _ => null,
            };
        }

        public string ToDisplayHeadingSize(DisplayHeadingSize displayHeadingSize)
        {
            return displayHeadingSize switch
            {
                BlazorBootstrap.DisplayHeadingSize.H1 => "1",
                BlazorBootstrap.DisplayHeadingSize.H2 => "2",
                BlazorBootstrap.DisplayHeadingSize.H3 => "3",
                BlazorBootstrap.DisplayHeadingSize.H4 => "4",
                _ => null,
            };
        }

        public string ToDividerType(DividerType dividerType)
        {
            return dividerType switch
            {
                BlazorBootstrap.DividerType.Dashed => "dashed",
                BlazorBootstrap.DividerType.Dotted => "dotted",
                BlazorBootstrap.DividerType.TextContent => "text",
                _ => "solid",
            };
        }

        public string ToScreenreader(Screenreader screenreader)
        {
            return screenreader switch
            {
                BlazorBootstrap.Screenreader.Only => "sr-only",
                BlazorBootstrap.Screenreader.OnlyFocusable => "sr-only-focusable",
                _ => null,
            };
        }

        public string ToPlacement(Placement placement)
        {
            return placement switch
            {
                BlazorBootstrap.Placement.Start => "start",
                BlazorBootstrap.Placement.End => "end",
                BlazorBootstrap.Placement.Top => "top",
                _ => "bottom",
            };
        }

        #endregion
    }
}
