
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

    /// <summary>
    /// Default Bootstrap --bs-secondary color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="Gray600Color"/>
    /// </remarks>
    public Color? SecondaryColor { get; set; }


    /// <summary>
    /// Default Bootstrap --bs-danger color. 
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="RedColor"/>
    /// </remarks>
    public Color? DangerColor { get; set; }


    /// <summary>
    /// Default Bootstrap --bs-dark color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="Gray900Color"/>
    /// </remarks>
    public Color? DarkColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-info color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="CyanColor"/>
    /// </remarks>
    public Color? InfoColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-light color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="Gray100Color"/>
    /// </remarks>
    public Color? LightColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-success color. 
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="GreenColor"/>
    /// </remarks>
    public Color? SuccessColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-warning color.
    /// </summary>
    /// <remarks>
    /// Default value derived from <see cref="YellowColor"/>
    /// </remarks>
    public Color? WarningColor { get; set; }

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
    /// Default Bootstrap --bs-border-width-*
    /// </summary>
    /// <remarks>
    /// Default value is 1: 1px, 2: 2px, 3: 3px, 4: 4px, 5: 5px
    /// </remarks>
    public Dictionary<int,CssPropertyValue>? BorderWidths { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-style
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Solid"/>
    /// </remarks>
    public CssStyleEnum? BorderStyle { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-color
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="Gray300Color"/>
    /// </remarks>
    public Color? BorderColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-border-color-translucent
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="BlackColor"/> at 17.5% opacity
    /// </remarks>
    public Color? BorderColorTranslucent { get; set; }

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

    #region Containers

    /// <summary>
    /// Define the maximum width of `.container` for different screen sizes. <br/>
    /// This property is for the sm breakpoint. (Small screens)
    /// </summary>
    /// <remarks>
    /// Default value is 540px
    /// </remarks>
    public CssPropertyValue? ContainerMaxWidthSm { get; set; }

    /// <summary>
    /// Define the maximum width of `.container` for different screen sizes. <br/>
    /// This property is for the md breakpoint. (Medium screens)
    /// </summary>
    /// <remarks>
    /// Default value is 720px
    /// </remarks>
    public CssPropertyValue? ContainerMaxWidthMd { get; set; }

    /// <summary>
    /// Define the maximum width of `.container` for different screen sizes. <br/>
    /// This property is for the lg breakpoint. (Large screens)
    /// </summary>
    /// <remarks>
    /// Default value is 960px
    /// </remarks>
    public CssPropertyValue? ContainerMaxWidthLg { get; set; }

    /// <summary>
    /// Define the maximum width of `.container` for different screen sizes. <br/>
    /// This property is for the xl breakpoint. (Extra large screens)
    /// </summary>
    /// <remarks>
    /// Default value is 1140px
    /// </remarks>
    public CssPropertyValue? ContainerMaxWidthXl { get; set; }

    /// <summary>
    /// Define the maximum width of `.container` for different screen sizes. <br/>
    /// This property is for the xxl breakpoint. (Extra, extra large screens)
    /// </summary>
    /// <remarks>
    /// Default value is 1320px
    /// </remarks>
    public CssPropertyValue? ContainerMaxWidthXxl { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-container-padding-x 
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="GutterWidth"/>
    /// </remarks>
    public CssPropertyValue? ContainerPaddingX { get; set; }

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
    /// Default Bootstrap --bs-link-hover-decoration
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.Null"/> 
    /// </remarks>
    public CssStyleEnum? LinkHoverDecoration { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-stretched-link-pseudo-element
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="CssStyleEnum.After"/>
    /// </remarks>
    public CssStyleEnum? StretchedLinkPseudoElement { get; set; }

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
    /// Default Bootstrap --bs-icon-link-icon-size
    /// </summary>
    /// <remarks>
    /// Default value is 1em
    /// </remarks>
    public CssPropertyValue? IconLinkIconSize { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-icon-link-icon-transition
    /// </summary>
    /// <remarks>
    /// Default value is "0.2s ease-in-out transform"
    /// </remarks>
    public string? IconLinkIconTransition { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-icon-link-icon-transform
    /// </summary>
    /// <remarks>
    /// Default value is "translate3d(.25em, 0, 0)"
    /// </remarks>
    public string? IconLinkIconTransform { get; set; }


    #endregion

    /// <summary>
    /// Default value for --bs-emphasis-color
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="BlackColor"/>
    /// </remarks>
    public Color? EmphasisColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-gradient value
    /// </summary>
    /// <remarks>
    /// The default value is "linear-gradient(180deg, rgba(255, 255, 255, 0.15), rgba(255, 255, 255, 0));"
    /// </remarks>
    public string? Gradient { get; set; }


    /// <summary>
    /// Default Gutter Width for Bootstrap --bs-gutter-width
    /// </summary>
    /// <remarks>
    /// Default value is 1.5rem
    /// </remarks>
    public CssPropertyValue? GutterWidth { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-paragraph-margin-bottom 
    /// </summary>
    /// <remarks>
    /// Default value is 1rem
    /// </remarks>
    public CssPropertyValue? ParagraphMarginBottom { get; set; }


    /// <summary>
    /// Parses a user supplied settings class and sets the default values if not provided.
    /// </summary>
    /// <returns>The same settings class instance, now with the lacking fields supplied.</returns>
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

        // text (emphasis)
        Light.DangerTextEmphasisColor ??= Color.FromArgb(100, 0, 3);
        Dark.DangerTextEmphasisColor ??=  Color.FromArgb(253, 102, 106);
        Light.DarkTextEmphasisColor ??= Gray700Color;
        Dark.DarkTextEmphasisColor ??= Gray300Color;
        Light.PrimaryTextEmphasisColor ??= Color.FromArgb(12, 46, 76);
        Dark.PrimaryTextEmphasisColor ??= Color.FromArgb(120, 171, 216);
        Light.SecondaryTextEmphasisColor ??= Color.FromArgb(43, 47, 50);
        Dark.SecondaryTextEmphasisColor ??= Gray600Color;
        Light.SuccessTextEmphasisColor ??= Color.FromArgb(0, 51, 0);
        Dark.SuccessTextEmphasisColor ??= Color.FromArgb(102, 179, 102);
        Light.InfoTextEmphasisColor ??= Color.FromArgb(5, 81, 96);
        Dark.InfoTextEmphasisColor ??= Color.FromArgb(110, 223, 246);
        Light.LightTextEmphasisColor ??= Gray700Color;
        Dark.LightTextEmphasisColor ??= Gray300Color;
        Light.WarningTextEmphasisColor ??= Color.FromArgb(95, 95, 14);
        Dark.WarningTextEmphasisColor ??= Color.FromArgb(245, 245, 122);

        // Colors based on events
        PrimaryColor ??= Color.FromArgb(30, 115, 190);
        SecondaryColor ??= Gray600Color;
        DangerColor ??= RedColor;
        DarkColor ??= Gray900Color;
        InfoColor ??= CyanColor;
        LightColor ??= Gray100Color;
        SuccessColor ??= GreenColor;
        WarningColor ??= YellowColor;
        
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
        Light.PrimaryBgSubtleColor ??= PrimaryColor.Value.TintColor(0.8);
        Dark.PrimaryBgSubtleColor ??= Color.FromArgb(6, 23, 38);
        Light.SecondaryBgSubtleColor ??= SecondaryColor.Value.TintColor(0.8);
        Dark.SecondaryBgSubtleColor ??= Color.FromArgb(22, 23, 25);
        Light.DangerBgSubtleColor ??= DangerColor.Value.TintColor(0.8);
        Dark.DangerBgSubtleColor ??= Color.FromArgb(50, 0, 1);
        Light.DarkBgSubtleColor ??= Gray400Color;
        Dark.DarkBgSubtleColor ??= Color.FromArgb(26, 29, 32);
        Light.LightBgSubtleColor ??= Gray100Color.Value.TintColor(0.8);
        Dark.LightBgSubtleColor ??= Gray800Color;
        Light.InfoBgSubtleColor ??= InfoColor.Value.TintColor(0.8);
        Dark.InfoBgSubtleColor ??= Color.FromArgb(3, 40, 48);
        Light.SuccessBgSubtleColor ??= SuccessColor.Value.TintColor(0.8);
        Dark.SuccessBgSubtleColor ??= Color.FromArgb(0, 26, 0);
        Light.WarningBgSubtleColor ??= WarningColor.Value.TintColor(0.8);
        Dark.WarningBgSubtleColor ??= Color.FromArgb(48, 48, 7);

        // body
        Light.BodyBackground ??= WhiteColor;
        Dark.BodyBackground ??= Gray900Color;
        Light.BodyColor ??= Gray900Color;
        Dark.BodyColor ??= Gray300Color;
        EmphasisColor ??= BlackColor;
        BodyFontFamily ??= "var(--bs-font-sans-serif)";
        BodyFontSize ??= CssPropertyValue.Rem(1);
        BodyFontWeight ??= 400;
        BodyLineHeight ??= CssPropertyValue.RawNumber(1.5f);
        Light.BodySecondaryBackgroundColor ??= Gray200Color;
        Light.BodySecondaryColor ??= Color.FromArgb(192, Light.BodyColor.Value);
        Light.BodyTertiaryBackgroundColor ??= Gray100Color;
        Light.BodyTertiaryColor ??= Color.FromArgb(128, Light.BodyColor.Value);
        Light.BodyEmphasisColor ??= BlackColor;

        // borders
        BorderWidth ??= CssPropertyValue.Pixels(1);
        BorderWidths ??= new Dictionary<int, CssPropertyValue>
        {
                       { 1, CssPropertyValue.Pixels(1) },
                                  { 2, CssPropertyValue.Pixels(2) },
                                             { 3, CssPropertyValue.Pixels(3) },
                                                        { 4, CssPropertyValue.Pixels(4) },
                                                                   { 5, CssPropertyValue.Pixels(5) }
                                                                          };
        BorderStyle ??= CssStyleEnum.Solid;
        BorderColor ??= Gray300Color;
        BorderColorTranslucent ??= Color.FromArgb((int)(256f * 0.175f), BlackColor.Value);
        BorderRadius ??= CssPropertyValue.Rem(0.375f);
        BorderRadiusSm ??= CssPropertyValue.Rem(0.25f);
        BorderRadiusLg ??= CssPropertyValue.Rem(0.5f);
        BorderRadiusXl ??= CssPropertyValue.Rem(1);
        BorderRadiusXxl ??= CssPropertyValue.Rem(2);
        BorderRadiusPill ??= CssPropertyValue.Rem(50);


        // border colors (subtle)
        Light.PrimaryBorderSubtleColor ??= PrimaryColor.Value.TintColor(0.6);
        Dark.PrimaryBorderSubtleColor ??= Color.FromArgb(18, 69, 114);
        Light.SecondaryBorderSubtleColor ??= SecondaryColor.Value.TintColor(0.6);
        Dark.SecondaryBorderSubtleColor ??= Color.FromArgb(65, 70, 75);
        Light.DangerBorderSubtleColor ??= DangerColor.Value.TintColor(0.6);
        Dark.DangerBorderSubtleColor ??= Color.FromArgb(151, 0, 4);
        Light.DarkBorderSubtleColor ??= Gray500Color;
        Dark.DarkBorderSubtleColor ??= Gray800Color;
        Light.LightBorderSubtleColor ??= Gray100Color;
        Dark.LightBorderSubtleColor ??= Gray700Color;
        Light.InfoBorderSubtleColor ??= InfoColor.Value.TintColor(0.6);
        Dark.InfoBorderSubtleColor ??= Color.FromArgb(087990);
        Light.SuccessBorderSubtleColor ??= SuccessColor.Value.TintColor(0.6);
        Dark.SuccessBorderSubtleColor ??= Color.FromArgb(0, 77, 0);
        Light.WarningBorderSubtleColor ??= WarningColor.Value.TintColor(0.6);
        Dark.WarningBgSubtleColor ??= Color.FromArgb(143, 143, 20);

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
        
        // Containers
ContainerMaxWidthSm ??= CssPropertyValue.Pixels(540);
        ContainerMaxWidthMd ??= CssPropertyValue.Pixels(720);
        ContainerMaxWidthLg ??= CssPropertyValue.Pixels(960);
        ContainerMaxWidthXl ??= CssPropertyValue.Pixels(1140);
        ContainerMaxWidthXxl ??= CssPropertyValue.Pixels(1320);
        GutterWidth ??= CssPropertyValue.Rem(1.5f);
        ContainerPaddingX ??= GutterWidth;

        // code
        Light.CodeColor ??= PinkColor;
Dark.CodeColor ??= Color.FromArgb(230, 133, 181);

        // Forms
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
        LinkHoverDecoration ??= CssStyleEnum.Null;
        StretchedLinkPseudoElement ??= CssStyleEnum.After;
        StretchedLinkZIndex ??= 1;
        IconLinkGap ??= CssPropertyValue.Pixels(0.375f);
        IconLinkUnderlineOffset ??= CssPropertyValue.Rem(0.25f);
        IconLinkIconSize ??= CssPropertyValue.Rem(1);
        IconLinkIconTransition ??= "0.2s ease-in-out transform";
        IconLinkIconTransform ??= "translate3d(.25em, 0, 0)";
        


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

    #region subtle

    /// <summary>
    /// Default Bootstrap --bs-primary-bg-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default is 80% lighter than <see cref="BootstrapCssSettings.PrimaryColor"/>
    /// </remarks>
    public Color? PrimaryBgSubtleColor { get; set; }


    /// <summary>
    /// Default Bootstrap --bs-secondary-bg-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default is 80% lighter than <see cref="BootstrapCssSettings.SecondaryColor"/>
    /// </remarks>
    public Color? SecondaryBgSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-danger-bg-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default is 80% lighter than <see cref="BootstrapCssSettings.DangerColor"/>
    /// </remarks>
    public Color? DangerBgSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-dark-bg-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default is <see cref="BootstrapCssSettings.Gray400Color"/>
    /// </remarks>
    public Color? DarkBgSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-light-bg-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default is 80% lighter than <see cref="BootstrapCssSettings.Gray100Color"/>
    /// </remarks>
    public Color? LightBgSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-info-bg-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default is 80% lighter than <see cref="BootstrapCssSettings.InfoColor"/>
    /// </remarks>
    public Color? InfoBgSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-success-bg-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default is 80% lighter than <see cref="BootstrapCssSettings.SuccessColor"/>
    /// </remarks>
    public Color? SuccessBgSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-warning-bg-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default is 80% lighter than <see cref="BootstrapCssSettings.WarningColor"/>
    /// </remarks>
    public Color? WarningBgSubtleColor { get; set; }

    #endregion

    #endregion

    #region Default Bootstrap Border colors


    #region subtle colors

    /// <summary>
    /// Default Bootstrap --bs-primary-border-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default value for light mode is  80% lighter than <see cref="BootstrapCssSettings.PrimaryColor"/>
    /// </remarks>
    public Color? PrimaryBorderSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-secondary-border-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default value for light mode is  60% lighter than <see cref="BootstrapCssSettings.SecondaryColor"/>
    /// </remarks>
    public Color? SecondaryBorderSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-danger-border-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default value for light mode is  60% lighter than <see cref="BootstrapCssSettings.DangerColor"/>
    /// </remarks>
    public Color? DangerBorderSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-dark-border-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default value for light mode is  <see cref="BootstrapCssSettings.Gray500Color"/>
    /// </remarks>
    public Color? DarkBorderSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-light-border-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default value for light mode is  <see cref="BootstrapCssSettings.Gray100Color"/>
    /// </remarks>
    public Color? LightBorderSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-info-border-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default value for light mode is 60% lighter than <see cref="BootstrapCssSettings.InfoColor"/>
    /// </remarks>
    public Color? InfoBorderSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-success-border-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default value for light mode is 60% lighter than <see cref="BootstrapCssSettings.SuccessColor"/>
    /// </remarks>
    public Color? SuccessBorderSubtleColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-warning-border-subtle color. 
    /// </summary>
    /// <remarks>
    /// Default value for light mode is 60% lighter than <see cref="BootstrapCssSettings.WarningColor"/>
    /// </remarks>
    public Color? WarningBorderSubtleColor { get; set; }
    
    #endregion
    
    #endregion

    #region Default Bootstrap Text colors

    #region Emphasis

    /// <summary>
    /// Default Bootstrap --bs-danger-text-emphasis color.
    /// </summary>
    /// <remarks>
    /// Default value for light mode is RGB(100, 0, 3), hex #640003
    /// </remarks>
    public Color? DangerTextEmphasisColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-dark-text-emphasis color.
    /// </summary>
    /// <remarks>
    /// Default value for light mode will be derived from <see cref="BootstrapCssSettings.Gray700Color"/>
    /// </remarks>
    public Color? DarkTextEmphasisColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-info-text-emphasis color.
    /// </summary>
    /// <remarks>
    /// Default value for light mode is RGB(5, 81, 96), hex #055160
    /// </remarks>
    public Color? InfoTextEmphasisColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-light-text-emphasis color.
    /// </summary>
    /// <remarks>
    /// Default value for light mode will be derived from <see cref="BootstrapCssSettings.Gray700Color"/>
    /// </remarks>
    public Color? LightTextEmphasisColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-primary-text-emphasis color.
    /// </summary>
    /// <remarks>
    /// Default value for light mode is RGB(12, 46, 76), hex #0c2e4c
    /// </remarks>
    public Color? PrimaryTextEmphasisColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-secondary-text-emphasis color.
    /// </summary>
    /// <remarks>
    /// Default value for light mode is RGB(43, 47, 50), hex #2b2f32
    /// </remarks>
    public Color? SecondaryTextEmphasisColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-success-text-emphasis color.
    /// </summary>
    /// <remarks>
    /// Default value  for light mode is RGB(0, 51, 0), hex #003300
    /// </remarks>
    public Color? SuccessTextEmphasisColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-warning-text-emphasis color.
    /// </summary>
    /// <remarks>
    /// Default value for light mode is RGB(95, 95, 14), hex #5f5f0e
    /// </remarks>
    public Color? WarningTextEmphasisColor { get; set; }

    #endregion

    #endregion

    #region Body

    /// <summary>
    /// Default value for --bs-body-background
    /// </summary>
    /// <remarks>
    /// Default value for light mode is <see cref="BootstrapCssSettings.WhiteColor"/>
    /// </remarks>
    public Color? BodyBackground { get; set; }

    /// <summary>
    /// Default value for --bs-body-color
    /// </summary>
    /// <remarks>
    /// Default value for light mode is <see cref="BootstrapCssSettings.Gray900Color"/>
    /// </remarks>
    public Color? BodyColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-body-secondary-bg-color
    /// </summary>
    /// <remarks>
    /// Default value for light mode is derived from <see cref="BootstrapCssSettings.Gray200Color"/>
    /// </remarks>
    public Color? BodySecondaryBackgroundColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-body-secondary-color
    /// </summary>
    /// <remarks>
    /// Default value for light mode is derived from <see cref="BodyColor"/> at 75% opacity
    /// </remarks>
    public Color? BodySecondaryColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-body-tertiary-bg-color
    /// </summary>
    /// <remarks>
    /// Default value for light mode is derived from <see cref="BootstrapCssSettings.Gray100Color"/>
    /// </remarks>
    public Color? BodyTertiaryBackgroundColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-body-tertiary-color
    /// </summary>
    /// <remarks>
    /// Default value for light mode is derived from <see cref="BodyColor"/> at 50% opacity
    /// </remarks>
    public Color? BodyTertiaryColor { get; set; }

    /// <summary>
    /// Default Bootstrap --bs-body-emphasis-color
    /// </summary>
    /// <remarks>
    /// Default value is derived from <see cref="BootstrapCssSettings.BlackColor"/>
    /// </remarks>
    public Color? BodyEmphasisColor { get; set; }


    #endregion

    #region Forms


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