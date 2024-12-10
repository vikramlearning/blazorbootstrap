namespace BlazorBootstrap;

/// <summary>
/// Enum extensions
/// Pattern matching: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching
/// Discard pattern: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#discard-pattern
/// </summary>
public static class EnumExtensions
{
    #region Methods

    internal static readonly Dictionary<AlertColor, string> AlertColorClassMap = new()
    {
        { AlertColor.Primary, "alert-primary" },
        { AlertColor.Secondary, "alert-secondary" },
        { AlertColor.Success, "alert-success" },
        { AlertColor.Danger, "alert-danger" },
        { AlertColor.Warning, "alert-warning" },
        { AlertColor.Info, "alert-info" },
        { AlertColor.Light, "alert-light" },
        { AlertColor.Dark, "alert-dark" }
    };

    internal static readonly Dictionary<AutoCompleteSize, string> AutoCompleteSizeClassMap = new()
    {
        { AutoCompleteSize.Large, "form-control-lg" },
        { AutoCompleteSize.Small, "form-control-sm" },
        { AutoCompleteSize.Default, "" }
    };


    internal static readonly Dictionary<BadgeColor, string> BadgeColorClassMap = new()
    {
        { BadgeColor.Primary, "text-bg-primary" },
        { BadgeColor.Secondary, "text-bg-secondary" },
        { BadgeColor.Success, "text-bg-success" },
        { BadgeColor.Danger, "text-bg-danger" },
        { BadgeColor.Warning, "text-bg-warning" },
        { BadgeColor.Info, "text-bg-info" },
        { BadgeColor.Light, "text-bg-light" },
        { BadgeColor.Dark, "text-bg-dark" },
        { BadgeColor.None, "" }
    };

    internal static readonly Dictionary<BadgeIndicatorType, string> BadgeIndicatorClassMap = new()
    {
        { BadgeIndicatorType.RoundedPill, "rounded-pill" },
        { BadgeIndicatorType.RoundedCircle, "rounded-circle" },
        { BadgeIndicatorType.None, "" }
    };


    internal static readonly Dictionary<BadgePlacement, string> BadgePlacementClassMap = new()
    {
        { BadgePlacement.TopLeft, "top-0 start-0 translate-middle" },
        { BadgePlacement.TopCenter, "top-0 start-50 translate-middle" },
        { BadgePlacement.TopRight, "top-0 start-100 translate-middle" },
        { BadgePlacement.MiddleLeft, "top-50 start-0 translate-middle" },
        { BadgePlacement.MiddleCenter, "top-50 start-50 translate-middle" },
        { BadgePlacement.MiddleRight, "top-50 start-100 translate-middle" },
        { BadgePlacement.BottomLeft, "top-100 start-0 translate-middle" },
        { BadgePlacement.BottomCenter, "top-100 start-50 translate-middle" },
        { BadgePlacement.BottomRight, "top-100 start-100 translate-middle" },
        { BadgePlacement.None, "top-0 start-100 translate-middle" }
    };
     
    internal static readonly Dictionary<ButtonColor, string> ButtonColorClassMap = new()
    {
        { ButtonColor.Primary, "btn-primary" },
        { ButtonColor.Secondary, "btn-secondary" },
        { ButtonColor.Success, "btn-success" },
        { ButtonColor.Danger, "btn-danger" },
        { ButtonColor.Warning, "btn-warning" },
        { ButtonColor.Info, "btn-info" },
        { ButtonColor.Light, "btn-light" },
        { ButtonColor.Dark, "btn-dark" },
        { ButtonColor.Link, "btn-link" },
        { ButtonColor.None, "" }
    };

    internal static readonly Dictionary<Alignment, string> ChartDatasetDataLabelAlignmentMap = new()
    {
        { Alignment.Start, "start" },
        { Alignment.Center, "center" },
        { Alignment.None, "center" }, // default
        { Alignment.End, "end" }
    };


    internal static readonly Dictionary<Anchor, string> ChartDatasetDataLabelAnchorMap = new()
    {
        { Anchor.Start, "start" },
        { Anchor.Center, "center" },
        { Anchor.None, "center" }, // default
        { Anchor.End, "end" }
    };

    internal static readonly Dictionary<DropdownColor, string> DropdownButtonColorClassMap = new()
    {
        { DropdownColor.Primary, "btn-primary" },
        { DropdownColor.Secondary, "btn-secondary" },
        { DropdownColor.Success, "btn-success" },
        { DropdownColor.Danger, "btn-danger" },
        { DropdownColor.Warning, "btn-warning" },
        { DropdownColor.Info, "btn-info" },
        { DropdownColor.Light, "btn-light" },
        { DropdownColor.Dark, "btn-dark" },
        { DropdownColor.Link, "btn-link" },
        { DropdownColor.None, "" }
    };

    internal static readonly Dictionary<ButtonColor, string> ButtonOutlineColorClassMap = new()
    {
        { ButtonColor.Primary, "btn-outline-primary" },
        { ButtonColor.Secondary, "btn-outline-secondary" },
        { ButtonColor.Success, "btn-outline-success" },
        { ButtonColor.Danger, "btn-outline-danger" },
        { ButtonColor.Warning, "btn-outline-warning" },
        { ButtonColor.Info, "btn-outline-info" },
        { ButtonColor.Light, "btn-outline-light" },
        { ButtonColor.Dark, "btn-outline-dark" },
        { ButtonColor.Link, "btn-outline-link" },
        { ButtonColor.None, "" }
    };

    internal static readonly Dictionary<ButtonSize, string> ButtonSizeClassMap = new()
    {
        { ButtonSize.ExtraSmall, "btn-xs" },
        { ButtonSize.Small, "btn-sm" },
        { ButtonSize.Medium, "btn-md" },
        { ButtonSize.Large, "btn-lg" },
        { ButtonSize.ExtraLarge, "btn-xl" },
        { ButtonSize.None, "" }
    };

    internal static readonly Dictionary<ButtonSize, string> ButtonSpinnerSizeClassMap = new()
    {
        { ButtonSize.ExtraSmall, "xs" },
        { ButtonSize.Small, "sm" },
        { ButtonSize.Medium, "md" },
        { ButtonSize.Large, "lg" },
        { ButtonSize.ExtraLarge, "xl" },
        { ButtonSize.None, "" }
    };

    internal static readonly Dictionary<ButtonType, string> ButtonTypeStringMap = new()
    {
        { ButtonType.Button, "button" },
        { ButtonType.Submit, "submit" },
        { ButtonType.Reset, "reset" },
        { ButtonType.Link, "button" }
    };

     
    internal static readonly Dictionary<CalloutColor, string> CalloutColorClassMap = new()
    {
        { CalloutColor.Default, "" },
        { CalloutColor.Danger, "bb-callout-danger" },
        { CalloutColor.Warning, "bb-callout-warning" },
        { CalloutColor.Info, "bb-callout-info" },
        { CalloutColor.Success, "bb-callout-success" }
    };

    internal static readonly Dictionary<CardColor, string> CardColorClassMap = new()
    {
        { CardColor.Primary, "text-bg-primary" },
        { CardColor.Secondary, "text-bg-secondary" },
        { CardColor.Success, "text-bg-success" },
        { CardColor.Danger, "text-bg-danger" },
        { CardColor.Warning, "text-bg-warning" },
        { CardColor.Info, "text-bg-info" },
        { CardColor.Light, "text-bg-light" },
        { CardColor.Dark, "text-bg-dark" },
        { CardColor.None, "" }
    };

    internal static readonly Dictionary<CarouselAutoPlay, string> CarouselAutoPlayStringMap = new()
    {
        { CarouselAutoPlay.StartOnPageLoad, "carousel" },
        { CarouselAutoPlay.StartAfterUserInteraction, "true" },
        { CarouselAutoPlay.None, "" }
    };

    internal static readonly Dictionary<Unit, string> UnitCssStringMap = new()
    {
        { Unit.Em, "em" },
        { Unit.Percentage, "%" },
        { Unit.Pt, "pt" },
        { Unit.Px, "px" },
        { Unit.Rem, "rem" },
        { Unit.Vh, "vh" },
        { Unit.VMax, "vmax" },
        { Unit.VMin, "vmin" },
        { Unit.Vw, "vw" },
        { Unit.None, "" }
    };

    internal static readonly Dictionary<DialogSize, string> DialogSizeClassMap = new()
    {
        { DialogSize.Regular, "" },
        { DialogSize.Small, "modal-sm" },
        { DialogSize.Large, "modal-lg" },
        { DialogSize.ExtraLarge, "modal-xl" }
    };

    internal static readonly Dictionary<DropdownDirection, string> DropdownDirectionClassMap = new()
    {
        { DropdownDirection.Dropdown, "dropdown" },
        { DropdownDirection.DropdownCentered, "dropdown dropdown-center" },
        { DropdownDirection.DropEnd, "dropend" },
        { DropdownDirection.DropUp, "dropup" },
        { DropdownDirection.DropUpCentered, "dropup dropup-center" },
        { DropdownDirection.DropStart, "dropstart" }
    };

    public static string ToDropdownMenuPositionClass(this DropdownMenuPosition dropdownMenuPosition)
    {
        return dropdownMenuPosition == DropdownMenuPosition.Start ? "dropdown-menu-start" : "dropdown-menu-end";
    }

    internal static readonly Dictionary<DropdownSize, string> DropdownButtonSizeClassMap = new()
    {
        { DropdownSize.Small, "btn-sm" },
        { DropdownSize.Large, "btn-lg" },
        { DropdownSize.None, "" }
    };

     
    internal static readonly Dictionary<IconColor, string> IconColorClassMap = new()
    {
        { IconColor.Primary, "text-primary" },
        { IconColor.Secondary, "text-secondary" },
        { IconColor.Success, "text-success" },
        { IconColor.Danger, "text-danger" },
        { IconColor.Warning, "text-warning" },
        { IconColor.Info, "text-info" },
        { IconColor.Light, "text-light" },
        { IconColor.Dark, "text-dark" },
        { IconColor.Body, "text-body" },
        { IconColor.Muted, "text-muted" },
        { IconColor.White, "text-white" },
        { IconColor.None, "" }
    };

    internal static readonly Dictionary<ModalFullscreen, string> ModalFullscreenClassMap = new()
    {
        { ModalFullscreen.Disabled, "" },
        { ModalFullscreen.Always, "modal-fullscreen" },
        { ModalFullscreen.SmallDown, "modal-fullscreen-sm-down" },
        { ModalFullscreen.MediumDown, "modal-fullscreen-md-down" },
        { ModalFullscreen.LargeDown, "modal-fullscreen-lg-down" },
        { ModalFullscreen.ExtraLargeDown, "modal-fullscreen-xl-down" },
        { ModalFullscreen.ExtraExtraLargeDown, "modal-fullscreen-xxl-down" }
    };

    internal static readonly Dictionary<ModalType, string> ModalHeaderColorClassMap = new()
    {
        { ModalType.Primary, "text-bg-primary border-bottom border-primary" },
        { ModalType.Secondary, "text-bg-secondary border-bottom border-secondary" },
        { ModalType.Success, "text-bg-success border-bottom border-success" },
        { ModalType.Danger, "text-bg-danger border-bottom border-danger" },
        { ModalType.Warning, "text-bg-warning border-bottom border-warning" },
        { ModalType.Info, "text-bg-info border-bottom border-info" },
        { ModalType.Light, "text-bg-light border-bottom" },
        { ModalType.Dark, "text-bg-dark border-bottom border-dark" },
        { ModalType.None, "" }
    };

    internal static readonly Dictionary<ModalSize, string> ModalSizeClassMap = new()
    {
        { ModalSize.Regular, "" },
        { ModalSize.Small, "modal-sm" },
        { ModalSize.Large, "modal-lg" },
        { ModalSize.ExtraLarge, "modal-xl" }
    };

    internal static readonly Dictionary<Placement, string> OffcanvasPlacementClassMap = new()
    {
        { Placement.Start, "offcanvas-start" },
        { Placement.End, "offcanvas-end" },
        { Placement.Top, "offcanvas-top" },
        { Placement.Bottom, "offcanvas-bottom" }
    };

    internal static readonly Dictionary<OffcanvasSize, string> OffcanvasSizeClassMap = new()
    {
        { OffcanvasSize.Regular, "" },
        { OffcanvasSize.Small, "bb-offcanvas-sm" },
        { OffcanvasSize.Large, "bb-offcanvas-lg" }
    };

    internal static readonly Dictionary<Alignment, string> PaginationAlignmentClassMap = new()
    {
        { Alignment.Start, "justify-content-start" },
        { Alignment.Center, "justify-content-center" },
        { Alignment.End, "justify-content-end" },
        { Alignment.None, "" }
    };

    internal static readonly Dictionary<PaginationSize, string> PaginationSizeClassMap = new()
    {
        { PaginationSize.Small, "pagination-sm" },
        { PaginationSize.Large, "pagination-lg" },
        { PaginationSize.None, "" }
    };
    
    public static string ToPlaceholderAnimationClass(this PlaceholderAnimation placeholderAnimation)
        => placeholderAnimation == PlaceholderAnimation.Glow ? "placeholder-glow" : "placeholder-wave";

    internal static readonly Dictionary<PlaceholderColor, string> PlaceholderColorClassMap = new()
    {
        { PlaceholderColor.Primary, "bg-primary" },
        { PlaceholderColor.Secondary, "bg-secondary" },
        { PlaceholderColor.Success, "bg-success" },
        { PlaceholderColor.Danger, "bg-danger" },
        { PlaceholderColor.Warning, "bg-warning" },
        { PlaceholderColor.Info, "bg-info" },
        { PlaceholderColor.Light, "bg-light" },
        { PlaceholderColor.Dark, "bg-dark" },
        { PlaceholderColor.None, "" }
    };

    internal static readonly Dictionary<PlaceholderSize, string> PlaceholderSizeClassMap = new()
    {
        { PlaceholderSize.ExtraSmall, "placeholder-xs" },
        { PlaceholderSize.Small, "placeholder-sm" },
        { PlaceholderSize.Large, "placeholder-lg" },
        { PlaceholderSize.None, "" }
    };

    internal static readonly Dictionary<PlaceholderWidth, string> PlaceholderWidthClassMap = new()
    {
        { PlaceholderWidth.Col1, "col-1" },
        { PlaceholderWidth.Col2, "col-2" },
        { PlaceholderWidth.Col3, "col-3" },
        { PlaceholderWidth.Col4, "col-4" },
        { PlaceholderWidth.Col5, "col-5" },
        { PlaceholderWidth.Col6, "col-6" },
        { PlaceholderWidth.Col7, "col-7" },
        { PlaceholderWidth.Col8, "col-8" },
        { PlaceholderWidth.Col9, "col-9" },
        { PlaceholderWidth.Col10, "col-10" },
        { PlaceholderWidth.Col11, "col-11" },
        { PlaceholderWidth.Col12, "col-12" }
    };

    internal static readonly Dictionary<Position, string> PositionClassMap = new()
    {
        { Position.Static, BootstrapClass.PositionAbsolute },
        { Position.Relative, BootstrapClass.PositionRelative },
        { Position.Absolute, BootstrapClass.PositionAbsolute },
        { Position.Fixed, BootstrapClass.PositionFixed },
        { Position.Sticky, BootstrapClass.PositionSticky },
        { Position.None, "" }
    };

    internal static readonly Dictionary<ProgressColor, string> ProgressColorClassMap = new()
    {
        { ProgressColor.Primary, "bg-primary" },
        { ProgressColor.Secondary, "bg-secondary" },
        { ProgressColor.Success, "bg-success" },
        { ProgressColor.Danger, "bg-danger" },
        { ProgressColor.Warning, "bg-warning" },
        { ProgressColor.Info, "bg-info" },
        { ProgressColor.Dark, "bg-dark" },
        { ProgressColor.None, "" }
    };

    public static object ToSortableListPullMode(this SortableListPullMode sortableListPullMode) =>
        sortableListPullMode switch
        {
            SortableListPullMode.True => true,
            SortableListPullMode.False => false,
            SortableListPullMode.Clone => "clone", 
            _ => throw new ArgumentOutOfRangeException(nameof(sortableListPullMode), sortableListPullMode, "Invalid SortableListPullMode value supplied")
        };

    public static object ToSortableListPutMode(this SortableListPutMode sortableListPutMode) =>
        sortableListPutMode == SortableListPutMode.True;

    internal static readonly Dictionary<SpinnerColor, string> SpinnerColorClassMap = new()
    {
        { SpinnerColor.Primary, "text-primary" },
        { SpinnerColor.Secondary, "text-secondary" },
        { SpinnerColor.Success, "text-success" },
        { SpinnerColor.Danger, "text-danger" },
        { SpinnerColor.Warning, "text-warning" },
        { SpinnerColor.Info, "text-info" },
        { SpinnerColor.Light, "text-light" },
        { SpinnerColor.Dark, "text-dark" }
    };
     
    internal static readonly Dictionary<SpinnerSize, string> SpinnerSizeClassMap = new()
    {
        { SpinnerSize.Small, "sm" },
        { SpinnerSize.Medium, "md" },
        { SpinnerSize.Large, "lg" },
        { SpinnerSize.ExtraLarge, "xl" }
    };

    internal static readonly Dictionary<SpinnerType, string> SpinnerTypeClassMap = new()
    {
        { SpinnerType.Border, "spinner-border" },
        { SpinnerType.Grow, "spinner-grow" },
        { SpinnerType.Dots, "spinner-dots" }
    };

    internal static readonly Dictionary<Target, string> TargetStringMap = new()
    {
        { Target.Blank, "_blank" },
        { Target.Parent, "_parent" },
        { Target.Top, "_top" },
        { Target.Self, "_self" },
        { Target.None, "" }
    };

    internal static readonly Dictionary<Alignment, string> TextAlignmentClassMap = new()
    {
        { Alignment.Start, "text-start" },
        { Alignment.Center, "text-center" },
        { Alignment.End, "text-end" }, 
        { Alignment.None, "" }
    };

    internal static readonly Dictionary<TextColor, string> TextColorClassMap = new()
    {
        { TextColor.Primary, "text-primary" },
        { TextColor.Secondary, "text-secondary" },
        { TextColor.Success, "text-success" },
        { TextColor.Danger, "text-danger" },
        { TextColor.Warning, "text-warning" },
        { TextColor.Info, "text-info" },
        { TextColor.Light, "text-light" },
        { TextColor.Dark, "text-dark" },
        { TextColor.Body, "text-body" },
        { TextColor.Muted, "text-muted" },
        { TextColor.White, "text-white" },
        { TextColor.None, "" }
    };

    internal static readonly Dictionary<TooltipColor, string?> TooltipColorClassMap = new()
    {
        { TooltipColor.Primary, "bb-tooltip-primary" },
        { TooltipColor.Secondary, "bb-tooltip-tooltip-secondary" },
        { TooltipColor.Success, "bb-tooltip-success" },
        { TooltipColor.Danger, "bb-tooltip-danger" },
        { TooltipColor.Warning, "bb-tooltip-warning" },
        { TooltipColor.Info, "bb-tooltip-info" },
        { TooltipColor.Light, "bb-tooltip-light" },
        { TooltipColor.Dark, "bb-tooltip-dark" },
        { TooltipColor.None, "" }
    };
     
    internal static readonly Dictionary<TooltipPlacement, string> TooltipPlacementNameMap = new()
    {
        { TooltipPlacement.Auto, "auto" },
        { TooltipPlacement.Right, "right" },
        { TooltipPlacement.Bottom, "bottom" },
        { TooltipPlacement.Left, "left" },
        { TooltipPlacement.Top, "top" }
    };

    internal static readonly Dictionary<ToastType, string> ToastBackgroundColorClassMap = new()
    {
        { ToastType.Primary, "bg-primary" },
        { ToastType.Secondary, "bg-secondary" },
        { ToastType.Success, "bg-success" },
        { ToastType.Danger, "bg-danger" },
        { ToastType.Warning, "bg-warning" },
        { ToastType.Info, "bg-info" },
        { ToastType.Light, "bg-light" },
        { ToastType.Dark, "bg-dark" }
    };
    internal static readonly Dictionary<ToastsPlacement, string> ToastsPlacementClassMap = new()
    {
        { ToastsPlacement.TopLeft, "top-0 start-0" },
        { ToastsPlacement.TopCenter, "top-0 start-50 translate-middle-x" },
        { ToastsPlacement.TopRight, "top-0 end-0" },
        { ToastsPlacement.MiddleLeft, "top-50 start-0 translate-middle-y" },
        { ToastsPlacement.MiddleCenter, "top-50 start-50 translate-middle" },
        { ToastsPlacement.MiddleRight, "top-50 end-0 translate-middle-y" },
        { ToastsPlacement.BottomLeft, "bottom-0 start-0" },
        { ToastsPlacement.BottomCenter, "bottom-0 start-50 translate-middle-x" },
        { ToastsPlacement.BottomRight, "bottom-0 end-0" }
    };

    public static string ToToastTextColorClass(this ToastType toastType)
        => toastType is ToastType.Primary or ToastType.Secondary or ToastType.Success or ToastType.Danger
            or ToastType.Dark
            ? "text-white"
            : "text-dark";

    #endregion
}
