namespace BlazorBootstrap;

public class SortableListJsInterop : JsInteropBase
{
    #region Constructors

    public SortableListJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, "./_content/Blazor.Bootstrap/blazor.bootstrap.sortable-list.js")
    {
    }

    #endregion

    #region Methods

    public async Task InitializeAsync(string elementId, string elementName, string handle, string group, bool allowSorting, object pull, object put, string filter, object objRef)
    {
        await SafeInvokeVoidAsync("initialize", elementId, elementName, handle, group, allowSorting, pull, put, filter, objRef);
    }

    #endregion
}
