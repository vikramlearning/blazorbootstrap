namespace BlazorBootstrap;

/// <summary>
/// Uploads an image selected in a <see cref="RichTextEditor" />.
/// </summary>
public delegate Task<RichTextEditorImageUploadResult?> RichTextEditorImageUploadDelegate(RichTextEditorImageUploadRequest request);
