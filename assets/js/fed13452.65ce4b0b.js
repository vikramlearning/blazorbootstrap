"use strict";(self.webpackChunkblazorbootstrap=self.webpackChunkblazorbootstrap||[]).push([[8428],{3905:(t,e,a)=>{a.d(e,{Zo:()=>p,kt:()=>u});var o=a(7294);function n(t,e,a){return e in t?Object.defineProperty(t,e,{value:a,enumerable:!0,configurable:!0,writable:!0}):t[e]=a,t}function s(t,e){var a=Object.keys(t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(t);e&&(o=o.filter((function(e){return Object.getOwnPropertyDescriptor(t,e).enumerable}))),a.push.apply(a,o)}return a}function r(t){for(var e=1;e<arguments.length;e++){var a=null!=arguments[e]?arguments[e]:{};e%2?s(Object(a),!0).forEach((function(e){n(t,e,a[e])})):Object.getOwnPropertyDescriptors?Object.defineProperties(t,Object.getOwnPropertyDescriptors(a)):s(Object(a)).forEach((function(e){Object.defineProperty(t,e,Object.getOwnPropertyDescriptor(a,e))}))}return t}function l(t,e){if(null==t)return{};var a,o,n=function(t,e){if(null==t)return{};var a,o,n={},s=Object.keys(t);for(o=0;o<s.length;o++)a=s[o],e.indexOf(a)>=0||(n[a]=t[a]);return n}(t,e);if(Object.getOwnPropertySymbols){var s=Object.getOwnPropertySymbols(t);for(o=0;o<s.length;o++)a=s[o],e.indexOf(a)>=0||Object.prototype.propertyIsEnumerable.call(t,a)&&(n[a]=t[a])}return n}var i=o.createContext({}),m=function(t){var e=o.useContext(i),a=e;return t&&(a="function"==typeof t?t(e):r(r({},e),t)),a},p=function(t){var e=m(t.components);return o.createElement(i.Provider,{value:e},t.children)},c={inlineCode:"code",wrapper:function(t){var e=t.children;return o.createElement(o.Fragment,{},e)}},g=o.forwardRef((function(t,e){var a=t.components,n=t.mdxType,s=t.originalType,i=t.parentName,p=l(t,["components","mdxType","originalType","parentName"]),g=m(a),u=n,d=g["".concat(i,".").concat(u)]||g[u]||c[u]||s;return a?o.createElement(d,r(r({ref:e},p),{},{components:a})):o.createElement(d,r({ref:e},p))}));function u(t,e){var a=arguments,n=e&&e.mdxType;if("string"==typeof t||n){var s=a.length,r=new Array(s);r[0]=g;var l={};for(var i in e)hasOwnProperty.call(e,i)&&(l[i]=e[i]);l.originalType=t,l.mdxType="string"==typeof t?t:n,r[1]=l;for(var m=2;m<s;m++)r[m]=a[m];return o.createElement.apply(null,r)}return o.createElement.apply(null,a)}g.displayName="MDXCreateElement"},255:(t,e,a)=>{a.r(e),a.d(e,{assets:()=>i,contentTitle:()=>r,default:()=>c,frontMatter:()=>s,metadata:()=>l,toc:()=>m});var o=a(7462),n=(a(7294),a(3905));const s={title:"Blazor Toasts Component",description:"Push notifications to your visitors with a toast, a lightweight and easily customizable BlazorBootstrap toast message.",image:"https://getblazorbootstrap.com/img/logo.svg",sidebar_label:"Toasts",sidebar_position:16},r="Blazor Toasts",l={unversionedId:"components/toasts",id:"components/toasts",title:"Blazor Toasts Component",description:"Push notifications to your visitors with a toast, a lightweight and easily customizable BlazorBootstrap toast message.",source:"@site/docs/components/toasts.md",sourceDirName:"components",slug:"/components/toasts",permalink:"/docs/components/toasts",draft:!1,editUrl:"https://github.com/vikramlearning/blazorbootstrap/edit/master/docs/docs/components/toasts.md",tags:[],version:"current",sidebarPosition:16,frontMatter:{title:"Blazor Toasts Component",description:"Push notifications to your visitors with a toast, a lightweight and easily customizable BlazorBootstrap toast message.",image:"https://getblazorbootstrap.com/img/logo.svg",sidebar_label:"Toasts",sidebar_position:16},sidebar:"tutorialSidebar",previous:{title:"Tabs",permalink:"/docs/components/tabs"},next:{title:"Tooltips",permalink:"/docs/components/tooltips"}},i={},m=[{value:"Toasts Parameters",id:"toasts-parameters",level:2},{value:"ToastMessage Properties",id:"toastmessage-properties",level:2},{value:"Examples:",id:"examples",level:2},{value:"Toast",id:"toast",level:3},{value:"Toast without title",id:"toast-without-title",level:3},{value:"Auto hide",id:"auto-hide",level:3},{value:"Placement",id:"placement",level:3},{value:"Stack Length",id:"stack-length",level:3}],p={toc:m};function c(t){let{components:e,...a}=t;return(0,n.kt)("wrapper",(0,o.Z)({},p,a,{components:e,mdxType:"MDXLayout"}),(0,n.kt)("h1",{id:"blazor-toasts"},"Blazor Toasts"),(0,n.kt)("p",null,"Push notifications to your visitors with a toast, a lightweight and easily customizable BlazorBootstrap toast message."),(0,n.kt)("p",null,"Toasts are lightweight notifications designed to mimic the push notifications that have been popularized by mobile and desktop operating systems. They\u2019re built with flexbox, so they\u2019re easy to align and position."),(0,n.kt)("img",{src:"https://i.imgur.com/OCQUchu.png",alt:"Blazor Bootstrap: Blazor Toasts Component"}),(0,n.kt)("p",null,(0,n.kt)("strong",{parentName:"p"},"Things to know when using the toasts component:")),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},"Toasts will not hide automatically if you do not specify ",(0,n.kt)("inlineCode",{parentName:"li"},'AutoHide="true"'),".")),(0,n.kt)("h2",{id:"toasts-parameters"},"Toasts Parameters"),(0,n.kt)("table",null,(0,n.kt)("thead",{parentName:"table"},(0,n.kt)("tr",{parentName:"thead"},(0,n.kt)("th",{parentName:"tr",align:"left"},"Name"),(0,n.kt)("th",{parentName:"tr",align:"left"},"Type"),(0,n.kt)("th",{parentName:"tr",align:"left"},"Descritpion"),(0,n.kt)("th",{parentName:"tr",align:"left"},"Required"),(0,n.kt)("th",{parentName:"tr",align:"left"},"Default"))),(0,n.kt)("tbody",{parentName:"table"},(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"AutoHide"),(0,n.kt)("td",{parentName:"tr",align:"left"},"bool"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Auto hide the toast."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"},(0,n.kt)("inlineCode",{parentName:"td"},"false"))),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"Delay"),(0,n.kt)("td",{parentName:"tr",align:"left"},"int"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Delay hiding the toast (milli seconds)."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"},"5000")),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"Messages"),(0,n.kt)("td",{parentName:"tr",align:"left"},(0,n.kt)("inlineCode",{parentName:"td"},"List<ToastMessage>")),(0,n.kt)("td",{parentName:"tr",align:"left"},"List of all the toasts."),(0,n.kt)("td",{parentName:"tr",align:"left"},"\u2714\ufe0f"),(0,n.kt)("td",{parentName:"tr",align:"left"})),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"Placement"),(0,n.kt)("td",{parentName:"tr",align:"left"},(0,n.kt)("inlineCode",{parentName:"td"},"ToastsPlacement")),(0,n.kt)("td",{parentName:"tr",align:"left"},"Specifies the toasts placement."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"},(0,n.kt)("inlineCode",{parentName:"td"},"ToastsPlacement.TopRight"))),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"ShowCloseButton"),(0,n.kt)("td",{parentName:"tr",align:"left"},"bool"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Show close button."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"},(0,n.kt)("inlineCode",{parentName:"td"},"true"))),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"StackLength"),(0,n.kt)("td",{parentName:"tr",align:"left"},"int"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Specifies the toast container maximum capacity."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"},"5")))),(0,n.kt)("h2",{id:"toastmessage-properties"},"ToastMessage Properties"),(0,n.kt)("table",null,(0,n.kt)("thead",{parentName:"table"},(0,n.kt)("tr",{parentName:"thead"},(0,n.kt)("th",{parentName:"tr",align:"left"},"Name"),(0,n.kt)("th",{parentName:"tr",align:"left"},"Type"),(0,n.kt)("th",{parentName:"tr",align:"left"},"Description"),(0,n.kt)("th",{parentName:"tr",align:"left"},"Required"),(0,n.kt)("th",{parentName:"tr",align:"left"},"Default"))),(0,n.kt)("tbody",{parentName:"table"},(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"Type"),(0,n.kt)("td",{parentName:"tr",align:"left"},(0,n.kt)("inlineCode",{parentName:"td"},"ToastType")),(0,n.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the type of the toast."),(0,n.kt)("td",{parentName:"tr",align:"left"},"\u2714\ufe0f"),(0,n.kt)("td",{parentName:"tr",align:"left"})),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"ImageSource"),(0,n.kt)("td",{parentName:"tr",align:"left"},"string"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the source of the image."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"})),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"IconName"),(0,n.kt)("td",{parentName:"tr",align:"left"},(0,n.kt)("inlineCode",{parentName:"td"},"IconName")),(0,n.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the bootstarp icon name."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"})),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"Id"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Guid"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Gets the toast id."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"})),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"CustomIconName"),(0,n.kt)("td",{parentName:"tr",align:"left"},"string"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the custom icon name."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"})),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"Title"),(0,n.kt)("td",{parentName:"tr",align:"left"},"string"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the toast''s message title."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"})),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"HelpText"),(0,n.kt)("td",{parentName:"tr",align:"left"},"string"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the help text."),(0,n.kt)("td",{parentName:"tr",align:"left"}),(0,n.kt)("td",{parentName:"tr",align:"left"})),(0,n.kt)("tr",{parentName:"tbody"},(0,n.kt)("td",{parentName:"tr",align:"left"},"Message"),(0,n.kt)("td",{parentName:"tr",align:"left"},"string"),(0,n.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the toast message."),(0,n.kt)("td",{parentName:"tr",align:"left"},"\u2714\ufe0f"),(0,n.kt)("td",{parentName:"tr",align:"left"})))),(0,n.kt)("h2",{id:"examples"},"Examples:"),(0,n.kt)("h3",{id:"toast"},"Toast"),(0,n.kt)("p",null,"To encourage extensible and predictable toasts, we recommend a header and body."),(0,n.kt)("p",null,'Toasts are as flexible as you need and have very little required markup. At a minimum, we require a single element to contain your "toasted" content and strongly encourage a dismiss button.'),(0,n.kt)("img",{src:"https://i.imgur.com/OCQUchu.png",alt:"Blazor Bootstrap: Blazor Toasts Component - Example"}),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{1} showLineNumbers","{1}":!0,showLineNumbers:!0},'<Toasts class="p-3" Messages="messages" Placement="ToastsPlacement.TopRight" />\n\n<Button Color="ButtonColor.Primary" @onclick="() => ShowMessage(ToastType.Primary)">Primary Toast</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ShowMessage(ToastType.Secondary)">Secondary Toast</Button>\n<Button Color="ButtonColor.Success" @onclick="() => ShowMessage(ToastType.Success)">Success Toast</Button>\n<Button Color="ButtonColor.Danger" @onclick="() => ShowMessage(ToastType.Danger)">Danger Toast</Button>\n<Button Color="ButtonColor.Warning" @onclick="() => ShowMessage(ToastType.Warning)">Warning Toast</Button>\n<Button Color="ButtonColor.Info" @onclick="() => ShowMessage(ToastType.Info)">Info Toast</Button>\n<Button Color="ButtonColor.Dark" @onclick="() => ShowMessage(ToastType.Dark)">Dark Toast</Button>\n')),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cs",metastring:"{7-13} showLineNumbers","{7-13}":!0,showLineNumbers:!0},'@code {\n    List<ToastMessage> messages = new List<ToastMessage>();\n\n    private void ShowMessage(ToastType toastType) => messages.Add(CreateToastMessage(toastType));\n\n    private ToastMessage CreateToastMessage(ToastType toastType)\n    => new ToastMessage\n        {\n            Type = toastType,\n            Title = "Blazor Bootstrap",\n            HelpText = $"{DateTime.Now}",\n            Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",\n        };\n}\n')),(0,n.kt)("p",null,(0,n.kt)("a",{parentName:"p",href:"https://demos.getblazorbootstrap.com/toasts#examples"},"See toasts demo here.")),(0,n.kt)("h3",{id:"toast-without-title"},"Toast without title"),(0,n.kt)("p",null,"Customize your toasts by removing sub-components, tweaking them with utilities."),(0,n.kt)("p",null,"Here we've created a simple toast. You can create different toast color schemes with the ",(0,n.kt)("inlineCode",{parentName:"p"},"Color")," parameter."),(0,n.kt)("div",null,(0,n.kt)("img",{src:"https://i.imgur.com/VRglJqU.jpg",alt:"Blazor Bootstrap: Toasts Component - Example"})),(0,n.kt)("div",null,(0,n.kt)("img",{src:"https://i.imgur.com/SUB90wN.jpg",alt:"Blazor Bootstrap: Toasts Component - Example"})),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{1} showLineNumbers","{1}":!0,showLineNumbers:!0},'<Toasts class="p-3" Messages="messages" Placement="ToastsPlacement.TopRight" />\n\n<Button Color="ButtonColor.Primary" @onclick="() => ShowMessage(ToastType.Primary)">Primary Toast</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ShowMessage(ToastType.Secondary)">Secondary Toast</Button>\n<Button Color="ButtonColor.Success" @onclick="() => ShowMessage(ToastType.Success)">Success Toast</Button>\n<Button Color="ButtonColor.Danger" @onclick="() => ShowMessage(ToastType.Danger)">Danger Toast</Button>\n<Button Color="ButtonColor.Warning" @onclick="() => ShowMessage(ToastType.Warning)">Warning Toast</Button>\n<Button Color="ButtonColor.Info" @onclick="() => ShowMessage(ToastType.Info)">Info Toast</Button>\n<Button Color="ButtonColor.Light" @onclick="() => ShowMessage(ToastType.Light)">Light Toast</Button>\n<Button Color="ButtonColor.Dark" @onclick="() => ShowMessage(ToastType.Dark)">Dark Toast</Button>\n')),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cs",metastring:"{7-11} showLineNumbers","{7-11}":!0,showLineNumbers:!0},'@code {\n    List<ToastMessage> messages = new List<ToastMessage>();\n\n    private void ShowMessage(ToastType toastType) => messages.Add(CreateToastMessage(toastType));\n\n    private ToastMessage CreateToastMessage(ToastType toastType)\n    => new ToastMessage\n        {\n            Type = toastType,\n            Message = $"Hello, world! This is a simple toast message. DateTime: {DateTime.Now}",\n        };\n}\n')),(0,n.kt)("p",null,(0,n.kt)("a",{parentName:"p",href:"https://demos.getblazorbootstrap.com/toasts#toast-without-title"},"See toasts without title demo here.")),(0,n.kt)("h3",{id:"auto-hide"},"Auto hide"),(0,n.kt)("p",null,"Add ",(0,n.kt)("inlineCode",{parentName:"p"},'AutoHide="true"')," parameter to hide the Blazor Toasts after the delay. The default delay is 5000 milliseconds, be sure to update the delay timeout so that users have enough time to read the toast."),(0,n.kt)("img",{src:"https://i.imgur.com/W1YkmJH.png",alt:"Blazor Bootstrap: Blazor Toasts Component - Auto hide"}),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{1} showLineNumbers","{1}":!0,showLineNumbers:!0},'<Toasts class="p-3" Messages="messages" AutoHide="true" Delay="6000" Placement="ToastsPlacement.TopRight" />\n\n<Button Color="ButtonColor.Primary" @onclick="() => ShowMessage(ToastType.Primary)">Primary Toast</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ShowMessage(ToastType.Secondary)">Secondary Toast</Button>\n<Button Color="ButtonColor.Success" @onclick="() => ShowMessage(ToastType.Success)">Success Toast</Button>\n<Button Color="ButtonColor.Danger" @onclick="() => ShowMessage(ToastType.Danger)">Danger Toast</Button>\n<Button Color="ButtonColor.Warning" @onclick="() => ShowMessage(ToastType.Warning)">Warning Toast</Button>\n<Button Color="ButtonColor.Info" @onclick="() => ShowMessage(ToastType.Info)">Info Toast</Button>\n<Button Color="ButtonColor.Dark" @onclick="() => ShowMessage(ToastType.Dark)">Dark Toast</Button>\n')),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cs",metastring:"showLineNumbers",showLineNumbers:!0},'@code {\n    List<ToastMessage> messages = new List<ToastMessage>();\n\n    private void ShowMessage(ToastType toastType) => messages.Add(CreateToastMessage(toastType));\n\n    private ToastMessage CreateToastMessage(ToastType toastType)\n    => new ToastMessage\n        {\n            Type = toastType,\n            Title = "Blazor Bootstrap",\n            HelpText = $"{DateTime.Now}",\n            Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",\n        };\n}\n')),(0,n.kt)("p",null,(0,n.kt)("a",{parentName:"p",href:"https://demos.getblazorbootstrap.com/toasts#auto-hide"},"See auto hide toasts demo here.")),(0,n.kt)("h3",{id:"placement"},"Placement"),(0,n.kt)("p",null,"Change the Blazor Toasts placement according to your need. The default placement will be top right corner. Use the ",(0,n.kt)("inlineCode",{parentName:"p"},"ToastsPlacement")," parameter to update the Blazor Toasts placement."),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{1} showLineNumbers","{1}":!0,showLineNumbers:!0},'<Toasts class="p-3" Messages="messages" Placement="@toastsPlacement" />\n\n<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.TopLeft)">Top Left</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.TopCenter)">Top Center</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.TopRight)">Top Right</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.MiddleLeft)">Middle Left</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.MiddleCenter)">Middle Center</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.MiddleRight)">Middle Right</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.BottomLeft)">Bottom Left</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.BottomCenter)">Bottom Center</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ChangePlacement(ToastsPlacement.BottomRight)">Bottom Right</Button>\n')),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cs",metastring:"showLineNumbers",showLineNumbers:!0},'@code {\n    ToastsPlacement toastsPlacement = ToastsPlacement.TopRight;\n    List<ToastMessage> messages = new();\n\n    private void ChangePlacement(ToastsPlacement placement)\n    {\n        if (!messages.Any())\n        {\n            messages.Add(\n                new ToastMessage()\n                    {\n                        Type = ToastType.Success,\n                        Title = "Blazor Bootstrap",\n                        HelpText = $"{DateTime.Now}",\n                        Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",\n                    });\n        }\n        toastsPlacement = placement;\n    }\n}\n')),(0,n.kt)("p",null,(0,n.kt)("a",{parentName:"p",href:"https://demos.getblazorbootstrap.com/toasts#placement"},"See demo here.")),(0,n.kt)("h3",{id:"stack-length"},"Stack Length"),(0,n.kt)("p",null,"Blazor Toasts component shows a maximum of 5 toasts by default. If you add a new toast to the existing list, the first toast gets deleted like FIFO (First In First Out). Change the maximum capacity according to your need by using the ",(0,n.kt)("strong",{parentName:"p"},"StackLength")," parameter."),(0,n.kt)("p",null,"In the below example, StackLength is set to 3. It shows a maximum of 3 toast messages at any time."),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{1} showLineNumbers","{1}":!0,showLineNumbers:!0},'<Toasts class="p-3" Messages="messages" AutoHide="true" StackLength="3" Placement="ToastsPlacement.TopRight" />\n\n<Button Color="ButtonColor.Primary" @onclick="() => ShowMessage(ToastType.Primary)">Primary Toast</Button>\n<Button Color="ButtonColor.Secondary" @onclick="() => ShowMessage(ToastType.Secondary)">Secondary Toast</Button>\n<Button Color="ButtonColor.Success" @onclick="() => ShowMessage(ToastType.Success)">Success Toast</Button>\n<Button Color="ButtonColor.Danger" @onclick="() => ShowMessage(ToastType.Danger)">Danger Toast</Button>\n<Button Color="ButtonColor.Warning" @onclick="() => ShowMessage(ToastType.Warning)">Warning Toast</Button>\n<Button Color="ButtonColor.Info" @onclick="() => ShowMessage(ToastType.Info)">Info Toast</Button>\n<Button Color="ButtonColor.Dark" @onclick="() => ShowMessage(ToastType.Dark)">Dark Toast</Button>\n')),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre",className:"language-cs",metastring:"showLineNumbers",showLineNumbers:!0},'@code {\n    List<ToastMessage> messages = new List<ToastMessage>();\n\n    private void ShowMessage(ToastType toastType) => messages.Add(CreateToastMessage(toastType));\n\n    private ToastMessage CreateToastMessage(ToastType toastType)\n    => new ToastMessage\n        {\n            Type = toastType,\n            Title = "Blazor Bootstrap",\n            HelpText = $"{DateTime.Now}",\n            Message = $"Hello, world! This is a toast message. DateTime: {DateTime.Now}",\n        };\n}\n')),(0,n.kt)("p",null,(0,n.kt)("a",{parentName:"p",href:"https://demos.getblazorbootstrap.com/toasts#stack-length"},"See demo here.")))}c.isMDXComponent=!0}}]);