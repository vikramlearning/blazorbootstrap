namespace BlazorBootstrap;

internal sealed class RichTextEditorJsInterop : JsInteropBase
{
    #region Constructors

    public RichTextEditorJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, "./_content/Blazor.Bootstrap/blazor.bootstrap.rich-text-editor.js")
    {
    }

    #endregion

    #region Methods

    public Task ClearAsync(string id) => SafeInvokeVoidAsync("clear", id);

    public Task DisposeEditorAsync(string id) => SafeInvokeVoidAsync("dispose", id);

    public Task FocusAsync(string id) => SafeInvokeVoidAsync("focus", id);

    public Task InitializeAsync(string id, DotNetObjectReference<RichTextEditor> objRef, int debounceInterval, int? maxLength, bool readOnly, bool disabled) =>
        SafeInvokeVoidAsync("initialize", id, objRef, debounceInterval, maxLength, readOnly, disabled);

    public Task InsertImageAsync(string id, string url, string altText) => SafeInvokeVoidAsync("insertImage", id, url, altText);

    public Task SetValueAsync(string id, string value) => SafeInvokeVoidAsync("setValue", id, value);

    #endregion
}
