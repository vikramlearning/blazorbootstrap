"use strict";(self.webpackChunkblazorbootstrap=self.webpackChunkblazorbootstrap||[]).push([[14826],{3905:(e,t,n)=>{n.d(t,{Zo:()=>p,kt:()=>b});var a=n(67294);function r(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function s(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);t&&(a=a.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,a)}return n}function o(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?s(Object(n),!0).forEach((function(t){r(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):s(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function i(e,t){if(null==e)return{};var n,a,r=function(e,t){if(null==e)return{};var n,a,r={},s=Object.keys(e);for(a=0;a<s.length;a++)n=s[a],t.indexOf(n)>=0||(r[n]=e[n]);return r}(e,t);if(Object.getOwnPropertySymbols){var s=Object.getOwnPropertySymbols(e);for(a=0;a<s.length;a++)n=s[a],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(r[n]=e[n])}return r}var l=a.createContext({}),d=function(e){var t=a.useContext(l),n=t;return e&&(n="function"==typeof e?e(t):o(o({},t),e)),n},p=function(e){var t=d(e.components);return a.createElement(l.Provider,{value:t},e.children)},m="mdxType",u={inlineCode:"code",wrapper:function(e){var t=e.children;return a.createElement(a.Fragment,{},t)}},c=a.forwardRef((function(e,t){var n=e.components,r=e.mdxType,s=e.originalType,l=e.parentName,p=i(e,["components","mdxType","originalType","parentName"]),m=d(n),c=r,b=m["".concat(l,".").concat(c)]||m[c]||u[c]||s;return n?a.createElement(b,o(o({ref:t},p),{},{components:n})):a.createElement(b,o({ref:t},p))}));function b(e,t){var n=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var s=n.length,o=new Array(s);o[0]=c;var i={};for(var l in t)hasOwnProperty.call(t,l)&&(i[l]=t[l]);i.originalType=e,i[m]="string"==typeof e?e:r,o[1]=i;for(var d=2;d<s;d++)o[d]=n[d];return a.createElement.apply(null,o)}return a.createElement.apply(null,n)}c.displayName="MDXCreateElement"},74404:(e,t,n)=>{n.d(t,{ZP:()=>p});var a=n(87462),r=n(67294),s=n(3905);class o extends r.Component{constructor(e){super(e),this.name=this.props.name||"docsblazorbootstrapcom",this.serve=this.props.serve||"CWYICKJI",this.script=this.props.script||null,this.placement=this.props.placement||"",this.fallback=this.props.fallback||null,this.showFallback=!1}adShowing=()=>null!==document.getElementById(`${this.name} #carbonads`);componentDidMount=()=>{let e=document.createElement("script");e.defer=!!this.script,e.async=!0,e.id=this.script?"":"_carbonads_js",e.type="text/javascript",e.src=this.script||`//cdn.carbonads.com/carbon.js?serve=${this.serve}&placement=${this.placement}`,e.onerror=()=>{this.showFallback=!0,this.forceUpdate()},e.addEventListener("load",(()=>{this.adShowing||_.invoke(window._carbonads,"refresh")})),document.querySelector(`#${this.name}`).appendChild(e)};render(){return this.showFallback&&this.fallback?this.fallback:r.createElement("div",{id:this.name})}}const i=o,l={toc:[]},d="wrapper";function p(e){let{components:t,...n}=e;return(0,s.kt)(d,(0,a.Z)({},l,n,{components:t,mdxType:"MDXLayout"}),(0,s.kt)(i,{mdxType:"CarbonAd"}))}p.isMDXComponent=!0},26364:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>d,contentTitle:()=>i,default:()=>c,frontMatter:()=>o,metadata:()=>l,toc:()=>p});var a=n(87462),r=(n(67294),n(3905)),s=n(74404);const o={title:"Blazor Password Input Component",description:"The Blazor Bootstrap PasswordInput component is constructed using an HTML input of type 'password'.",image:"https://i.imgur.com/0k1C2XS.png",sidebar_label:"Password Input",sidebar_position:6},i="Blazor Password Input",l={unversionedId:"forms/password-input",id:"forms/password-input",title:"Blazor Password Input Component",description:"The Blazor Bootstrap PasswordInput component is constructed using an HTML input of type 'password'.",source:"@site/docs/04-forms/password-input.mdx",sourceDirName:"04-forms",slug:"/forms/password-input",permalink:"/forms/password-input",draft:!1,editUrl:"https://github.com/vikramlearning/blazorbootstrap/edit/master/docs/docs/04-forms/password-input.mdx",tags:[],version:"current",sidebarPosition:6,frontMatter:{title:"Blazor Password Input Component",description:"The Blazor Bootstrap PasswordInput component is constructed using an HTML input of type 'password'.",image:"https://i.imgur.com/0k1C2XS.png",sidebar_label:"Password Input",sidebar_position:6},sidebar:"tutorialSidebar",previous:{title:"Number Input",permalink:"/forms/number-input"},next:{title:"Radio Input",permalink:"/forms/radio-input"}},d={},p=[{value:"Parameters",id:"parameters",level:2},{value:"Methods",id:"methods",level:2},{value:"Events",id:"events",level:2},{value:"Examples",id:"examples",level:2},{value:"Basic Usage",id:"basic-usage",level:3},{value:"Disable",id:"disable",level:3},{value:"Valdations",id:"valdations",level:3},{value:"Events: ValueChanged",id:"events-valuechanged",level:3}],m={toc:p},u="wrapper";function c(e){let{components:t,...n}=e;return(0,r.kt)(u,(0,a.Z)({},m,n,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("h1",{id:"blazor-password-input"},"Blazor Password Input"),(0,r.kt)("p",null,"The Blazor Bootstrap ",(0,r.kt)("inlineCode",{parentName:"p"},"PasswordInput")," component is constructed using an HTML input of type ",(0,r.kt)("em",{parentName:"p"},"password"),"."),(0,r.kt)(s.ZP,{mdxType:"CarbonAd"}),(0,r.kt)("img",{src:"https://i.imgur.com/0k1C2XS.png",alt:"Blazor Bootstrap: Password Input Component"}),(0,r.kt)("h2",{id:"parameters"},"Parameters"),(0,r.kt)("table",null,(0,r.kt)("thead",{parentName:"table"},(0,r.kt)("tr",{parentName:"thead"},(0,r.kt)("th",{parentName:"tr",align:"left"},"Name"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Type"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Default"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Required"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Description"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Added Version"))),(0,r.kt)("tbody",{parentName:"table"},(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Disabled"),(0,r.kt)("td",{parentName:"tr",align:"left"},"bool"),(0,r.kt)("td",{parentName:"tr",align:"left"},"false"),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the disabled state."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.3.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"ShowHidePasswordButtonCssClass"),(0,r.kt)("td",{parentName:"tr",align:"left"},"string"),(0,r.kt)("td",{parentName:"tr",align:"left"},(0,r.kt)("inlineCode",{parentName:"td"},"btn border-top border-end border-bottom border border-start-0")),(0,r.kt)("td",{parentName:"tr",align:"left"}),(0,r.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the show/hide password button CSS class."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.3.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Value"),(0,r.kt)("td",{parentName:"tr",align:"left"},"string"),(0,r.kt)("td",{parentName:"tr",align:"left"},"null"),(0,r.kt)("td",{parentName:"tr",align:"left"},"\u2714\ufe0f"),(0,r.kt)("td",{parentName:"tr",align:"left"},"Gets or sets the value."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.3.0")))),(0,r.kt)("h2",{id:"methods"},"Methods"),(0,r.kt)("table",null,(0,r.kt)("thead",{parentName:"table"},(0,r.kt)("tr",{parentName:"thead"},(0,r.kt)("th",{parentName:"tr",align:"left"},"Name"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Returns"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Description"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Added Version"))),(0,r.kt)("tbody",{parentName:"table"},(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Disable()"),(0,r.kt)("td",{parentName:"tr",align:"left"},"void"),(0,r.kt)("td",{parentName:"tr",align:"left"},"Disables checkbox input."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.3.0")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"Enable()"),(0,r.kt)("td",{parentName:"tr",align:"left"},"void"),(0,r.kt)("td",{parentName:"tr",align:"left"},"Enables checkbox input."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.3.0")))),(0,r.kt)("h2",{id:"events"},"Events"),(0,r.kt)("table",null,(0,r.kt)("thead",{parentName:"table"},(0,r.kt)("tr",{parentName:"thead"},(0,r.kt)("th",{parentName:"tr",align:"left"},"Name"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Description"),(0,r.kt)("th",{parentName:"tr",align:"left"},"Added Version"))),(0,r.kt)("tbody",{parentName:"table"},(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:"left"},"ValueChanged"),(0,r.kt)("td",{parentName:"tr",align:"left"},"This event is fired when the ",(0,r.kt)("inlineCode",{parentName:"td"},"PasswordInput")," value changes."),(0,r.kt)("td",{parentName:"tr",align:"left"},"3.3.0")))),(0,r.kt)("h2",{id:"examples"},"Examples"),(0,r.kt)("h3",{id:"basic-usage"},"Basic Usage"),(0,r.kt)("img",{src:"https://i.imgur.com/0k1C2XS.png",alt:"Blazor Bootstrap Password Input Component - Basic Usage"}),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<div class="mb-3">\n    <PasswordInput @bind-Value="@enteredPassword" />\n</div>\n<div class="mb-3">Entered password: @enteredPassword</div>\n')),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cs",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},"@code {\n    private string? enteredPassword = null;\n}\n")),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/form/password-input#basic-usage"},"See demo here")),(0,r.kt)("h3",{id:"disable"},"Disable"),(0,r.kt)("p",null,"Use the ",(0,r.kt)("inlineCode",{parentName:"p"},"Disabled")," parameter to disable the ",(0,r.kt)("inlineCode",{parentName:"p"},"PasswordInput"),"."),(0,r.kt)("img",{src:"https://i.imgur.com/kx7YYCD.png",alt:"Blazor Bootstrap Password Input Component - Disable"}),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<div class="mb-3">\n    <PasswordInput @bind-Value="@enteredPassword" Disabled="@disabled" />\n</div>\n<div class="mb-3">Entered password: @enteredPassword</div>\n\n<Button Color="ButtonColor.Primary" @onclick="Enable"> Enable </Button>\n<Button Color="ButtonColor.Secondary" @onclick="Disable"> Disable </Button>\n<Button Color="ButtonColor.Warning" @onclick="Toggle"> Toggle </Button>\n')),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cs",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},"@code {\n    private string? enteredPassword = null;\n\n    private bool disabled = true;\n\n    private void Enable() => disabled = false;\n\n    private void Disable() => disabled = true;\n\n    private void Toggle() => disabled = !disabled;\n}\n")),(0,r.kt)("p",null,"Also, use ",(0,r.kt)("inlineCode",{parentName:"p"},"Enable()")," and ",(0,r.kt)("inlineCode",{parentName:"p"},"Disable()")," methods to enable and disable the PasswordInput."),(0,r.kt)("admonition",{title:"NOTE",type:"caution"},(0,r.kt)("p",{parentName:"admonition"},"Do not use both the ",(0,r.kt)("inlineCode",{parentName:"p"},"Disabled")," parameter and ",(0,r.kt)("inlineCode",{parentName:"p"},"Enable()")," & ",(0,r.kt)("inlineCode",{parentName:"p"},"Disable()")," methods.")),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<div class="mb-3">\n    <PasswordInput @ref="passwordInputRef" @bind-Value="@enteredPassword" />\n</div>\n<div class="mb-3">Entered text: @enteredPassword</div>\n\n<Button Color="ButtonColor.Secondary" @onclick="Disable"> Disable </Button>\n<Button Color="ButtonColor.Primary" @onclick="Enable"> Enable </Button>\n')),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cs",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},"@code {\n    private PasswordInput? passwordInputRef;\n\n    private string? enteredPassword = null;\n\n    private void Disable() => passwordInputRef.Disable();\n\n    private void Enable() => passwordInputRef.Enable();\n}\n")),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/form/password-input#disable"},"See demo here")),(0,r.kt)("h3",{id:"valdations"},"Valdations"),(0,r.kt)("p",null,"Like any other blazor input component, ",(0,r.kt)("inlineCode",{parentName:"p"},"PasswordInput")," supports validations.\nAdd the DataAnnotations on the ",(0,r.kt)("inlineCode",{parentName:"p"},"PasswordInput")," component to validate the user input before submitting the form.\nIn the below example, we used ",(0,r.kt)("strong",{parentName:"p"},"Required")," attribute."),(0,r.kt)("img",{src:"https://i.imgur.com/8QB9rxh.png",alt:"Blazor Bootstrap Password Input Component - Events: ValueChanged"}),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'@using System.ComponentModel.DataAnnotations\n\n<style>\n    .valid.modified:not([type=checkbox]) {\n        outline: 1px solid #26b050;\n    }\n\n    .invalid {\n        outline: 1px solid red;\n    }\n\n    .validation-message {\n        color: red;\n    }\n</style>\n\n<EditForm EditContext="@editContext" OnValidSubmit="HandleOnValidSubmit">\n    <DataAnnotationsValidator />\n\n    <div class="form-group row mb-3">\n        <label class="col-md-2 col-form-label">User name: <span class="text-danger">*</span></label>\n        <div class="col-md-10">\n            <TextInput @bind-Value="@userLogin.UserName" Placeholder="Enter user name" />\n            <ValidationMessage For="@(() => userLogin.UserName)" />\n        </div>\n    </div>\n\n    <div class="form-group row mb-3">\n        <label class="col-md-2 col-form-label">Password: <span class="text-danger">*</span></label>\n        <div class="col-md-10">\n            <PasswordInput @bind-Value="@userLogin.Password" />\n            <ValidationMessage For="@(() => userLogin.Password)" />\n        </div>\n    </div>\n\n    <div class="row">\n        <div class="col-md-12 text-right">\n            <Button Type="ButtonType.Button" Color="ButtonColor.Secondary" Class="float-end" @onclick="ResetForm">Reset</Button>\n            <Button Type="ButtonType.Submit" Color="ButtonColor.Success" Class="float-end me-2">Login</Button>\n        </div>\n    </div>\n\n</EditForm>\n')),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cs",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'@code {\n    private UserLogin userLogin = new();\n    private EditContext? editContext;\n\n    protected override void OnInitialized()\n    {\n        editContext = new EditContext(userLogin);\n        base.OnInitialized();\n    }\n\n    public void HandleOnValidSubmit()\n    {\n        // additional check\n        if (editContext.Validate())\n        {\n            // do something\n            // submit the form\n            Console.WriteLine("Login successful");\n        }\n    }\n\n    private void ResetForm()\n    {\n        userLogin = new();\n        editContext = new EditContext(userLogin);\n    }\n\n    public class UserLogin\n    {\n        [Required(ErrorMessage = "User name required.")]\n        public string? UserName { get; set; }\n\n        [Required(ErrorMessage = "Password required.")]\n        public string? Password { get; set; }\n    }\n}\n')),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/form/password-input#validations"},"See demo here")),(0,r.kt)("h3",{id:"events-valuechanged"},"Events: ValueChanged"),(0,r.kt)("p",null,"This event fires when the ",(0,r.kt)("inlineCode",{parentName:"p"},"PasswordInput")," value changes, but not on every keystroke."),(0,r.kt)("img",{src:"https://i.imgur.com/0k1C2XS.png",alt:"Blazor Bootstrap Password Input Component - Events: ValueChanged"}),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cshtml",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},'<div class="mb-3">\n    <PasswordInput Value="@enteredPassword" ValueExpression="() => enteredPassword" ValueChanged="(value) => PasswordChanged(value)" />\n</div>\n<div class="mb-3">Entered password: @enteredPassword</div>\n')),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-cs",metastring:"{} showLineNumbers","{}":!0,showLineNumbers:!0},"@code {\n    private string? enteredPassword = null;\n\n    private void PasswordChanged(string? value)\n    {\n        enteredPassword = value;\n\n        // do something\n    }\n}\n")),(0,r.kt)("p",null,(0,r.kt)("a",{parentName:"p",href:"https://demos.blazorbootstrap.com/form/password-input#events-value-changed"},"See demo here")))}c.isMDXComponent=!0}}]);