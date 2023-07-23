namespace BlazorBootstrap.Utilities;

public class BootstrapClassProvider
{
    #region Accordion

    public string Accordion() => "accordion";
    public string AccordionFlush() => $"{Accordion()}-flush";
    public string AccordionItem() => $"{Accordion()}-item";
    public string AccordionItemHeader() => $"{Accordion()}-header";
    public string AccordionItemBody() => $"{Accordion()}-body";
    public string AccordionButton() => $"{Accordion()}-button";

    #endregion

    #region Alert

    public string Alert() => "alert";

    public string AlertColor(AlertColor color) => $"{Alert()}-{ToAlertColor(color)}";

    public string AlertDismisable() => "alert-dismissible";

    public string AlertFade() => Fade();

    public string AlertShow() => Show();

    public string? AlertHasMessage() => null;

    public string? AlertHasDescription() => null;

    public string? AlertMessage() => null;

    public string? AlertDescription() => null;

    #endregion

    #region Badge

    public string Badge() => "badge";

    public string BadgeColor(BadgeColor color) => $"text-bg-{ToBadgeColor(color)}";

    public string PillBadge() => "rounded-pill";

    #endregion

    #region Button

    public string Button() => "btn";

    public string ButtonColor(ButtonColor color) => $"{Button()}-{ToButtonColor(color)}";

    public string ButtonOutline(ButtonColor color) => color != BlazorBootstrap.ButtonColor.None ? $"{Button()}-outline-{ToButtonColor(color)}" : $"{Button()}-outline";

    public string ButtonSize(Size size) => $"{Button()}-{ToSize(size)}";

    public string ButtonBlock() => $"{Button()}-block";

    public string ButtonActive() => "active";

    public string ButtonDisabled() => "disabled";

    public string? ButtonLoading() => null;

    #endregion

    #region Background Colors

    public string BackgroundColor(BackgroundColor backgroundColor) => $"bg-{ToBackgroundColor(backgroundColor)}";

    public string TextAndBackgroundColor(BackgroundColor backgroundColor) => $"text-bg-{ToBackgroundColor(backgroundColor)}";

    #endregion

    #region Callout

    public string Callout() => "bb-callout";

    public string CalloutHeading() => $"{Callout()}-heading";

    #endregion

    #region Card

    public string Card() => "card";

    public string CardHeader() => $"{Card()}-heading";
    public string CardBody() => $"{Card()}-body";
    public string CardTitle() => $"{Card()}-heading";
    public string CardSubTitle() => $"{Card()}-subtitle";
    public string CardText() => $"{Card()}-text";
    public string CardFooter() => $"{Card()}-footer";

    #endregion

    #region Checks

    public string Checks() => "form-check-input";
    public string ChecksReverse() => "form-check-reverse";

    #endregion

    #region Confirmation Modal

    public string ConfirmationModal() => "modal-confirmation";

    #endregion

    #region Collapse

    public string Collapse() => "collapse";
    public string CollapseHorizontal() => $"{Collapse()}-horizontal";

    #endregion

    #region DisplayHeading

    public string DisplayHeadingSize(DisplayHeadingSize displayHeadingSize) => $"display-{ToDisplayHeadingSize(displayHeadingSize)}";

    #endregion

    #region Divider

    public string Divider() => "divider";

    public string DividerType(DividerType dividerType) => $"{Divider()}-{ToDividerType(dividerType)}";

    #endregion

    #region Dropdown

    public string DropdownToggle() => "dropdown-toggle";

    #endregion Dropdown

    #region Flex

    public string FlexAlignment(Alignment alignment) => $"justify-content-{ToAlignment(alignment)}";

    #endregion

    #region FormControl

    public string FormControl() => "form-control";

    public string IsValid() => "valid";

    public string IsInValid() => "invalid";

    #endregion FormControl

    #region Heading

    public string HeadingSize(HeadingSize headingSize) => $"h{ToHeadingSize(headingSize)}";

    #endregion

    #region Icons

    public string IconColor(IconColor iconColor) => $"text-{ToIconColor(iconColor)}";

    #endregion

    #region Modal

    public string Modal() => "modal";

    public string ModalHeader(ModalType modalType)
        => $"text-bg-{ToModalTypeColor(modalType)} border-bottom {ToModalHeaderBottomBorderColor(modalType)}";

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

    public string? PaginationLinkActive() => null;

    public string? PaginationLinkDisabled() => null;

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
    public string PositionStatic() => $"{Position()}-static";
    public string PositionRelative() => $"{Position()}-relative";
    public string PositionAbsolute() => $"{Position()}-absolute";
    public string PositionFixed() => $"{Position()}-fixed";
    public string PositionSticky() => $"{Position()}-sticky";

    #endregion

    #region Progress

    public string Progress() => "progress";
    public string ProgressBar() => $"{Progress()}-bar";
    public string ProgressBarStriped() => $"{ProgressBar()}-striped";
    public string ProgressBarAnimated() => $"{ProgressBar()}-animated";
    public string ProgressBackgroundColor(ProgressColor color) => $"bg-{ToProgressColor(color)}";

    #endregion

    #region Text

    public string TextAlignment(Alignment alignment) => $"text-{ToAlignment(alignment)}";

    public string TextNoWrap() => "text-nowrap";

    #endregion

    #region Text Colors

    public string TextColor(TextColor textColor) => $"text-{ToTextColor(textColor)}";

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
    public string NavPills => $"{Nav}-pills";

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

    public string ToAlignment(Alignment alignment) => alignment switch
    {
        BlazorBootstrap.Alignment.Start => "start",
        BlazorBootstrap.Alignment.Center => "center",
        BlazorBootstrap.Alignment.End => "end",
        _ => null,
    };

    public string ToBackgroundColor(BackgroundColor color) => color switch
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

    public string ToButtonColor(ButtonColor color) => color switch
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

    public string ToProgressColor(ProgressColor color) => color switch
    {
        BlazorBootstrap.ProgressColor.Primary => "primary",
        BlazorBootstrap.ProgressColor.Secondary => "secondary",
        BlazorBootstrap.ProgressColor.Success => "success",
        BlazorBootstrap.ProgressColor.Danger => "danger",
        BlazorBootstrap.ProgressColor.Warning => "warning",
        BlazorBootstrap.ProgressColor.Info => "info",
        BlazorBootstrap.ProgressColor.Dark => "dark",
        _ => null,
    };

    public string ToIconColor(IconColor color) => color switch
    {
        BlazorBootstrap.IconColor.Primary => "primary",
        BlazorBootstrap.IconColor.Secondary => "secondary",
        BlazorBootstrap.IconColor.Success => "success",
        BlazorBootstrap.IconColor.Danger => "danger",
        BlazorBootstrap.IconColor.Warning => "warning",
        BlazorBootstrap.IconColor.Info => "info",
        BlazorBootstrap.IconColor.Light => "light",
        BlazorBootstrap.IconColor.Dark => "dark",
        BlazorBootstrap.IconColor.Body => "body",
        BlazorBootstrap.IconColor.Muted => "muted",
        BlazorBootstrap.IconColor.White => "white",
        _ => null,
    };

    public string ToTextColor(TextColor color) => color switch
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

    public string ToToastBackgroundColor(ToastType toastType) => toastType switch
    {
        BlazorBootstrap.ToastType.Primary => "primary",
        BlazorBootstrap.ToastType.Secondary => "secondary",
        BlazorBootstrap.ToastType.Success => "success",
        BlazorBootstrap.ToastType.Danger => "danger",
        BlazorBootstrap.ToastType.Warning => "warning",
        BlazorBootstrap.ToastType.Info => "info",
        BlazorBootstrap.ToastType.Light => "light",
        BlazorBootstrap.ToastType.Dark => "dark",
        _ => null,
    };

    public string ToToastTextColor(ToastType toastType) => toastType switch
    {
        BlazorBootstrap.ToastType.Primary
        or BlazorBootstrap.ToastType.Secondary
        or BlazorBootstrap.ToastType.Success
        or BlazorBootstrap.ToastType.Danger
        or BlazorBootstrap.ToastType.Dark => "white",

        BlazorBootstrap.ToastType.Warning
        or BlazorBootstrap.ToastType.Info
        or BlazorBootstrap.ToastType.Light => "dark",

        _ => null,
    };

    public string ToAlertColor(AlertColor color) => color switch
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

    public string ToBadgeColor(BadgeColor color) => color switch
    {
        BlazorBootstrap.BadgeColor.Primary => "primary",
        BlazorBootstrap.BadgeColor.Secondary => "secondary",
        BlazorBootstrap.BadgeColor.Success => "success",
        BlazorBootstrap.BadgeColor.Danger => "danger",
        BlazorBootstrap.BadgeColor.Warning => "warning",
        BlazorBootstrap.BadgeColor.Info => "info",
        BlazorBootstrap.BadgeColor.Light => "light",
        BlazorBootstrap.BadgeColor.Dark => "dark",
        _ => null,
    };

    public string ToBadgePlacement(BadgePlacement badgePlacement) => badgePlacement switch
    {
        BlazorBootstrap.BadgePlacement.TopLeft => "top-0 start-0 translate-middle",
        BlazorBootstrap.BadgePlacement.TopCenter => "top-0 start-50 translate-middle",
        BlazorBootstrap.BadgePlacement.TopRight => "top-0 start-100 translate-middle",
        BlazorBootstrap.BadgePlacement.MiddleLeft => "top-50 start-0 translate-middle",
        BlazorBootstrap.BadgePlacement.MiddleCenter => "top-50 start-50 translate-middle",
        BlazorBootstrap.BadgePlacement.MiddleRight => "top-50 start-100 translate-middle",
        BlazorBootstrap.BadgePlacement.BottomLeft => "top-100 start-0 translate-middle",
        BlazorBootstrap.BadgePlacement.BottomCenter => "top-100 start-50 translate-middle",
        BlazorBootstrap.BadgePlacement.BottomRight => "top-100 start-100 translate-middle",
        _ => "top-0 start-100 translate-middle", // default: Top right
    };

    public string ToBadgeIndicator(BadgeIndicatorType indicatorType) => indicatorType switch
    {
        BlazorBootstrap.BadgeIndicatorType.RoundedPill => "rounded-pill",
        BlazorBootstrap.BadgeIndicatorType.RoundedCircle => "rounded-circle",
        _ => "", // default: Top right
    };

    public string ToCalloutType(CalloutType type) => type switch
    {
        BlazorBootstrap.CalloutType.Default => "",
        BlazorBootstrap.CalloutType.Danger => $"{Callout()}-danger",
        BlazorBootstrap.CalloutType.Warning => $"{Callout()}-warning",
        BlazorBootstrap.CalloutType.Info => $"{Callout()}-info",
        BlazorBootstrap.CalloutType.Tip => $"{Callout()}-success",
        _ => "",
    };

    public string ToColor(TextColor color) => color switch
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

    public string ToSize(Size size) => size switch
    {
        BlazorBootstrap.Size.ExtraSmall => "xs",
        BlazorBootstrap.Size.Small => "sm",
        BlazorBootstrap.Size.Medium => "md",
        BlazorBootstrap.Size.Large => "lg",
        BlazorBootstrap.Size.ExtraLarge => "xl",
        _ => null,
    };

    public string ToToastsPlacement(ToastsPlacement toastsPlacement) => toastsPlacement switch
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

    public string ToTooltipPlacement(TooltipPlacement tooltipPlacement) => tooltipPlacement switch
    {
        BlazorBootstrap.TooltipPlacement.Bottom => "bottom",
        BlazorBootstrap.TooltipPlacement.Left => "left",
        BlazorBootstrap.TooltipPlacement.Right => "right",
        _ => "top",
    };

    public string ToAutoCompleteSize(AutoCompleteSize size) => size switch
    {
        BlazorBootstrap.AutoCompleteSize.Large => "form-control-lg",
        BlazorBootstrap.AutoCompleteSize.Small => "form-control-sm",
        _ => ""
    };

    public string ToHeadingSize(HeadingSize headingSize) => headingSize switch
    {
        BlazorBootstrap.HeadingSize.H1 => "1",
        BlazorBootstrap.HeadingSize.H2 => "2",
        BlazorBootstrap.HeadingSize.H3 => "3",
        BlazorBootstrap.HeadingSize.H4 => "4",
        BlazorBootstrap.HeadingSize.H5 => "5",
        BlazorBootstrap.HeadingSize.H6 => "6",
        _ => null,
    };

    public string ToDisplayHeadingSize(DisplayHeadingSize displayHeadingSize) => displayHeadingSize switch
    {
        BlazorBootstrap.DisplayHeadingSize.H1 => "1",
        BlazorBootstrap.DisplayHeadingSize.H2 => "2",
        BlazorBootstrap.DisplayHeadingSize.H3 => "3",
        BlazorBootstrap.DisplayHeadingSize.H4 => "4",
        _ => null,
    };

    public string ToDividerType(DividerType dividerType) => dividerType switch
    {
        BlazorBootstrap.DividerType.Dashed => "dashed",
        BlazorBootstrap.DividerType.Dotted => "dotted",
        BlazorBootstrap.DividerType.TextContent => "text",
        _ => "solid",
    };

    public string ToScreenreader(Screenreader screenreader) => screenreader switch
    {
        BlazorBootstrap.Screenreader.Only => "sr-only",
        BlazorBootstrap.Screenreader.OnlyFocusable => "sr-only-focusable",
        _ => null,
    };

    public string ToPaginationSize(PaginationSize size) => size switch
    {
        BlazorBootstrap.PaginationSize.Small => "sm",
        BlazorBootstrap.PaginationSize.Large => "lg",
        _ => null,
    };

    public string ToPlacement(Placement placement) => placement switch
    {
        BlazorBootstrap.Placement.Start => "start",
        BlazorBootstrap.Placement.End => "end",
        BlazorBootstrap.Placement.Top => "top",
        _ => "bottom",
    };

    public string ToDialogSize(DialogSize size) => size switch
    {
        BlazorBootstrap.DialogSize.Regular => null,
        BlazorBootstrap.DialogSize.Small => "modal-sm",
        BlazorBootstrap.DialogSize.Large => "modal-lg",
        BlazorBootstrap.DialogSize.ExtraLarge => "modal-xl",
        _ => null,
    };

    public string ToModalFullscreen(ModalFullscreen fullscreen) => fullscreen switch
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

    public string ToModalSize(ModalSize size) => size switch
    {
        BlazorBootstrap.ModalSize.Regular => null,
        BlazorBootstrap.ModalSize.Small => "modal-sm",
        BlazorBootstrap.ModalSize.Large => "modal-lg",
        BlazorBootstrap.ModalSize.ExtraLarge => "modal-xl",
        _ => null,
    };

    public string ToModalTypeColor(ModalType modalType) => modalType switch
    {
        BlazorBootstrap.ModalType.Primary => "primary",
        BlazorBootstrap.ModalType.Secondary => "secondary",
        BlazorBootstrap.ModalType.Success => "success",
        BlazorBootstrap.ModalType.Danger => "danger",
        BlazorBootstrap.ModalType.Warning => "warning",
        BlazorBootstrap.ModalType.Info => "info",
        BlazorBootstrap.ModalType.Light => "light",
        BlazorBootstrap.ModalType.Dark => "dark",
        _ => null,
    };

    public string ToModalHeaderBottomBorderColor(ModalType modalType) => modalType switch
    {
        BlazorBootstrap.ModalType.Primary => "border-primary",
        BlazorBootstrap.ModalType.Secondary => "border-secondary",
        BlazorBootstrap.ModalType.Success => "border-success",
        BlazorBootstrap.ModalType.Danger => "border-danger",
        BlazorBootstrap.ModalType.Warning => "border-warning",
        BlazorBootstrap.ModalType.Info => "border-info",
        BlazorBootstrap.ModalType.Light => null,
        BlazorBootstrap.ModalType.Dark => "border-dark",
        _ => null,
    };

    public string ToOffcanvasSize(OffcanvasSize size) => size switch
    {
        BlazorBootstrap.OffcanvasSize.Regular => null,
        BlazorBootstrap.OffcanvasSize.Small => "bb-offcanvas-sm",
        BlazorBootstrap.OffcanvasSize.Large => "bb-offcanvas-lg",
        _ => null,
    };

    public string ToPlaceholderAnimation(PlaceholderAnimation animation) => animation switch
    {
        BlazorBootstrap.PlaceholderAnimation.Glow => "glow",
        BlazorBootstrap.PlaceholderAnimation.Wave => "wave",
        _ => null,
    };

    public string ToPlaceholderWidth(PlaceholderWidth width) => width switch
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

    public string ToPlaceholderColor(PlaceholderColor color) => color switch
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

    public string ToPlaceholderSize(PlaceholderSize size) => size switch
    {
        BlazorBootstrap.PlaceholderSize.ExtraSmall => "xs",
        BlazorBootstrap.PlaceholderSize.Small => "sm",
        BlazorBootstrap.PlaceholderSize.Large => "lg",
        _ => null,
    };

    public string ToPosition(Position position) => position switch
    {
        BlazorBootstrap.Position.Static => PositionAbsolute(),
        BlazorBootstrap.Position.Relative => PositionRelative(),
        BlazorBootstrap.Position.Absolute => PositionAbsolute(),
        BlazorBootstrap.Position.Fixed => PositionFixed(),
        BlazorBootstrap.Position.Sticky => PositionSticky(),
        _ => "",
    };

    public string ToTabColor(TabColor color) => color switch
    {
        BlazorBootstrap.TabColor.Primary => "bg-primary text-white",
        BlazorBootstrap.TabColor.Secondary => "bg-secondary text-white",
        BlazorBootstrap.TabColor.Success => "bg-success text-white",
        BlazorBootstrap.TabColor.Danger => "bg-danger text-white",
        BlazorBootstrap.TabColor.Warning => "bg-warning text-dark",
        BlazorBootstrap.TabColor.Info => "bg-info text-dark",
        BlazorBootstrap.TabColor.Light => "bg-light text-dark",
        BlazorBootstrap.TabColor.Dark => "bg-dark text-white",
        _ => null,
    };

    #endregion
}
