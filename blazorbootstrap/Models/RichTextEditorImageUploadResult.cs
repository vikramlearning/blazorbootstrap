namespace BlazorBootstrap;

/// <summary>
/// Represents the result of a <see cref="RichTextEditor" /> image upload.
/// </summary>
public sealed class RichTextEditorImageUploadResult
{
    #region Constructors

    public RichTextEditorImageUploadResult(string url) => Url = url;

    #endregion

    #region Properties, Indexers

    /// <summary>Gets the URL to insert into the editor.</summary>
    public string Url { get; }

    #endregion
}
