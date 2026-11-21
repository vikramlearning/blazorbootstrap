namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Foundation;

/// <summary>
/// The stored part of one list's saved view.
/// </summary>
public sealed class ShowcaseGridViewState
{
    #region Properties, Indexers

    /// <summary>Gets or sets the grid's own settings: filters, page, page size, column order, and hidden columns.</summary>
    public GridSettings? Settings { get; set; }

    /// <summary>Gets or sets the sort direction of <see cref="SortKey" />.</summary>
    public SortDirection SortDirection { get; set; }

    /// <summary>Gets or sets the <c>SortString</c> of the sorted column, or <see langword="null" /> for the list's default sort.</summary>
    public string? SortKey { get; set; }

    #endregion
}
