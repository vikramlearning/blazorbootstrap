namespace BlazorBootstrap;

public class PdfViewerEventArgs
{
    #region Constructors

    public PdfViewerEventArgs(int currentPage, int totalPages)
    {
        CurrentPage = currentPage;
        TotalPages = totalPages;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int CurrentPage { get; }

    /// <summary>
    /// Gets the total pages count.
    /// </summary>
    public int TotalPages { get; }

    #endregion
}
