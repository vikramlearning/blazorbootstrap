namespace BlazorBootstrap;

/// <summary>
/// Defines the width below which a <see cref="Table{TItem}" /> scrolls horizontally inside its wrapper (Bootstrap <c>table-responsive-*</c>).
/// </summary>
public enum TableResponsiveBreakpoint
{
    /// <summary>
    /// Indicates no breakpoint: <see cref="Table{TItem}.Responsive" /> decides whether the table scrolls at every width.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates no breakpoint: Responsive decides whether the table scrolls at every width.")]
    None,

    /// <summary>
    /// Indicates scrolling below the small breakpoint (<c>table-responsive-sm</c>, 576px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates scrolling below the small breakpoint (table-responsive-sm, 576px).")]
    Small,

    /// <summary>
    /// Indicates scrolling below the medium breakpoint (<c>table-responsive-md</c>, 768px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates scrolling below the medium breakpoint (table-responsive-md, 768px).")]
    Medium,

    /// <summary>
    /// Indicates scrolling below the large breakpoint (<c>table-responsive-lg</c>, 992px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates scrolling below the large breakpoint (table-responsive-lg, 992px).")]
    Large,

    /// <summary>
    /// Indicates scrolling below the extra large breakpoint (<c>table-responsive-xl</c>, 1200px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates scrolling below the extra large breakpoint (table-responsive-xl, 1200px).")]
    ExtraLarge,

    /// <summary>
    /// Indicates scrolling below the extra extra large breakpoint (<c>table-responsive-xxl</c>, 1400px).
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("Indicates scrolling below the extra extra large breakpoint (table-responsive-xxl, 1400px).")]
    ExtraExtraLarge
}
