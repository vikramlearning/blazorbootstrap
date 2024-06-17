namespace BlazorBootstrap;

/// <summary>
/// Contains settings for the Bootstrap CSS based on <see href="https://github.com/twbs/bootstrap/blob/main/dist/css/bootstrap.css" />. <br/>
/// All properties can be overridden if need be; otherwise, the defaults of Bootstrap are used as described in the original CSS file.
/// </summary>
public sealed class BootstrapCssSettings
{

    /// <summary>
    /// Default colors for the Bootstrap CSS Light theme [data-bs-theme=light]
    /// </summary>
    public BootstrapCssSettingsColorTheme Light { get; set; } = new();

    /// <summary>
    /// Default colors for the Bootstrap CSS Dark theme [data-bs-theme=dark]
    /// </summary>
    public BootstrapCssSettingsColorTheme Dark { get; set; } = new();


    /// <summary>
    /// If <see langword="true"/>, the CSS output will be minimized.
    /// </summary>
    /// <remarks>
    /// Default value is <see langword="false"/>.
    /// </remarks>
    public bool MinimizeCssOutput { get; set; } = false;

    #region Default Bootstrap Colors

    /// <summary>
    /// Default Bootstrap --bs-black color. 
    /// </summary>
    /// <remarks>
    /// Default value is RGB(0,0,0), hex #000000, a.k.a <see cref="Color.Black"/>
    /// </remarks>
    public Color? BlackColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-blue color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(13, 110, 253), hex #0d6efd
    /// </remarks>
    public Color? BlueColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-cyan color. 
    /// </summary>
    /// <remarks>
    /// Default value is RGB(13, 202, 240), hex #0dcaf0
    /// </remarks>
    public Color? CyanColor { get; set; }

#region Grays
    /// <summary>
    /// Default Bootstrap --bs-gray color. 
    /// </summary>
    /// <remarks>
    /// Default value is RGB(108, 117, 125), hex #6c757d
    /// </remarks>
    public Color? GrayColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gray-100 color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(248, 249, 250), hex #f8f9fa
    /// </remarks>
    public Color? Gray100Color { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gray-200 color. 
    /// </summary>
    /// <remarks>
    /// Default value is RGB(233, 236, 239), hex #e9ecef
    /// </remarks>
    public Color? Gray200Color { get; set; }
    
    /// <summary>
    /// Default Bootstrap --bs-gray-300 color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(222, 226, 230), hex #dee2e6
    /// </remarks>
    public Color? Gray300Color { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gray-400 color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(206, 212, 218), hex #ced4da
    /// </remarks>
    public Color? Gray400Color { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gray-500 color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(173, 181, 189), hex #adb5bd
    /// </remarks>
    public Color? Gray500Color { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gray-600 color.
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="GrayColor"/>
    /// </remarks>
    public Color? Gray600Color { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gray-700 color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(73, 80, 87), hex #495057
    /// </remarks>
    public Color? Gray700Color { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gray-800 color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(52, 58, 64), hex #343a40
    /// </remarks>
    public Color? Gray800Color { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gray-900 color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(33, 37, 41), hex #212529
    /// </remarks>
    public Color? Gray900Color { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gray-dark color.
    /// </summary>
    /// <remarks>
    /// Default value based on <see cref="Gray800Color"/>
    /// </remarks>
    public Color? GrayDarkColor { get; set; }


    #endregion

    /// <summary>
    /// Default Bootstrap --bs-green color. 
    /// </summary>
    /// <remarks>
    /// Default value is RGB(0, 128, 0), hex #008000
    /// </remarks>
    public Color? GreenColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-indigo color. 
    /// </summary>
    /// <remarks>
    /// Default value is RGB(102, 16, 242), hex #6610f2
    /// </remarks>
    public Color? IndigoColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-orange color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(245, 107, 19), hex #f56b13
    /// </remarks>
    public Color? OrangeColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-pink color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(214, 51, 132), hex #d63384
    /// </remarks>
    public Color? PinkColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-purple color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(68, 75, 137), hex #444b89
    /// </remarks>
    public Color? PurpleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-red color. 
    /// </summary>
    /// <remarks>
    /// Default value is RGB(251, 0, 7), hex #fb0007
    /// </remarks>
    public Color? RedColor { get; set; }


    /// <summary>
    /// Default Bootstrap --bs-teal color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(32, 201, 151), hex #20c997
    /// </remarks>
    public Color? TealColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-yellow color. 
    /// </summary>
    /// <remarks>
    /// Default value is RGB(238, 238, 34), hex #eeee22
    /// </remarks>
    public Color? YellowColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-white color.
    /// </summary>
    /// <remarks>
    /// Default value is RGB(255, 255, 255), hex #ffffff, a.k.a. <see cref="Color.White"/>
    /// </remarks>
    public Color? WhiteColor { get; set; }

    #endregion

    #region Default Bootstrap Event colors

    /// <summary>
    /// Default Bootstrap --bs-primary color. 
    /// </summary>
    /// <remarks>
    /// Default value is RGB(30, 115, 190), hex #1e73be
    /// </remarks>
    public Color? PrimaryColor { get; set; }

    internal IReadOnlyDictionary<int, Color> PrimaryColors { get; set; } = default!;


    /// <summary>
    /// Default Bootstrap --bs-secondary color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="Gray600Color"/>
    /// </remarks>
    public Color? SecondaryColor { get; set; }

    internal IReadOnlyDictionary<int, Color> SecondaryColors { get; set; } = default!;
    
    /// <summary>
    /// Default Bootstrap --bs-danger color. 
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="RedColor"/>
    /// </remarks>
    public Color? DangerColor { get; set; }

    internal IReadOnlyDictionary<int, Color> DangerColors { get; set; } = default!;


    /// <summary>
    /// Default Bootstrap --bs-dark color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="Gray900Color"/>
    /// </remarks>
    public Color? DarkColor { get; set; }

    internal IReadOnlyDictionary<int, Color> DarkColors { get; set; } = default!;

    /// <summary>
    /// Default Bootstrap --bs-info color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="CyanColor"/>
    /// </remarks>
    public Color? InfoColor { get; set; }

    internal IReadOnlyDictionary<int, Color> InfoColors { get; set; } = default!;

    /// <summary>
    /// Default Bootstrap --bs-light color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="Gray100Color"/>
    /// </remarks>
    public Color? LightColor { get; set; }

    internal IReadOnlyDictionary<int, Color> LightColors { get; set; } = default!;

    /// <summary>
    /// Default Bootstrap --bs-success color. 
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="GreenColor"/>
    /// </remarks>
    public Color? SuccessColor { get; set; }

    internal IReadOnlyDictionary<int, Color> SuccessColors { get; set; } = default!;

    /// <summary>
    /// Default Bootstrap --bs-warning color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="YellowColor"/>
    /// </remarks>
    public Color? WarningColor { get; set; }

    internal IReadOnlyDictionary<int, Color> WarningColors { get; set; } = default!;

    #endregion

    #region Default Bootstrap Fonts

    /// <summary>
    /// Default value for --bs-font-sans-serif
    /// </summary>
    /// <remarks>
    /// Default value is
    /// <codee>
    /// SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
    /// </codee>
    /// </remarks>
    public string? FontMonospace { get; set; }


    /// <summary>
    /// Default value for --bs-font-sans-serif
    /// </summary>
    /// <remarks>
    /// Default value is
    /// <codee>
    /// system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", "Noto Sans", "Liberation Sans", Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol", "Noto Color Emoji"
    /// </codee>
    /// </remarks>
    public string? FontSansSerif { get; set; }

    #endregion


    #region Body

    /// <summary>
    /// Default value for --bs-body-font-family
    /// </summary>
    /// <remarks>
    /// Default value is a reference to <see cref="FontSansSerif"/>, resulting in var(--bs-font-sans-serif) being rendered.
    /// </remarks>
    public string? BodyFontFamily { get; set; }

    /// <summary>
    /// Default value for --bs-body-font-size
    /// </summary>
    /// <remarks>
    /// Default value is 1rem
    /// </remarks>
    public CssPropertyValue? BodyFontSize { get; set; }

    /// <summary>
    /// Default value for --bs-body-font-weight
    /// </summary>
    /// <remarks>
    /// Default value is 400
    /// </remarks>
    public ushort? BodyFontWeight { get; set; }

    /// <summary>
    /// Default value for --bs-body-line-height
    /// </summary>
    /// <remarks>
    /// Default value is 1.5
    /// </remarks>
    public CssPropertyValue? BodyLineHeight { get; set; }
    
    
    #endregion

    #region Border
    
    /// <summary>
    /// Default Bootstrap --bs-border-width
    /// </summary>
    /// <remarks>
    /// Default value is 1px
    /// </remarks>
    public CssPropertyValue? BorderWidth { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-style
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Solid"/>
    /// </remarks>
    public CssStyleEnum? BorderStyle { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-radius
    /// </summary>
    /// <remarks>
    /// Default value is 0.375rem
    /// </remarks>
    public CssPropertyValue? BorderRadius { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-radius-sm
    /// </summary>
    /// <remarks>
    /// Default value is 0.25rem
    /// </remarks>
    public CssPropertyValue? BorderRadiusSm { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-radius-lg
    /// </summary>
    /// <remarks>
    /// Default value is 0.5rem
    /// </remarks>
    public CssPropertyValue? BorderRadiusLg { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-radius-xl
    /// </summary>
    /// <remarks>
    /// Default value is 1rem
    /// </remarks>
    public CssPropertyValue? BorderRadiusXl { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-radius-xxl
    /// </summary>
    /// <remarks>
    /// Default value is 2rem
    /// </remarks>
    public CssPropertyValue? BorderRadiusXxl { get; set; }

    /// <summary>
    /// Default Bootstrap  --bs-border-radius-pill
    /// </summary>
    /// <remarks>
    /// Default value is 50rem
    /// </remarks>
    public CssPropertyValue? BorderRadiusPill { get; set; }

    #endregion

    #region Box-Shadow
 
    /// <summary>
    /// Default Bootstrap --bs-box-shadow
    /// </summary>
    /// <remarks>
    /// Default value is 0 .5rem 1rem with the color derived from <see cref="BlackColor"/> at 15% opacity
    /// </remarks>
    public string? BoxShadow { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-box-shadow-sm
    /// </summary>
    /// <remarks>
    /// Default value is 0 1rem 3rem with the color derived from <see cref="BlackColor"/> at 7.5% opacity
    /// </remarks>
    public string? BoxShadowSm { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-box-shadow-lg
    /// </summary>
    /// <remarks>
    /// Default value is 0 1rem 3rem with the color derived from <see cref="BlackColor"/> at 17.5% opacity
    /// </remarks>
    public string? BoxShadowLg { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-box-shadow-inset
    /// </summary>
    /// <remarks>
    /// Default value is inset 0 1px 2px with the color derived from <see cref="BlackColor"/> at 7.5% opacity
    /// </remarks>
    public string? BoxShadowInset { get; set; }

    #endregion

    #region BreakPoints

    /// <summary>
    /// Define the minimum dimensions at which your layout will change, adapting to different screen sizes, for use in media queries. <br/>
    /// This property is for the xs breakpoint. (Tiny screens)
    /// </summary>
    /// <remarks>
    /// Default value is 0
    /// </remarks>
    public CssPropertyValue? BreakPointXs { get; set; }

    /// <summary>
    /// Define the minimum dimensions at which your layout will change, adapting to different screen sizes, for use in media queries. <br/>
    /// This property is for the sm breakpoint. (Small screens)
    /// </summary>
    /// <remarks>
    /// Default value is 576px
    /// </remarks>
    public CssPropertyValue? BreakPointSm { get; set; }

    /// <summary>
    /// Define the minimum dimensions at which your layout will change, adapting to different screen sizes, for use in media queries. <br/>
    /// This property is for the md breakpoint. (Medium screens)
    /// </summary>
    /// <remarks>
    /// Default value is 768px
    /// </remarks>
    public CssPropertyValue? BreakPointMd { get; set; }

    /// <summary>
    /// Define the minimum dimensions at which your layout will change, adapting to different screen sizes, for use in media queries. <br/>
    /// This property is for the lg breakpoint. (Large screens)
    /// </summary>
    /// <remarks>
    /// Default value is 992px
    /// </remarks>
    public CssPropertyValue? BreakPointLg { get; set; }

    /// <summary>
    /// Define the minimum dimensions at which your layout will change, adapting to different screen sizes, for use in media queries. <br/>
    /// This property is for the xl breakpoint. (Extra large screens)
    /// </summary>
    /// <remarks>
    /// Default value is 1200px
    /// </remarks>
    public CssPropertyValue? BreakPointXl { get; set; }

    /// <summary>
    /// Define the minimum dimensions at which your layout will change, adapting to different screen sizes, for use in media queries. <br/>
    /// This property is for the xxl breakpoint. (Extra, extra large screens)
    /// </summary>
    /// <remarks>
    /// Default value is 1400px
    /// </remarks>
    public CssPropertyValue? BreakPointXxl { get; set; }

    #endregion

    #region Button

    /// <summary>
    /// Default Bootstrap --bs-btn-padding-x
    /// </summary>
    /// <remarks>
    /// Default value is 0.75rem
    /// </remarks>
    public CssPropertyValue? BtnPaddingX { get; set; }
    /// <summary>
    /// Default Bootstrap --bs-btn-padding-y
    /// </summary>
    /// <remarks>
    /// Default value is 0.375rem
    /// </remarks>
    public CssPropertyValue? BtnPaddingY { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-btn-font-family
    /// </summary>
    /// <remarks>
    /// By default, this property is not set.
    /// </remarks>
    public string? BtnFontFamily { get; set; }
    
    /// <summary>
    /// Default Bootstrap --bs-btn-font-size
    /// </summary>
    /// <remarks>
    /// Default value is 1rem
    /// </remarks>
    public CssPropertyValue? BtnFontSize { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-btn-font-weight
    /// </summary>
    /// <remarks>
    /// Default value is 400
    /// </remarks>
    public CssPropertyValue? BtnFontWeight { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-btn-line-height
    /// </summary>
    /// <remarks>
    /// Default value is 1.5
    /// </remarks>
    public CssPropertyValue? BtnLineHeight { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-btn-border-width 
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="BorderWidth"/>
    /// </remarks>
    public CssPropertyValue? BtnBorderWidth { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-btn-border-radius
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="BorderRadius"/>
    /// </remarks>
    public CssPropertyValue? BtnBorderRadius { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-btn-border-style
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Solid" />
    /// </remarks>
    public CssStyleEnum? BtnBorderStyle { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-btn-box-shadow
    /// </summary>
    /// <remarks>
    /// Default value is inset 0 1px 0 rgba(255, 255, 255, 0.15), 0 1px 1px rgba(0, 0, 0, 0.075);
    /// </remarks>
    public string? BtnBoxShadow { get; set; }
    /// <summary>
    /// Default Bootstrap --bs-btn-focus-box-shadow
    /// </summary>
    /// <remarks>
    /// Default value is 0 0 0 0.25rem rgba(var(--bs-btn-focus-shadow-rgb), .5);
    /// </remarks>
    public string? BtnFocusBoxShadow { get; set; }

    /// <summary>
    /// Default Bootstrap .btn display property
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.InlineBlock"/>
    /// </remarks>
    public CssPropertyValue? BtnDisplay { get; set; }

    /// <summary>
    /// Default Bootstrap .btn text-align property
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Center"/>
    /// </remarks>
    public CssStyleEnum? BtnTextAlign { get; set; }

    #endregion

    #region Card

    /// <summary>
    /// Default Bootstrap --bs-card-spacer-x
    /// </summary>
    /// <remarks>
    /// Default value is 1rem
    /// </remarks>
    public CssPropertyValue? CardSpacerX { get; set; }
    
    /// <summary>
    /// Default Bootstrap --bs-card-spacer-y
    /// </summary>
    /// <remarks>
    /// Default value is 1rem
    /// </remarks>
    public CssPropertyValue? CardSpacerY { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-title-spacer-y
    /// </summary>
    /// <remarks>
    /// Default value is 0.5rem
    /// </remarks>
    public CssPropertyValue? CardTitleSpacerY { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-border-width
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="BorderWidth"/>
    /// </remarks>
    public CssPropertyValue? CardBorderWidth { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-border-radius
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="BorderRadius"/>
    /// </remarks>
    public CssPropertyValue? CardBorderRadius { get; set; }

    /// <summary>
    /// Default Bootstrap .card border-style property
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Solid"/>
    /// </remarks>
    public CssPropertyValue? CardBorderStyle { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-box-shadow
    /// </summary>
    /// <remarks>
    /// Default value is to leave it empty.
    /// </remarks>
    public string? CardBoxShadow { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-inner-border-radius
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="BorderRadius"/> subtracted by <see cref="BorderWidth"/>
    /// </remarks>
    public CssPropertyValue? CardInnerBorderRadius { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-cap-padding-x
    /// </summary>
    /// <remarks>
    /// Default value is 1rem
    /// </remarks>
    public CssPropertyValue? CardCapPaddingX { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-cap-padding-y
    /// </summary>
    /// <remarks>
    /// Default value is 0.5rem;
    /// </remarks>
    public CssPropertyValue? CardCapPaddingY { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-height
    /// </summary>
    /// <remarks>
    /// Default value is to leave it empty.
    /// </remarks>
    public CssPropertyValue? CardHeight { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-img-overlay-padding
    /// </summary>
    /// <remarks>
    /// Default value is 1rem
    /// </remarks>
    public CssPropertyValue? CardImgOverlayPadding { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-img-overlay-border-radius
    /// </summary>
    /// <remarks>
    /// Default value is 0.75rem 
    /// </remarks>
    public CssPropertyValue? CardGroupMargin { get; set; }

    /// <summary>
    /// Default value for .card position property
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Relative"/>
    /// </remarks>
    public CssPropertyValue? CardPosition { get; set; }

    /// <summary>
    /// Default value for .card display property
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Flex"/>
    /// </remarks>
    public CssPropertyValue? CardDisplay { get; set; }

    /// <summary>
    /// Default value for .card flex-direction property. <br/>
    /// Will be applied to the card if <see cref="CardDisplay"/> is set to <see cref="CssStyleEnum.Flex"/>
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Column"/>
    /// </remarks>
    public CssPropertyValue? CardFlexDirection { get; set; }

    /// <summary>
    /// Default value for .card min-width property.
    /// </summary>
    /// <remarks>
    /// Default value is 0
    /// </remarks>
    public CssPropertyValue? CardMinWidth { get; set; }

    /// <summary>
    /// Default value for .card word-wrap property.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.BreakWord"/>
    /// </remarks>
    public CssPropertyValue? CardWordWrap { get; set; }

    /// <summary>
    /// Default value for .card background-clip property.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.BorderBox"/>
    /// </remarks>
    public CssPropertyValue? CardBackgroundClip { get; set; }

    #endregion

    #region :Focus
 
    /// <summary>
    /// Default Bootstrap --bs-focus-ring-width
    /// </summary>
    /// <remarks>
    /// Default value is 0.25rem
    /// </remarks>
    public CssPropertyValue? FocusRingWidth { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-focus-ring-opacity
    /// </summary>
    /// <remarks>
    /// Default value is 0.25
    /// </remarks>
    public CssPropertyValue? FocusRingOpacity { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-focus-ring-color
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="PrimaryColor"/> on 25% opacity
    /// </remarks>
    public Color? FocusRingColor { get; set; }

    #endregion

    #region Links
    
    /// <summary>
    /// Default Bootstrap --bs-link-decoration 
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Underline"/>
    /// </remarks>
    public CssStyleEnum? LinkDecoration { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-link-shade-percentage
    /// </summary>
    /// <remarks>
    /// Default value is 20%
    /// </remarks>
    public CssPropertyValue? LinkShadePercentage { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-stretched-link-z-index
    /// </summary>
    /// <remarks>
    /// Default value is 1
    /// </remarks>
    public int? StretchedLinkZIndex { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-icon-link-gap
    /// </summary>
    /// <remarks>
    /// Default value is 0.375rem
    /// </remarks>
    public CssPropertyValue? IconLinkGap { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-icon-link-underline-offset
    /// </summary>
    /// <remarks>
    /// Default value is 0.25em
    /// </remarks>
    public CssPropertyValue? IconLinkUnderlineOffset { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-icon-link-size
    /// </summary>
    /// <remarks>
    /// Default value is 1em
    /// </remarks>
    public CssPropertyValue? IconLinkSize { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-icon-link-transition
    /// </summary>
    /// <remarks>
    /// Default value is "0.2s ease-in-out transform"
    /// </remarks>
    public string? IconLinkTransition { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-icon-link-transform
    /// </summary>
    /// <remarks>
    /// Default value is "translate3d(.25em, 0, 0)"
    /// </remarks>
    public string? IconLinkTransform { get; set; }


    #endregion


    #region Texts

    /// <summary>
    /// To be applied on the background-color of .text-bg-* components.
    /// </summary>
    /// <remarks>
    /// Default values are derived from the default colors, multiplied by the opacity set in this property.
    /// </remarks>
    public CssColorSet TextBackgroundColors { get; set; } = new();

    /// <summary>
    /// To be applied to the text color of .text-bg-* components.
    /// </summary>
    /// <remarks>
    /// Default values are derived from the default colors, multiplied by the opacity set in this property.
    /// </remarks>
    public CssColorSet TextBackgroundTextColors { get; set; } = new();


    #endregion

    /// <summary>
    /// Default Bootstrap --bs-gradient value
    /// </summary>
    /// <remarks>
    /// The default value is "linear-gradient(180deg, rgba(255, 255, 255, 0.15), rgba(255, 255, 255, 0));"
    /// </remarks>
    public string? Gradient { get; set; }
    

    /// <summary>
    /// Parses a user supplied settings class and sets the default values if not provided.
    /// </summary>
    /// <returns>The same settings class instance, now with the lacking fields supplied with their default values.</returns>
    internal BootstrapCssSettings Parse()
 {
        // Default colors
        BlackColor ??= Color.Black;
        BlueColor ??= Color.FromArgb(13, 110, 253);
        CyanColor ??= Color.FromArgb(13, 202, 240);
        
        GrayColor ??= Color.FromArgb(108, 117, 125);
        Gray100Color ??= Color.FromArgb(248, 249, 250);
        Gray200Color ??= Color.FromArgb(233, 236, 239);
        Gray300Color ??= Color.FromArgb(222, 226, 230);
        Gray400Color ??= Color.FromArgb(206, 212, 218);
        Gray500Color ??= Color.FromArgb(173, 181, 189);
        Gray600Color ??= GrayColor;
        Gray700Color ??= Color.FromArgb(73, 80, 87);
        Gray800Color ??= Color.FromArgb(52, 58, 64);
        Gray900Color ??= Color.FromArgb(33, 37, 41);
        GrayDarkColor ??= Gray800Color;

        GreenColor ??= Color.FromArgb(0, 128, 0);
        IndigoColor ??= Color.FromArgb(102, 16, 242);
        OrangeColor ??= Color.FromArgb(245, 107, 19);
        PinkColor ??= Color.FromArgb(214, 51, 132);
        PurpleColor ??= Color.FromArgb(68, 75, 137);
        RedColor ??= Color.FromArgb(251, 0, 7);
        TealColor ??= Color.FromArgb(32, 201, 151);
        YellowColor ??= Color.FromArgb(238, 238, 34);
        WhiteColor ??= Color.White;

        // Colors based on events
        PrimaryColor ??= Color.FromArgb(30, 115, 190);
        PrimaryColors = new Dictionary<int, Color>()
        {
            { 100, PrimaryColor.Value.TintColor(0.8D)},
            { 200, PrimaryColor.Value.TintColor(0.6D)},
            { 300, PrimaryColor.Value.TintColor(0.4D)},
            { 400, PrimaryColor.Value.TintColor(0.2D)},
            { 500, PrimaryColor.Value },
            { 600, PrimaryColor.Value.ShadeColor(0.2D)},
            { 700, PrimaryColor.Value.ShadeColor(0.4D)},
            { 800, PrimaryColor.Value.ShadeColor(0.6D)},
            { 900, PrimaryColor.Value.ShadeColor(0.8D)}
        };

        SecondaryColor ??= Gray600Color;
        SecondaryColors = new Dictionary<int, Color>()
        {
            { 100, SecondaryColor.Value.TintColor(0.8D)},
            { 200, SecondaryColor.Value.TintColor(0.6D)},
            { 300, SecondaryColor.Value.TintColor(0.4D)},
            { 400, SecondaryColor.Value.TintColor(0.2D)},
            { 500, SecondaryColor.Value },
            { 600, SecondaryColor.Value.ShadeColor(0.2D)},
            { 700, SecondaryColor.Value.ShadeColor(0.4D)},
            { 800, SecondaryColor.Value.ShadeColor(0.6D)},
            { 900, SecondaryColor.Value.ShadeColor(0.8D)}
        };
        
        DangerColor ??= RedColor;
        DangerColors = new Dictionary<int, Color>()
        {
            { 100, DangerColor.Value.TintColor(0.8D)},
            { 200, DangerColor.Value.TintColor(0.6D)},
            { 300, DangerColor.Value.TintColor(0.4D)},
            { 400, DangerColor.Value.TintColor(0.2D)},
            { 500, DangerColor.Value },
            { 600, DangerColor.Value.ShadeColor(0.2D)},
            { 700, DangerColor.Value.ShadeColor(0.4D)},
            { 800, DangerColor.Value.ShadeColor(0.6D)},
            { 900, DangerColor.Value.ShadeColor(0.8D)}
        };
        
        DarkColor ??= Gray900Color;
        DarkColors = new Dictionary<int, Color>()
        {
            { 100, DarkColor.Value.TintColor(0.8D)},
            { 200, DarkColor.Value.TintColor(0.6D)},
            { 300, DarkColor.Value.TintColor(0.4D)},
            { 400, DarkColor.Value.TintColor(0.2D)},
            { 500, DarkColor.Value },
            { 600, DarkColor.Value.ShadeColor(0.2D)},
            { 700, DarkColor.Value.ShadeColor(0.4D)},
            { 800, DarkColor.Value.ShadeColor(0.6D)},
            { 900, DarkColor.Value.ShadeColor(0.8D)}
        };

        InfoColor ??= CyanColor;
        InfoColors = new Dictionary<int, Color>()
        {
            { 100, InfoColor.Value.TintColor(0.8D)},
            { 200, InfoColor.Value.TintColor(0.6D)},
            { 300, InfoColor.Value.TintColor(0.4D)},
            { 400, InfoColor.Value.TintColor(0.2D)},
            { 500, InfoColor.Value },
            { 600, InfoColor.Value.ShadeColor(0.2D)},
            { 700, InfoColor.Value.ShadeColor(0.4D)},
            { 800, InfoColor.Value.ShadeColor(0.6D)},
            { 900, InfoColor.Value.ShadeColor(0.8D)}
        };

        LightColor ??= Gray100Color;
        LightColors = new Dictionary<int, Color>()
        {
            { 100, LightColor.Value.TintColor(0.8D)},
            { 200, LightColor.Value.TintColor(0.6D)},
            { 300, LightColor.Value.TintColor(0.4D)},
            { 400, LightColor.Value.TintColor(0.2D)},
            { 500, LightColor.Value },
            { 600, LightColor.Value.ShadeColor(0.2D)},
            { 700, LightColor.Value.ShadeColor(0.4D)},
            { 800, LightColor.Value.ShadeColor(0.6D)},
            { 900, LightColor.Value.ShadeColor(0.8D)}
        };

        SuccessColor ??= GreenColor;
        SuccessColors = new Dictionary<int, Color>()
        {
            { 100, SuccessColor.Value.TintColor(0.8D)},
            { 200, SuccessColor.Value.TintColor(0.6D)},
            { 300, SuccessColor.Value.TintColor(0.4D)},
            { 400, SuccessColor.Value.TintColor(0.2D)},
            { 500, SuccessColor.Value },
            { 600, SuccessColor.Value.ShadeColor(0.2D)},
            { 700, SuccessColor.Value.ShadeColor(0.4D)},
            { 800, SuccessColor.Value.ShadeColor(0.6D)},
            { 900, SuccessColor.Value.ShadeColor(0.8D)}
        };

        WarningColor ??= YellowColor;
        WarningColors = new Dictionary<int, Color>()
        {
            { 100, WarningColor.Value.TintColor(0.8D)},
            { 200, WarningColor.Value.TintColor(0.6D)},
            { 300, WarningColor.Value.TintColor(0.4D)},
            { 400, WarningColor.Value.TintColor(0.2D)},
            { 500, WarningColor.Value },
            { 600, WarningColor.Value.ShadeColor(0.2D)},
            { 700, WarningColor.Value.ShadeColor(0.4D)},
            { 800, WarningColor.Value.ShadeColor(0.6D)},
            { 900, WarningColor.Value.ShadeColor(0.8D)}
        };

        // Default page colors
        Light.EmphasisColor ??= BlackColor;
        Dark.EmphasisColor ??= WhiteColor;
        Light.SecondaryBackgroundColor ??= Gray200Color;
        Dark.SecondaryBackgroundColor ??= Gray800Color;
        Light.SecondaryColor ??= Color.FromArgb(192, Gray600Color.Value);
        Dark.SecondaryColor ??= Color.FromArgb(192, Gray800Color.Value);
        Light.TertiaryBackgroundColor ??= Gray100Color;
        Dark.TertiaryBackgroundColor ??= Gray900Color;
        Light.TertiaryColor ??= Color.FromArgb(128, Gray600Color.Value);
        Dark.TertiaryColor ??= Color.FromArgb(128, Gray800Color.Value);

        // bg (subtle)
        Light.BgSubtleColors.Opacity ??= 255;
        Dark.BgSubtleColors.Opacity ??= 255;
        Light.BgSubtleColors.Primary ??= Color.FromArgb(Light.BgSubtleColors.Opacity.Value, PrimaryColors[100]);
        Dark.BgSubtleColors.Primary ??= Color.FromArgb(Dark.BgSubtleColors.Opacity.Value, PrimaryColors[900]);
        Light.BgSubtleColors.Secondary ??= Color.FromArgb(Light.BgSubtleColors.Opacity.Value, PrimaryColors[100]);
        Dark.BgSubtleColors.Secondary ??= Color.FromArgb(Dark.BgSubtleColors.Opacity.Value, SecondaryColors[900]);
        Light.BgSubtleColors.Danger ??= Color.FromArgb(Light.BgSubtleColors.Opacity.Value, DangerColors[100]);
        Dark.BgSubtleColors.Danger ??= Color.FromArgb(Dark.BgSubtleColors.Opacity.Value, DangerColors[900]);
        Light.BgSubtleColors.Dark ??= Color.FromArgb(Dark.BgSubtleColors.Opacity.Value, DarkColors[100]);
        Dark.BgSubtleColors.Dark ??= Color.FromArgb(Dark.BgSubtleColors.Opacity.Value, DarkColors[900]);
        Light.BgSubtleColors.Light ??= Color.FromArgb(Light.BgSubtleColors.Opacity.Value, LightColors[100]);
        Dark.BgSubtleColors.Light ??= Color.FromArgb(Dark.BgSubtleColors.Opacity.Value, LightColors[900]);
        Light.BgSubtleColors.Info ??= Color.FromArgb(Light.BgSubtleColors.Opacity.Value, InfoColors[100]);
        Dark.BgSubtleColors.Info ??= Color.FromArgb(Dark.BgSubtleColors.Opacity.Value, InfoColors[900]);
        Light.BgSubtleColors.Success ??= Color.FromArgb(Light.BgSubtleColors.Opacity.Value, SuccessColors[100]);
        Dark.BgSubtleColors.Success ??= Color.FromArgb(Dark.BgSubtleColors.Opacity.Value, SuccessColors[900]);
        Light.BgSubtleColors.Warning ??= Color.FromArgb(Light.BgSubtleColors.Opacity.Value, WarningColors[100]);
        Dark.BgSubtleColors.Warning ??= Color.FromArgb(Dark.BgSubtleColors.Opacity.Value, WarningColors[900]);

        // body
        Light.BodyBgColor ??= WhiteColor;
        Dark.BodyBgColor ??= Gray900Color;
        Light.BodyTextColor ??= Gray900Color;
        Dark.BodyTextColor ??= Gray300Color;
        BodyFontFamily ??= "var(--bs-font-sans-serif)";
        BodyFontSize ??= CssPropertyValue.Rem(1);
        BodyFontWeight ??= 400;
        BodyLineHeight ??= CssPropertyValue.RawNumber(1.5f);
        // borders
        BorderWidth ??= CssPropertyValue.Pixels(1);
        BorderStyle ??= CssStyleEnum.Solid;
        Light.BorderColor ??= Gray300Color;
        Dark.BorderColor ??= Gray700Color;
        Light.BorderColorTranslucent ??= Color.FromArgb(45, BlackColor.Value);
        Dark.BorderColorTranslucent ??= Color.FromArgb((int)(256f * 0.175f), WhiteColor.Value);
        BorderRadius ??= CssPropertyValue.Rem(0.375f);
        BorderRadiusSm ??= CssPropertyValue.Rem(0.25f);
        BorderRadiusLg ??= CssPropertyValue.Rem(0.5f);
        BorderRadiusXl ??= CssPropertyValue.Rem(1);
        BorderRadiusXxl ??= CssPropertyValue.Rem(2);
        BorderRadiusPill ??= CssPropertyValue.Rem(50);


        // border colors (subtle)
        Light.BorderSubtleColors.Opacity ??= 255;
        Dark.BorderSubtleColors.Opacity ??= 255;
        Light.BorderSubtleColors.Primary ??= Color.FromArgb(Light.BorderSubtleColors.Opacity.Value, PrimaryColors[200]);
        Dark.BorderSubtleColors.Primary ??= Color.FromArgb(Dark.BorderSubtleColors.Opacity.Value, WarningColors[800]);
        Light.BorderSubtleColors.Secondary ??= Color.FromArgb(Light.BorderSubtleColors.Opacity.Value, SecondaryColors[200]);
        Dark.BorderSubtleColors.Secondary ??= Color.FromArgb(Dark.BorderSubtleColors.Opacity.Value, WarningColors[800]);
        Light.BorderSubtleColors.Danger ??= Color.FromArgb(Light.BorderSubtleColors.Opacity.Value, DangerColors[200]);
        Dark.BorderSubtleColors.Danger ??= Color.FromArgb(Dark.BorderSubtleColors.Opacity.Value, WarningColors[800]);
        Light.BorderSubtleColors.Dark ??= Color.FromArgb(Light.BorderSubtleColors.Opacity.Value, DarkColors[200]);
        Dark.BorderSubtleColors.Dark ??= Color.FromArgb(Light.BorderSubtleColors.Opacity.Value, DarkColors[800]);
        Light.BorderSubtleColors.Light ??= Color.FromArgb(Light.BorderSubtleColors.Opacity.Value, LightColors[200]);
        Dark.BorderSubtleColors.Light ??= Color.FromArgb(Dark.BorderSubtleColors.Opacity.Value, LightColors[800]);
        Light.BorderSubtleColors.Info ??= Color.FromArgb(Light.BorderSubtleColors.Opacity.Value, InfoColors[200]);
        Dark.BorderSubtleColors.Info ??= Color.FromArgb(Dark.BorderSubtleColors.Opacity.Value, InfoColors[800]);
        Light.BorderSubtleColors.Success ??= Color.FromArgb(Light.BorderSubtleColors.Opacity.Value, SuccessColors[200]);
        Dark.BorderSubtleColors.Success ??= Color.FromArgb(Dark.BorderSubtleColors.Opacity.Value, SuccessColors[800]);
        Light.BorderSubtleColors.Warning ??= Color.FromArgb(Light.BorderSubtleColors.Opacity.Value, WarningColors[200]);
        Dark.BorderSubtleColors.Warning ??= Color.FromArgb(Dark.BorderSubtleColors.Opacity.Value, WarningColors[800]);

        // box-shadow
        BoxShadow ??= $"0 .5rem 1rem rgba({BlackColor.Value.ToRgbStringValues()}, 0.15)";
        BoxShadowSm ??= $"0 1rem 3rem rgba({BlackColor.Value.ToRgbStringValues()}, 0.075)";
        BoxShadowLg ??= $"0 1rem 3rem rgba({BlackColor.Value.ToRgbStringValues()}, 0.175)";
        BoxShadowInset ??= $"inset 0 1px 2px rgba({BlackColor.Value.ToRgbStringValues()}, 0.075)";


        // breakpoints
        BreakPointXs ??= CssPropertyValue.RawNumber(0);
        BreakPointSm ??= CssPropertyValue.Pixels(576);
        BreakPointMd ??= CssPropertyValue.Pixels(768);
        BreakPointLg ??= CssPropertyValue.Pixels(992);
        BreakPointXl ??= CssPropertyValue.Pixels(1200);
        BreakPointXxl ??= CssPropertyValue.Pixels(1400);

        // Buttons
        BtnPaddingX ??= CssPropertyValue.Rem(0.75f);
        BtnPaddingY ??= CssPropertyValue.Rem(0.375f);
        BtnFontSize ??= CssPropertyValue.Rem(1);
        BtnFontWeight ??= 400;
        BtnLineHeight ??= CssPropertyValue.RawNumber(1.5f);
        BtnBorderWidth ??= BorderWidth;
        BtnBorderRadius ??= BorderRadius;
        BtnBorderStyle ??= CssStyleEnum.Solid;
        BtnBoxShadow ??= "inset 0 1px 0 rgba(255, 255, 255, 0.15), 0 1px 1px rgba(0, 0, 0, 0.075)";
        BtnFocusBoxShadow ??= "0 0 0 0.25rem rgba(var(--bs-btn-focus-shadow-rgb), .5)";
        BtnDisplay ??= CssStyleEnum.InlineBlock;
        BtnTextAlign ??= CssStyleEnum.Center;
        Light.ButtonTextColor ??= Light.BodyTextColor;
        Dark.ButtonTextColor ??= Dark.BodyTextColor;
        Light.ButtonBorderColor ??= Color.Transparent;
        Dark.ButtonBorderColor ??= Color.Transparent;
        Light.ButtonBackgroundColor ??= Color.Transparent;
        Dark.ButtonBackgroundColor ??= Color.Transparent;
        Light.ButtonHoverBorderColor ??= Color.Transparent;
        Dark.ButtonHoverBorderColor ??= Color.Transparent;


        // Cards
        CardSpacerX ??= CssPropertyValue.Rem(1);
        CardSpacerY ??= CssPropertyValue.Rem(1);
        CardTitleSpacerY ??= CssPropertyValue.Rem(0.5f);
        CardBorderWidth ??= BorderWidth;
        CardBorderRadius ??= BorderRadius;
        CardBorderStyle ??= CssStyleEnum.Solid;
        CardBoxShadow ??= "";
        CardInnerBorderRadius ??= BorderRadius.Value.Value - BorderWidth.Value.Value;
        CardCapPaddingX ??= CssPropertyValue.Rem(1);
        CardCapPaddingY ??= CssPropertyValue.Rem(0.5f); 
        CardImgOverlayPadding ??= CssPropertyValue.Rem(1);
        CardGroupMargin ??= CssPropertyValue.Rem(0.5f);
        CardPosition ??= CssStyleEnum.Relative;
        CardDisplay ??= CssStyleEnum.Flex;
        CardFlexDirection ??= CssStyleEnum.Column;
        CardMinWidth ??= CssPropertyValue.Pixels(0);
        CardWordWrap ??= CssStyleEnum.BreakWord;
        CardBackgroundClip ??= CssStyleEnum.BorderBox;
        Light.CardBorderColor ??= Light.BorderColorTranslucent;
        Dark.CardBorderColor ??= Dark.BorderColorTranslucent;
        Light.CardCapBgColor ??= Color.FromArgb(8, Light.BodyTextColor.Value);
        Dark.CardCapBgColor ??= Color.FromArgb(8, Dark.BodyTextColor.Value);
        Light.CardBgColor ??= Light.BodyBgColor.Value;
        Dark.CardBgColor ??= Dark.BodyBgColor.Value;

        // code
        Light.CodeColor ??= PinkColor;
Dark.CodeColor ??= Color.FromArgb(230, 133, 181);

        // Focus
        FocusRingWidth ??= CssPropertyValue.Rem(0.25f);
        FocusRingOpacity ??= CssPropertyValue.RawNumber(0.25f);
        FocusRingColor ??= Color.FromArgb(64, PrimaryColor.Value);
        
        // Forms
        Light.FormValidColor ??= GreenColor;
        Dark.FormValidColor ??= Color.FromArgb(102, 179, 102);
        Light.FormValidBorderColor ??= GreenColor;
        Dark.FormValidBorderColor ??= Color.FromArgb(102, 179, 102);
        
        Light.FormInvalidColor ??= RedColor;
        Dark.FormInvalidColor ??= Color.FromArgb(253, 102, 106);
        Light.FormInvalidBorderColor ??= RedColor;
        Dark.FormInvalidBorderColor ??= Color.FromArgb(253, 102, 106);

        // Fonts
        FontSansSerif ??= @"system-ui, -apple-system, ""Segoe UI"", Roboto, ""Helvetica Neue"", ""Noto Sans"", ""Liberation Sans"", Arial, sans-serif, ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol"", ""Noto Color Emoji""";
        FontMonospace ??= @"SFMono-Regular, Menlo, Monaco, Consolas, ""Liberation Mono"", ""Courier New"", monospace";

        // Gradient
        Gradient ??= "linear-gradient(180deg, rgba(255, 255, 255, 0.15), rgba(255, 255, 255, 0));";
        
        // highlight
        Light.HighlightBackgroundColor ??= Color.FromArgb(252, 252, 211);
        Dark.HighlightBackgroundColor ??= Color.FromArgb(95, 95, 14);
        Light.HighlightColor ??= Gray900Color;
        Dark.HighlightColor ??= Gray300Color;

        // Links
        Light.LinkColor ??= PrimaryColor;
        Dark.LinkColor ??= Color.FromArgb(120, 171, 216);
        LinkDecoration ??= CssStyleEnum.Underline;
        LinkShadePercentage ??= CssPropertyValue.Percentage(20);
        Light.LinkHoverColor ??= Light.LinkColor.Value.ShadeColor(LinkShadePercentage.Value.Value * 0.01f);
        Dark.LinkHoverColor ??= Dark.LinkColor.Value.TintColor(LinkShadePercentage.Value.Value * 0.01f);
        
          StretchedLinkZIndex ??= 1;
        IconLinkGap ??= CssPropertyValue.Pixels(0.375f);
        IconLinkUnderlineOffset ??= CssPropertyValue.Rem(0.25f);
        IconLinkSize ??= CssPropertyValue.Rem(1);
        IconLinkTransition ??= "0.2s ease-in-out transform";
        IconLinkTransform ??= "translate3d(.25em, 0, 0)";

        // texts
        TextBackgroundColors.Opacity ??= 255;
        TextBackgroundColors.Primary ??= Color.FromArgb(TextBackgroundColors.Opacity.Value, PrimaryColor.Value);
        TextBackgroundColors.Secondary ??= Color.FromArgb(TextBackgroundColors.Opacity.Value, SecondaryColor.Value);
        TextBackgroundColors.Success ??= Color.FromArgb(TextBackgroundColors.Opacity.Value, SuccessColor.Value);
        TextBackgroundColors.Danger ??= Color.FromArgb(TextBackgroundColors.Opacity.Value, DangerColor.Value);
        TextBackgroundColors.Dark ??= Color.FromArgb(TextBackgroundColors.Opacity.Value, DarkColor.Value);
        TextBackgroundColors.Light ??= Color.FromArgb(TextBackgroundColors.Opacity.Value, LightColor.Value);
        TextBackgroundColors.Info ??= Color.FromArgb(TextBackgroundColors.Opacity.Value, InfoColor.Value);
        TextBackgroundColors.Warning ??= Color.FromArgb(TextBackgroundColors.Opacity.Value, WarningColor.Value);

        TextBackgroundTextColors.Opacity ??= 255;
        TextBackgroundTextColors.Primary ??= Color.FromArgb(TextBackgroundTextColors.Opacity.Value, WhiteColor.Value);
        TextBackgroundTextColors.Secondary ??= Color.FromArgb(TextBackgroundTextColors.Opacity.Value, WhiteColor.Value);
        TextBackgroundTextColors.Success ??= Color.FromArgb(TextBackgroundTextColors.Opacity.Value, WhiteColor.Value);
        TextBackgroundTextColors.Danger ??= Color.FromArgb(TextBackgroundTextColors.Opacity.Value, BlackColor.Value);
        TextBackgroundTextColors.Dark ??= Color.FromArgb(TextBackgroundTextColors.Opacity.Value, WhiteColor.Value);
        TextBackgroundTextColors.Light ??= Color.FromArgb(TextBackgroundTextColors.Opacity.Value, BlackColor.Value);
        TextBackgroundTextColors.Info ??= Color.FromArgb(TextBackgroundTextColors.Opacity.Value, WhiteColor.Value);
        TextBackgroundTextColors.Warning ??= Color.FromArgb(TextBackgroundTextColors.Opacity.Value, BlackColor.Value);

        // text (emphasis)
        Light.TextEmphasisColors.Opacity ??= 255;
        Dark.TextEmphasisColors.Opacity ??= 255;
        Light.TextEmphasisColors.Danger ??= Color.FromArgb(Light.TextEmphasisColors.Opacity.Value, DangerColors[700]);
        Dark.TextEmphasisColors.Danger ??= Color.FromArgb(Dark.TextEmphasisColors.Opacity.Value, DangerColors[300]);
        Light.TextEmphasisColors.Dark ??= Color.FromArgb(Light.TextEmphasisColors.Opacity.Value, DarkColors[700]);
        Dark.TextEmphasisColors.Dark ??= Color.FromArgb(Dark.TextEmphasisColors.Opacity.Value, DarkColors[300]);
        Light.TextEmphasisColors.Info ??= Color.FromArgb(Light.TextEmphasisColors.Opacity.Value, InfoColors[700]);
        Dark.TextEmphasisColors.Info ??= Color.FromArgb(Dark.TextEmphasisColors.Opacity.Value, InfoColors[300]);
        Light.TextEmphasisColors.Light ??= Color.FromArgb(Light.TextEmphasisColors.Opacity.Value, Gray700Color.Value);
        Dark.TextEmphasisColors.Light ??= Color.FromArgb(Dark.TextEmphasisColors.Opacity.Value, Gray300Color.Value);
        Light.TextEmphasisColors.Primary ??= Color.FromArgb(Light.TextEmphasisColors.Opacity.Value, PrimaryColors[700]);
        Dark.TextEmphasisColors.Primary ??= Color.FromArgb(Dark.TextEmphasisColors.Opacity.Value, PrimaryColors[300]);
        Light.TextEmphasisColors.Secondary ??= Color.FromArgb(Light.TextEmphasisColors.Opacity.Value, SecondaryColors[700]);
        Dark.TextEmphasisColors.Secondary ??= Color.FromArgb(Dark.TextEmphasisColors.Opacity.Value, SecondaryColors[300]);
        Light.TextEmphasisColors.Success ??= Color.FromArgb(Light.TextEmphasisColors.Opacity.Value, SuccessColors[700]);
        Dark.TextEmphasisColors.Success ??= Color.FromArgb(Dark.TextEmphasisColors.Opacity.Value, SuccessColors[300]);
        Light.TextEmphasisColors.Warning ??= Color.FromArgb(Light.TextEmphasisColors.Opacity.Value, WarningColors[700]);
        Dark.TextEmphasisColors.Warning ??= Color.FromArgb(Dark.TextEmphasisColors.Opacity.Value, WarningColors[300]);

        return this;
    }
}

/// <summary>
/// Specifies the color theme for the <see cref="BootstrapCssSettings"/>
/// </summary>
public sealed class BootstrapCssSettingsColorTheme
{
    #region Default Bootstrap Colors

    /// <summary>
    /// Default Bootstrap --bs-emphasis color.
    /// </summary>
    /// <remarks>
    /// Default value for light theme is derived from <see cref="BootstrapCssSettings.BlackColor"/>
    /// </remarks>
    public Color? EmphasisColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-secondary color.
    /// </summary>
    /// <remarks>
    /// Default value for light theme is derived from <see cref="BootstrapCssSettings.Gray600Color"/> at 75% opacity
    /// </remarks>
    public Color? SecondaryColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-secondary-bg-color 
    /// </summary>
    /// <remarks>
    /// Default value for light theme is <see cref="BootstrapCssSettings.Gray200Color"/>
    /// </remarks>
    public Color? SecondaryBackgroundColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-tertiary color.
    /// </summary>
    /// <remarks>
    /// Default value for light theme is derived from <see cref="BootstrapCssSettings.Gray600Color"/> at 50% opacity
    /// </remarks>
    public Color? TertiaryColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-tertiary-bg-color 
    /// </summary>
    /// <remarks>
    /// Default value for light theme is <see cref="BootstrapCssSettings.Gray100Color"/>
    /// </remarks>
    public Color? TertiaryBackgroundColor { get; set; }

    #endregion

    #region Default Bootstrap Background colors

    /// <summary>
    /// Default bootstrap --bs-*-bg-subtle colors.
    /// </summary>
    /// <remarks>
    /// Default values are in the 100/900 ranges than the original color, with the provided opacity.
    /// </remarks>
    public CssColorSet BgSubtleColors { get; set; } = new();

    #endregion

    #region Default Bootstrap Border colors
    
    /// <summary>
    /// Default Bootstrap --bs-border-color
    /// </summary>
    /// <remarks>
    /// Default value for light mode is derived from <see cref="BootstrapCssSettings.Gray300Color"/>
    /// </remarks>
    public Color? BorderColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-color-translucent
    /// </summary>
    /// <remarks>
    /// Default value for light mode is derived from <see cref="BootstrapCssSettings.BlackColor"/> at 17.5% opacity
    /// </remarks>
    public Color? BorderColorTranslucent { get; set; }

    #region subtle colors

    /// <summary>
    /// Default Bootstrap --bs-*-border-subtle color.
    /// </summary>
    /// <remarks>
    /// Default values in the 200/800 ranges than the original color, with the provided opacity.
    /// </remarks>
    public CssColorSet BorderSubtleColors { get; set; } = new();

    #endregion

    #endregion

    #region Default Bootstrap Text colors

    #region Emphasis

    /// <summary>
    /// Default Bootstrap --bs-*-text-emphasis colors
    /// </summary>
    /// <remarks>
    /// Default values are the original colors multiplied by the provided opacity value.
    /// </remarks>
    public CssColorSet TextEmphasisColors { get; set; } = new();

    #endregion

    #endregion

    #region Body

    /// <summary>
    /// Default value for --bs-body-background
    /// </summary>
    /// <remarks>
    /// Default value for light mode is <see cref="BootstrapCssSettings.WhiteColor"/>
    /// </remarks>
    public Color? BodyBgColor { get; set; }

    /// <summary>
    /// Default value for --bs-body-color
    /// </summary>
    /// <remarks>
    /// Default value for light mode is <see cref="BootstrapCssSettings.Gray900Color"/>
    /// </remarks>
    public Color? BodyTextColor { get; set; }

    #endregion

    #region Button

    /// <summary>
    /// Default Bootstrap --bs-btn-color
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="BodyTextColor"/>
    /// </remarks>
    public Color? ButtonTextColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-btn-border-color
    /// </summary>
    /// <remarks>
    /// Default value is transparent.
    /// </remarks>
    public Color? ButtonBorderColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-btn-hover-border-color
    /// </summary>
    /// <remarks>
    /// Default value is transparent.
    /// </remarks>
    public Color? ButtonHoverBorderColor { get; set; }
    
    /// <summary>
    /// Default Bootstrap --bs-btn-bg-color
    /// </summary>
    /// <remarks>
    /// Default value is transparent.
    /// </remarks>
    public Color? ButtonBackgroundColor { get; set; }

    #endregion

    #region Card

    /// <summary>
    /// Default Bootstrap --bs-card-title-color
    /// </summary>
    /// <remarks>
    /// The default value is to leave it empty. <br />
    /// Will be applied to <see cref="CardTitle"/> unless <see cref="Card.Color"/> has been specified.
    /// </remarks>
    public Color? CardTitleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-subtitle-color
    /// </summary>
    /// <remarks>
    /// The default value is to leave it empty. <br/>
    /// </remarks>
    public Color? CardSubTitleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-border-color
    /// </summary>
    /// <remarks>
    /// The default value is derived from <see cref="BorderColorTranslucent" /> <br/>
    /// Will be applied to <see cref="Card"/> unless <see cref="Card.Color"/> has been specified.
    /// </remarks>
    public Color? CardBorderColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-cap-bg-color
    /// </summary>
    /// <remarks>
    /// The default color is derived from <see cref="BodyTextColor"/> at 3% opacity. <br/>
    /// Will be applied to <see cref="CardHeader"/>, unless <see cref="Card.Color"/> has been specified.
    /// </remarks>
    public Color? CardCapBgColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-cap-color
    /// </summary>
    /// <remarks>
    /// The default value is to leave it empty. <br/>
    /// Will be applied to <see cref="CardHeader"/>, unless <see cref="Card.Color"/> has been specified.
    /// </remarks>
    public Color? CardCapTextColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-color
    /// </summary>
    /// <remarks>
    /// The default value is to leave it empty. <br/>
    /// Will be applied to <see cref="Card"/> unless <see cref="Card.Color"/> has been specified.
    /// </remarks>
    public Color? CardTextColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-card-bg
    /// </summary>
    /// <remarks>
    /// The default value is derived from <see cref="BodyBgColor" /> <br/>
    /// Will be applied to <see cref="Card"/> unless <see cref="Card.Color"/> has been specified.
    /// </remarks>
    public Color? CardBgColor { get; set; }

    #endregion

    #region Forms

    /// <summary>
    /// Default Bootstrap --bs-form-valid-color text color.
    /// </summary>
    /// <remarks>
    /// The default value for light mode will be derived from <see cref="BootstrapCssSettings.GreenColor"/>
    /// </remarks>
    public Color? FormValidColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-form-valid-border-color text color.
    /// </summary>
    /// <remarks>
    /// The default value for light mode will be derived from <see cref="BootstrapCssSettings.GreenColor"/>
    /// </remarks>
    public Color? FormValidBorderColor { get; set; }


    /// <summary>
    /// Default Bootstrap --bs-form-invalid-color text color.
    /// </summary>
    /// <remarks>
    /// The default value for light mode will be derived from <see cref="BootstrapCssSettings.RedColor"/>
    /// </remarks>
    public Color? FormInvalidColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-form-invalid-border-color text color.
    /// </summary>
    /// <remarks>
    /// The default value for light mode will be derived from <see cref="BootstrapCssSettings.RedColor"/>
    /// </remarks>
    public Color? FormInvalidBorderColor { get; set; }


    #endregion
    
    #region Links

    /// <summary>
    /// Default Bootstrap --bs-link-color
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="BootstrapCssSettings.PrimaryColor"/>
    /// </remarks>
    public Color? LinkColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-link-hover-color
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="LinkColor"/> darkened by <see cref="BootstrapCssSettings.LinkShadePercentage"/>
    /// </remarks>
    public Color? LinkHoverColor { get; set; }

    #endregion

    /// <summary>
    /// Default Bootstrap --bs-code-color.
    /// </summary>
    /// <remarks>
    /// The default value for light mode will be derived from <see cref="BootstrapCssSettings.PinkColor"/>
    /// </remarks>
    public Color? CodeColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-highlight-bg value
    /// </summary>
    /// <remarks>
    /// The default value for light mode is derived from <see cref="BootstrapCssSettings.Gray700Color"/>  
    /// </remarks>
    public Color? HighlightBackgroundColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-highlight-color value
    /// </summary>
    /// <remarks>
    /// The default value for light mode is derived from <see cref="BootstrapCssSettings.Gray900Color"/>  
    /// </remarks>
    public Color? HighlightColor { get; set; }
}
