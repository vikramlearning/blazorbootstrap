namespace BlazorBootstrap;

/// <summary>
/// Declares one column of a <see cref="Table{TItem}" />: its header, the cell rendered for each row item, and an optional footer cell.
/// The column renders nothing itself; its table renders the cells.
/// </summary>
/// <typeparam name="TItem">The type of each row item.</typeparam>
[AddedVersion("4.1.0")]
public partial class TableColumn<TItem> : BlazorBootstrapComponentBase
{
    #region Fields and Constants

    private bool isRegistered;

    #endregion

    #region Methods

    #region Protected Override Methods

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (isRegistered)
        {
            isRegistered = false;
            Parent?.RemoveColumn(this);
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        isRegistered = true;
        Parent.AddColumn(this);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (isRegistered)
            Parent.ColumnChanged();

        base.OnParametersSet();
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Returns the classes of this column's body cell for a row item: alignment, no-wrap, vertical alignment, color, active state,
    /// and <see cref="ColumnClass" />.
    /// </summary>
    internal string? GetCellClassNames(TItem item) =>
        Table<TItem>.NullIfEmpty(BuildClassNames(ColumnClass?.Invoke(item),
            (TextAlignment.ToTextAlignmentClass(), TextAlignment != Alignment.None),
            (BootstrapClass.TextNoWrap, TextNoWrap),
            (VerticalAlignment.ToTableVerticalAlignmentClass(), VerticalAlignment != TableVerticalAlignment.None),
            ((CellColor?.Invoke(item) ?? TableColor.None).ToTableColorClass(), CellColor is not null),
            (BootstrapClass.TableActive, CellActive?.Invoke(item) ?? false)));

    #endregion

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets or sets the function that marks this column's cell active (<c>table-active</c>) for a row item.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the function that marks this column's cell active (<b>table-active</b>) for a row item.")]
    [Parameter]
    public Func<TItem, bool>? CellActive { get; set; }

    /// <summary>
    /// Gets or sets the function that returns this column's cell color variant (<c>table-*</c>) for a row item.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the function that returns this column's cell color variant (<b>table-*</b>) for a row item.")]
    [Parameter]
    public Func<TItem, TableColor>? CellColor { get; set; }

    /// <summary>
    /// Gets or sets the content of this column's cell, rendered once per row with the row item as <c>context</c>.
    /// A column without it renders empty cells.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of this column's cell, rendered once per row with the row item as <b>context</b>. A column without it renders empty cells.")]
    [Parameter]
    public RenderFragment<TItem>? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the function that returns custom CSS classes for this column's cell in a row.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the function that returns custom CSS classes for this column's cell in a row.")]
    [Parameter]
    public Func<TItem, string>? ColumnClass { get; set; }

    internal string? FooterClassNames =>
        Table<TItem>.NullIfEmpty(BuildClassNames(
            (TextAlignment.ToTextAlignmentClass(), TextAlignment != Alignment.None),
            (BootstrapClass.TextNoWrap, TextNoWrap)));

    /// <summary>
    /// Gets or sets the content of this column's footer cell. Wins over <see cref="FooterText" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of this column's footer cell. Wins over <b>FooterText</b>.")]
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets the text of this column's footer cell.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the text of this column's footer cell.")]
    [Parameter]
    public string? FooterText { get; set; }

    internal bool HasFooter => FooterTemplate is not null || !string.IsNullOrEmpty(FooterText);

    /// <summary>
    /// Gets or sets the CSS class added to this column's header cell (<c>&lt;th&gt;</c>).
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the CSS class added to this column's header cell (th).")]
    [Parameter]
    public string? HeaderClass { get; set; }

    internal string? HeaderClassNames =>
        Table<TItem>.NullIfEmpty(BuildClassNames(HeaderClass,
            (HeaderTextAlignment.ToTextAlignmentClass(), HeaderTextAlignment != Alignment.None),
            (BootstrapClass.TextNoWrap, TextNoWrap)));

    /// <summary>
    /// Gets or sets the content of this column's header cell. Wins over <see cref="HeaderText" />.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the content of this column's header cell. Wins over <b>HeaderText</b>.")]
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Gets or sets the inline style of this column's header cell (<c>&lt;th&gt;</c>).
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the inline style of this column's header cell (th).")]
    [Parameter]
    public string? HeaderStyle { get; set; }

    /// <summary>
    /// Gets or sets the text of this column's header cell.
    /// <para>
    /// Default value is <see langword="null" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(null)]
    [Description("Gets or sets the text of this column's header cell.")]
    [Parameter]
    public string? HeaderText { get; set; }

    /// <summary>
    /// Gets or sets the horizontal alignment of this column's header cell. Every value except <see cref="Alignment.None" /> adds its
    /// <c>text-*</c> class, which overrides alignment set by page CSS; <see cref="Alignment.None" /> adds no class, so the cells
    /// keep the alignment they inherit.
    /// <para>
    /// Default value is <see cref="Alignment.Start" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(Alignment.Start)]
    [Description("Gets or sets the horizontal alignment of this column's header cell. Every value except <b>None</b> adds its <b>text-*</b> class, which overrides alignment set by page CSS; <b>None</b> adds no class, so the cells keep the alignment they inherit.")]
    [Parameter]
    public Alignment HeaderTextAlignment { get; set; } = Alignment.Start;

    /// <summary>
    /// Gets or sets whether this column's body cells are row headers (<c>&lt;th scope="row"&gt;</c>) instead of data cells.
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether this column's body cells are row headers (th scope=\"row\") instead of data cells.")]
    [Parameter]
    public bool IsRowHeader { get; set; }

    /// <summary>
    /// Gets or sets whether the column is shown. A hidden column renders no header, body, footer, or placeholder cell.
    /// Prefer it over wrapping the column in <c>@if</c>, which moves the column to the end when it comes back.
    /// <para>
    /// Default value is <see langword="true" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(true)]
    [Description("Gets or sets whether the column is shown. A hidden column renders no header, body, footer, or placeholder cell. Prefer it over wrapping the column in @if, which moves the column to the end when it comes back.")]
    [Parameter]
    public bool IsVisible { get; set; } = true;

    [CascadingParameter(Name = "Parent")]
    internal Table<TItem> Parent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the horizontal alignment of this column's body and footer cells. Every value except <see cref="Alignment.None" /> adds its
    /// <c>text-*</c> class, which overrides alignment set by page CSS; <see cref="Alignment.None" /> adds no class, so the cells
    /// keep the alignment they inherit.
    /// <para>
    /// Default value is <see cref="Alignment.Start" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(Alignment.Start)]
    [Description("Gets or sets the horizontal alignment of this column's body and footer cells. Every value except <b>None</b> adds its <b>text-*</b> class, which overrides alignment set by page CSS; <b>None</b> adds no class, so the cells keep the alignment they inherit.")]
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.Start;

    /// <summary>
    /// Gets or sets whether this column's header, body, and footer cells keep their text on one line (<c>text-nowrap</c>).
    /// <para>
    /// Default value is <see langword="false" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(false)]
    [Description("Gets or sets whether this column's header, body, and footer cells keep their text on one line (<b>text-nowrap</b>).")]
    [Parameter]
    public bool TextNoWrap { get; set; }

    /// <summary>
    /// Gets or sets the vertical alignment of this column's body cells (<c>align-*</c>); <see cref="TableVerticalAlignment.None" /> follows the table.
    /// <para>
    /// Default value is <see cref="TableVerticalAlignment.None" />.
    /// </para>
    /// </summary>
    [AddedVersion("4.1.0")]
    [DefaultValue(TableVerticalAlignment.None)]
    [Description("Gets or sets the vertical alignment of this column's body cells (<b>align-*</b>); <b>None</b> follows the table.")]
    [Parameter]
    public TableVerticalAlignment VerticalAlignment { get; set; } = TableVerticalAlignment.None;

    #endregion
}
