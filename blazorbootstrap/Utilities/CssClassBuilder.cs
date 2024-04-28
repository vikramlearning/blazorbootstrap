// Copyright (c) Microsoft Corporation. All rights reserved.
// License: MIT
// See https://github.com/microsoft/fluentui-blazor/blob/dev/src/Core/Utilities/CssBuilder.cs

namespace BlazorBootstrap;

public readonly struct CssClassBuilder
{
    private readonly HashSet<string> classList;
    private readonly string? @class;

    public CssClassBuilder(string? cssClass)
    {
        classList = new HashSet<string>();
        @class = cssClass;
    }

    public CssClassBuilder AddClass(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            classList.Add(value);

        return this;
    }

    public CssClassBuilder AddClass(string value, bool when)
    {
        if (when && !string.IsNullOrWhiteSpace(value))
            classList.Add(value);

        return this;
    }

    public string? Build()
    {
        var classNames = string.IsNullOrWhiteSpace(@class) ? classList : classList.Union(new[] { @class });

        if (!classNames.Any())
            return null;

        return string.Join(" ", classNames);
    }

    public override string? ToString() => Build();
}
