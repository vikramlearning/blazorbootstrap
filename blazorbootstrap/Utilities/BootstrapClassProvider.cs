namespace BlazorBootstrap;

public class BootstrapClassProvider
{
    #region Methods

    public string Accordion() => "accordion";
    public string AccordionButton() => $"{Accordion()}-button";
    public string AccordionFlush() => $"{Accordion()}-flush";
    public string AccordionItem() => $"{Accordion()}-item";
    public string AccordionItemBody() => $"{Accordion()}-body";
    public string AccordionItemHeader() => $"{Accordion()}-header";

    public string Active() => "active";

    public string Alert() => "alert";

    public string AlertColor(AlertColor color) => $"{Alert()}-{ToAlertColor(color)}";

    public string? AlertDescription() => null;

    public string AlertDismisable() => "alert-dismissible";

    public string AlertFade() => Fade();

    public string? AlertHasDescription() => null;

    public string? AlertHasMessage() => null;

    public string? AlertMessage() => null;

    public string AlertShow() => Show();

    public string BackgroundColor(BackgroundColor backgroundColor) => $"bg-{ToBackgroundColor(backgroundColor)}";

    public string Badge() => "badge";

    public string BadgeColor(BadgeColor color) => $"text-bg-{ToBadgeColor(color)}";

    public string Button() => "btn";

    public string ButtonActive() => "active";

    public string ButtonBlock() => $"{Button()}-block";

    public string ButtonColor(ButtonColor color) => $"{Button()}-{ToButtonColor(color)}";

    public string ButtonDisabled() => "disabled";

    public string ButtonGroup() => $"{Button()}-group";

    public string? ButtonLoading() => null;

    public string ButtonOutline(ButtonColor color) => color != BlazorBootstrap.ButtonColor.None ? $"{Button()}-outline-{ToButtonColor(color)}" : $"{Button()}-outline";

    public string ButtonSize(Size size) => $"{Button()}-{ToSize(size)}";

    public string Callout() => "bb-callout";

    public string CalloutHeading() => $"{Callout()}-heading";

    public string Card() => "card";
    public string CardBody() => $"{Card()}-body";
    public string CardFooter() => $"{Card()}-footer";
    public string CardGroup() => $"{Card()}-group";
    public string CardHeader() => $"{Card()}-header";
    public string CardLink() => $"{Card()}-link";
    public string CardSubTitle() => $"{Card()}-subtitle";
    public string CardText() => $"{Card()}-text";
    public string CardTitle() => $"{Card()}-title";

    public string Checks() => "form-check-input";
    public string ChecksReverse() => "form-check-reverse";

    public string Collapse() => "collapse";

    public string Collapsed() => "collapsed";
    public string CollapseHorizontal() => $"{Collapse()}-horizontal";

    public string ConfirmationModal() => "modal-confirmation";

    public string Disabled() => "disabled";

    public string DisplayHeadingSize(DisplayHeadingSize displayHeadingSize) => $"display-{ToDisplayHeadingSize(displayHeadingSize)}";

    public string Divider() => "divider";

    public string DividerType(DividerType dividerType) => $"{Divider()}-{ToDividerType(dividerType)}";

    public string Dropdown() => "dropdown";

    public string DropdownDirection(DropdownDirection direction) => ToDropdownDirection(direction);
    public string DropdownDivider() => $"{Dropdown()}-divider";
    public string DropdownHeader() => $"{Dropdown()}-header";
    public string DropdownItem() => $"{Dropdown()}-item";
    public string DropdownMenu() => $"{Dropdown()}-menu";
    public string DropdownMenuPosition(DropdownMenuPosition position) => ToDropdownMenuPosition(position);
    public string DropdownToggle() => $"{Dropdown()}-toggle";
    public string DropdownToggleSplit() => $"{DropdownToggle()}-split";

    public string Fade() => "fade";

    public string FlexAlignment(Alignment alignment) => $"justify-content-{ToAlignment(alignment)}";

    public string FormControl() => "form-control";

    public string HeadingSize(HeadingSize headingSize) => $"h{ToHeadingSize(headingSize)}";

    public string IconColor(IconColor iconColor) => $"text-{ToIconColor(iconColor)}";

    public string IsInValid() => "invalid";

    public string IsValid() => "valid";

    public string Modal() => "modal";

    public string ModalFade() => Fade();

    public string ModalHeader(ModalType modalType) => $"text-bg-{ToModalTypeColor(modalType)} border-bottom {ToModalHeaderBottomBorderColor(modalType)}";

    public string Offcanvas() => "offcanvas";

    public string Offcanvas(Placement placement) => $"{Offcanvas()}-{ToPlacement(placement)}";

    public string PageLoadingModal() => "modal-page-loading";

    public string Pagination() => "pagination";

    public string PaginationItem() => "page-item";

    public string PaginationItemActive() => Active();

    public string PaginationItemDisabled() => Disabled();

    public string PaginationLink() => "page-link";

    public string? PaginationLinkActive() => null;

    public string? PaginationLinkDisabled() => null;

    public string PaginationSize(PaginationSize size) => $"{Pagination()}-{ToPaginationSize(size)}";

    public string PillBadge() => "rounded-pill";

    public string Placeholder() => "placeholder";
    public string PlaceholderAnimation(PlaceholderAnimation animation) => $"{Placeholder()}-{ToPlaceholderAnimation(animation)}";
    public string PlaceholderColor(PlaceholderColor color) => $"bg-{ToPlaceholderColor(color)}";
    public string PlaceholderSize(PlaceholderSize size) => $"{Placeholder()}-{ToPlaceholderSize(size)}";
    public string PlaceholderWidth(PlaceholderWidth width) => $"{ToPlaceholderWidth(width)}";

    public string Position() => "position";
    public string PositionAbsolute() => $"{Position()}-absolute";
    public string PositionFixed() => $"{Position()}-fixed";
    public string PositionRelative() => $"{Position()}-relative";
    public string PositionStatic() => $"{Position()}-static";
    public string PositionSticky() => $"{Position()}-sticky";

    public string Progress() => "progress";
    public string ProgressBackgroundColor(ProgressColor color) => $"bg-{ToProgressColor(color)}";
    public string ProgressBar() => $"{Progress()}-bar";
    public string ProgressBarAnimated() => $"{ProgressBar()}-animated";
    public string ProgressBarStriped() => $"{ProgressBar()}-striped";

    public string Show() => "show";

    public string Table() => "table";
    public string TableActive() => "table-active";
    public string TableBordered() => "table-bordered";
    public string TableBorderless() => "table-borderless";
    public string TableDanger() => "table-danger";
    public string TableDark() => "table-dark";
    public string TableGroupDivider() => "table-group-divider";
    public string TableHover() => "table-hover";
    public string TableInfo() => "table-info";
    public string TableLight() => "table-light";
    public string TablePrimary() => "table-primary";
    public string TableResponsive() => "table-responsive";
    public string TableSecondary() => "table-secondary";
    public string TableSticky() => "bb-table-sticky";
    public string TableStriped() => "table-striped";
    public string TableSmall() => "table-sm";
    public string TableStripedColumns() => "table-striped-columns";
    public string TableSuccess() => "table-success";
    public string TableWarning() => "table-warning";

    public string TextAlignment(Alignment alignment) => $"text-{ToAlignment(alignment)}";

    public string TextAndBackgroundColor(BackgroundColor backgroundColor) => $"text-bg-{ToBackgroundColor(backgroundColor)}";

    public string TextColor(TextColor textColor) => $"text-{ToTextColor(textColor)}";

    public string TextNoWrap() => "text-nowrap";

    public string ToAlertColor(AlertColor color) =>
        color switch
        {
            BlazorBootstrap.AlertColor.Primary => "primary",
            BlazorBootstrap.AlertColor.Secondary => "secondary",
            BlazorBootstrap.AlertColor.Success => "success",
            BlazorBootstrap.AlertColor.Danger => "danger",
            BlazorBootstrap.AlertColor.Warning => "warning",
            BlazorBootstrap.AlertColor.Info => "info",
            BlazorBootstrap.AlertColor.Light => "light",
            BlazorBootstrap.AlertColor.Dark => "dark",
            _ => null
        };

    public string ToAlignment(Alignment alignment) =>
        alignment switch
        {
            Alignment.Start => "start",
            Alignment.Center => "center",
            Alignment.End => "end",
            _ => null
        };

    public string Toast() => "toast";
    public string ToastContainer() => $"{Toast()}-container";

    public string ToAutoCompleteSize(AutoCompleteSize size) =>
        size switch
        {
            AutoCompleteSize.Large => "form-control-lg",
            AutoCompleteSize.Small => "form-control-sm",
            _ => ""
        };

    public string ToBackgroundColor(BackgroundColor color) =>
        color switch
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
            _ => null
        };

    public string ToBadgeColor(BadgeColor color) =>
        color switch
        {
            BlazorBootstrap.BadgeColor.Primary => "primary",
            BlazorBootstrap.BadgeColor.Secondary => "secondary",
            BlazorBootstrap.BadgeColor.Success => "success",
            BlazorBootstrap.BadgeColor.Danger => "danger",
            BlazorBootstrap.BadgeColor.Warning => "warning",
            BlazorBootstrap.BadgeColor.Info => "info",
            BlazorBootstrap.BadgeColor.Light => "light",
            BlazorBootstrap.BadgeColor.Dark => "dark",
            _ => null
        };

    public string ToBadgeIndicator(BadgeIndicatorType indicatorType) =>
        indicatorType switch
        {
            BadgeIndicatorType.RoundedPill => "rounded-pill",
            BadgeIndicatorType.RoundedCircle => "rounded-circle",
            _ => "" // default: Top right
        };

    public string ToBadgePlacement(BadgePlacement badgePlacement) =>
        badgePlacement switch
        {
            BadgePlacement.TopLeft => "top-0 start-0 translate-middle",
            BadgePlacement.TopCenter => "top-0 start-50 translate-middle",
            BadgePlacement.TopRight => "top-0 start-100 translate-middle",
            BadgePlacement.MiddleLeft => "top-50 start-0 translate-middle",
            BadgePlacement.MiddleCenter => "top-50 start-50 translate-middle",
            BadgePlacement.MiddleRight => "top-50 start-100 translate-middle",
            BadgePlacement.BottomLeft => "top-100 start-0 translate-middle",
            BadgePlacement.BottomCenter => "top-100 start-50 translate-middle",
            BadgePlacement.BottomRight => "top-100 start-100 translate-middle",
            _ => "top-0 start-100 translate-middle" // default: Top right
        };

    public string ToButtonColor(ButtonColor color) =>
        color switch
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
            _ => null
        };

    public string ToCalloutType(CalloutType type) =>
        type switch
        {
            CalloutType.Default => "",
            CalloutType.Danger => $"{Callout()}-danger",
            CalloutType.Warning => $"{Callout()}-warning",
            CalloutType.Info => $"{Callout()}-info",
            CalloutType.Tip => $"{Callout()}-success",
            _ => ""
        };

    public string ToCardColor(CardColor color) =>
        color switch
        {
            CardColor.Primary => "text-bg-primary",
            CardColor.Secondary => "text-bg-secondary",
            CardColor.Success => "text-bg-success",
            CardColor.Danger => "text-bg-danger",
            CardColor.Warning => "text-bg-warning",
            CardColor.Info => "text-bg-info",
            CardColor.Light => "text-bg-light",
            CardColor.Dark => "text-bg-dark",
            _ => ""
        };

    public string ToColor(TextColor color) =>
        color switch
        {
            BlazorBootstrap.TextColor.Primary => "primary",
            BlazorBootstrap.TextColor.Secondary => "secondary",
            BlazorBootstrap.TextColor.Success => "success",
            BlazorBootstrap.TextColor.Danger => "danger",
            BlazorBootstrap.TextColor.Warning => "warning",
            BlazorBootstrap.TextColor.Info => "info",
            BlazorBootstrap.TextColor.Light => "light",
            BlazorBootstrap.TextColor.Dark => "dark",
            _ => null
        };

    public string ToDialogSize(DialogSize size) =>
        size switch
        {
            DialogSize.Regular => null,
            DialogSize.Small => "modal-sm",
            DialogSize.Large => "modal-lg",
            DialogSize.ExtraLarge => "modal-xl",
            _ => null
        };

    public string ToDisplayHeadingSize(DisplayHeadingSize displayHeadingSize) =>
        displayHeadingSize switch
        {
            BlazorBootstrap.DisplayHeadingSize.H1 => "1",
            BlazorBootstrap.DisplayHeadingSize.H2 => "2",
            BlazorBootstrap.DisplayHeadingSize.H3 => "3",
            BlazorBootstrap.DisplayHeadingSize.H4 => "4",
            _ => null
        };

    public string ToDividerType(DividerType dividerType) =>
        dividerType switch
        {
            BlazorBootstrap.DividerType.Dashed => "dashed",
            BlazorBootstrap.DividerType.Dotted => "dotted",
            BlazorBootstrap.DividerType.TextContent => "text",
            _ => "solid"
        };

    public string ToDropdownDirection(DropdownDirection direction) =>
        direction switch
        {
            BlazorBootstrap.DropdownDirection.Dropdown => "dropdown",
            BlazorBootstrap.DropdownDirection.DropdownCentered => "dropdown dropdown-center",
            BlazorBootstrap.DropdownDirection.Dropend => "dropend",
            BlazorBootstrap.DropdownDirection.Dropup => "dropup",
            BlazorBootstrap.DropdownDirection.DropupCentered => "dropup dropup-center",
            BlazorBootstrap.DropdownDirection.Dropstart => "dropstart",
            _ => ""
        };

    public string ToDropdownMenuPosition(DropdownMenuPosition position) =>
        position switch
        {
            BlazorBootstrap.DropdownMenuPosition.Start => "dropdown-menu-start",
            BlazorBootstrap.DropdownMenuPosition.End => "dropdown-menu-end",
            _ => ""
        };

    public string ToHeadingSize(HeadingSize headingSize) =>
        headingSize switch
        {
            BlazorBootstrap.HeadingSize.H1 => "1",
            BlazorBootstrap.HeadingSize.H2 => "2",
            BlazorBootstrap.HeadingSize.H3 => "3",
            BlazorBootstrap.HeadingSize.H4 => "4",
            BlazorBootstrap.HeadingSize.H5 => "5",
            BlazorBootstrap.HeadingSize.H6 => "6",
            _ => null
        };

    public string ToIconColor(IconColor color) =>
        color switch
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
            _ => null
        };

    public string ToModalFullscreen(ModalFullscreen fullscreen) =>
        fullscreen switch
        {
            ModalFullscreen.Disabled => null,
            ModalFullscreen.Always => "modal-fullscreen",
            ModalFullscreen.SmallDown => "modal-fullscreen-sm-down",
            ModalFullscreen.MediumDown => "modal-fullscreen-md-down",
            ModalFullscreen.LargeDown => "modal-fullscreen-lg-down",
            ModalFullscreen.ExtraLargeDown => "modal-fullscreen-xl-down",
            ModalFullscreen.ExtraExtraLargeDown => "modal-fullscreen-xxl-down",
            _ => null
        };

    public string ToModalHeaderBottomBorderColor(ModalType modalType) =>
        modalType switch
        {
            ModalType.Primary => "border-primary",
            ModalType.Secondary => "border-secondary",
            ModalType.Success => "border-success",
            ModalType.Danger => "border-danger",
            ModalType.Warning => "border-warning",
            ModalType.Info => "border-info",
            ModalType.Light => null,
            ModalType.Dark => "border-dark",
            _ => null
        };

    public string ToModalSize(ModalSize size) =>
        size switch
        {
            ModalSize.Regular => null,
            ModalSize.Small => "modal-sm",
            ModalSize.Large => "modal-lg",
            ModalSize.ExtraLarge => "modal-xl",
            _ => null
        };

    public string ToModalTypeColor(ModalType modalType) =>
        modalType switch
        {
            ModalType.Primary => "primary",
            ModalType.Secondary => "secondary",
            ModalType.Success => "success",
            ModalType.Danger => "danger",
            ModalType.Warning => "warning",
            ModalType.Info => "info",
            ModalType.Light => "light",
            ModalType.Dark => "dark",
            _ => null
        };

    public string ToOffcanvasSize(OffcanvasSize size) =>
        size switch
        {
            OffcanvasSize.Regular => null,
            OffcanvasSize.Small => "bb-offcanvas-sm",
            OffcanvasSize.Large => "bb-offcanvas-lg",
            _ => null
        };

    public string Tooltip() => "b-tooltip";

    public string TooltipAlwaysActive() => "b-tooltip-active";

    public string TooltipColor(TooltipColor color) => ToTooltipColor(color);

    public string TooltipFade() => "b-tooltip-fade";

    public string TooltipInline() => "b-tooltip-inline";

    public string TooltipMultiline() => "b-tooltip-multiline";

    public string TooltipPlacement(TooltipPlacement tooltipPlacement) => $"{Tooltip()}-{ToTooltipPlacement(tooltipPlacement)}";

    public string ToPaginationSize(PaginationSize size) =>
        size switch
        {
            BlazorBootstrap.PaginationSize.Small => "sm",
            BlazorBootstrap.PaginationSize.Large => "lg",
            _ => null
        };

    public string ToPlaceholderAnimation(PlaceholderAnimation animation) =>
        animation switch
        {
            BlazorBootstrap.PlaceholderAnimation.Glow => "glow",
            BlazorBootstrap.PlaceholderAnimation.Wave => "wave",
            _ => null
        };

    public string ToPlaceholderColor(PlaceholderColor color) =>
        color switch
        {
            BlazorBootstrap.PlaceholderColor.Primary => "primary",
            BlazorBootstrap.PlaceholderColor.Secondary => "secondary",
            BlazorBootstrap.PlaceholderColor.Success => "success",
            BlazorBootstrap.PlaceholderColor.Danger => "danger",
            BlazorBootstrap.PlaceholderColor.Warning => "warning",
            BlazorBootstrap.PlaceholderColor.Info => "info",
            BlazorBootstrap.PlaceholderColor.Light => "light",
            BlazorBootstrap.PlaceholderColor.Dark => "dark",
            _ => null
        };

    public string ToPlaceholderSize(PlaceholderSize size) =>
        size switch
        {
            BlazorBootstrap.PlaceholderSize.ExtraSmall => "xs",
            BlazorBootstrap.PlaceholderSize.Small => "sm",
            BlazorBootstrap.PlaceholderSize.Large => "lg",
            _ => null
        };

    public string ToPlaceholderWidth(PlaceholderWidth width) =>
        width switch
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
            _ => null
        };

    public string ToPlacement(Placement placement) =>
        placement switch
        {
            Placement.Start => "start",
            Placement.End => "end",
            Placement.Top => "top",
            _ => "bottom"
        };

    public string ToPosition(Position position) =>
        position switch
        {
            BlazorBootstrap.Position.Static => PositionAbsolute(),
            BlazorBootstrap.Position.Relative => PositionRelative(),
            BlazorBootstrap.Position.Absolute => PositionAbsolute(),
            BlazorBootstrap.Position.Fixed => PositionFixed(),
            BlazorBootstrap.Position.Sticky => PositionSticky(),
            _ => ""
        };

    public string ToProgressColor(ProgressColor color) =>
        color switch
        {
            ProgressColor.Primary => "primary",
            ProgressColor.Secondary => "secondary",
            ProgressColor.Success => "success",
            ProgressColor.Danger => "danger",
            ProgressColor.Warning => "warning",
            ProgressColor.Info => "info",
            ProgressColor.Dark => "dark",
            _ => null
        };

    public string ToScreenreader(Screenreader screenreader) =>
        screenreader switch
        {
            Screenreader.Only => "sr-only",
            Screenreader.OnlyFocusable => "sr-only-focusable",
            _ => null
        };

    public string ToSize(Size size) =>
        size switch
        {
            Size.ExtraSmall => "xs",
            Size.Small => "sm",
            Size.Medium => "md",
            Size.Large => "lg",
            Size.ExtraLarge => "xl",
            _ => null
        };

    public string ToTabColor(TabColor color) =>
        color switch
        {
            TabColor.Primary => "bg-primary text-white",
            TabColor.Secondary => "bg-secondary text-white",
            TabColor.Success => "bg-success text-white",
            TabColor.Danger => "bg-danger text-white",
            TabColor.Warning => "bg-warning text-dark",
            TabColor.Info => "bg-info text-dark",
            TabColor.Light => "bg-light text-dark",
            TabColor.Dark => "bg-dark text-white",
            _ => null
        };

    public string ToTextAlignment(Alignment alignment) =>
        alignment switch
        {
            Alignment.Start or Alignment.None => "text-start",
            Alignment.Center => "text-center",
            Alignment.End => "text-end",
            _ => ""
        };

    public string ToTextColor(TextColor color) =>
        color switch
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
            _ => null
        };

    public string ToToastBackgroundColor(ToastType toastType) =>
        toastType switch
        {
            ToastType.Primary => "primary",
            ToastType.Secondary => "secondary",
            ToastType.Success => "success",
            ToastType.Danger => "danger",
            ToastType.Warning => "warning",
            ToastType.Info => "info",
            ToastType.Light => "light",
            ToastType.Dark => "dark",
            _ => null
        };

    public string ToToastsPlacement(ToastsPlacement toastsPlacement) =>
        toastsPlacement switch
        {
            ToastsPlacement.TopLeft => "top-0 start-0",
            ToastsPlacement.TopCenter => "top-0 start-50 translate-middle-x",
            ToastsPlacement.TopRight => "top-0 end-0",
            ToastsPlacement.MiddleLeft => "top-50 start-0 translate-middle-y",
            ToastsPlacement.MiddleCenter => "top-50 start-50 translate-middle",
            ToastsPlacement.MiddleRight => "top-50 end-0 translate-middle-y",
            ToastsPlacement.BottomLeft => "bottom-0 start-0",
            ToastsPlacement.BottomCenter => "bottom-0 start-50 translate-middle-x",
            ToastsPlacement.BottomRight => "bottom-0 end-0",
            _ => "top-0 end-0" // default: Top right
        };

    public string ToToastTextColor(ToastType toastType) =>
        toastType switch
        {
            ToastType.Primary
                or ToastType.Secondary
                or ToastType.Success
                or ToastType.Danger
                or ToastType.Dark => "white",
            ToastType.Warning
                or ToastType.Info
                or ToastType.Light => "dark",
            _ => null
        };

    public string ToTooltipColor(TooltipColor color) =>
        color switch
        {
            BlazorBootstrap.TooltipColor.Primary => "bb-tooltip-primary",
            BlazorBootstrap.TooltipColor.Secondary => "bb-tooltip-tooltip-secondary",
            BlazorBootstrap.TooltipColor.Success => "bb-tooltip-success",
            BlazorBootstrap.TooltipColor.Danger => "bb-tooltip-danger",
            BlazorBootstrap.TooltipColor.Warning => "bb-tooltip-warning",
            BlazorBootstrap.TooltipColor.Info => "bb-tooltip-info",
            BlazorBootstrap.TooltipColor.Light => "bb-tooltip-light",
            BlazorBootstrap.TooltipColor.Dark => "bb-tooltip-dark",
            _ => null
        };

    public string ToTooltipPlacement(TooltipPlacement tooltipPlacement) =>
        tooltipPlacement switch
        {
            BlazorBootstrap.TooltipPlacement.Bottom => "bottom",
            BlazorBootstrap.TooltipPlacement.Left => "left",
            BlazorBootstrap.TooltipPlacement.Right => "right",
            _ => "top"
        };

    #endregion

    #region Properties, Indexers

    public string Nav => "nav";
    public string NavPills => $"{Nav}-pills";
    public string NavTabs => $"{Nav}-tabs";

    #endregion
}
