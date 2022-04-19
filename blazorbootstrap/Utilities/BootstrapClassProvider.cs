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

        #region Colors

        public string BackgroundColor(BackgroundColor backgroundColor) => $"bg-{ToBackgroundColor(backgroundColor)}";

        #endregion

        #region Callout

        public string Callout() => "bb-callout";

        #endregion

        #region Confirmation Modal

        public string ConfirmationModal() => "modal-confirmation";

        #endregion

        #region DisplayHeading

        public string DisplayHeadingSize(DisplayHeadingSize displayHeadingSize) => $"display-{ToDisplayHeadingSize(displayHeadingSize)}";

        #endregion

        #region Divider

        public string Divider() => "divider";

        public string DividerType(DividerType dividerType) => $"{Divider()}-{ToDividerType(dividerType)}";

        #endregion

        #region Flex

        public string FlexAlignment(Alignment alignment) => $"justify-content-{ToAlignment(alignment)}";

        #endregion

        #region Heading

        public string HeadingSize(HeadingSize headingSize) => $"h{ToHeadingSize(headingSize)}";

        #endregion

        #region Modal

        public string Modal() => "modal";
        public string ModalFade() => Fade();

        #endregion Modal

        #region Offcanvas

        public string Offcanvas() => "offcanvas";

        public string Offcanvas(Placement placement) => $"{Offcanvas()}-{ToPlacement(placement)}";

        #endregion Offcanvas

        #region PageLoading

        public string PageLoadingModal() => "modal-page-loading";

        #endregion PageLoading

        #region Pagination

        public string Pagination() => "pagination";

        public string PaginationSize(PaginationSize size) => $"{Pagination()}-{ToPaginationSize(size)}";

        public string PaginationItem() => "page-item";

        public string PaginationItemActive() => Active();

        public string PaginationItemDisabled() => Disabled();

        public string PaginationLink() => "page-link";

        public string PaginationLinkActive() => null;

        public string PaginationLinkDisabled() => null;

        #endregion

        #region Placeholder

        public string Placeholder() => "placeholder";
        public string PlaceholderAnimation(PlaceholderAnimation animation) => $"{Placeholder()}-{ToPlaceholderAnimation(animation)}";
        public string PlaceholderWidth(PlaceholderWidth width) => $"{ToPlaceholderWidth(width)}";
        public string PlaceholderColor(PlaceholderColor color) => $"bg-{ToPlaceholderColor(color)}";
        public string PlaceholderSize(PlaceholderSize size) => $"{Placeholder()}-{ToPlaceholderSize(size)}";

        #endregion

        #region Position

        public string Position() => "position";
        public string PositionAbsolute() => $"{Position()}-absolute";
        public string PositionFixed() => $"{Position()}-fixed";

        #endregion

        #region States

        public string Show() => "show";

        public string Fade() => "fade";

        public string Active() => "active";

        public string Disabled() => "disabled";

        public string Collapsed() => "collapsed";

        #endregion

        #region Tabs

        public string Nav => "nav";
        public string NavTabs => $"{Nav}-tabs";

        #endregion

        #region Toast

        public string Toast() => "toast";
        public string ToastContainer() => $"{Toast()}-container";

        #endregion

        #region Tooltip

        public string Tooltip() => "b-tooltip";

        public string TooltipPlacement(TooltipPlacement tooltipPlacement) => $"b-tooltip-{ToTooltipPlacement(tooltipPlacement)}";

        public string TooltipMultiline() => "b-tooltip-multiline";

        public string TooltipAlwaysActive() => "b-tooltip-active";

        public string TooltipFade() => "b-tooltip-fade";

        public string TooltipInline() => "b-tooltip-inline";

        #endregion

        #region Methods

        public string ToAlignment(Alignment alignment)
        {
            return alignment switch
            {
                BlazorBootstrap.Alignment.Start => "start",
                BlazorBootstrap.Alignment.Center => "center",
                BlazorBootstrap.Alignment.End => "end",
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

        public string ToToastBackgroundColor(ToastType toastType)
        {
            return toastType switch
            {
                BlazorBootstrap.ToastType.Primary => "primary",
                BlazorBootstrap.ToastType.Secondary => "secondary",
                BlazorBootstrap.ToastType.Success => "success",
                BlazorBootstrap.ToastType.Danger => "danger",
                BlazorBootstrap.ToastType.Warning => "warning",
                BlazorBootstrap.ToastType.Info => "info",
                BlazorBootstrap.ToastType.Light => "light",
                BlazorBootstrap.ToastType.Dark => "dark",
                BlazorBootstrap.ToastType.Body => "body",
                BlazorBootstrap.ToastType.White => "white",
                BlazorBootstrap.ToastType.Transparent => "transparent",
                _ => null,
            };
        }

        public string ToToastTextColor(ToastType toastType)
        {
            return toastType switch
            {
                BlazorBootstrap.ToastType.Primary
                or BlazorBootstrap.ToastType.Secondary
                or BlazorBootstrap.ToastType.Success
                or BlazorBootstrap.ToastType.Danger
                or BlazorBootstrap.ToastType.Dark => "white",

                BlazorBootstrap.ToastType.Warning
                or BlazorBootstrap.ToastType.Info
                or BlazorBootstrap.ToastType.Light
                or BlazorBootstrap.ToastType.Body
                or BlazorBootstrap.ToastType.White
                or BlazorBootstrap.ToastType.Transparent => "dark",

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

        public string ToCalloutColor(CalloutColor color)
        {
            return color switch
            {
                BlazorBootstrap.CalloutColor.None => "",
                BlazorBootstrap.CalloutColor.Danger => "bb-callout-danger",
                BlazorBootstrap.CalloutColor.Warning => "bb-callout-warning",
                BlazorBootstrap.CalloutColor.Info => "bb-callout-info",
                _ => "",
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

        public string ToToastsPlacement(ToastsPlacement toastsPlacement)
        {
            return toastsPlacement switch
            {
                BlazorBootstrap.ToastsPlacement.TopLeft => "top-0 start-0",
                BlazorBootstrap.ToastsPlacement.TopCenter => "top-0 start-50 translate-middle-x",
                BlazorBootstrap.ToastsPlacement.TopRight => "top-0 end-0",
                BlazorBootstrap.ToastsPlacement.MiddleLeft => "top-50 start-0 translate-middle-y",
                BlazorBootstrap.ToastsPlacement.MiddleCenter => "top-50 start-50 translate-middle",
                BlazorBootstrap.ToastsPlacement.MiddleRight => "top-50 end-0 translate-middle-y",
                BlazorBootstrap.ToastsPlacement.BottomLeft => "bottom-0 start-0",
                BlazorBootstrap.ToastsPlacement.BottomCenter => "bottom-0 start-50 translate-middle-x",
                BlazorBootstrap.ToastsPlacement.BottomRight => "bottom-0 end-0",
                _ => "top-0 end-0", // default: Top right
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

        public string ToPaginationSize(PaginationSize size)
        {
            return size switch
            {
                BlazorBootstrap.PaginationSize.Small => "sm",
                BlazorBootstrap.PaginationSize.Large => "lg",
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

        public string ToModalSize(ModalSize size)
        {
            return size switch
            {
                BlazorBootstrap.ModalSize.Regular => null,
                BlazorBootstrap.ModalSize.Small => "modal-sm",
                BlazorBootstrap.ModalSize.Large => "modal-lg",
                BlazorBootstrap.ModalSize.ExtraLarge => "modal-xl",
                _ => null,
            };
        }

        public string ToModalFullscreen(ModalFullscreen fullscreen)
        {
            return fullscreen switch
            {
                BlazorBootstrap.ModalFullscreen.Disabled => null,
                BlazorBootstrap.ModalFullscreen.Always => "modal-fullscreen",
                BlazorBootstrap.ModalFullscreen.SmallDown => "modal-fullscreen-sm-down",
                BlazorBootstrap.ModalFullscreen.MediumDown => "modal-fullscreen-md-down",
                BlazorBootstrap.ModalFullscreen.LargeDown => "modal-fullscreen-lg-down",
                BlazorBootstrap.ModalFullscreen.ExtraLargeDown => "modal-fullscreen-xl-down",
                BlazorBootstrap.ModalFullscreen.ExtraExtraLargeDown => "modal-fullscreen-xxl-down",
                _ => null,
            };
        }

        public string ToOffcanvasSize(OffcanvasSize size)
        {
            return size switch
            {
                BlazorBootstrap.OffcanvasSize.Regular => null,
                BlazorBootstrap.OffcanvasSize.Small => "bb-offcanvas-sm",
                BlazorBootstrap.OffcanvasSize.Large => "bb-offcanvas-lg",
                _ => null,
            };
        }

        public string ToPlaceholderAnimation(PlaceholderAnimation animation)
        {
            return animation switch
            {
                BlazorBootstrap.PlaceholderAnimation.Glow => "glow",
                BlazorBootstrap.PlaceholderAnimation.Wave => "wave",
                _ => null,
            };
        }

        public string ToPlaceholderWidth(PlaceholderWidth width)
        {
            return width switch
            {
                BlazorBootstrap.PlaceholderWidth.Col1 => "col-1",
                BlazorBootstrap.PlaceholderWidth.Col2 => "col-2",
                BlazorBootstrap.PlaceholderWidth.Col3 => "col-3",
                BlazorBootstrap.PlaceholderWidth.Col4 => "col-4",
                BlazorBootstrap.PlaceholderWidth.Col5 => "col-5",
                BlazorBootstrap.PlaceholderWidth.Col6 => "col-6",
                BlazorBootstrap.PlaceholderWidth.Col7 => "col-7",
                BlazorBootstrap.PlaceholderWidth.Col8 => "col-8",
                BlazorBootstrap.PlaceholderWidth.Col9 => "col-9",
                BlazorBootstrap.PlaceholderWidth.Col10 => "col-10",
                BlazorBootstrap.PlaceholderWidth.Col11 => "col-11",
                BlazorBootstrap.PlaceholderWidth.Col12 => "col-12",
                _ => null,
            };
        }
        public string ToPlaceholderColor(PlaceholderColor color)
        {
            return color switch
            {
                BlazorBootstrap.PlaceholderColor.Primary => "primary",
                BlazorBootstrap.PlaceholderColor.Secondary => "secondary",
                BlazorBootstrap.PlaceholderColor.Success => "success",
                BlazorBootstrap.PlaceholderColor.Danger => "danger",
                BlazorBootstrap.PlaceholderColor.Warning => "warning",
                BlazorBootstrap.PlaceholderColor.Info => "info",
                BlazorBootstrap.PlaceholderColor.Light => "light",
                BlazorBootstrap.PlaceholderColor.Dark => "dark",
                _ => null,
            };
        }

        public string ToPlaceholderSize(PlaceholderSize size)
        {
            return size switch
            {
                BlazorBootstrap.PlaceholderSize.ExtraSmall => "xs",
                BlazorBootstrap.PlaceholderSize.Small => "sm",
                BlazorBootstrap.PlaceholderSize.Large => "lg",
                _ => null,
            };
        }

        #endregion
    }
}
