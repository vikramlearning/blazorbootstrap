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

	[Parameter] public bool Tabs { get; set; } = true;

	[Inject] protected IJSRuntime JSRuntime { get; set; }

	private bool showingDemo = true;
	private string code;

	protected override async Task OnParametersSetAsync()
	{
		if (code is null)
		{
			var resourceName = Type.FullName + ".razor";

			Console.WriteLine(resourceName);

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
		builder.AddAttribute(101, "class", "card card-demo my-3");

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

		builder.OpenElement(400, "div");
		builder.AddAttribute(401, "class", "card-body p-0");

		if (showingDemo || !Tabs)
		{
			builder.OpenElement(500, "div");
			builder.AddAttribute(501, "class", "p-3");
			builder.OpenComponent(504, Type);
			builder.CloseComponent();
			builder.CloseElement();
		}
		if (!showingDemo || !Tabs)
		{
			builder.OpenElement(600, "pre");
			if (!Tabs)
			{
				builder.AddAttribute(601, "class", "gray-background");
			}

			builder.OpenElement(602, "code");
			builder.AddAttribute(603, "class", "language-cshtml");
			builder.AddContent(604, code.Trim());
			builder.CloseElement();

			builder.CloseElement();
		}

		builder.CloseElement(); // card-body
		builder.CloseElement(); // card

		builder.AddMarkupContent(700, "<!--googleon: index-->"); // source: https://perishablepress.com/tell-google-to-not-index-certain-parts-of-your-page/
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		await JSRuntime.InvokeVoidAsync("highlightCode");
	}
}
