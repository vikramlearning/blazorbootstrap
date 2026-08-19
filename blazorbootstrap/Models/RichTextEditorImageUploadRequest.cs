namespace BlazorBootstrap;

/// <summary>
/// Contains an image selected for upload from a <see cref="RichTextEditor" />.
/// </summary>
public sealed class RichTextEditorImageUploadRequest
{
    #region Constructors

    public RichTextEditorImageUploadRequest(IBrowserFile file, string altText, CancellationToken cancellationToken)
    {
        File = file;
        AltText = altText;
        CancellationToken = cancellationToken;
    }

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the selected image file.</summary>
    public IBrowserFile File { get; }

    /// <summary>Gets the alternate text supplied for the image.</summary>
    public string AltText { get; }

    /// <summary>Gets a token cancelled when the editor is disposed or another upload starts.</summary>
    public CancellationToken CancellationToken { get; }

    #endregion
}
