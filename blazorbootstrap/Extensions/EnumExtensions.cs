namespace BlazorBootstrap;

/// <summary>
/// Enum extensions
/// Pattern matching: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching
/// Discard pattern: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#discard-pattern
/// </summary>
public static class EnumExtensions
{
    #region Methods

    public static string ToAlertColorClass(this AlertColor alertColor) =>
        alertColor switch
        {
            AlertColor.Primary => "alert-primary",
            AlertColor.Secondary => "alert-secondary",
            AlertColor.Success => "alert-success",
            AlertColor.Danger => "alert-danger",
            AlertColor.Warning => "alert-warning",
            AlertColor.Info => "alert-info",
            AlertColor.Light => "alert-light",
            AlertColor.Dark => "alert-dark",
            _ => ""
        };

    public static string ToAutoCompleteSizeClass(this AutoCompleteSize autoCompleteSize) =>
        autoCompleteSize switch
        {
            AutoCompleteSize.Large => "form-control-lg",
            AutoCompleteSize.Small => "form-control-sm",
            _ => ""
        };

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

    public static string ToBadgeColorClass(this BadgeColor badgeColor) =>
        badgeColor switch
        {
            BadgeColor.Primary => "text-bg-primary",
            BadgeColor.Secondary => "text-bg-secondary",
            BadgeColor.Success => "text-bg-success",
            BadgeColor.Danger => "text-bg-danger",
            BadgeColor.Warning => "text-bg-warning",
            BadgeColor.Info => "text-bg-info",
            BadgeColor.Light => "text-bg-light",
            BadgeColor.Dark => "text-bg-dark",
            _ => ""
        };

    public static string ToBadgeIndicatorClass(this BadgeIndicatorType badgeIndicatorType) =>
        badgeIndicatorType switch
        {
            BadgeIndicatorType.RoundedPill => "rounded-pill",
            BadgeIndicatorType.RoundedCircle => "rounded-circle",
            _ => "" // default: Top right
        };

    public static string ToBadgePlacementClass(this BadgePlacement badgePlacement) =>
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

    /// <summary>
    /// Returns the Bootstrap CSS Class for a specific button color.
    /// </summary>
    /// <param name="buttonColor">Button color to retrieve the class from</param>
    /// <returns>Button color</returns>
    public static string ToButtonColorClass(this ButtonColor buttonColor) =>
        buttonColor switch
        {
            ButtonColor.Primary => "btn-primary",
            ButtonColor.Secondary => "btn-secondary",
            ButtonColor.Success => "btn-success",
            ButtonColor.Danger => "btn-danger",
            ButtonColor.Warning => "btn-warning",
            ButtonColor.Info => "btn-info",
            ButtonColor.Light => "btn-light",
            ButtonColor.Dark => "btn-dark",
            ButtonColor.Link => "btn-link",
            _ => ""
        };

    public static string? ToChartDatasetDataLabelAlignmentString(this Alignment alignment) =>
        alignment switch
        {
            Alignment.Start => "start",
            Alignment.Center or Alignment.None => "center", // default
            Alignment.End => "end",
            _ => null
        };

    public static string? ToChartDatasetDataLabelAnchorString(this Anchor anchor) =>
        anchor switch
        {
            Anchor.Start => "start",
            Anchor.Center or Anchor.None => "center", // default
            Anchor.End => "end",
            _ => null
        };

    public static string? ToChartDatasetDataLabelAnchorString(this Alignment anchor) =>
        anchor switch
        {
            Alignment.Start => "start",
            Alignment.Center or Alignment.None => "center", // default
            Alignment.End => "end",
            _ => null
        };

    public static string ToDropdownButtonColorClass(this DropdownColor dropdownColor) =>
        dropdownColor switch
        {
            DropdownColor.Primary => "btn-primary",
            DropdownColor.Secondary => "btn-secondary",
            DropdownColor.Success => "btn-success",
            DropdownColor.Danger => "btn-danger",
            DropdownColor.Warning => "btn-warning",
            DropdownColor.Info => "btn-info",
            DropdownColor.Light => "btn-light",
            DropdownColor.Dark => "btn-dark",
            DropdownColor.Link => "btn-link",
            _ => ""
        };

    public static string ToButtonOutlineColorClass(this ButtonColor buttonColor) =>
        buttonColor switch
        {
            ButtonColor.Primary => "btn-outline-primary",
            ButtonColor.Secondary => "btn-outline-secondary",
            ButtonColor.Success => "btn-outline-success",
            ButtonColor.Danger => "btn-outline-danger",
            ButtonColor.Warning => "btn-outline-warning",
            ButtonColor.Info => "btn-outline-info",
            ButtonColor.Light => "btn-outline-light",
            ButtonColor.Dark => "btn-outline-dark",
            ButtonColor.Link => "btn-outline-link",
            _ => ""
        };

    public static string ToButtonSizeClass(this ButtonSize buttonSize) =>
        buttonSize switch
        {
            ButtonSize.ExtraSmall => "btn-xs",
            ButtonSize.Small => "btn-sm",
            ButtonSize.Medium => "btn-md",
            ButtonSize.Large => "btn-lg",
            ButtonSize.ExtraLarge => "btn-xl",
            _ => ""
        };

    public static string ToButtonSpinnerSizeClass(this ButtonSize buttonSize) =>
        buttonSize switch
        {
            ButtonSize.ExtraSmall => "xs",
            ButtonSize.Small => "sm",
            ButtonSize.Medium => "md",
            ButtonSize.Large => "lg",
            ButtonSize.ExtraLarge => "xl",
            _ => ""
        };

    public static string ToButtonTypeString(this ButtonType buttonType) =>
        buttonType switch
        {
            ButtonType.Button => "button",
            ButtonType.Submit => "submit",
            ButtonType.Reset => "reset",
            _ => ""
        };

    public static string ToCalloutColorClass(this CalloutColor calloutColor) =>
        calloutColor switch
        {
            CalloutColor.Default => "",
            CalloutColor.Danger => $"bb-callout-danger",
            CalloutColor.Warning => $"bb-callout-warning",
            CalloutColor.Info => $"bb-callout-info",
            CalloutColor.Success => $"bb-callout-success",
            _ => ""
        };

    public static string ToCardColorClass(this CardColor cardColor) =>
        cardColor switch
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

    public static string? ToCarouselAutoPlayString(this CarouselAutoPlay autoplay) =>
        autoplay switch
        {
            CarouselAutoPlay.StartOnPageLoad => "carousel",
            CarouselAutoPlay.StartAfterUserInteraction => "true",
            _ => null
        };

    public static string ToCssString(this Unit unit) =>
        unit switch
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

    public static string ToDialogSizeClass(this DialogSize dialogSize) =>
        dialogSize switch
        {
            DialogSize.Regular => "",
            DialogSize.Small => "modal-sm",
            DialogSize.Large => "modal-lg",
            DialogSize.ExtraLarge => "modal-xl",
            _ => ""
        };

    public static string ToDropdownDirectionClass(this DropdownDirection dropdownDirection) =>
        dropdownDirection switch
        {
            DropdownDirection.Dropdown => "dropdown",
            DropdownDirection.DropdownCentered => "dropdown dropdown-center",
            DropdownDirection.DropEnd => "dropend",
            DropdownDirection.DropUp => "dropup",
            DropdownDirection.DropUpCentered => "dropup dropup-center",
            DropdownDirection.DropStart => "dropstart",
            _ => ""
        };

    public static string ToDropdownMenuPositionClass(this DropdownMenuPosition dropdownMenuPosition) =>
        dropdownMenuPosition switch
        {
            DropdownMenuPosition.Start => "dropdown-menu-start",
            DropdownMenuPosition.End => "dropdown-menu-end",
            _ => ""
        };

    public static string ToDropdownButtonSizeClass(this DropdownSize dropdownSize) =>
        dropdownSize switch
        {
            DropdownSize.Small => "btn-sm",
            DropdownSize.Large => "btn-lg",
            _ => ""
        };

    public static string ToIconColorClass(this IconColor iconColor) =>
        iconColor switch
        {
            IconColor.Primary => "text-primary",
            IconColor.Secondary => "text-secondary",
            IconColor.Success => "text-success",
            IconColor.Danger => "text-danger",
            IconColor.Warning => "text-warning",
            IconColor.Info => "text-info",
            IconColor.Light => "text-light",
            IconColor.Dark => "text-dark",
            IconColor.Body => "text-body",
            IconColor.Muted => "text-muted",
            IconColor.White => "text-white",
            _ => ""
        };

    public static string ToModalFullscreenClass(this ModalFullscreen modalFullscreen) =>
        modalFullscreen switch
        {
            ModalFullscreen.Disabled => "",
            ModalFullscreen.Always => "modal-fullscreen",
            ModalFullscreen.SmallDown => "modal-fullscreen-sm-down",
            ModalFullscreen.MediumDown => "modal-fullscreen-md-down",
            ModalFullscreen.LargeDown => "modal-fullscreen-lg-down",
            ModalFullscreen.ExtraLargeDown => "modal-fullscreen-xl-down",
            ModalFullscreen.ExtraExtraLargeDown => "modal-fullscreen-xxl-down",
            _ => ""
        };

    public static string ToModalHeaderColorClass(this ModalType modalType) =>
        modalType switch
        {
            ModalType.Primary => "text-bg-primary border-bottom border-primary",
            ModalType.Secondary => "text-bg-secondary border-bottom border-secondary",
            ModalType.Success => "text-bg-success border-bottom border-success",
            ModalType.Danger => "text-bg-danger border-bottom border-danger",
            ModalType.Warning => "text-bg-warning border-bottom border-warning",
            ModalType.Info => "text-bg-info border-bottom border-info",
            ModalType.Light => "text-bg-light border-bottom",
            ModalType.Dark => "text-bg-dark border-bottom border-dark",
            _ => ""
        };

    public static string ToModalSizeClass(this ModalSize modalSize) =>
        modalSize switch
        {
            ModalSize.Regular => "",
            ModalSize.Small => "modal-sm",
            ModalSize.Large => "modal-lg",
            ModalSize.ExtraLarge => "modal-xl",
            _ => ""
        };

    public static string ToOffcanvasPlacementClass(this Placement placement) =>
        placement switch
        {
            Placement.Start => "offcanvas-start",
            Placement.End => "offcanvas-end",
            Placement.Top => "offcanvas-top",
            _ => "offcanvas-bottom"
        };

    public static string ToOffcanvasSizeClass(this OffcanvasSize offcanvasSize) =>
        offcanvasSize switch
        {
            OffcanvasSize.Regular => "",
            OffcanvasSize.Small => "bb-offcanvas-sm",
            OffcanvasSize.Large => "bb-offcanvas-lg",
            _ => ""
        };

    public static string ToPaginationAlignmentClass(this Alignment alignment) =>
        alignment switch
        {
            Alignment.Start => "justify-content-start",
            Alignment.Center => "justify-content-center",
            Alignment.End => "justify-content-end",
            _ => ""
        };

    public static string ToPaginationSizeClass(this PaginationSize paginationSize) =>
        paginationSize switch
        {
            PaginationSize.Small => "pagination-sm",
            PaginationSize.Large => "pagination-lg",
            _ => ""
        };

    public static string ToPlaceholderAnimationClass(this PlaceholderAnimation placeholderAnimation) =>
        placeholderAnimation switch
        {
            PlaceholderAnimation.Glow => "placeholder-glow",
            PlaceholderAnimation.Wave => "placeholder-wave",
            _ => ""
        };

    public static string ToPlaceholderColorClass(this PlaceholderColor placeholderColor) =>
        placeholderColor switch
        {
            PlaceholderColor.Primary => "bg-primary",
            PlaceholderColor.Secondary => "bg-secondary",
            PlaceholderColor.Success => "bg-success",
            PlaceholderColor.Danger => "bg-danger",
            PlaceholderColor.Warning => "bg-warning",
            PlaceholderColor.Info => "bg-info",
            PlaceholderColor.Light => "bg-light",
            PlaceholderColor.Dark => "bg-dark",
            _ => ""
        };

    public static string ToPlaceholderSizeClass(this PlaceholderSize placeholderSize) =>
        placeholderSize switch
        {
            PlaceholderSize.ExtraSmall => "placeholder-xs",
            PlaceholderSize.Small => "placeholder-sm",
            PlaceholderSize.Large => "placeholder-lg",
            _ => ""
        };

    public static string ToPlaceholderWidthClass(this PlaceholderWidth placeholderWidth) =>
        placeholderWidth switch
        {
            PlaceholderWidth.Col1 => "col-1",
            PlaceholderWidth.Col2 => "col-2",
            PlaceholderWidth.Col3 => "col-3",
            PlaceholderWidth.Col4 => "col-4",
            PlaceholderWidth.Col5 => "col-5",
            PlaceholderWidth.Col6 => "col-6",
            PlaceholderWidth.Col7 => "col-7",
            PlaceholderWidth.Col8 => "col-8",
            PlaceholderWidth.Col9 => "col-9",
            PlaceholderWidth.Col10 => "col-10",
            PlaceholderWidth.Col11 => "col-11",
            PlaceholderWidth.Col12 => "col-12",
            _ => ""
        };

    public static string ToPositionClass(this Position position) =>
        position switch
        {
            Position.Static => BootstrapClass.PositionAbsolute,
            Position.Relative => BootstrapClass.PositionRelative,
            Position.Absolute => BootstrapClass.PositionAbsolute,
            Position.Fixed => BootstrapClass.PositionFixed,
            Position.Sticky => BootstrapClass.PositionSticky,
            _ => ""
        };

    public static string ToProgressColorClass(this ProgressColor progressColor) =>
        progressColor switch
        {
            ProgressColor.Primary => "bg-primary",
            ProgressColor.Secondary => "bg-secondary",
            ProgressColor.Success => "bg-success",
            ProgressColor.Danger => "bg-danger",
            ProgressColor.Warning => "bg-warning",
            ProgressColor.Info => "bg-info",
            ProgressColor.Dark => "bg-dark",
            _ => ""
        };

    public static object ToSortableListPullMode(this SortableListPullMode sortableListPullMode) =>
        sortableListPullMode switch
        {
            SortableListPullMode.True => true,
            SortableListPullMode.False => false,
            SortableListPullMode.Clone => "clone",
            //SortableListPullMode.Array => "array"
            _ => throw new ArgumentOutOfRangeException(nameof(sortableListPullMode), sortableListPullMode, "Invalid SortableListPullMode value supplied")
        };

    public static object ToSortableListPutMode(this SortableListPutMode sortableListPutMode) =>
        sortableListPutMode == SortableListPutMode.True;

    public static string ToSpinnerColorClass(this SpinnerColor spinnerColor) =>
        spinnerColor switch
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

    public static string ToSpinnerSizeClass(this SpinnerSize spinnerSize) =>
        spinnerSize switch
        {
            SpinnerSize.Small => "sm",
            SpinnerSize.Medium => "md",
            SpinnerSize.Large => "lg",
            SpinnerSize.ExtraLarge => "xl",
            _ => "md"
        };

    public static string ToSpinnerTypeClass(this SpinnerType spinnerType) =>
        spinnerType switch
        {
            SpinnerType.Border => "spinner-border",
            SpinnerType.Grow => "spinner-grow",
            SpinnerType.Dots => "spinner-dots",
            _ => "spinner-border"
        };

    public static string ToTargetString(this Target target) =>
        target switch
        {
            Target.Blank => "_blank",
            Target.Parent => "_parent",
            Target.Top => "_top",
            Target.Self => "_self",
            _ => ""
        };

    public static string ToTextAlignmentClass(this Alignment alignment) =>
        alignment switch
        {
            BlazorBootstrap.Alignment.Start => "text-start",
            BlazorBootstrap.Alignment.Center => "text-center",
            BlazorBootstrap.Alignment.End => "text-end",
            _ => ""
        };

    public static string ToTextColorClass(this TextColor textColor) =>
        textColor switch
        {
            TextColor.Primary => "text-primary",
            TextColor.Secondary => "text-secondary",
            TextColor.Success => "text-success",
            TextColor.Danger => "text-danger",
            TextColor.Warning => "text-warning",
            TextColor.Info => "text-info",
            TextColor.Light => "text-light",
            TextColor.Dark => "text-dark",
            TextColor.Body => "text-body",
            TextColor.Muted => "text-muted",
            TextColor.White => "text-white",
            _ => ""
        };

    public static string? ToTooltipColorClass(this TooltipColor tooltipColor) =>
        tooltipColor switch
        {
            TooltipColor.Primary => "bb-tooltip-primary",
            TooltipColor.Secondary => "bb-tooltip-tooltip-secondary",
            TooltipColor.Success => "bb-tooltip-success",
            TooltipColor.Danger => "bb-tooltip-danger",
            TooltipColor.Warning => "bb-tooltip-warning",
            TooltipColor.Info => "bb-tooltip-info",
            TooltipColor.Light => "bb-tooltip-light",
            TooltipColor.Dark => "bb-tooltip-dark",
            _ => null
        };

    public static string ToTooltipPlacementName(this TooltipPlacement tooltipPlacement) =>
        tooltipPlacement switch
        {
            TooltipPlacement.Auto => "auto",
            TooltipPlacement.Right => "right",
            TooltipPlacement.Bottom => "bottom",
            TooltipPlacement.Left => "left",
            _ => "top"
        };

    public static string ToToastBackgroundColorClass(this ToastType toastType) =>
        toastType switch
        {
            ToastType.Primary => "bg-primary",
            ToastType.Secondary => "bg-secondary",
            ToastType.Success => "bg-success",
            ToastType.Danger => "bg-danger",
            ToastType.Warning => "bg-warning",
            ToastType.Info => "bg-info",
            ToastType.Light => "bg-light",
            ToastType.Dark => "bg-dark",
            _ => ""
        };

    public static string ToToastsPlacementClass(this ToastsPlacement toastsPlacement) =>
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

    public static string ToToastTextColorClass(this ToastType toastType) =>
        toastType switch
        {
            ToastType.Primary
                or ToastType.Secondary
                or ToastType.Success
                or ToastType.Danger
                or ToastType.Dark => "text-white",
            ToastType.Warning
                or ToastType.Info
                or ToastType.Light => "text-dark",
            _ => ""
        };

    #endregion
}
