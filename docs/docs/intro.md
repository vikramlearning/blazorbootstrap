---
id: intro
sidebar_label: Introduction
sidebar_position: 1
title: Introduction
---

# Introduction

Get started with BlazorBootstrap, with ready made components.

## Quick start

Looking to quickly add BlazorBootstrap to your project? Use NuGet package manager.

```shell
Install-Package Blazor.Bootstrap -Version 0.2.1
```

## Starter template

Change your `index.html` file and include the CSS and JS files:
 - Add Bootstrap 5 CSS and JS files as recommended in the official Bootstrap website.
 - Include `blazor.bootstrap.js` file
 - Include `blazor.bootstrap.css` file

   ```html
   <!DOCTYPE html>
   <html lang="en">
   <head>
       <meta charset="utf-8" />
       <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
       <title>BlazorBootstrap - Starter Template</title>
       <base href="/" />
       <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3" crossorigin="anonymous">
       <link href="_content/Blazor.Bootstrap/blazor.bootstrap.css" rel="stylesheet" />
       <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.8.0/font/bootstrap-icons.css">
       <link href="BlazorBootstrap.UI.styles.css" rel="stylesheet" />
   </head>
   <body>
       <div id="app">Loading...</div>
       <script src="_framework/blazor.webassembly.js"></script>
       <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p" crossorigin="anonymous"></script>
       <script src="_content/Blazor.Bootstrap/blazor.bootstrap.js"></script>
   </body>
   </html>
   ```
 - Add BlazorBootstrap service in `Program.cs`

   ```cs {11}
   public class Program
   {
       public static async Task Main(string[] args)
       {
           var builder = WebAssemblyHostBuilder.CreateDefault(args);
           builder.RootComponents.Add<App>("#app");
   
           builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
   
           // Inject services           
           builder.Services.AddBlazorBootstrap();
   
           await builder.Build().RunAsync();
       }
   }
   ```