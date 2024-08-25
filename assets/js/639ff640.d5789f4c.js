"use strict";(self.webpackChunkblazorbootstrap=self.webpackChunkblazorbootstrap||[]).push([[45410],{3905:(e,t,a)=>{a.d(t,{Zo:()=>p,kt:()=>g});var n=a(67294);function r(e,t,a){return t in e?Object.defineProperty(e,t,{value:a,enumerable:!0,configurable:!0,writable:!0}):e[t]=a,e}function o(e,t){var a=Object.keys(e);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(e);t&&(n=n.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),a.push.apply(a,n)}return a}function l(e){for(var t=1;t<arguments.length;t++){var a=null!=arguments[t]?arguments[t]:{};t%2?o(Object(a),!0).forEach((function(t){r(e,t,a[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(a)):o(Object(a)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(a,t))}))}return e}function s(e,t){if(null==e)return{};var a,n,r=function(e,t){if(null==e)return{};var a,n,r={},o=Object.keys(e);for(n=0;n<o.length;n++)a=o[n],t.indexOf(a)>=0||(r[a]=e[a]);return r}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(n=0;n<o.length;n++)a=o[n],t.indexOf(a)>=0||Object.prototype.propertyIsEnumerable.call(e,a)&&(r[a]=e[a])}return r}var i=n.createContext({}),m=function(e){var t=n.useContext(i),a=t;return e&&(a="function"==typeof e?e(t):l(l({},t),e)),a},p=function(e){var t=m(e.components);return n.createElement(i.Provider,{value:t},e.children)},u="mdxType",d={inlineCode:"code",wrapper:function(e){var t=e.children;return n.createElement(n.Fragment,{},t)}},c=n.forwardRef((function(e,t){var a=e.components,r=e.mdxType,o=e.originalType,i=e.parentName,p=s(e,["components","mdxType","originalType","parentName"]),u=m(a),c=r,g=u["".concat(i,".").concat(c)]||u[c]||d[c]||o;return a?n.createElement(g,l(l({ref:t},p),{},{components:a})):n.createElement(g,l({ref:t},p))}));function g(e,t){var a=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var o=a.length,l=new Array(o);l[0]=c;var s={};for(var i in t)hasOwnProperty.call(t,i)&&(s[i]=t[i]);s.originalType=e,s[u]="string"==typeof e?e:r,l[1]=s;for(var m=2;m<o;m++)l[m]=a[m];return n.createElement.apply(null,l)}return n.createElement.apply(null,a)}c.displayName="MDXCreateElement"},74404:(e,t,a)=>{a.d(t,{ZP:()=>p});var n=a(87462),r=a(67294),o=a(3905);class l extends r.Component{constructor(e){super(e),this.name=this.props.name||"docsblazorbootstrapcom",this.serve=this.props.serve||"CWYICKJI",this.script=this.props.script||null,this.placement=this.props.placement||"",this.fallback=this.props.fallback||null,this.showFallback=!1}adShowing=()=>null!==document.getElementById(`${this.name} #carbonads`);componentDidMount=()=>{let e=document.createElement("script");e.defer=!!this.script,e.async=!0,e.id=this.script?"":"_carbonads_js",e.type="text/javascript",e.src=this.script||`//cdn.carbonads.com/carbon.js?serve=${this.serve}&placement=${this.placement}`,e.onerror=()=>{this.showFallback=!0,this.forceUpdate()},e.addEventListener("load",(()=>{this.adShowing||_.invoke(window._carbonads,"refresh")})),document.querySelector(`#${this.name}`).appendChild(e)};render(){return this.showFallback&&this.fallback?this.fallback:r.createElement("div",{id:this.name})}}const s=l,i={toc:[]},m="wrapper";function p(e){let{components:t,...a}=e;return(0,o.kt)(m,(0,n.Z)({},i,a,{components:t,mdxType:"MDXLayout"}),(0,o.kt)(s,{mdxType:"CarbonAd"}))}p.isMDXComponent=!0},52156:(e,t,a)=>{a.r(t),a.d(t,{assets:()=>m,contentTitle:()=>s,default:()=>c,frontMatter:()=>l,metadata:()=>i,toc:()=>p});var n=a(87462),r=(a(67294),a(3905)),o=a(74404);const l={title:"Blazor Carousel Component",description:"Blazor Carousel component is a slideshow component that cycles through elements, images, or slides of text.",image:"https://i.imgur.com/YoZd9Hy.png",sidebar_label:"Carousel",sidebar_position:7},s="Blazor Carousel",i={unversionedId:"components/carousel",id:"components/carousel",title:"Blazor Carousel Component",description:"Blazor Carousel component is a slideshow component that cycles through elements, images, or slides of text.",source:"@site/docs/05-components/carousel.mdx",sourceDirName:"05-components",slug:"/components/carousel",permalink:"/components/carousel",draft:!1,editUrl:"https://github.com/vikramlearning/blazorbootstrap/edit/master/docs/docs/05-components/carousel.mdx",tags:[],version:"current",sidebarPosition:7,frontMatter:{title:"Blazor Carousel Component",description:"Blazor Carousel component is a slideshow component that cycles through elements, images, or slides of text.",image:"https://i.imgur.com/YoZd9Hy.png",sidebar_label:"Carousel",sidebar_position:7},sidebar:"tutorialSidebar",previous:{title:"Card",permalink:"/components/card"},next:{title:"Charts",permalink:"/components/charts"}},m={},p=[{value:"Parameters",id:"parameters",level:2},{value:"Carousel Parameters",id:"carousel-parameters",level:3},{value:"CarouselItem Parameters",id:"carouselitem-parameters",level:3},{value:"Methods",id:"methods",level:2},{value:"Callback Events",id:"callback-events",level:2},{value:"Examples",id:"examples",level:2},{value:"Carousel",id:"carousel",level:3},{value:"Indicators",id:"indicators",level:3},{value:"Captions",id:"captions",level:3},{value:"Crossfade",id:"crossfade",level:3},{value:"Autoplaying carousels",id:"autoplaying-carousels",level:3},{value:"Individual carousel item interval",id:"individual-carousel-item-interval",level:3},{value:"Autoplaying carousels without controls",id:"autoplaying-carousels-without-controls",level:3},{value:"Disable touch swiping",id:"disable-touch-swiping",level:3},{value:"Events",id:"events",level:3}],u={toc:p},d="wrapper";function c(e){let{components:t,...a}=e;return(0,r.kt)(d,(0,n.Z)({},u,a,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("h1",{id:"blazor-carousel"},"Blazor Carousel"),(0,r.kt)("p",null,"Blazor Carousel component is a slideshow component that cycles through elements, images, or slides of text."),(0,r.kt)(o.ZP,{mdxType:"CarbonAd"}),(0,r.kt)("img",{src:"https://i.imgur.com/YoZd9Hy.png",alt:"Blazor Bootstrap: Carousel component"}),(0,r.kt)("h2",{id:"parameters"},"Parameters"),(0,r.kt)("h3",{id:"carousel-parameters"},"Carousel Parameters"),(0,r.kt)("table",null,(0,r.kt)("thead",{parentName:"table"},(0,r.kt)("tr",{parentName:"thead"},(0,r.kt)("th",{parentName:"tr",align:"left"},"Name"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Type"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Default"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Required"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Description"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Added Version"))),(0,r.kt)("tbody",{parentName:"table"},(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Autoplay"),(0,r.kt)("td",{parentName:"tr",align:"left"},(0,r.kt)("inlineCode",{parentName:"td"},"CarouselAutoPlay")),(0,r.kt)("td",{parentName:"tr",align:"left"},(0,r.kt)("inlineCode",{parentName:"td"},"CarouselAutoPlay.None")),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Controls the autoplay behavior of the carousel."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"ChildContent"),(0,r.kt)("td",{parentName:"tr",align:"left"},"RenderFragment?"),(0,r.kt)("td",{parentName:"tr",align:"left"},"null"),(0,r.kt)("td",{parentName:"tr",align:"left"},"\u2714\ufe0f"),(0,r.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the content to be rendered within the component."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Crossfade"),(0,r.kt)("td",{parentName:"tr",align:"left"},"bool"),(0,r.kt)("td",{parentName:"tr",align:"left"},"false"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Determines whether to use a crossfade effect when transitioning between slides."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Interval"),(0,r.kt)("td",{parentName:"tr",align:"left"},"int?"),(0,r.kt)("td",{parentName:"tr",align:"left"},"5000 milliseconds"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"The amount of time to delay between automatically cycling an item."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Keyboard"),(0,r.kt)("td",{parentName:"tr",align:"left"},"bool"),(0,r.kt)("td",{parentName:"tr",align:"left"},"true"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Whether the carousel should react to keyboard events."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"ShowIndicators"),(0,r.kt)("td",{parentName:"tr",align:"left"},"bool"),(0,r.kt)("td",{parentName:"tr",align:"left"},"false"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Indicates whether to show indicators (dots) below the carousel to navigate between slides."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"ShowPreviousNextControls"),(0,r.kt)("td",{parentName:"tr",align:"left"},"bool"),(0,r.kt)("td",{parentName:"tr",align:"left"},"true"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Specifies whether to display the previous and next controls (arrows) for navigating slides."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Touch"),(0,r.kt)("td",{parentName:"tr",align:"left"},"bool"),(0,r.kt)("td",{parentName:"tr",align:"left"},"true"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Carousels support swiping left/right on touchscreen devices to move between slides. This can be disabled by setting the ",(0,r.kt)("inlineCode",{parentName:"td"},"Touch")," parameter to ",(0,r.kt)("inlineCode",{parentName:"td"},"false"),"."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")))),(0,r.kt)("h3",{id:"carouselitem-parameters"},"CarouselItem Parameters"),(0,r.kt)("table",null,(0,r.kt)("thead",{parentName:"table"},(0,r.kt)("tr",{parentName:"thead"},(0,r.kt)("th",{parentName:"tr",align:"left"},"Name"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Type"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Default"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Required"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Description"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Added Version"))),(0,r.kt)("tbody",{parentName:"table"},(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Active"),(0,r.kt)("td",{parentName:"tr",align:"left"},"bool"),(0,r.kt)("td",{parentName:"tr",align:"left"},"false"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the active state."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"ChildContent"),(0,r.kt)("td",{parentName:"tr",align:"left"},"RenderFragment"),(0,r.kt)("td",{parentName:"tr",align:"left"},"null"),(0,r.kt)("td",{parentName:"tr",align:"left"},"\u2714\ufe0f"),(0,r.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the content to be rendered within the component."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Interval"),(0,r.kt)("td",{parentName:"tr",align:"left"},"int?"),(0,r.kt)("td",{parentName:"tr",align:"left"},"5000 milliseconds"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"The amount of time to delay between automatically cycling an item."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Label"),(0,r.kt)("td",{parentName:"tr",align:"left"},"string?"),(0,r.kt)("td",{parentName:"tr",align:"left"},"null"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the aria-label."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.0.0")))),(0,r.kt)("h2",{id:"methods"},"Methods"),(0,r.kt)("table",null,(0,r.kt)("thead",{parentName:"table"},(0,r.kt)("tr",{parentName:"thead"},(0,r.kt)("th",{parentName:"tr",align:null},"Name"),(0,r.kt)("th",{parentName:"tr",align:null},"Description"),(0,r.kt)("th",{parentName:"tr",align:null},"Added Version"))),(0,r.kt)("tbody",{parentName:"table"},(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"ShowItemByIndexAsync(int index)"),(0,r.kt)("td",{parentName:"tr",align:null},"Shows ",(0,r.kt)("inlineCode",{parentName:"td"},"CarouselItem")," by index."),(0,r.kt)("td",{parentName:"tr",align:null},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"PauseCarouselAsync()"),(0,r.kt)("td",{parentName:"tr",align:null},"Shows next ",(0,r.kt)("inlineCode",{parentName:"td"},"CarouselItem"),"."),(0,r.kt)("td",{parentName:"tr",align:null},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"ShowNextItemAsync()"),(0,r.kt)("td",{parentName:"tr",align:null},"Shows next ",(0,r.kt)("inlineCode",{parentName:"td"},"CarouselItem"),"."),(0,r.kt)("td",{parentName:"tr",align:null},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"ShowPreviousItemAsync()"),(0,r.kt)("td",{parentName:"tr",align:null},"Shows previous ",(0,r.kt)("inlineCode",{parentName:"td"},"CarouselItem"),"."),(0,r.kt)("td",{parentName:"tr",align:null},"3.0.0")))),(0,r.kt)("h2",{id:"callback-events"},"Callback Events"),(0,r.kt)("table",null,(0,r.kt)("thead",{parentName:"table"},(0,r.kt)("tr",{parentName:"thead"},(0,r.kt)("th",{parentName:"tr",align:null},"Name"),(0,r.kt)("th",{parentName:"tr",align:null},"Description"),(0,r.kt)("th",{parentName:"tr",align:null},"Added Version"))),(0,r.kt)("tbody",{parentName:"table"},(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"Onslide"),(0,r.kt)("td",{parentName:"tr",align:null},"Fires immediately when the slide instance method is invoked."),(0,r.kt)("td",{parentName:"tr",align:null},"3.0.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"Onslid"),(0,r.kt)("td",{parentName:"tr",align:null},"Fired when the carousel has completed its slide transition."),(0,r.kt)("td",{parentName:"tr",align:null},"3.0.0")))),(0,r.kt)("h2",{id:"examples"},"Examples"),(0,r.kt)("h3",{id:"carousel"},"Carousel"),(0,r.kt)("p",null,"Here is a basic example of a carousel with three slides."),(0,r.kt)("img",{src:"https://i.imgur.com/7TCatHj.png",alt:"Blazor Bootstrap: Carousel Component - Examples"}),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel>\n    <CarouselItem Active="true">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-01.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-02.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-03.png" />\n    </CarouselItem>\n</Carousel>\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/carousel#examples"},"See the demo here.")),(0,r.kt)("h3",{id:"indicators"},"Indicators"),(0,r.kt)("p",null,"You can add indicators to the carousel, alongside the previous/next controls.\nThe indicators allow users to jump directly to a particular slide.\nSet ",(0,r.kt)("inlineCode",{parentName:"p"},"ShowIndicators")," to ",(0,r.kt)("inlineCode",{parentName:"p"},"true")," to show the indicators."),(0,r.kt)("img",{src:"https://i.imgur.com/e2MK7nE.png",alt:"Blazor Bootstrap: Carousel Component - Indicators"}),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel ShowIndicators="true">\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-01.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-02.png" />\n    </CarouselItem>\n    <CarouselItem Active="true">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-03.png" />\n    </CarouselItem>\n</Carousel>\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/carousel#indicators"},"See the demo here.")),(0,r.kt)("h3",{id:"captions"},"Captions"),(0,r.kt)("p",null,"You can add captions to your slides with the ",(0,r.kt)("inlineCode",{parentName:"p"},"CarouselCaption")," component within any ",(0,r.kt)("inlineCode",{parentName:"p"},"CarouselItem"),".\nThey can be easily hidden on smaller viewports."),(0,r.kt)("img",{src:"https://i.imgur.com/HoYACSr.png",alt:"Blazor Bootstrap: Carousel Component - Captions"}),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel ShowIndicators="true">\n    <CarouselItem Active="true">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-04.png" />\n        <CarouselCaption>\n            <h2>Earth Day</h2>\n            <p>Let\'s unite to protect our planet and create a sustainable future for generations to come.</p>\n        </CarouselCaption>\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-05.png" />\n        <CarouselCaption>\n            <h2>International Yoga Day</h2>\n            <p>Embrace the ancient art of harmony for a healthier, happier you.</p>\n        </CarouselCaption>\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-06.png" />\n        <CarouselCaption>\n            <h2>World Water Day</h2>\n            <p>Every drop counts, let\'s protect our planet\'s most precious resource.</p>\n        </CarouselCaption>\n    </CarouselItem>\n</Carousel>\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/carousel#captions"},"See the demo here.")),(0,r.kt)("h3",{id:"crossfade"},"Crossfade"),(0,r.kt)("p",null,"To animate slides with a fading transition instead of sliding, set ",(0,r.kt)("inlineCode",{parentName:"p"},"Crossfade")," to ",(0,r.kt)("inlineCode",{parentName:"p"},"true"),"."),(0,r.kt)("img",{src:"https://i.imgur.com/vfV9VGu.png",alt:"Blazor Bootstrap: Carousel Component - Crossfade"}),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel Crossfade="true">\n    <CarouselItem Active="true">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-01.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-02.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-03.png" />\n    </CarouselItem>\n</Carousel>\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/carousel#crossfade"},"See the demo here.")),(0,r.kt)("h3",{id:"autoplaying-carousels"},"Autoplaying carousels"),(0,r.kt)("p",null,"You can make your carousels autoplay on page load by setting the ",(0,r.kt)("inlineCode",{parentName:"p"},"Autoplay")," parameter to ",(0,r.kt)("inlineCode",{parentName:"p"},"CarouselAutoPlay.StartOnPageLoad"),".\nAutoplaying carousels automatically pause while hovered with the mouse."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel Autoplay="CarouselAutoPlay.StartOnPageLoad">\n    <CarouselItem Active="true">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-01.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-02.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-03.png" />\n    </CarouselItem>\n</Carousel>\n')),(0,r.kt)("p",null,"When the ",(0,r.kt)("inlineCode",{parentName:"p"},"Autoplay")," parameter is set to ",(0,r.kt)("inlineCode",{parentName:"p"},"CarouselAutoPlay.StartAfterUserInteraction"),", the carousel won't automatically start to cycle on page load.\nInstead, it will only start after the first user interaction."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel Autoplay="CarouselAutoPlay.StartAfterUserInteraction">\n    <CarouselItem Active="true">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-01.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-02.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-03.png" />\n    </CarouselItem>\n</Carousel>\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/carousel#autoplaying-carousels"},"See the demo here.")),(0,r.kt)("h3",{id:"individual-carousel-item-interval"},"Individual carousel item interval"),(0,r.kt)("p",null,"Add ",(0,r.kt)("inlineCode",{parentName:"p"},"Interval")," parameter to a ",(0,r.kt)("inlineCode",{parentName:"p"},"CarouselItem")," component to change the amount of time to delay between automatically cycling to the next item."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel Autoplay="CarouselAutoPlay.StartOnPageLoad">\n    <CarouselItem Active="true" Interval="10000">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-01.png" />\n    </CarouselItem>\n    <CarouselItem Interval="2000">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-02.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-03.png" />\n    </CarouselItem>\n</Carousel>\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/carousel#individual-carousel-item-interval"},"See the demo here.")),(0,r.kt)("h3",{id:"autoplaying-carousels-without-controls"},"Autoplaying carousels without controls"),(0,r.kt)("p",null,"Hide the controls by setting ",(0,r.kt)("inlineCode",{parentName:"p"},"ShowPreviousNextControls")," parameter to ",(0,r.kt)("inlineCode",{parentName:"p"},"false"),"."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel Autoplay="CarouselAutoPlay.StartOnPageLoad" ShowPreviousNextControls="false">\n    <CarouselItem Active="true">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-01.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-02.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-03.png" />\n    </CarouselItem>\n</Carousel>\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/carousel#autoplaying-carousels-without-controls"},"See the demo here.")),(0,r.kt)("h3",{id:"disable-touch-swiping"},"Disable touch swiping"),(0,r.kt)("p",null,"Carousels support swiping left/right on touchscreen devices to move between slides.\nThis can be disabled by setting the ",(0,r.kt)("inlineCode",{parentName:"p"},"Touch")," option to ",(0,r.kt)("inlineCode",{parentName:"p"},"false"),"."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel Touch="false">\n    <CarouselItem Active="true">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-01.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-02.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-03.png" />\n    </CarouselItem>\n</Carousel>\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/carousel#disable-touch-swiping"},"See the demo here.")),(0,r.kt)("h3",{id:"events"},"Events"),(0,r.kt)("p",null,"Blazor Bootstrap Carousel component exposes a two events for hooking into Carousel functionality."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<Carousel Onslid="Onslid" Onslide="Onslide">\n    <CarouselItem Active="true">\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-01.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-02.png" />\n    </CarouselItem>\n    <CarouselItem>\n        <Image Src="_content/BlazorBootstrap.Demo.RCL/images/slide-03.png" />\n    </CarouselItem>\n</Carousel>\n\n@code {\n    [Inject]\n    ToastService ToastService { get; set; } = default!;\n    private void Onslid(CarouselEventArgs e)\n    {\n        var message = new ToastMessage\n            {\n                Type = ToastType.Secondary,\n                Title = "Carousel Events",\n                HelpText = $"{DateTime.Now}",\n                Message = $"Onslid: from={e.From}, to={e.To}"\n            };\n        ToastService.Notify(message);\n    }\n\n    private void Onslide(CarouselEventArgs e)\n    {\n        var message = new ToastMessage\n            {\n                Type = ToastType.Secondary,\n                Title = "Carousel Events",\n                HelpText = $"{DateTime.Now}",\n                Message = $"Onslide: from={e.From}, to={e.To}"\n            };\n        ToastService.Notify(message);\n    }\n}\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/carousel#events"},"See the demo here.")))}c.isMDXComponent=!0}}]);