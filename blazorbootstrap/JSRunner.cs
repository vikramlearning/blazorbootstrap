using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public abstract class JSRunner : IJSRunner
    {
        #region Members

        private readonly IJSRuntime runtime;

        private const string BLAZOR_BOOTSTRAP_NAMESPACE = "blazorbootstrap";

        #endregion

        #region Constructors

        protected JSRunner()
        {
        }

        public JSRunner(IJSRuntime runtime)
        {
            this.runtime = runtime;
        }

        #endregion

        #region Utilities

        public ValueTask AddClass(ElementReference elementRef, string classname)
        {
            throw new NotImplementedException();
        }

        public ValueTask RemoveClass(ElementReference elementRef, string classname)
        {
            throw new NotImplementedException();
        }

        public ValueTask ToggleClass(ElementReference elementId, string classname)
        {
            throw new NotImplementedException();
        }

        public ValueTask AddClassToBody(string classname)
        {
            throw new NotImplementedException();
        }

        public ValueTask RemoveClassFromBody(string classname)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> ParentHasClass(ElementReference elementRef, string classaname)
        {
            throw new NotImplementedException();
        }

        public ValueTask SetProperty(ElementReference elementRef, string property, object value)
        {
            throw new NotImplementedException();
        }

        public ValueTask<DomElement> GetElementInfo(ElementReference elementRef, string elementId)
        {
            throw new NotImplementedException();
        }

        public ValueTask SetTextValue(ElementReference elementRef, object value)
        {
            throw new NotImplementedException();
        }

        public ValueTask SetCaret(ElementReference elementRef, int caret)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> GetCaret(ElementReference elementRef)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Button

        public virtual ValueTask InitializeButton(ElementReference elementRef, string elementId, bool preventDefaultSubmit)
        {
            return runtime.InvokeVoidAsync($"{BLAZOR_BOOTSTRAP_NAMESPACE}.button.initialize", elementRef, elementId, preventDefaultSubmit);
        }

        public ValueTask DestroyButton(string elementId)
        {
            return runtime.InvokeVoidAsync($"{BLAZOR_BOOTSTRAP_NAMESPACE}.button.destroy", elementId);
        }

        #endregion

        #region Tooltip

        public virtual ValueTask InitializeTooltip(ElementReference elementRef, string elementId, object options)
        {
            return runtime.InvokeVoidAsync($"{BLAZOR_BOOTSTRAP_NAMESPACE}.tooltip.initialize", elementRef, elementId, options);
        }

        public virtual ValueTask DestroyTooltip(ElementReference elementRef, string elementId)
        {
            return Runtime.InvokeVoidAsync($"{BLAZOR_BOOTSTRAP_NAMESPACE}.tooltip.destroy", elementRef, elementId);
        }

        public virtual ValueTask UpdateTooltipContent(ElementReference elementRef, string elementId, string content)
        {
            return Runtime.InvokeVoidAsync($"{BLAZOR_BOOTSTRAP_NAMESPACE}.tooltip.updateContent", elementRef, elementId, content);
        }

        public ValueTask Focus(ElementReference elementRef, string elementId, bool scrollToElement)
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetBreakpoint()
        {
            throw new NotImplementedException();
        }

        public ValueTask ScrollIntoView(string anchorTarget)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Properties

        protected IJSRuntime Runtime => runtime;

        #endregion
    }
}
