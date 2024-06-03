// Copyright (c) Microsoft Corporation. All rights reserved.
// License: MIT
// See https://github.com/microsoft/fluentui-blazor/blob/dev/src/Core/Utilities/StyleBuilder.cs

namespace BlazorBootstrap;

public readonly struct CssStyleBuilder
{
    private readonly HashSet<string> styleList;
    private readonly string? style;

    public CssStyleBuilder(string? cssStyle)
    {
        styleList = new HashSet<string>();
        style = cssStyle;
    }

    public CssStyleBuilder AddStyle(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            styleList.Add(value);

        return this;
    }

    public CssStyleBuilder AddStyle(string value, bool when)
    {
        if (when && !string.IsNullOrWhiteSpace(value))
            styleList.Add(value);

        return this;
    }

    public string? Build()
    {
        var styleNames = string.IsNullOrWhiteSpace(style) ? styleList : styleList.Union(new[] { style }).ToHashSet();

        if (!styleNames.Any())
            return null;

        return string.Join(';', styleNames);
    }

    public override string? ToString() => Build();
}
