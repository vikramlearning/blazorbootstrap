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

    public int CurrentPage { get; }

    public int TotalPages { get; }

    #endregion
}
