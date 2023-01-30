---
id: blazor-server
sidebar_label: Blazor Server
sidebar_position: 2
title: Blazor Server
---

# Getting started - Blazor Server

Get started with the Enterprise-class Blazor Bootstrap Component library built on the Blazor and Bootstrap CSS framework.

## Install Nuget Package

Looking to quickly add **Blazor Bootstrap** to your project? Use NuGet package manager.

```shell
Install-Package Blazor.Bootstrap -Version 1.4.2
```

## Add CSS references

Add the following references to the `head` section in the **Pages/_Host.cshtml**.

```html showLineNumbers
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3" crossorigin="anonymous" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.3/font/bootstrap-icons.css" rel="stylesheet" />
<link href="_content/Blazor.Bootstrap/blazor.bootstrap.css" rel="stylesheet" />
```

:::note
If you use the **Blazor Server App Empty*** template (without demonstration code and Bootstrap), add the following references to the `head` section in the **Pages/_Host.cshtml**. 
There is a known GitHub issue [Blazor empty template doesn't load scoped CSS](https://github.com/dotnet/aspnetcore/issues/43975).
:::

```html showLineNumbers
<link href="_content/Blazor.Bootstrap/Blazor.Bootstrap.bundle.scp.css" rel="stylesheet" />
```

:::danger IMPORTANT
In .NET 6 Blazor Server App default template, you may see **Pages/_Layout.cshtml**. So, add these references in the **Pages/_Layout.cshtml** instead of in the **Pages/_Host.cshtml**.
:::

## Add script references

Add the following references to the `body` section in the **Pages/_Host.cshtml** after the **_framework/blazor.server.js** reference.

``` js showLineNumbers
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p" crossorigin="anonymous"></script>
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

In the **Pages/_Host.cshtml** file, the default blazor template includes demonstration code, icons, and Bootstrap. 
These files are no longer needed.

``` html showLineNumbers
<link rel="stylesheet" href="css/bootstrap/bootstrap.min.css" />
<link href="css/site.css" rel="stylesheet" />
```

Delete the default **bootstrap** and **open-iconic** folders from the **wwwroot** folder.

1. wwwroot/css/bootstrap
1. wwwroot/css/open-iconic

Either remove or keep the **site.css** file but make sure you clear it out of any content when the [Sidebar](/docs/components/sidebar#full-layout-with-sidebar) component with full layout is used.


## Checkout Starter templates

### .NET 6

- [Blazor Bootstrap - Server App](https://github.com/vikramlearning/blazorbootstrap-starter-templates/tree/master/src/BlazorBootstrap.Templates.Starter/NET6.BlazorServerApp)

### .NET 7

- [Blazor Bootstrap - Server App](https://github.com/vikramlearning/blazorbootstrap-starter-templates/tree/master/src/BlazorBootstrap.Templates.Starter/NET7.BlazorServerApp)
- [Blazor Bootstrap - Empty Server App](https://github.com/vikramlearning/blazorbootstrap-starter-templates/tree/master/src/BlazorBootstrap.Templates.Starter/NET7.BlazorServerAppEmpty)
