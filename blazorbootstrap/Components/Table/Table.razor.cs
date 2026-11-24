namespace BlazorBootstrap;

/// <summary>
/// Renders a read-only Bootstrap 5.3 table from <see cref="Data" />, one row per item, with columns declared as
/// <see cref="TableColumn{TItem}" /> children. It is the lightweight sibling of <see cref="Grid{TItem}" />: no paging, sorting,
/// filtering, selection, or data providers.
/// </summary>
/// <typeparam name="TItem">The type of each row item.</typeparam>
[AddedVersion("4.1.0")]
public partial class Table<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    /// <summary>
    /// The number of placeholder rows shown while <see cref="Data" /> is <see langword="null" /> and no <see cref="LoadingTemplate" /> is set.
    /// </summary>
    private const int PlaceholderRowCount = 5;

    private readonly List<TableColumn<TItem>> columns = new();

    #endregion

    #region Methods

    #region Internal Methods

    /// <summary>
    /// Registers a column in declaration order and re-renders the table, which cannot see its columns until they initialize.
    /// </summary>
    internal void AddColumn(TableColumn<TItem> column)
    {
        if (columns.Contains(column))
            return;

        columns.Add(column);
        StateHasChanged();
    }

    /// <summary>
    /// Re-renders the table after a column parameter that changes its markup, such as <see cref="TableColumn{TItem}.IsVisible" />, changed.
    /// </summary>
    internal void ColumnChanged() => StateHasChanged();

    /// <summary>
    /// Removes a disposed column and re-renders the table.
    /// </summary>
    internal void RemoveColumn(TableColumn<TItem> column)
    {
        if (columns.Remove(column))
            StateHasChanged();
    }

    /// <summary>
    /// Returns <see langword="null" /> for an empty class list, so the <c>class</c> attribute is left out.
    /// </summary>
    internal static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    #endregion

    #region Private Methods

    private string? GetRowClassNames(TItem item) =>
        NullIfEmpty(BuildClassNames(RowClass?.Invoke(item),
            ((RowColor?.Invoke(item) ?? TableColor.None).ToTableColorClass(), RowColor is not null),
            (BootstrapClass.TableActive, RowActive?.Invoke(item) ?? false)));

    private List<TableColumn<TItem>> GetVisibleColumns() => columns.Where(column => column.IsVisible).ToList();

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the border color of a bordered table (Bootstrap <c>border-*</c> utilities).
    /// <para>
    /// Default value is <see cref="TableBorderColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TableBorderColor.None)]
    [Description("Gets or sets the border color of a bordered table (Bootstrap <b>border-*</b> utilities).")]
    [Parameter]
    public TableBorderColor BorderColor { get; set; } = TableBorderColor.None;

    /// <summary>
    /// Gets or sets whether the table has borders on all sides of the table and its cells (<c>table-bordered</c>).
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the table has borders on all sides of the table and its cells (<b>table-bordered</b>).")]
    [Parameter]
    public bool Bordered { get; set; }

    /// <summary>
    /// Gets or sets whether the table has no borders (<c>table-borderless</c>).
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the table has no borders (<b>table-borderless</b>).")]
    [Parameter]
    public bool Borderless { get; set; }

    /// <summary>
    /// Gets or sets the caption text, rendered as the table's <c>&lt;caption&gt;</c>. <see cref="CaptionTemplate" /> wins when both are set.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the caption text, rendered as the table's caption. <b>CaptionTemplate</b> wins when both are set.")]
    [Parameter]
    public string? Caption { get; set; }

    /// <summary>
    /// Gets or sets the CSS class added to the <c>&lt;caption&gt;</c> element.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS class added to the caption element.")]
    [Parameter]
    public string? CaptionClass { get; set; }

    private string? CaptionClassNames =>
        NullIfEmpty(BuildClassNames(CaptionClass, ("visually-hidden", VisuallyHiddenCaption)));

    private string CaptionId => $"{Id}-caption";

    /// <summary>
    /// Gets or sets the caption content, rendered as the table's <c>&lt;caption&gt;</c>.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the caption content, rendered as the table's caption.")]
    [Parameter]
    public RenderFragment? CaptionTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether the caption shows above the table (<c>caption-top</c>) instead of below it.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the caption shows above the table (<b>caption-top</b>) instead of below it.")]
    [Parameter]
    public bool CaptionTop { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="TableColumn{TItem}" /> children that declare the columns.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the <b>TableColumn</b> children that declare the columns.")]
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override string? ClassNames =>
        BuildClassNames(Class,
            (BootstrapClass.Table, true),
            (Color.ToTableColorClass(), Color != TableColor.None),
            (BootstrapClass.TableStriped, Striped),
            (BootstrapClass.TableStripedColumns, StripedColumns),
            (BootstrapClass.TableHover, Hover),
            (BootstrapClass.TableBordered, Bordered),
            (BootstrapClass.TableBorderless, Borderless),
            (BorderColor.ToTableBorderColorClass(), BorderColor != TableBorderColor.None),
            (BootstrapClass.TableSmall, Small),
            (VerticalAlignment.ToTableVerticalAlignmentClass(), VerticalAlignment != TableVerticalAlignment.None),
            (BootstrapClass.TableCaptionTop, CaptionTop));

    /// <summary>
    /// Gets or sets the color variant of the whole table (<c>table-*</c>).
    /// <para>
    /// Default value is <see cref="TableColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TableColor.None)]
    [Description("Gets or sets the color variant of the whole table (<b>table-*</b>).")]
    [Parameter]
    public TableColor Color { get; set; } = TableColor.None;

    private string ContainerClassNames =>
        BuildClassNames(TableContainerClass,
            (BootstrapClass.TableResponsive, ResponsiveBreakpoint == TableResponsiveBreakpoint.None),
            (ResponsiveBreakpoint.ToTableResponsiveClass(), ResponsiveBreakpoint != TableResponsiveBreakpoint.None));

    /// <summary>
    /// Gets or sets the rows, one per item. While it is <see langword="null" />, the table shows its loading state;
    /// when it is empty, it shows <see cref="EmptyText" /> or <see cref="EmptyDataTemplate" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the rows, one per item. While it is <b>null</b>, the table shows its loading state; when it is empty, it shows <b>EmptyText</b> or <b>EmptyDataTemplate</b>.")]
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>
    /// Gets or sets the content of the single row shown when <see cref="Data" /> is empty. Wins over <see cref="EmptyText" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of the single row shown when <b>Data</b> is empty. Wins over <b>EmptyText</b>.")]
    [Parameter]
    public RenderFragment? EmptyDataTemplate { get; set; }

    /// <summary>
    /// Gets or sets the text of the single row shown when <see cref="Data" /> is empty.
    /// <para>
    /// Default value is "No records to display".
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue("No records to display")]
    [Description("Gets or sets the text of the single row shown when <b>Data</b> is empty.")]
    [Parameter]
    public string EmptyText { get; set; } = "No records to display";

    /// <summary>
    /// Gets or sets the footer rows (<c>&lt;tr&gt;</c> elements) written by the consumer, rendered inside <c>&lt;tfoot&gt;</c> in place
    /// of the per-column footers. Use it for labels that span columns or for several footer rows. Like the per-column footers,
    /// it is hidden while <see cref="Data" /> is <see langword="null" /> or empty.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the footer rows written by the consumer, rendered inside tfoot in place of the per-column footers. Use it for labels that span columns or for several footer rows. It is hidden while <b>Data</b> is <b>null</b> or empty.")]
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether the table body puts a thicker border between itself and the head (<c>table-group-divider</c>).
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the table body puts a thicker border between itself and the head (<b>table-group-divider</b>).")]
    [Parameter]
    public bool GroupDivider { get; set; }

    private bool HasCaption => CaptionTemplate is not null || !string.IsNullOrWhiteSpace(Caption);

    private string? HeadClassNames => NullIfEmpty(HeaderColor.ToTableColorClass());

    /// <summary>
    /// Gets or sets the color variant of the table head (<c>table-*</c> on <c>&lt;thead&gt;</c>), such as <see cref="TableColor.Light" /> or <see cref="TableColor.Dark" />.
    /// <para>
    /// Default value is <see cref="TableColor.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TableColor.None)]
    [Description("Gets or sets the color variant of the table head (<b>table-*</b> on thead), such as <b>Light</b> or <b>Dark</b>.")]
    [Parameter]
    public TableColor HeaderColor { get; set; } = TableColor.None;

    /// <summary>
    /// Gets or sets whether body rows highlight on hover (<c>table-hover</c>).
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether body rows highlight on hover (<b>table-hover</b>).")]
    [Parameter]
    public bool Hover { get; set; }

    private bool IsResponsive => Responsive || ResponsiveBreakpoint != TableResponsiveBreakpoint.None;

    /// <summary>
    /// Gets or sets the content shown in the body while <see cref="Data" /> is <see langword="null" />, in place of the placeholder rows.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content shown in the body while <b>Data</b> is <b>null</b>, in place of the placeholder rows.")]
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether the table scrolls horizontally inside its own wrapper at every width (<c>table-responsive</c>).
    /// <see cref="ResponsiveBreakpoint" /> wins when it is set.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the table scrolls horizontally inside its own wrapper at every width (<b>table-responsive</b>). <b>ResponsiveBreakpoint</b> wins when it is set.")]
    [Parameter]
    public bool Responsive { get; set; }

    /// <summary>
    /// Gets or sets the width below which the table scrolls horizontally inside its own wrapper (<c>table-responsive-*</c>).
    /// <para>
    /// Default value is <see cref="TableResponsiveBreakpoint.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TableResponsiveBreakpoint.None)]
    [Description("Gets or sets the width below which the table scrolls horizontally inside its own wrapper (<b>table-responsive-*</b>).")]
    [Parameter]
    public TableResponsiveBreakpoint ResponsiveBreakpoint { get; set; } = TableResponsiveBreakpoint.None;

    /// <summary>
    /// Gets or sets the function that marks a row active (<c>table-active</c>).
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the function that marks a row active (<b>table-active</b>).")]
    [Parameter]
    public Func<TItem, bool>? RowActive { get; set; }

    /// <summary>
    /// Gets or sets the function that returns custom CSS classes for a row.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the function that returns custom CSS classes for a row.")]
    [Parameter]
    public Func<TItem, string>? RowClass { get; set; }

    /// <summary>
    /// Gets or sets the function that returns a row's color variant (<c>table-*</c>).
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the function that returns a row's color variant (<b>table-*</b>).")]
    [Parameter]
    public Func<TItem, TableColor>? RowColor { get; set; }

    /// <summary>
    /// Gets or sets the function that returns a unique key for each row, so rows are kept, not re-created, when the same records
    /// come back as new instances. Keys must be unique. When it is not set, rows have no key and are matched by position.
    /// Example usage: <c>RowKeySelector="invoice => invoice.Id"</c>.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the function that returns a unique key for each row, so rows are kept, not re-created, when the same records come back as new instances. Keys must be unique. When it is not set, rows have no key and are matched by position.")]
    [Parameter]
    public Func<TItem, object>? RowKeySelector { get; set; }

    /// <summary>
    /// Gets or sets whether the table uses compact cell padding (<c>table-sm</c>).
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the table uses compact cell padding (<b>table-sm</b>).")]
    [Parameter]
    public bool Small { get; set; }

    /// <summary>
    /// Gets or sets whether body rows have zebra stripes (<c>table-striped</c>).
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether body rows have zebra stripes (<b>table-striped</b>).")]
    [Parameter]
    public bool Striped { get; set; }

    /// <summary>
    /// Gets or sets whether columns have zebra stripes (<c>table-striped-columns</c>).
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether columns have zebra stripes (<b>table-striped-columns</b>).")]
    [Parameter]
    public bool StripedColumns { get; set; }

    /// <summary>
    /// Gets or sets the CSS class added to the responsive wrapper, which renders only when <see cref="Responsive" /> or
    /// <see cref="ResponsiveBreakpoint" /> is set.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS class added to the responsive wrapper, which renders only when <b>Responsive</b> or <b>ResponsiveBreakpoint</b> is set.")]
    [Parameter]
    public string? TableContainerClass { get; set; }

    /// <summary>
    /// Gets or sets the inline style of the responsive wrapper, which renders only when <see cref="Responsive" /> or
    /// <see cref="ResponsiveBreakpoint" /> is set.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the inline style of the responsive wrapper, which renders only when <b>Responsive</b> or <b>ResponsiveBreakpoint</b> is set.")]
    [Parameter]
    public string? TableContainerStyle { get; set; }

    /// <summary>
    /// Gets or sets the vertical alignment of every cell (<c>align-*</c>). A column can override it.
    /// <para>
    /// Default value is <see cref="TableVerticalAlignment.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TableVerticalAlignment.None)]
    [Description("Gets or sets the vertical alignment of every cell (<b>align-*</b>). A column can override it.")]
    [Parameter]
    public TableVerticalAlignment VerticalAlignment { get; set; } = TableVerticalAlignment.None;

    /// <summary>
    /// Gets or sets whether the caption is kept for screen readers only (<c>visually-hidden</c>).
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether the caption is kept for screen readers only (<b>visually-hidden</b>).")]
    [Parameter]
    public bool VisuallyHiddenCaption { get; set; }

    #endregion
}
