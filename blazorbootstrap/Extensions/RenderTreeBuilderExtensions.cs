using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BlazorBootstrap.Extensions
{
    public static class RenderTreeBuilderExtensions
    {
        public static RenderTreeBuilder OpenElement(this RenderTreeBuilder builder, string name, [CallerLineNumber] int line = 0)
        {
            builder.OpenElement(line, name);

            return builder;
        }

        public static RenderTreeBuilder OpenComponent(this RenderTreeBuilder builder, Type componentType, [CallerLineNumber] int line = 0)
        {
            builder.OpenComponent(line, componentType);

            return builder;
        }

        public static RenderTreeBuilder OpenComponent<TComponent>(this RenderTreeBuilder builder, [CallerLineNumber] int line = 0)
            where TComponent : IComponent
        {
            builder.OpenComponent<TComponent>(line);

            return builder;
        }

        public static RenderTreeBuilder Id(this RenderTreeBuilder builder, object value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, "id", value);

            return builder;
        }

        public static RenderTreeBuilder Type(this RenderTreeBuilder builder, string value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, "type", value);

            return builder;
        }

        public static RenderTreeBuilder Role(this RenderTreeBuilder builder, string value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, "role", value);

            return builder;
        }

        public static RenderTreeBuilder Class(this RenderTreeBuilder builder, string value, [CallerLineNumber] int line = 0)
        {
            if (!string.IsNullOrEmpty(value))
            {
                builder.AddAttribute(line, "class", value);
            }

            return builder;
        }

        public static RenderTreeBuilder Style(this RenderTreeBuilder builder, string value, [CallerLineNumber] int line = 0)
        {
            if (!string.IsNullOrEmpty(value))
            {
                builder.AddAttribute(line, "style", value);
            }

            return builder;
        }

        public static RenderTreeBuilder Href(this RenderTreeBuilder builder, string value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, "href", value);

            return builder;
        }

        public static RenderTreeBuilder Width(this RenderTreeBuilder builder, string value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, "width", value);

            return builder;
        }

        public static RenderTreeBuilder Height(this RenderTreeBuilder builder, string value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, "height", value);

            return builder;
        }

        public static RenderTreeBuilder Fill(this RenderTreeBuilder builder, string value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, "fill", value);

            return builder;
        }

        public static RenderTreeBuilder Target(this RenderTreeBuilder builder, Target target, [CallerLineNumber] int line = 0)
        {
            if (target != BlazorBootstrap.Target.None)
            {
                builder.AddAttribute(line, "target", target.ToTargetString());
            }

            return builder;
        }

        public static RenderTreeBuilder Disabled(this RenderTreeBuilder builder, bool value, [CallerLineNumber] int line = 0)
        {
            if (value)
                builder.AddAttribute(line, "disabled", true);

            return builder;
        }

        public static RenderTreeBuilder Readonly(this RenderTreeBuilder builder, bool value, [CallerLineNumber] int line = 0)
        {
            if (value)
                builder.AddAttribute(line, "readonly", "true");

            return builder;
        }

        public static RenderTreeBuilder Aria(this RenderTreeBuilder builder, string name, object value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, $"aria-{name}", value);

            return builder;
        }

        public static RenderTreeBuilder DataBootstrap(this RenderTreeBuilder builder, string name, object value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, $"data-bs-{name}", value);
            return builder;
        }

        public static RenderTreeBuilder TabIndex(this RenderTreeBuilder builder, int? value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, "tabindex", value);

            return builder;
        }

        public static RenderTreeBuilder AriaPressed(this RenderTreeBuilder builder, bool value, [CallerLineNumber] int line = 0)
        {
            if (value)
            {
                return Aria(builder, "pressed", "true", line);
            }
            return builder;
        }

        public static RenderTreeBuilder AriaHidden(this RenderTreeBuilder builder, object value, [CallerLineNumber] int line = 0)
        {
            return Aria(builder, "hidden", value, line);
        }

        public static RenderTreeBuilder AriaLabel(this RenderTreeBuilder builder, object value, [CallerLineNumber] int line = 0)
        {
            return Aria(builder, "label", value, line);
        }

        public static RenderTreeBuilder AriaDisabled(this RenderTreeBuilder builder, object value, [CallerLineNumber] int line = 0)
        {
            return Aria(builder, "disabled", value, line);
        }

        public static RenderTreeBuilder Data(this RenderTreeBuilder builder, string name, object value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, $"data-{name}", value);

            return builder;
        }

        public static RenderTreeBuilder Attribute(this RenderTreeBuilder builder, string name, object value, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, name, value);

            return builder;
        }

        public static RenderTreeBuilder OnClick(this RenderTreeBuilder builder, object receiver, EventCallback callback, [CallerLineNumber] int line = 0)
        {
            builder.AddAttribute(line, "onclick", EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(receiver, callback));

            return builder;
        }

        public static RenderTreeBuilder OnClickPreventDefault(this RenderTreeBuilder builder, bool preventDefault, [CallerLineNumber] int line = 0)
        {
            builder.AddEventPreventDefaultAttribute(line, "onclick", preventDefault);

            return builder;
        }

        public static RenderTreeBuilder Content(this RenderTreeBuilder builder, RenderFragment fragment, [CallerLineNumber] int line = 0)
        {
            builder.AddContent(line, fragment);

            return builder;
        }

        public static RenderTreeBuilder MarkupContent(this RenderTreeBuilder builder, string? markupContent, [CallerLineNumber] int line = 0)
        {
            builder.AddMarkupContent(line, markupContent);

            return builder;
        }

        public static RenderTreeBuilder Attributes(this RenderTreeBuilder builder, Dictionary<string, object> attributes, [CallerLineNumber] int line = 0)
        {
            builder.AddMultipleAttributes(line, attributes);

            return builder;
        }

        public static RenderTreeBuilder ComponentReferenceCapture(this RenderTreeBuilder builder, Action<object> action, [CallerLineNumber] int line = 0)
        {
            builder.AddComponentReferenceCapture(line, action);

            return builder;
        }

        public static RenderTreeBuilder ElementReferenceCapture(this RenderTreeBuilder builder, Action<ElementReference> action, [CallerLineNumber] int line = 0)
        {
            builder.AddElementReferenceCapture(line, action);

            return builder;
        }
    }
}
