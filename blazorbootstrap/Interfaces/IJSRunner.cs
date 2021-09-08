using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBootstrap
{
    public interface IJSRunner
    {
        #region Utilities

        ValueTask AddClass(ElementReference elementRef, string classname);

        ValueTask RemoveClass(ElementReference elementRef, string classname);

        ValueTask ToggleClass(ElementReference elementId, string classname);

        ValueTask AddClassToBody(string classname);

        ValueTask RemoveClassFromBody(string classname);

        ValueTask<bool> ParentHasClass(ElementReference elementRef, string classaname);

        ValueTask SetProperty(ElementReference elementRef, string property, object value);

        ValueTask<DomElement> GetElementInfo(ElementReference elementRef, string elementId);

        ValueTask SetTextValue(ElementReference elementRef, object value);

        ValueTask SetCaret(ElementReference elementRef, int caret);

        ValueTask<int> GetCaret(ElementReference elementRef);

        ValueTask Focus(ElementReference elementRef, string elementId, bool scrollToElement);

        ValueTask<string> GetBreakpoint();

        ValueTask ScrollIntoView(string anchorTarget);

        #endregion

        #region Button

        ValueTask InitializeButton(ElementReference elementRef, string elementId, bool preventDefaultSubmit);

        ValueTask DestroyButton(string elementId);

        #endregion

        #region Tooltip

        ValueTask InitializeTooltip(ElementReference elementRef, string elementId, object options);

        ValueTask DestroyTooltip(ElementReference elementRef, string elementId);

        ValueTask UpdateTooltipContent(ElementReference elementRef, string elementId, string content);

        #endregion
    }
}
