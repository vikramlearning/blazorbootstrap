---
id: blazor-webassembly
sidebar_label: Blazor WebAssembly
sidebar_position: 1
title: Blazor WebAssembly
---

# Getting started - Blazor WebAssembly

Get started with the Enterprise-class Blazor Bootstrap Component library built on the Blazor and Bootstrap CSS framework.

## Install Nuget Package

Looking to quickly add **Blazor Bootstrap** to your project? Use NuGet package manager.

```shell
Install-Package Blazor.Bootstrap -Version 1.8.0
```

## Add CSS references

Add the following references to the `head` section in the **wwwroot/index.html**.

```html showLineNumbers
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-rbsA2VBKQhggwzxH7pPCaAqO46MgnOM80zW1RWuH61DGLwZJEdK2Kadq2F9CUG65" crossorigin="anonymous" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.3/font/bootstrap-icons.css" rel="stylesheet" />
<link href="_content/Blazor.Bootstrap/blazor.bootstrap.css" rel="stylesheet" />
```

:::note
If you use the **Blazor WebAssembly App Empty*** template (without demonstration code and Bootstrap), add the following references to the `head` section in the **wwwroot/index.html**. 
There is a known GitHub issue [Blazor empty template doesn't load scoped CSS](https://github.com/dotnet/aspnetcore/issues/43975).
:::

```html showLineNumbers
<link href="_content/Blazor.Bootstrap/Blazor.Bootstrap.bundle.scp.css" rel="stylesheet" />
```

## Add script references

Add the following references to the `body` section in the **wwwroot/index.html** after the **_framework/blazor.webassembly.js** reference.


``` js showLineNumbers
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-kenU1KFdBIe4zVF0s0G1M5b4hcpxyD9F7jL+jjXkk+Q2h455rYXK/7HAuoJl+0I4" crossorigin="anonymous"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.0.1/chart.umd.js" integrity="sha512-gQhCDsnnnUfaRzD8k1L5llCCV6O9HN09zClIzzeJ8OJ9MpGmIlCxm+pdCkqTwqJ4JcjbojFr79rl2F1mzcoLMQ==" crossorigin="anonymous" referrerpolicy="no-referrer"></script> <!-- Add chart.js reference if Chart components are used in the application. -->
<script src="_content/Blazor.Bootstrap/blazor.bootstrap.js"></script>
```

:::tip
**chart.js** reference is optional. Add when the **Chart** components are used in the application.
:::

## Register services

Add Blazor Bootstrap service in the **Program.cs**

```cs showLineNumbers
using BlazorBootstrap; // Add this line

...
         
builder.Services.AddBlazorBootstrap(); // Add this line
```

Register tag helpers in **_Imports.razor**

```razor showLineNumbers
@using BlazorBootstrap;
```

## Remove default references

In the **wwwroot/index.html** file, the default blazor template includes demonstration code, icons, and Bootstrap. 
These files are no longer needed.

``` html showLineNumbers
<link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
<link href="css/app.css" rel="stylesheet" />
```

Delete the default **bootstrap** and **open-iconic** folders from the **wwwroot** folder.

1. wwwroot/css/bootstrap
1. wwwroot/css/open-iconic

Either remove or keep the **app.css** file but make sure you clear it out of any content when the [Sidebar](/docs/components/sidebar#full-layout-with-sidebar) component with full layout is used.

## Starter templates

### .NET 6

1. [Blazor Bootstrap - Blazor WebAssembly App](https://github.com/vikramlearning/blazorbootstrap-starter-templates/tree/master/src/BlazorBootstrap.Templates.Starter/NET6.BlazorWebAssemblyApp)

   <img src="https://i.imgur.com/aRV3rJm.png" alt="Blazor Bootstrap - Blazor WebAssembly App" />

### .NET 7

1. [Blazor Bootstrap - Blazor WebAssembly App](https://github.com/vikramlearning/blazorbootstrap-starter-templates/tree/master/src/BlazorBootstrap.Templates.Starter/NET7.BlazorWebAssemblyApp)

   <img src="https://i.imgur.com/4P8u0HR.png" alt="Blazor Bootstrap - Blazor WebAssembly App" />

1. [Blazor Bootstrap - Blazor Empty WebAssembly App](https://github.com/vikramlearning/blazorbootstrap-starter-templates/tree/master/src/BlazorBootstrap.Templates.Starter/NET7.BlazorWebAssemblyAppEmpty)

   <img src="https://i.imgur.com/CBEoZ6P.png" alt="Blazor Bootstrap - Blazor Empty WebAssembly App" />
