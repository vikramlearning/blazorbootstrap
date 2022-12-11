---
id: intro
sidebar_label: Introduction
sidebar_position: 1
title: Introduction
---

# Introduction

Get started with Blazor Bootstrap, with ready-made components.

## Quick start

Looking to quickly add Blazor Bootstrap to your project? Use NuGet package manager.

```shell
Install-Package BlazorBootstrap -Version 1.2.0
```

## Starter template

Change your `index.html` file and include the CSS and JS files:
 - Add `Bootstrap 5.x` CSS and JS files as recommended in the official Bootstrap website.
 - Add `Bootstrap Icons 1.x` CSS file as recommended in the official Bootstrap Icons website.
 - Include `blazorbootstrap.js` file
 - Include `blazorbootstrap.css` file

   ```html {8,9,10,16,17,18} showLineNumbers
   <!DOCTYPE html>
   <html lang="en">
   <head>
       <meta charset="utf-8" />
       <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
       <title>BlazorBootstrap - Starter Template</title>
       <base href="/" />
       <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3" crossorigin="anonymous" />
       <link href="_content/BlazorBootstrap/blazorbootstrap.css" rel="stylesheet" />
       <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.2/font/bootstrap-icons.css" rel="stylesheet" />
       <link href="BlazorBootstrap.UI.styles.css" rel="stylesheet" />
   </head>
   <body>
       <div id="app">Loading...</div>
       <script src="_framework/blazor.webassembly.js"></script>
       <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js" integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p" crossorigin="anonymous"></script>
       <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.0.1/chart.umd.js" integrity="sha512-gQhCDsnnnUfaRzD8k1L5llCCV6O9HN09zClIzzeJ8OJ9MpGmIlCxm+pdCkqTwqJ4JcjbojFr79rl2F1mzcoLMQ==" crossorigin="anonymous" referrerpolicy="no-referrer"></script> <!-- Add chart.js reference if Chart components are used in the application. -->
       <script src="_content/BlazorBootstrap/blazorbootstrap.js"></script>
   </body>
   </html>
   ```
   :::note NOTE
   ```js
   <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
   ```
   Add `chart.js` reference if the <b>Chart</b> components are used in the application.
   :::

 - Add Blazor Bootstrap service in the `Program.cs`

   ```cs {1,13} showLineNumbers
   using BlazorBootstrap;
   
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

 - Register tag helpers in `_Imports.razor`

   ```cs {1}
   @using BlazorBootstrap;
   ```