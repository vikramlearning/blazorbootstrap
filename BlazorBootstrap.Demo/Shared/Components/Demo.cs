using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazorBootstrap.Demo;

public class Demo : ComponentBase
{
	[Parameter] public Type Type { get; set; }

	[Parameter] public bool Tabs { get; set; } = false;

	[Inject] protected IJSRuntime JSRuntime { get; set; }

	private bool showingDemo = true;
	private string code;

	protected override async Task OnParametersSetAsync()
	{
		if (code is null)
		{
			var resourceName = Type.FullName + ".razor";

			using (Stream stream = Type.Assembly.GetManifestResourceStream(resourceName))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					code = await reader.ReadToEndAsync();
				}
			}
		}
	}

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		// no base call
		builder.AddMarkupContent(0, "<!--googleoff: index-->"); // source: https://perishablepress.com/tell-google-to-not-index-certain-parts-of-your-page/

		builder.OpenElement(100, "div");
		builder.AddAttribute(101, "class", "bb-example");

		//builder.OpenElement(200, "div");
		//builder.AddAttribute(201, "class", "p-3");
		builder.OpenComponent(202, Type);
		builder.CloseComponent();
		//builder.CloseElement();

		builder.CloseElement();

		//if (Tabs)
		//{
		//	builder.OpenElement(200, "div");
		//	builder.AddAttribute(201, "class", "card-header");

		//	builder.OpenComponent<HxTabPanel>(300);
		//	builder.AddAttribute(301, nameof(HxTabPanel.CssClass), "card-header-tabs");
		//	builder.AddAttribute(302, "ChildContent", (RenderFragment)((builder2) =>
		//	{
		//		builder2.OpenComponent<HxTab>(303);
		//		builder2.AddAttribute(304, "Id", "demo");
		//		builder2.AddAttribute(305, "Title", "Demo");
		//		builder2.AddAttribute(306, "OnTabActivated", EventCallback.Factory.Create(this, () => { showingDemo = true; }));
		//		builder2.CloseComponent();

		//		builder2.OpenComponent<HxTab>(354);
		//		builder2.AddAttribute(356, "Id", "source");
		//		builder2.AddAttribute(357, "Title", "Source");
		//		builder2.AddAttribute(358, "OnTabActivated", EventCallback.Factory.Create(this, () => { showingDemo = false; }));
		//		builder2.CloseComponent();
		//	}));
		//	builder.CloseComponent();

		//	builder.CloseElement(); // card-header
		//}


		builder.OpenElement(300, "div");
		builder.AddAttribute(301, "class", "highlight");

		builder.OpenElement(400, "pre");
		builder.OpenElement(401, "code");
		builder.AddAttribute(402, "class", "language-cshtml");
		builder.AddContent(403, code.Trim());
		builder.CloseElement(); // end: code
		builder.CloseElement(); // end: pre

		builder.CloseElement();

		builder.AddMarkupContent(700, "<!--googleon: index-->"); // source: https://perishablepress.com/tell-google-to-not-index-certain-parts-of-your-page/
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		await JSRuntime.InvokeVoidAsync("highlightCode");
	}
}
