namespace BlazorBootstrap;

internal sealed class RichTextEditorJsInterop : JsInteropBase
{
    #region Fields and Constants

    public const string Clear = "clear";
    public const string Dispose = "dispose";
    public const string Execute = "execute";
    public const string Focus = "focus";
    public const string HideModal = "hideModal";
    public const string Initialize = "initialize";
    public const string PrepareUploadedImage = "prepareUploadedImage";
    public const string ShowImageUploadError = "showImageUploadError";

    #endregion

    #region Constructors and Finalizers

    public RichTextEditorJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, "./_content/Blazor.Bootstrap/blazor.bootstrap.rich-text-editor.js")
    {
    }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Clears the editor content through the rich-text editor JavaScript module.</summary>
    [AddedVersion("4.0.0")]
    [Description("Clears the editor content through the rich-text editor JavaScript module.")]
    public async Task ClearAsync(object objRef, string editorId)
    {
        await SafeInvokeVoidAsync(Execute, objRef, editorId, null, "clear", null);
    }

    /// <summary>Disposes the editor instance through the rich-text editor JavaScript module.</summary>
    [AddedVersion("4.0.0")]
    [Description("Disposes the editor instance through the rich-text editor JavaScript module.")]
    public async Task DisposeAsync(object objRef, string editorId)
    {
        await SafeInvokeVoidAsync(Dispose, objRef, editorId);
    }

    /// <summary>Executes an editor command through the rich-text editor JavaScript module.</summary>
    [AddedVersion("4.0.0")]
    [Description("Executes an editor command through the rich-text editor JavaScript module.")]
    public async Task ExecuteAsync(object objRef, string editorId, string elementId, string command, string value)
    {
        await SafeInvokeVoidAsync(Execute, objRef, editorId, elementId, command, value);
    }

    /// <summary>Focuses the editor through the rich-text editor JavaScript module.</summary>
    [AddedVersion("4.0.0")]
    [Description("Focuses the editor through the rich-text editor JavaScript module.")]
    public async Task FocusAsync(object objRef, string editorId)
    {
        await SafeInvokeVoidAsync(Focus, objRef, editorId);
    }

    /// <summary>
    /// Hides a rendered editor modal so its close handler can dispose and unmount it.
    /// </summary>
    /// <param name="editorId">The editor identifier.</param>
    /// <param name="modalId">The editor-scoped modal identifier.</param>
    [AddedVersion("4.0.0")]
    [Description("Hides a rendered editor modal so its close handler can dispose and unmount it.")]
    public async Task HideModalAsync(string editorId, string modalId)
    {
        await SafeInvokeVoidAsync(HideModal, editorId, modalId);
    }

    /// <summary>Initializes the editor through the rich-text editor JavaScript module.</summary>
    [AddedVersion("4.0.0")]
    [Description("Initializes the editor through the rich-text editor JavaScript module.")]
    public async Task InitializeAsync(object objRef, string editorId, IEnumerable<string>? allowedLinkDomains, IEnumerable<string>? allowedImageDomains)
    {
        await SafeInvokeVoidAsync(Initialize, objRef, editorId, allowedLinkDomains, allowedImageDomains);
    }

    /// <summary>Prepares an uploaded image for insertion through the rich-text editor JavaScript module.</summary>
    [AddedVersion("4.0.0")]
    [Description("Prepares an uploaded image for insertion through the rich-text editor JavaScript module.")]
    public async Task<bool> PrepareUploadedImageAsync(string editorId, string imageUrl)
    {
        return await SafeInvokeAsync<bool>(PrepareUploadedImage, editorId, imageUrl);
    }

    /// <summary>Shows an image-upload error through the rich-text editor JavaScript module.</summary>
    [AddedVersion("4.0.0")]
    [Description("Shows an image-upload error through the rich-text editor JavaScript module.")]
    public async Task ShowImageUploadErrorAsync(string editorId, string message)
    {
        await SafeInvokeVoidAsync(ShowImageUploadError, editorId, message);
    }

    #endregion

    #endregion
}
