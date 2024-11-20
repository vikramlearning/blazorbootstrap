"use strict";(self.webpackChunkblazorbootstrap=self.webpackChunkblazorbootstrap||[]).push([[14239],{3905:(e,t,n)=>{n.d(t,{Zo:()=>p,kt:()=>u});var r=n(67294);function a(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function o(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function i(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?o(Object(n),!0).forEach((function(t){a(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):o(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function s(e,t){if(null==e)return{};var n,r,a=function(e,t){if(null==e)return{};var n,r,a={},o=Object.keys(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||(a[n]=e[n]);return a}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(a[n]=e[n])}return a}var l=r.createContext({}),m=function(e){var t=r.useContext(l),n=t;return e&&(n="function"==typeof e?e(t):i(i({},t),e)),n},p=function(e){var t=m(e.components);return r.createElement(l.Provider,{value:t},e.children)},c="mdxType",h={inlineCode:"code",wrapper:function(e){var t=e.children;return r.createElement(r.Fragment,{},t)}},d=r.forwardRef((function(e,t){var n=e.components,a=e.mdxType,o=e.originalType,l=e.parentName,p=s(e,["components","mdxType","originalType","parentName"]),c=m(n),d=a,u=c["".concat(l,".").concat(d)]||c[d]||h[d]||o;return n?r.createElement(u,i(i({ref:t},p),{},{components:n})):r.createElement(u,i({ref:t},p))}));function u(e,t){var n=arguments,a=t&&t.mdxType;if("string"==typeof e||a){var o=n.length,i=new Array(o);i[0]=d;var s={};for(var l in t)hasOwnProperty.call(t,l)&&(s[l]=t[l]);s.originalType=e,s[c]="string"==typeof e?e:a,i[1]=s;for(var m=2;m<o;m++)i[m]=n[m];return r.createElement.apply(null,i)}return r.createElement.apply(null,n)}d.displayName="MDXCreateElement"},74404:(e,t,n)=>{n.d(t,{ZP:()=>p});var r=n(87462),a=n(67294),o=n(3905);class i extends a.Component{constructor(e){super(e),this.name=this.props.name||"docsblazorbootstrapcom",this.serve=this.props.serve||"CWYICKJI",this.script=this.props.script||null,this.placement=this.props.placement||"",this.fallback=this.props.fallback||null,this.showFallback=!1}adShowing=()=>null!==document.getElementById(`${this.name} #carbonads`);componentDidMount=()=>{let e=document.createElement("script");e.defer=!!this.script,e.async=!0,e.id=this.script?"":"_carbonads_js",e.type="text/javascript",e.src=this.script||`//cdn.carbonads.com/carbon.js?serve=${this.serve}&placement=${this.placement}`,e.onerror=()=>{this.showFallback=!0,this.forceUpdate()},e.addEventListener("load",(()=>{this.adShowing||_.invoke(window._carbonads,"refresh")})),document.querySelector(`#${this.name}`).appendChild(e)};render(){return this.showFallback&&this.fallback?this.fallback:a.createElement("div",{id:this.name})}}const s=i,l={toc:[]},m="wrapper";function p(e){let{components:t,...n}=e;return(0,o.kt)(m,(0,r.Z)({},l,n,{components:t,mdxType:"MDXLayout"}),(0,o.kt)(s,{mdxType:"CarbonAd"}))}p.isMDXComponent=!0},69439:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>m,contentTitle:()=>s,default:()=>d,frontMatter:()=>i,metadata:()=>l,toc:()=>p});var r=n(87462),a=(n(67294),n(3905)),o=n(74404);const i={title:"Blazor Theme Switcher Component",description:"Documentation and examples for using Blazor Bootstrap Theme Switcher components.",image:"https://i.imgur.com/2eixWho.png",sidebar_label:"Theme Switcher",sidebar_position:28},s="Blazor Theme Switcher",l={unversionedId:"components/theme-switcher",id:"components/theme-switcher",title:"Blazor Theme Switcher Component",description:"Documentation and examples for using Blazor Bootstrap Theme Switcher components.",source:"@site/docs/05-components/theme-switcher.mdx",sourceDirName:"05-components",slug:"/components/theme-switcher",permalink:"/components/theme-switcher",draft:!1,editUrl:"https://github.com/vikramlearning/blazorbootstrap/edit/master/docs/docs/05-components/theme-switcher.mdx",tags:[],version:"current",sidebarPosition:28,frontMatter:{title:"Blazor Theme Switcher Component",description:"Documentation and examples for using Blazor Bootstrap Theme Switcher components.",image:"https://i.imgur.com/2eixWho.png",sidebar_label:"Theme Switcher",sidebar_position:28},sidebar:"tutorialSidebar",previous:{title:"Tabs",permalink:"/components/tabs"},next:{title:"Toasts",permalink:"/components/toasts"}},m={},p=[{value:"Parameters",id:"parameters",level:2},{value:"Events",id:"events",level:2},{value:"Examples",id:"examples",level:2},{value:"How it works",id:"how-it-works",level:3},{value:"Position",id:"position",level:3},{value:"Events",id:"events-1",level:3}],c={toc:p},h="wrapper";function d(e){let{components:t,...n}=e;return(0,a.kt)(h,(0,r.Z)({},c,n,{components:t,mdxType:"MDXLayout"}),(0,a.kt)("h1",{id:"blazor-theme-switcher"},"Blazor Theme Switcher"),(0,a.kt)("p",null,"Documentation and examples for using Blazor Bootstrap Theme Switcher components."),(0,a.kt)(o.ZP,{mdxType:"CarbonAd"}),(0,a.kt)("img",{src:"https://i.imgur.com/2eixWho.png",alt:"Blazor Theme Switcher Component"}),(0,a.kt)("h2",{id:"parameters"},"Parameters"),(0,a.kt)("table",null,(0,a.kt)("thead",{parentName:"table"},(0,a.kt)("tr",{parentName:"thead"},(0,a.kt)("th",{parentName:"tr",align:"left"},"Name"),(0,a.kt)("th",{parentName:"tr",align:"left"},"Type"),(0,a.kt)("th",{parentName:"tr",align:"left"},"Default"),(0,a.kt)("th",{parentName:"tr",align:"left"},"Required"),(0,a.kt)("th",{parentName:"tr",align:"left"},"Description"),(0,a.kt)("th",{parentName:"tr",align:"left"},"Added Version"))),(0,a.kt)("tbody",{parentName:"table"},(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:"left"},"Position"),(0,a.kt)("td",{parentName:"tr",align:"left"},(0,a.kt)("inlineCode",{parentName:"td"},"DropdownMenuPosition")),(0,a.kt)("td",{parentName:"tr",align:"left"},(0,a.kt)("inlineCode",{parentName:"td"},"DropdownMenuPosition.Start")),(0,a.kt)("td",{parentName:"tr",align:"left"},"\u2714\ufe0f"),(0,a.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the dropdown menu position."),(0,a.kt)("td",{parentName:"tr",align:"left"},"3.2.0")))),(0,a.kt)("h2",{id:"events"},"Events"),(0,a.kt)("table",null,(0,a.kt)("thead",{parentName:"table"},(0,a.kt)("tr",{parentName:"thead"},(0,a.kt)("th",{parentName:"tr",align:"left"},"Event"),(0,a.kt)("th",{parentName:"tr",align:"left"},"Description"),(0,a.kt)("th",{parentName:"tr",align:"left"},"Added Version"))),(0,a.kt)("tbody",{parentName:"table"},(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:"left"},"OnThemeChanged"),(0,a.kt)("td",{parentName:"tr",align:"left"},"Fired when the theme is changed."),(0,a.kt)("td",{parentName:"tr",align:"left"},"3.2.0")))),(0,a.kt)("h2",{id:"examples"},"Examples"),(0,a.kt)("h3",{id:"how-it-works"},"How it works"),(0,a.kt)("img",{src:"https://i.imgur.com/2eixWho.png",alt:"Blazor Theme Switcher Component - Examples"}),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<ThemeSwitcher Position="DropdownMenuPosition.End" />\n')),(0,a.kt)("p",null,(0,a.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/theme-switcher#"},"See demo here.")),(0,a.kt)("h3",{id:"position"},"Position"),(0,a.kt)("img",{src:"https://i.imgur.com/2eixWho.png",alt:"Blazor Theme Switcher Component - Examples"}),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{1-3} showLineNumbers","{1-3}":!0,showLineNumbers:!0},'<div class="float-end">\n    <ThemeSwitcher Position="DropdownMenuPosition.End" />\n</div>\n')),(0,a.kt)("p",null,(0,a.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/theme-switcher#position"},"See demo here.")),(0,a.kt)("h3",{id:"events-1"},"Events"),(0,a.kt)("img",{src:"https://i.imgur.com/2eixWho.png",alt:"Blazor Theme Switcher Component - Examples"}),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{1,8,9,12} showLineNumbers","{1,8,9,12}":!0,showLineNumbers:!0},'<ThemeSwitcher OnThemeChanged="OnThemeChanged" />\n\n@code\n{\n    [Inject]\n    ToastService ToastService { get; set; } = default!;\n\n    private void OnThemeChanged(string themeName)\n    {\n        // do something when the theme changes\n        ToastService.Notify(new(ToastType.Success, $"Theme changed to {themeName}"));\n    }\n}\n\n')),(0,a.kt)("p",null,(0,a.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/theme-switcher#events"},"See demo here.")))}d.isMDXComponent=!0}}]);