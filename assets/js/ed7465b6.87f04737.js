"use strict";(self.webpackChunkblazorbootstrap=self.webpackChunkblazorbootstrap||[]).push([[92115],{3905:(e,t,r)=>{r.d(t,{Zo:()=>c,kt:()=>h});var n=r(67294);function a(e,t,r){return t in e?Object.defineProperty(e,t,{value:r,enumerable:!0,configurable:!0,writable:!0}):e[t]=r,e}function s(e,t){var r=Object.keys(e);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(e);t&&(n=n.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),r.push.apply(r,n)}return r}function o(e){for(var t=1;t<arguments.length;t++){var r=null!=arguments[t]?arguments[t]:{};t%2?s(Object(r),!0).forEach((function(t){a(e,t,r[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(r)):s(Object(r)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(r,t))}))}return e}function i(e,t){if(null==e)return{};var r,n,a=function(e,t){if(null==e)return{};var r,n,a={},s=Object.keys(e);for(n=0;n<s.length;n++)r=s[n],t.indexOf(r)>=0||(a[r]=e[r]);return a}(e,t);if(Object.getOwnPropertySymbols){var s=Object.getOwnPropertySymbols(e);for(n=0;n<s.length;n++)r=s[n],t.indexOf(r)>=0||Object.prototype.propertyIsEnumerable.call(e,r)&&(a[r]=e[r])}return a}var l=n.createContext({}),p=function(e){var t=n.useContext(l),r=t;return e&&(r="function"==typeof e?e(t):o(o({},t),e)),r},c=function(e){var t=p(e.components);return n.createElement(l.Provider,{value:t},e.children)},m="mdxType",d={inlineCode:"code",wrapper:function(e){var t=e.children;return n.createElement(n.Fragment,{},t)}},u=n.forwardRef((function(e,t){var r=e.components,a=e.mdxType,s=e.originalType,l=e.parentName,c=i(e,["components","mdxType","originalType","parentName"]),m=p(r),u=a,h=m["".concat(l,".").concat(u)]||m[u]||d[u]||s;return r?n.createElement(h,o(o({ref:t},c),{},{components:r})):n.createElement(h,o({ref:t},c))}));function h(e,t){var r=arguments,a=t&&t.mdxType;if("string"==typeof e||a){var s=r.length,o=new Array(s);o[0]=u;var i={};for(var l in t)hasOwnProperty.call(t,l)&&(i[l]=t[l]);i.originalType=e,i[m]="string"==typeof e?e:a,o[1]=i;for(var p=2;p<s;p++)o[p]=r[p];return n.createElement.apply(null,o)}return n.createElement.apply(null,r)}u.displayName="MDXCreateElement"},74404:(e,t,r)=>{r.d(t,{ZP:()=>c});var n=r(87462),a=r(67294),s=r(3905);class o extends a.Component{constructor(e){super(e),this.name=this.props.name||"docsblazorbootstrapcom",this.serve=this.props.serve||"CWYICKJI",this.script=this.props.script||null,this.placement=this.props.placement||"",this.fallback=this.props.fallback||null,this.showFallback=!1}adShowing=()=>null!==document.getElementById(`${this.name} #carbonads`);componentDidMount=()=>{let e=document.createElement("script");e.defer=!!this.script,e.async=!0,e.id=this.script?"":"_carbonads_js",e.type="text/javascript",e.src=this.script||`//cdn.carbonads.com/carbon.js?serve=${this.serve}&placement=${this.placement}`,e.onerror=()=>{this.showFallback=!0,this.forceUpdate()},e.addEventListener("load",(()=>{this.adShowing||_.invoke(window._carbonads,"refresh")})),document.querySelector(`#${this.name}`).appendChild(e)};render(){return this.showFallback&&this.fallback?this.fallback:a.createElement("div",{id:this.name})}}const i=o,l={toc:[]},p="wrapper";function c(e){let{components:t,...r}=e;return(0,s.kt)(p,(0,n.Z)({},l,r,{components:t,mdxType:"MDXLayout"}),(0,s.kt)(i,{mdxType:"CarbonAd"}))}c.isMDXComponent=!0},4305:(e,t,r)=>{r.r(t),r.d(t,{assets:()=>p,contentTitle:()=>i,default:()=>u,frontMatter:()=>o,metadata:()=>l,toc:()=>c});var n=r(87462),a=(r(67294),r(3905)),s=r(74404);const o={id:"blazor-server-net-6",sidebar_label:"Blazor Server (.NET 6)",sidebar_position:9,title:"Blazor Server (.NET 6)"},i="Getting started - Blazor Server (.NET 6)",l={unversionedId:"getting-started/blazor-server-net-6",id:"getting-started/blazor-server-net-6",title:"Blazor Server (.NET 6)",description:"Get started with the Enterprise-class Blazor Bootstrap Component library built on the Blazor and Bootstrap CSS frameworks.",source:"@site/docs/01-getting-started/03-b-getting-started-server-NET-6.mdx",sourceDirName:"01-getting-started",slug:"/getting-started/blazor-server-net-6",permalink:"/getting-started/blazor-server-net-6",draft:!1,editUrl:"https://github.com/vikramlearning/blazorbootstrap/edit/master/docs/docs/01-getting-started/03-b-getting-started-server-NET-6.mdx",tags:[],version:"current",sidebarPosition:9,frontMatter:{id:"blazor-server-net-6",sidebar_label:"Blazor Server (.NET 6)",sidebar_position:9,title:"Blazor Server (.NET 6)"},sidebar:"tutorialSidebar",previous:{title:"Blazor WebAssembly (.NET 6)",permalink:"/getting-started/blazor-webassembly-net-6"},next:{title:"Blazor WebAssembly",permalink:"/layout/blazor-webassembly"}},p={},c=[{value:"Install Nuget Package",id:"install-nuget-package",level:2},{value:"Add CSS references",id:"add-css-references",level:2},{value:"Add script references",id:"add-script-references",level:2},{value:"Register services",id:"register-services",level:2},{value:"Remove default references",id:"remove-default-references",level:2},{value:"Starter template",id:"starter-template",level:2},{value:"Sample Code",id:"sample-code",level:2},{value:"_Layout.cshtml",id:"_layoutcshtml",level:3},{value:"Program.cs",id:"programcs",level:3},{value:"_Imports.razor",id:"_importsrazor",level:3},{value:"MainLayout.razor",id:"mainlayoutrazor",level:3}],m={toc:c},d="wrapper";function u(e){let{components:t,...r}=e;return(0,a.kt)(d,(0,n.Z)({},m,r,{components:t,mdxType:"MDXLayout"}),(0,a.kt)("h1",{id:"getting-started---blazor-server-net-6"},"Getting started - Blazor Server (.NET 6)"),(0,a.kt)("p",null,"Get started with the Enterprise-class Blazor Bootstrap Component library built on the Blazor and Bootstrap CSS frameworks."),(0,a.kt)(s.ZP,{mdxType:"CarbonAd"}),(0,a.kt)("h2",{id:"install-nuget-package"},"Install Nuget Package"),(0,a.kt)("p",null,"Looking to quickly add ",(0,a.kt)("strong",{parentName:"p"},"Blazor Bootstrap")," to your project? Use NuGet package manager."),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-shell"},"Install-Package Blazor.Bootstrap -Version 3.0.0\n")),(0,a.kt)("h2",{id:"add-css-references"},"Add CSS references"),(0,a.kt)("p",null,"After the ",(0,a.kt)("inlineCode",{parentName:"p"},'<base href="~/" />')," tag in the ",(0,a.kt)("strong",{parentName:"p"},"head")," section in the ",(0,a.kt)("strong",{parentName:"p"},"Pages/_Layout.cshtml")," file, add the following references:"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-html",metastring:"showLineNumbers",showLineNumbers:!0},'<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN" crossorigin="anonymous">\n<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />\n<link href="_content/Blazor.Bootstrap/blazor.bootstrap.css" rel="stylesheet" />\n')),(0,a.kt)("admonition",{type:"note"},(0,a.kt)("p",{parentName:"admonition"},"If you use the ",(0,a.kt)("strong",{parentName:"p"},"Blazor Server App Empty")," template (without demonstration code and Bootstrap), add the following references to the ",(0,a.kt)("inlineCode",{parentName:"p"},"head")," section in the ",(0,a.kt)("strong",{parentName:"p"},"Pages/_Host.cshtml"),".\nThere is a known GitHub issue ",(0,a.kt)("a",{parentName:"p",href:"https://github.com/dotnet/aspnetcore/issues/43975"},"Blazor empty template doesn't load scoped CSS"),".")),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-html",metastring:"showLineNumbers",showLineNumbers:!0},'<link href="_content/Blazor.Bootstrap/Blazor.Bootstrap.bundle.scp.css" rel="stylesheet" />\n')),(0,a.kt)("admonition",{title:"IMPORTANT",type:"danger"},(0,a.kt)("p",{parentName:"admonition"},"In .NET 6 Blazor Server App default template, you may see ",(0,a.kt)("strong",{parentName:"p"},"Pages/_Layout.cshtml"),". So, add these references in the ",(0,a.kt)("strong",{parentName:"p"},"Pages/_Layout.cshtml")," instead of in the ",(0,a.kt)("strong",{parentName:"p"},"Pages/_Host.cshtml"),".")),(0,a.kt)("h2",{id:"add-script-references"},"Add script references"),(0,a.kt)("p",null,"Insert the following references into the ",(0,a.kt)("strong",{parentName:"p"},"body")," section of the ",(0,a.kt)("strong",{parentName:"p"},"Pages/_Layout.cshtml")," file, immediately after the ",(0,a.kt)("strong",{parentName:"p"},"_framework/blazor.server.js")," reference:"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-js",metastring:"showLineNumbers",showLineNumbers:!0},'<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js" integrity="sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL" crossorigin="anonymous"><\/script>\n\x3c!-- Add chart.js reference if chart components are used in your application. --\x3e\n<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.0.1/chart.umd.js" integrity="sha512-gQhCDsnnnUfaRzD8k1L5llCCV6O9HN09zClIzzeJ8OJ9MpGmIlCxm+pdCkqTwqJ4JcjbojFr79rl2F1mzcoLMQ==" crossorigin="anonymous" referrerpolicy="no-referrer"><\/script>\n\x3c!-- Add chartjs-plugin-datalabels.min.js reference if chart components with data label feature is used in your application. --\x3e\n<script src="https://cdnjs.cloudflare.com/ajax/libs/chartjs-plugin-datalabels/2.2.0/chartjs-plugin-datalabels.min.js" integrity="sha512-JPcRR8yFa8mmCsfrw4TNte1ZvF1e3+1SdGMslZvmrzDYxS69J7J49vkFL8u6u8PlPJK+H3voElBtUCzaXj+6ig==" crossorigin="anonymous" referrerpolicy="no-referrer"><\/script>\n\x3c!-- Add sortable.js reference if SortableList component is used in your application. --\x3e\n<script src="https://cdn.jsdelivr.net/npm/sortablejs@latest/Sortable.min.js"><\/script>\n<script src="_content/Blazor.Bootstrap/blazor.bootstrap.js"><\/script>\n')),(0,a.kt)("admonition",{type:"tip"},(0,a.kt)("p",{parentName:"admonition"},(0,a.kt)("strong",{parentName:"p"},"chart.js")," reference is optional. Add when the ",(0,a.kt)("strong",{parentName:"p"},"Chart")," components are used in the application.")),(0,a.kt)("h2",{id:"register-services"},"Register services"),(0,a.kt)("p",null,"Add Blazor Bootstrap service in the ",(0,a.kt)("strong",{parentName:"p"},"Program.cs")),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-cs",metastring:"showLineNumbers",showLineNumbers:!0},"builder.Services.AddBlazorBootstrap();\n")),(0,a.kt)("p",null,"Register tag helpers in ",(0,a.kt)("strong",{parentName:"p"},"_Imports.razor")),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-razor",metastring:"showLineNumbers",showLineNumbers:!0},"@using BlazorBootstrap;\n")),(0,a.kt)("h2",{id:"remove-default-references"},"Remove default references"),(0,a.kt)("p",null,"The default Blazor template includes demonstration code, icons, and Bootstrap. To remove these components, follow these steps:"),(0,a.kt)("ol",null,(0,a.kt)("li",{parentName:"ol"},(0,a.kt)("p",{parentName:"li"},"Delete the ",(0,a.kt)("strong",{parentName:"p"},"bootstrap")," and ",(0,a.kt)("strong",{parentName:"p"},"open-iconic")," folders from the ",(0,a.kt)("strong",{parentName:"p"},"wwwroot")," directory:"),(0,a.kt)("ul",{parentName:"li"},(0,a.kt)("li",{parentName:"ul"},"Delete the ",(0,a.kt)("strong",{parentName:"li"},"wwwroot/css/bootstrap")," folder."),(0,a.kt)("li",{parentName:"ul"},"Delete the ",(0,a.kt)("strong",{parentName:"li"},"wwwroot/css/open-iconic")," folder."))),(0,a.kt)("li",{parentName:"ol"},(0,a.kt)("p",{parentName:"li"},"Remove the following line from ",(0,a.kt)("strong",{parentName:"p"},"Pages/_Layout.cshtml")," file:"),(0,a.kt)("pre",{parentName:"li"},(0,a.kt)("code",{parentName:"pre",className:"language-html",metastring:"showLineNumbers",showLineNumbers:!0},'<link rel="stylesheet" href="css/bootstrap/bootstrap.min.css" />\n'))),(0,a.kt)("li",{parentName:"ol"},(0,a.kt)("p",{parentName:"li"},"Remove the following line from ",(0,a.kt)("strong",{parentName:"p"},"wwwroot/css/site.css")," file:"),(0,a.kt)("pre",{parentName:"li"},(0,a.kt)("code",{parentName:"pre",className:"language-css",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},"@import url('open-iconic/font/css/open-iconic-bootstrap.min.css');\n")))),(0,a.kt)("h2",{id:"starter-template"},"Starter template"),(0,a.kt)("p",null,(0,a.kt)("a",{parentName:"p",href:"https://github.com/vikramlearning/blazorbootstrap-starter-templates/tree/master/src/BlazorBootstrap.Templates.Starter/NET6.BlazorServerApp"},"GitHub Repo: Blazor Bootstrap - Blazor Server App (.NET 6)")),(0,a.kt)("img",{src:"https://i.imgur.com/BfgYeNd.png",alt:"Blazor Bootstrap - Blazor Server App"}),(0,a.kt)("h2",{id:"sample-code"},"Sample Code"),(0,a.kt)("h3",{id:"_layoutcshtml"},"_Layout.cshtml"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{11-13,33-35} showLineNumbers","{11-13,33-35}":!0,showLineNumbers:!0},'@using Microsoft.AspNetCore.Components.Web\n@namespace NET6.BlazorServerApp.Pages\n@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers\n\n<!DOCTYPE html>\n<html lang="en">\n<head>\n    <meta charset="utf-8" />\n    <meta name="viewport" content="width=device-width, initial-scale=1.0" />\n    <base href="~/" />\n    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN" crossorigin="anonymous">\n    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />\n    <link href="_content/Blazor.Bootstrap/blazor.bootstrap.css" rel="stylesheet" />\n    <link href="css/site.css" rel="stylesheet" />\n    <link href="NET6.BlazorServerApp.styles.css" rel="stylesheet" />\n    <component type="typeof(HeadOutlet)" render-mode="ServerPrerendered" />\n</head>\n<body>\n    @RenderBody()\n\n    <div id="blazor-error-ui">\n        <environment include="Staging,Production">\n            An error has occurred. This application may no longer respond until reloaded.\n        </environment>\n        <environment include="Development">\n            An unhandled exception has occurred. See browser dev tools for details.\n        </environment>\n        <a href="" class="reload">Reload</a>\n        <a class="dismiss">\ud83d\uddd9</a>\n    </div>\n\n    <script src="_framework/blazor.server.js"><\/script>\n    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js" integrity="sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL" crossorigin="anonymous"><\/script>\n    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.0.1/chart.umd.js" integrity="sha512-gQhCDsnnnUfaRzD8k1L5llCCV6O9HN09zClIzzeJ8OJ9MpGmIlCxm+pdCkqTwqJ4JcjbojFr79rl2F1mzcoLMQ==" crossorigin="anonymous" referrerpolicy="no-referrer"><\/script> \x3c!-- Add chart.js reference if Chart components are used in the application. --\x3e\n    <script src="_content/Blazor.Bootstrap/blazor.bootstrap.js"><\/script>\n</body>\n</html>\n')),(0,a.kt)("h3",{id:"programcs"},"Program.cs"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-cs",metastring:"{11} showLineNumbers","{11}":!0,showLineNumbers:!0},'using Microsoft.AspNetCore.Components;\nusing Microsoft.AspNetCore.Components.Web;\nusing NET6.BlazorServerApp.Data;\n\nvar builder = WebApplication.CreateBuilder(args);\n\n// Add services to the container.\nbuilder.Services.AddRazorPages();\nbuilder.Services.AddServerSideBlazor();\nbuilder.Services.AddSingleton<WeatherForecastService>();\nbuilder.Services.AddBlazorBootstrap();\n\nvar app = builder.Build();\n\n// Configure the HTTP request pipeline.\nif (!app.Environment.IsDevelopment())\n{\n    app.UseExceptionHandler("/Error");\n    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.\n    app.UseHsts();\n}\n\napp.UseHttpsRedirection();\n\napp.UseStaticFiles();\n\napp.UseRouting();\n\napp.MapBlazorHub();\napp.MapFallbackToPage("/_Host");\n\napp.Run();\n')),(0,a.kt)("h3",{id:"_importsrazor"},"_Imports.razor"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-razor",metastring:"{11} showLineNumbers","{11}":!0,showLineNumbers:!0},"@using System.Net.Http\n@using Microsoft.AspNetCore.Authorization\n@using Microsoft.AspNetCore.Components.Authorization\n@using Microsoft.AspNetCore.Components.Forms\n@using Microsoft.AspNetCore.Components.Routing\n@using Microsoft.AspNetCore.Components.Web\n@using Microsoft.AspNetCore.Components.Web.Virtualization\n@using Microsoft.JSInterop\n@using NET6.BlazorServerApp\n@using NET6.BlazorServerApp.Shared\n@using BlazorBootstrap;\n")),(0,a.kt)("h3",{id:"mainlayoutrazor"},"MainLayout.razor"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-razor",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'@inherits LayoutComponentBase\n\n<div class="bb-page">\n\n    <Sidebar @ref="sidebar"\n             IconName="IconName.BootstrapFill"\n             Title="Blazor Bootstrap"\n             DataProvider="SidebarDataProvider" />\n\n    <main>\n        <div class="bb-top-row px-4 d-flex justify-content-end">\n            <a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>\n        </div>\n\n        <article class="content px-4">\n            <div class="py-2">\n                @Body\n            </div>\n        </article>\n    </main>\n\n</div>\n\n<Modal IsServiceModal="true" />\n<Preload />\n<Toasts class="p-3" AutoHide="true" Delay="4000" Placement="ToastsPlacement.TopRight" />\n\n@code {\n    private Sidebar sidebar = default!;\n    private IEnumerable<NavItem> navItems = default!;\n\n    private async Task<SidebarDataProviderResult> SidebarDataProvider(SidebarDataProviderRequest request)\n    {\n        if (navItems is null)\n            navItems = GetNavItems();\n\n        return await Task.FromResult(request.ApplyTo(navItems));\n    }\n\n    private IEnumerable<NavItem> GetNavItems()\n    {\n        navItems = new List<NavItem>\n        {\n            new NavItem { Id = "1", Href = "/", IconName = IconName.HouseDoorFill, Text = "Home", Match=NavLinkMatch.All},\n            new NavItem { Id = "2", Href = "/counter", IconName = IconName.PlusSquareFill, Text = "Counter"},\n            new NavItem { Id = "3", Href = "/fetchdata", IconName = IconName.Table, Text = "Fetch Data"},\n        };\n\n        return navItems;\n    }\n}\n')))}u.isMDXComponent=!0}}]);