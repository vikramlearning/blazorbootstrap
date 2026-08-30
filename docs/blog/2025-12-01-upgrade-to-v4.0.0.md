---
title: Upgrade to v4.0.0
authors:
  name: Vikram Reddy
  title: Creator
  url: https://github.com/gvreddy04
  image_url: https://avatars.githubusercontent.com/u/2337067
---

## Recommendation

We strongly recommend all **Blazor Bootstrap** users upgrade to version **4.0.0**.

This is a major release with two significant breaking changes that require action before updating:

1. **.NET 6.0 support has been dropped.**
2. **Chart components have been moved to the separate `BlazorExpress.ChartJS` package.**

---

## Breaking Change 1 — .NET 6.0 Support Dropped

.NET 6 reached end-of-life in November 2024. Blazor Bootstrap 4.0.0 targets **.NET 8**, **.NET 9**, and **.NET 10** only.

### What you need to do

Update your project's `TargetFramework` (or `TargetFrameworks`) in your `.csproj` file to `net8.0`, `net9.0`, or `net10.0`:

```xml
<TargetFramework>net8.0</TargetFramework>
```

If your project cannot yet move off .NET 6, continue using **Blazor Bootstrap 3.x** until you are ready to upgrade your application runtime.

---

## Breaking Change 2 — Charts Moved to BlazorExpress.ChartJS

All chart components that were part of Blazor Bootstrap have been extracted into the dedicated [BlazorExpress.ChartJS](https://chartjs.blazorexpress.com/) package. This gives chart functionality its own dedicated release cycle and allows the BlazorBootstrap package to stay lean.

**Affected components:**
- `BarChart`
- `LineChart`
- `DoughnutChart`
- `PieChart`
- `PolarAreaChart`
- `RadarChart`
- `ScatterChart`

### Migration steps

**1. Install BlazorExpress.ChartJS**

```bash
dotnet add package BlazorExpress.ChartJS
```

Or via NuGet Package Manager:

```
Install-Package BlazorExpress.ChartJS
```

**2. Add the script to your `index.html` or `App.razor`**

Remove any existing BlazorBootstrap chart scripts and add:

```html
<script src="_content/BlazorExpress.ChartJS/blazorexpress.chartjs.js"></script>
```

**3. Register the service in `Program.cs`**

```csharp
builder.Services.AddBlazorExpressChartJS();
```

**4. Update namespaces**

In your `_Imports.razor` or individual pages, replace:

```razor
@using BlazorBootstrap
```

with:

```razor
@using BlazorExpress.ChartJS
```

**5. Review API differences**

The BlazorExpress.ChartJS API is closely aligned with the former BlazorBootstrap chart API but may have minor differences in parameter names and dataset types. Consult the [BlazorExpress.ChartJS documentation](https://chartjs.blazorexpress.com/) and the [original demos](https://chartjs.blazorexpress.com/demos/bar-chart) for full reference.

---

## Upgrade Steps (Non-Chart)

1. **Update your NuGet package reference** to `4.0.0`:

   ```bash
   dotnet add package Blazor.Bootstrap --version 4.0.0
   ```

   Or via Package Manager Console:

   ```
   Install-Package Blazor.Bootstrap -Version 4.0.0
   ```

2. **Verify your target framework** is `net8.0`, `net9.0`, or `net10.0` (see Breaking Change 1 above).

3. **Migrate chart usage** to `BlazorExpress.ChartJS` (see Breaking Change 2 above).

4. **Build and test** your application. All other component APIs remain backward-compatible with Blazor Bootstrap 3.x.

---

## New in v4.0.0

See the [v4.0.0 release notes](https://docs.blazorbootstrap.com/blog/2025/12/01/blazorbootstrap-4.0.0) for a full list of new components and enhancements, including:

- **RichTextEditor** component
- **FileInput** and **DragAndDropFileInput** components
- **SplitView** component
- **OTP Input** component
- ToastService async support
- PdfViewer download & manual zoom
- ConfirmDialog Yes-button auto-focus
- CSS custom property for sticky header z-index
- Extensive API documentation and IntelliSense metadata across all components

---

## Questions & Support

- [GitHub Discussions](https://github.com/vikramlearning/blazorbootstrap/discussions)
- [GitHub Issues](https://github.com/vikramlearning/blazorbootstrap/issues)
- [Docs Website](https://docs.blazorbootstrap.com/)
