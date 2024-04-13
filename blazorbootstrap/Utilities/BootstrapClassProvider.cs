namespace BlazorBootstrap;

public static class BootstrapClassProvider
{
    #region Methods

    public static string Accordion { get; } = "accordion";
    public static string AccordionFlush => $"{Accordion}-flush";
    public static string AccordionItem => $"{Accordion}-item";

    public static string Active { get; } = "active";

    public static string AlignItemsBaseline { get; } = "align-items-baseline";
    public static string AlignItemsCenter { get; } = "align-items-center";
    public static string AlignItemsEnd { get; } = "align-items-end";
    public static string AlignItemsStart { get; } = "align-items-start";
    public static string AlignItemsStretch { get; } = "align-items-stretch";

    public static string Alert { get; } = "alert";
    public static string AlertColor(AlertColor color) => $"{Alert}-{ToAlertColor(color)}";
    public static string AlertDismisable { get; } = "alert-dismissible";
    
    public static string BackgroundColor(BackgroundColor backgroundColor) => $"bg-{ToBackgroundColor(backgroundColor)}";

    public static string Badge { get; } = "badge";
    public static string BadgeColor(BadgeColor color) => $"text-bg-{ToBadgeColor(color)}";

    public static string Border { get; } = "border";
    public static string BorderEnd { get; } = "border-end";
    public static string BorderStart { get; } = "border-start";

    public static string Button { get; } = "btn";
    public static string ButtonActive { get; } = "active";
    public static string ButtonBlock { get; } = $"{Button}-block";
    public static string ButtonColor(ButtonColor color) => $"{Button}-{ToButtonColor(color)}";
    public static string ButtonDisabled { get; } = "disabled";
    public static string ButtonGroup { get; } = $"{Button}-group";
    public static string? ButtonLoading { get; } = null;
    public static string ButtonOutline(ButtonColor color) => color != BlazorBootstrap.ButtonColor.None ? $"{Button}-outline-{ToButtonColor(color)}" : $"{Button}-outline";
    public static string ButtonSize(Size size) => $"{Button}-{ToSize(size)}";

    public static string Callout { get; } = "bb-callout";
    public static string CalloutHeading { get; } = $"{Callout}-heading";

    public static string Card { get; } = "card";
    public static string CardBody { get; } = $"{Card}-body";
    public static string CardFooter { get; } = $"{Card}-footer";
    public static string CardGroup { get; } = $"{Card}-group";
    public static string CardHeader { get; } = $"{Card}-header";
    public static string CardLink { get; } = $"{Card}-link";
    public static string CardSubTitle { get; } = $"{Card}-subtitle";
    public static string CardText { get; } = $"{Card}-text";
    public static string CardTitle { get; } = $"{Card}-title";

    public static string ChecksReverse { get; } = "form-check-reverse";

    public static string Collapse { get; } = "collapse";
    public static string CollapseHorizontal { get; } = $"{Collapse}-horizontal";

    public static string ConfirmationModal { get; } = "modal-confirmation";

    public static string Disabled { get; } = "disabled";

    public static string Dropdown { get; } = "dropdown";
    public static string DropdownDirection(DropdownDirection direction) => ToDropdownDirection(direction);
    public static string DropdownDivider { get; } = $"{Dropdown}-divider";
    public static string DropdownHeader { get; } = $"{Dropdown}-header";
    public static string DropdownItem { get; } = $"{Dropdown}-item";
    public static string DropdownMenu { get; } = $"{Dropdown}-menu";
    public static string DropdownMenuPosition(DropdownMenuPosition position) => ToDropdownMenuPosition(position);
    public static string DropdownToggle { get; } = $"{Dropdown}-toggle";
    public static string DropdownToggleSplit { get; } = $"{DropdownToggle}-split";

    public static string Fade { get; } = "fade";

    public static string FlexAlignment(Alignment alignment) => $"justify-content-{ToAlignment(alignment)}";

    public static string Flex { get; } = "d-flex";
    public static string FlexColumn { get; } = "flex-column";
    public static string FlexInline { get; } = "d-inline-flex";
    public static string FlexRow { get; } = "flex-row";

    public static string FormControl { get; } = "form-control";

    public static string FormRange { get; } = "form-range";

    public static string HeadingSize(HeadingSize headingSize) => $"h{ToHeadingSize(headingSize)}";

    public static string IconColor(IconColor iconColor) => $"text-{ToIconColor(iconColor)}";

    public static string Modal { get; } = "modal";
    public static string ModalFade { get; } = Fade;
    public static string ModalHeader(ModalType modalType) => $"text-bg-{ToModalTypeColor(modalType)} border-bottom {ToModalHeaderBottomBorderColor(modalType)}";

    public static string Nav { get; } = "nav";
    public static string NavPills { get; } = $"{Nav}-pills";
    public static string NavTabs { get; } = $"{Nav}-tabs";
    public static string NavUnderline { get; } = $"{Nav}-underline";

    public static string Offcanvas { get; } = "offcanvas";
    public static string OffcanvasPlacement(Placement placement) => $"{Offcanvas}-{ToPlacement(placement)}";

    public static string PageLoadingModal { get; } = "modal-page-loading";

    public static string Pagination { get; } = "pagination";
    public static string PaginationItem { get; } = "page-item";
    public static string PaginationItemActive { get; } = Active;
    public static string PaginationItemDisabled { get; } = Disabled;
    public static string PaginationLink { get; } = "page-link";
    public static string PaginationSize(PaginationSize size) => $"{Pagination}-{ToPaginationSize(size)}";

    public static string Placeholder { get; } = "placeholder";
    public static string PlaceholderAnimation(PlaceholderAnimation animation) => $"{Placeholder}-{ToPlaceholderAnimation(animation)}";
    public static string PlaceholderColor(PlaceholderColor color) => $"bg-{ToPlaceholderColor(color)}";
    public static string PlaceholderSize(PlaceholderSize size) => $"{Placeholder}-{ToPlaceholderSize(size)}";
    public static string PlaceholderWidth(PlaceholderWidth width) => $"{ToPlaceholderWidth(width)}";

    public static string Position { get; } = "position";
    public static string PositionAbsolute { get; } = $"{Position}-absolute";
    public static string PositionFixed { get; } = $"{Position}-fixed";
    public static string PositionRelative { get; } = $"{Position}-relative";
    public static string PositionSticky { get; } = $"{Position}-sticky";

    public static string Progress { get; } = "progress";
    public static string ProgressBackgroundColor(ProgressColor color) => $"bg-{ToProgressColor(color)}";
    public static string ProgressBar { get; } = $"{Progress}-bar";
    public static string ProgressBarAnimated { get; } = $"{ProgressBar}-animated";
    public static string ProgressBarStriped { get; } = $"{ProgressBar}-striped";

    public static string Show { get; } = "show";
    
    public static string Spinner { get; } = "spinner";
    public static string SpinnerColor(SpinnerColor color) => ToSpinnerColor(color)!;
    public static string SpinnerType(SpinnerType type) => $"{Spinner}-{ToSpinnerType(type)}";
    public static string SpinnerTypeSize(SpinnerType type, SpinnerSize size) => $"{SpinnerType(type)}-{ToSpinnerSize(size)}";
    
    public static string TableSticky { get; } = "bb-table-sticky";

    public static string TextAlignment(Alignment alignment) => $"text-{ToAlignment(alignment)}";
    
    public static string TextColor(TextColor textColor) => $"text-{ToTextColor(textColor)}";

    public static string TextNoWrap { get; } = "text-nowrap";

    public static string? ToAlertColor(AlertColor color) =>
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

    public static string? ToAlignment(Alignment alignment) =>
        alignment switch
        {
            BlazorBootstrap.Alignment.Start => "start",
            BlazorBootstrap.Alignment.Center => "center",
            BlazorBootstrap.Alignment.End => "end",
            _ => null
        };

    public static string Toast { get; } = "toast";
    public static string ToastContainer { get; } = $"{Toast}-container";

    public static string ToAutoCompleteSize(AutoCompleteSize size) =>
        size switch
        {
            BlazorBootstrap.AutoCompleteSize.Large => "form-control-lg",
            BlazorBootstrap.AutoCompleteSize.Small => "form-control-sm",
            _ => ""
        };

    public static string? ToBackgroundColor(BackgroundColor color) =>
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

    public static string? ToBadgeColor(BadgeColor color) =>
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

    public static string ToBadgeIndicator(BadgeIndicatorType indicatorType) =>
        indicatorType switch
        {
            BlazorBootstrap.BadgeIndicatorType.RoundedPill => "rounded-pill",
            BlazorBootstrap.BadgeIndicatorType.RoundedCircle => "rounded-circle",
            _ => "" // default: Top right
        };

    public static string ToBadgePlacement(BadgePlacement badgePlacement) =>
        badgePlacement switch
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
            _ => "top-0 start-100 translate-middle" // default: Top right
        };

    public static string? ToButtonColor(ButtonColor color) =>
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

    public static string ToCalloutType(CalloutType type) =>
        type switch
        {
            BlazorBootstrap.CalloutType.Default => "",
            BlazorBootstrap.CalloutType.Danger => $"{Callout}-danger",
            BlazorBootstrap.CalloutType.Warning => $"{Callout}-warning",
            BlazorBootstrap.CalloutType.Info => $"{Callout}-info",
            BlazorBootstrap.CalloutType.Tip or BlazorBootstrap.CalloutType.Success => $"{Callout}-success",
            _ => ""
        };

    public static string ToCardColor(CardColor color) =>
        color switch
        {
            BlazorBootstrap.CardColor.Primary => "text-bg-primary",
            BlazorBootstrap.CardColor.Secondary => "text-bg-secondary",
            BlazorBootstrap.CardColor.Success => "text-bg-success",
            BlazorBootstrap.CardColor.Danger => "text-bg-danger",
            BlazorBootstrap.CardColor.Warning => "text-bg-warning",
            BlazorBootstrap.CardColor.Info => "text-bg-info",
            BlazorBootstrap.CardColor.Light => "text-bg-light",
            BlazorBootstrap.CardColor.Dark => "text-bg-dark",
            _ => ""
        };

    public static string? ToDialogSize(DialogSize size) =>
        size switch
        {
            BlazorBootstrap.DialogSize.Regular => null,
            BlazorBootstrap.DialogSize.Small => "modal-sm",
            BlazorBootstrap.DialogSize.Large => "modal-lg",
            BlazorBootstrap.DialogSize.ExtraLarge => "modal-xl",
            _ => null
        };

    public static string ToDropdownDirection(DropdownDirection direction) =>
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

    public static string ToDropdownMenuPosition(DropdownMenuPosition position) =>
        position switch
        {
            BlazorBootstrap.DropdownMenuPosition.Start => "dropdown-menu-start",
            BlazorBootstrap.DropdownMenuPosition.End => "dropdown-menu-end",
            _ => ""
        };

    public static string? ToHeadingSize(HeadingSize headingSize) =>
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

    public static string? ToIconColor(IconColor color) =>
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

    public static string? ToModalFullscreen(ModalFullscreen fullscreen) =>
        fullscreen switch
        {
            BlazorBootstrap.ModalFullscreen.Disabled => null,
            BlazorBootstrap.ModalFullscreen.Always => "modal-fullscreen",
            BlazorBootstrap.ModalFullscreen.SmallDown => "modal-fullscreen-sm-down",
            BlazorBootstrap.ModalFullscreen.MediumDown => "modal-fullscreen-md-down",
            BlazorBootstrap.ModalFullscreen.LargeDown => "modal-fullscreen-lg-down",
            BlazorBootstrap.ModalFullscreen.ExtraLargeDown => "modal-fullscreen-xl-down",
            BlazorBootstrap.ModalFullscreen.ExtraExtraLargeDown => "modal-fullscreen-xxl-down",
            _ => null
        };

    public static string? ToModalHeaderBottomBorderColor(ModalType modalType) =>
        modalType switch
        {
            BlazorBootstrap.ModalType.Primary => "border-primary",
            BlazorBootstrap.ModalType.Secondary => "border-secondary",
            BlazorBootstrap.ModalType.Success => "border-success",
            BlazorBootstrap.ModalType.Danger => "border-danger",
            BlazorBootstrap.ModalType.Warning => "border-warning",
            BlazorBootstrap.ModalType.Info => "border-info",
            BlazorBootstrap.ModalType.Light => null,
            BlazorBootstrap.ModalType.Dark => "border-dark",
            _ => null
        };

    public static string? ToModalSize(ModalSize size) =>
        size switch
        {
            BlazorBootstrap.ModalSize.Regular => null,
            BlazorBootstrap.ModalSize.Small => "modal-sm",
            BlazorBootstrap.ModalSize.Large => "modal-lg",
            BlazorBootstrap.ModalSize.ExtraLarge => "modal-xl",
            _ => null
        };

    public static string? ToModalTypeColor(ModalType modalType) =>
        modalType switch
        {
            BlazorBootstrap.ModalType.Primary => "primary",
            BlazorBootstrap.ModalType.Secondary => "secondary",
            BlazorBootstrap.ModalType.Success => "success",
            BlazorBootstrap.ModalType.Danger => "danger",
            BlazorBootstrap.ModalType.Warning => "warning",
            BlazorBootstrap.ModalType.Info => "info",
            BlazorBootstrap.ModalType.Light => "light",
            BlazorBootstrap.ModalType.Dark => "dark",
            _ => null
        };

    public static string? ToOffcanvasSize(OffcanvasSize size) =>
        size switch
        {
            BlazorBootstrap.OffcanvasSize.Regular => null,
            BlazorBootstrap.OffcanvasSize.Small => "bb-offcanvas-sm",
            BlazorBootstrap.OffcanvasSize.Large => "bb-offcanvas-lg",
            _ => null
        };

    public static string? TooltipColor(TooltipColor color) => ToTooltipColor(color);

    public static string? ToPaginationSize(PaginationSize size) =>
        size switch
        {
            BlazorBootstrap.PaginationSize.Small => "sm",
            BlazorBootstrap.PaginationSize.Large => "lg",
            _ => null
        };

    public static string? ToPlaceholderAnimation(PlaceholderAnimation animation) =>
        animation switch
        {
            BlazorBootstrap.PlaceholderAnimation.Glow => "glow",
            BlazorBootstrap.PlaceholderAnimation.Wave => "wave",
            _ => null
        };

    public static string? ToPlaceholderColor(PlaceholderColor color) =>
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

    public static string? ToPlaceholderSize(PlaceholderSize size) =>
        size switch
        {
            BlazorBootstrap.PlaceholderSize.ExtraSmall => "xs",
            BlazorBootstrap.PlaceholderSize.Small => "sm",
            BlazorBootstrap.PlaceholderSize.Large => "lg",
            _ => null
        };

    public static string? ToPlaceholderWidth(PlaceholderWidth width) =>
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

    public static string ToPlacement(Placement placement) =>
        placement switch
        {
            BlazorBootstrap.Placement.Start => "start",
            BlazorBootstrap.Placement.End => "end",
            BlazorBootstrap.Placement.Top => "top",
            _ => "bottom"
        };

    public static string ToPosition(Position position) =>
        position switch
        {
            BlazorBootstrap.Position.Static => PositionAbsolute,
            BlazorBootstrap.Position.Relative => PositionRelative,
            BlazorBootstrap.Position.Absolute => PositionAbsolute,
            BlazorBootstrap.Position.Fixed => PositionFixed,
            BlazorBootstrap.Position.Sticky => PositionSticky,
            _ => ""
        };

    public static string? ToProgressColor(ProgressColor color) =>
        color switch
        {
            BlazorBootstrap.ProgressColor.Primary => "primary",
            BlazorBootstrap.ProgressColor.Secondary => "secondary",
            BlazorBootstrap.ProgressColor.Success => "success",
            BlazorBootstrap.ProgressColor.Danger => "danger",
            BlazorBootstrap.ProgressColor.Warning => "warning",
            BlazorBootstrap.ProgressColor.Info => "info",
            BlazorBootstrap.ProgressColor.Dark => "dark",
            _ => null
        };

    public static string? ToSize(Size size) =>
        size switch
        {
            BlazorBootstrap.Size.ExtraSmall => "xs",
            BlazorBootstrap.Size.Small => "sm",
            BlazorBootstrap.Size.Medium => "md",
            BlazorBootstrap.Size.Large => "lg",
            BlazorBootstrap.Size.ExtraLarge => "xl",
            _ => null
        };

    public static string? ToSpinnerColor(SpinnerColor color) =>
        color switch
        {
            BlazorBootstrap.SpinnerColor.Primary => "text-primary",
            BlazorBootstrap.SpinnerColor.Secondary => "text-secondary",
            BlazorBootstrap.SpinnerColor.Success => "text-success",
            BlazorBootstrap.SpinnerColor.Danger => "text-danger",
            BlazorBootstrap.SpinnerColor.Warning => "text-warning",
            BlazorBootstrap.SpinnerColor.Info => "text-info",
            BlazorBootstrap.SpinnerColor.Light => "text-light",
            BlazorBootstrap.SpinnerColor.Dark => "text-dark",
            _ => null
        };

    public static string ToSpinnerSize(SpinnerSize size) =>
        size switch
        {
            BlazorBootstrap.SpinnerSize.Small => "sm",
            BlazorBootstrap.SpinnerSize.Medium => "md",
            BlazorBootstrap.SpinnerSize.Large => "lg",
            BlazorBootstrap.SpinnerSize.ExtraLarge => "xl",
            _ => "md"
        };

    public static string ToSpinnerType(SpinnerType type) =>
        type switch
        {
            BlazorBootstrap.SpinnerType.Border => "border",
            BlazorBootstrap.SpinnerType.Grow => "grow",
            BlazorBootstrap.SpinnerType.Dots => "dots",
            _ => "border"
        };

    public static string ToTextAlignment(Alignment alignment) =>
        alignment switch
        {
            BlazorBootstrap.Alignment.Start or Alignment.None => "text-start",
            BlazorBootstrap.Alignment.Center => "text-center",
            BlazorBootstrap.Alignment.End => "text-end",
            _ => ""
        };

    public static string? ToTextColor(TextColor color) =>
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

    public static string? ToToastBackgroundColor(ToastType toastType) =>
        toastType switch
        {
            BlazorBootstrap.ToastType.Primary => "primary",
            BlazorBootstrap.ToastType.Secondary => "secondary",
            BlazorBootstrap.ToastType.Success => "success",
            BlazorBootstrap.ToastType.Danger => "danger",
            BlazorBootstrap.ToastType.Warning => "warning",
            BlazorBootstrap.ToastType.Info => "info",
            BlazorBootstrap.ToastType.Light => "light",
            BlazorBootstrap.ToastType.Dark => "dark",
            _ => null
        };

    public static string ToToastsPlacement(ToastsPlacement toastsPlacement) =>
        toastsPlacement switch
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
            _ => "top-0 end-0" // default: Top right
        };

    public static string? ToToastTextColor(ToastType toastType) =>
        toastType switch
        {
            BlazorBootstrap.ToastType.Primary
                or BlazorBootstrap.ToastType.Secondary
                or BlazorBootstrap.ToastType.Success
                or BlazorBootstrap.ToastType.Danger
                or BlazorBootstrap.ToastType.Dark => "white",
            ToastType.Warning
                or BlazorBootstrap.ToastType.Info
                or BlazorBootstrap.ToastType.Light => "dark",
            _ => null
        };

    public static string? ToTooltipColor(TooltipColor color) =>
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

    #endregion
}
