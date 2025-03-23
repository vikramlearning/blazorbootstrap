namespace BlazorBootstrap;

public partial class GridSummaryColumn<TItem> : BlazorBootstrapComponentBase
{
    /// <summary>
    /// Gets or sets the summary column type.
    /// <para>
    /// Default value is <see cref="GridSummaryColumnType.None"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public GridSummaryColumnType SummaryType { get; set; } = GridSummaryColumnType.None;

    /// <summary>
    /// Gets or sets the property name.
    /// <para>
    /// Default value is <see langword="null"/>.
    /// </para>
    /// </summary>
    [Parameter]
    public string PropertyName { get; set; } = default!;
}
