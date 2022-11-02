using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazorBootstrap.Demo;

public class Demo : ComponentBase
{
    #region Members

    private string code;

    #endregion

    #region Methods

    protected override async Task OnParametersSetAsync()
    {
        if (code is null)
        {
            var resourceName = Type.FullName + ".razor";
            using (Stream stream = Type.Assembly.GetManifestResourceStream(resourceName))
            {
                try
                {
                    if (stream is null)
                        return;

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        code = await reader.ReadToEndAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // no base call
        builder.AddMarkupContent(0, "<!--googleoff: index-->"); // source: https://perishablepress.com/tell-google-to-not-index-certain-parts-of-your-page/

        if (ShowCodeOnly)
        {
            builder.OpenElement(300, "div");
            builder.AddAttribute(301, "class", "highlight show-code-only");
            builder.OpenElement(400, "pre");
            builder.OpenElement(401, "code");
            builder.AddAttribute(402, "class", "language-cshtml");
            if (code != null)
            {
                builder.AddContent(403, code.Trim());
            }
            builder.CloseElement(); // end: code
            builder.CloseElement(); // end: pre
            builder.CloseElement();
        }
        else if (!Tabs)
        {
            builder.OpenElement(100, "div");
            builder.AddAttribute(101, "class", "bb-example");
            builder.OpenComponent(202, Type);
            builder.CloseComponent(); // end: div
            builder.CloseElement();

            builder.OpenElement(300, "div");
            builder.AddAttribute(301, "class", "highlight");
            builder.OpenElement(400, "pre");
            builder.OpenElement(401, "code");
            builder.AddAttribute(402, "class", "language-cshtml");
            if (code != null)
            {
                builder.AddContent(403, code.Trim());
            }
            builder.CloseElement(); // end: code
            builder.CloseElement(); // end: pre
            builder.CloseElement();
        }
        else // Tabs = false
        {
            builder.OpenComponent<Tabs>(300);
            builder.AddAttribute(301, "EnableFadeEffect", true);
            builder.AddAttribute(302, "ChildContent", (RenderFragment)((childContentBuilder) =>
            {
                childContentBuilder.OpenComponent<Tab>(303);
                childContentBuilder.AddAttribute(304, "Title", "EXAMPLE");

                childContentBuilder.AddAttribute(305, "Content", (RenderFragment)((tabContentBuilder) =>
                {
                    tabContentBuilder.OpenElement(306, "div");
                    tabContentBuilder.AddAttribute(307, "class", "bb-example border-top-0 mt-0");

                    tabContentBuilder.OpenComponent(308, Type);
                    tabContentBuilder.CloseComponent(); // end: div

                    tabContentBuilder.CloseElement();
                }));

                childContentBuilder.CloseComponent();

                childContentBuilder.OpenComponent<Tab>(400);
                childContentBuilder.AddAttribute(401, "Title", "VIEW SOURCE");

                childContentBuilder.AddAttribute(402, "Content", (RenderFragment)((tabContentBuilder) =>
                {
                    tabContentBuilder.OpenElement(403, "div");
                    tabContentBuilder.AddAttribute(404, "class", "highlight");
                    tabContentBuilder.OpenElement(405, "pre");
                    tabContentBuilder.OpenElement(406, "code");
                    tabContentBuilder.AddAttribute(407, "class", "language-cshtml");
                    if (code != null)
                    {
                        tabContentBuilder.AddContent(408, code.Trim());
                    }
                    tabContentBuilder.CloseElement(); // end: code
                    tabContentBuilder.CloseElement(); // end: pre
                    tabContentBuilder.CloseElement();
                }));

                childContentBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        }

        builder.AddMarkupContent(700, "<!--googleon: index-->"); // source: https://perishablepress.com/tell-google-to-not-index-certain-parts-of-your-page/
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        await JS.InvokeVoidAsync("highlightCode");
    }

    #endregion

    #region Properties

    [Inject] protected IJSRuntime JS { get; set; } = null!;

    [Parameter] public bool ShowCodeOnly { get; set; }

    [Parameter] public bool Tabs { get; set; } = false;

    [Parameter] public Type Type { get; set; }

    #endregion
}
