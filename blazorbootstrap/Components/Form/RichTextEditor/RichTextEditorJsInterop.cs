namespace BlazorBootstrap;

internal sealed class RichTextEditorJsInterop : JsInteropBase
{
    #region Fields and Constants

    public const string Clear = "clear";
    public const string Dispose = "dispose";
    public const string Execute = "execute";
    public const string Focus = "focus";
    public const string Initialize = "initialize";
    public const string PrepareUploadedImage = "prepareUploadedImage";
    public const string ShowImageUploadError = "showImageUploadError";

    #endregion

    #region Constructors

    public RichTextEditorJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, "./_content/Blazor.Bootstrap/blazor.bootstrap.rich-text-editor.js")
    {
    }

    #endregion

    #region Methods

    public async Task ClearAsync(object objRef, string editorId)
    {
        await SafeInvokeVoidAsync(Execute, objRef, editorId, null, "clear", null);
    }

    public async Task DisposeAsync(object objRef, string editorId)
    {
        await SafeInvokeVoidAsync(Dispose, objRef, editorId);
    }

    public async Task ExecuteAsync(object objRef, string editorId, string elementId, string command, string value)
    {
        Console.WriteLine($"Executing command: {command} with value: {value} on element: {elementId} in editor: {editorId}");
        await SafeInvokeVoidAsync(Execute, objRef, editorId, elementId, command, value);
    }

    public async Task FocusAsync(object objRef, string editorId)
    {
        await SafeInvokeVoidAsync(Focus, objRef, editorId);
    }

    public async Task InitializeAsync(object objRef, string editorId, IEnumerable<string>? allowedLinkDomains, IEnumerable<string>? allowedImageDomains)
    {
        await SafeInvokeVoidAsync(Initialize, objRef, editorId, allowedLinkDomains, allowedImageDomains);
    }

    public async Task PrepareUploadedImageAsync(string editorId, string imageUrl)
    {
        await SafeInvokeVoidAsync(PrepareUploadedImage, editorId, imageUrl);
    }

    public async Task ShowImageUploadErrorAsync(string editorId, string message)
    {
        await SafeInvokeVoidAsync(ShowImageUploadError, editorId, message);
    }

    #endregion
}
