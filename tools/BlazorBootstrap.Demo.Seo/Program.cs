using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using BlazorBootstrap.Demo.RCL;
using BlazorBootstrap.Demo.RCL.Components.Pages.Home;
using Microsoft.AspNetCore.Components;

namespace BlazorBootstrap.Demo.Seo;

/// <summary>
/// Writes the demos site's crawl files into a host's <c>wwwroot</c>: <c>sitemap.xml</c> from the RCL's routes,
/// <c>llms.txt</c> from the home catalog, <c>robots.txt</c>, and, for the WebAssembly host,
/// <c>staticwebapp.config.json</c> with the legacy redirects. Any inconsistency exits non-zero so the build fails.
/// Usage: <c>BlazorBootstrap.Demo.Seo --out &lt;wwwroot&gt; [--host server|webassembly]</c>.
/// </summary>
public static class Program
{
    #region Methods

    #region Public Methods

    public static int Main(string[] args)
    {
        try
        {
            var outDir = GetArgument(args, "--out") ?? throw new ArgumentException("Missing --out <wwwroot directory>.");
            var host = GetArgument(args, "--host") ?? "server";

            if (host is not ("server" or "webassembly"))
                throw new ArgumentException($"Unknown --host '{host}'. Use server or webassembly.");

            Directory.CreateDirectory(outDir);

            var routes = GetRoutes();
            Validate(routes);

            var written = 0;
            written += WriteIfChanged(Path.Combine(outDir, "sitemap.xml"), BuildSitemap(routes));
            written += WriteIfChanged(Path.Combine(outDir, "llms.txt"), BuildLlmsTxt(routes));
            written += WriteIfChanged(Path.Combine(outDir, "robots.txt"), BuildRobotsTxt());

            if (host == "webassembly")
                written += WriteIfChanged(Path.Combine(outDir, "staticwebapp.config.json"), BuildStaticWebAppConfig());

            Console.WriteLine($"BlazorBootstrap.Demo.Seo: {routes.Count} routes, {written} file(s) updated in {outDir}.");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error SEO001: {ex.Message}");

            return 1;
        }
    }

    #endregion

    #region Private Methods

    private static string BuildLlmsTxt(IReadOnlySet<string> routes)
    {
        var text = new StringBuilder();
        text.Append("# Blazor Bootstrap demos\n\n");
        text.Append("> Live examples of Blazor Bootstrap, a free, open-source library of Bootstrap 5.3 components for Blazor ");
        text.Append("Server, WebAssembly and MAUI Blazor Hybrid apps on .NET 8, 9, 10 and 11. Each page runs the component ");
        text.Append("and shows its source. Guides and the API reference are on the docs site.\n\n");

        text.Append("## Start here\n\n");
        AppendLink(text, "Demos home", SeoRoutes.DemosSiteUrl + "/", "every component with a live example, grouped by category");
        AppendLink(text, "Getting started", SeoRoutes.DemosSiteUrl + DemoRouteConstants.Demos_URL_GettingStarted, "install the NuGet package and set up a Blazor app");
        AppendLink(text, "Docs site", SeoRoutes.DocsSiteUrl + "/", "guides, getting-started pages for .NET 10 and .NET 11, API reference and release notes");
        AppendLink(text, "Docs site llms.txt", SeoRoutes.DocsSiteUrl + "/llms.txt", "the docs site's own index for AI tools");
        text.Append('\n');

        foreach (var category in HomeCatalog.Categories)
        {
            text.Append("## ").Append(category.Name).Append("\n\n");

            foreach (var item in category.Items)
            {
                if (!routes.Contains(SeoRoutes.Normalize(item.Href)))
                    throw new InvalidOperationException($"HomeCatalog item '{item.Name}' links to '{item.Href}', which is not a route.");

                AppendLink(text, item.Name, SeoRoutes.DemosSiteUrl + item.Href, item.Description.TrimEnd('.'));
            }

            text.Append('\n');
        }

        text.Append("## Showcase\n\n");
        AppendLink(text, "Showcase gallery", SeoRoutes.DemosSiteUrl + DemoRouteConstants.Showcase_URL_Prefix, "sample business apps built only with Blazor Bootstrap components, with fake data");

        return text.ToString();
    }

    private static string BuildRobotsTxt() =>
        "# Blazor Bootstrap demos. Every crawler, AI crawlers included, may crawl every page.\n" +
        "# Pages that should stay out of search results carry a noindex meta tag instead.\n" +
        "User-agent: *\n" +
        "Allow: /\n" +
        "\n" +
        $"Sitemap: {SeoRoutes.DemosSiteUrl}/sitemap.xml\n";

    private static string BuildSitemap(IReadOnlySet<string> routes)
    {
        var settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true, NewLineChars = "\n" };
        using var stream = new MemoryStream();

        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            foreach (var route in routes.Where(SeoRoutes.IsSitemapRoute).Order(StringComparer.Ordinal))
            {
                writer.WriteStartElement("url");
                writer.WriteElementString("loc", SeoRoutes.DemosSiteUrl + route);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray()) + "\n";
    }

    private static string BuildStaticWebAppConfig()
    {
        var redirects = new JsonArray();

        foreach (var (from, to) in SeoRoutes.LegacyRedirects.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            redirects.Add(new JsonObject { ["route"] = from, ["redirect"] = to, ["statusCode"] = 301 });
            redirects.Add(new JsonObject { ["route"] = from + "/", ["redirect"] = to, ["statusCode"] = 301 });
        }

        var config = new JsonObject
        {
            ["navigationFallback"] = new JsonObject
            {
                ["rewrite"] = "/index.html",
                ["exclude"] = new JsonArray("/images/*.{png,jpg,gif}", "/css/*", "/sitemap.xml", "/robots.txt", "/llms.txt"),
            },
            ["routes"] = redirects,
        };

        return config.ToJsonString(new JsonSerializerOptions { WriteIndented = true }).ReplaceLineEndings("\n") + "\n";
    }

    private static void AppendLink(StringBuilder text, string name, string url, string note) =>
        text.Append("- [").Append(EscapeMarkdown(name)).Append("](").Append(url).Append("): ").Append(EscapeMarkdown(note)).Append('\n');

    private static string EscapeMarkdown(string value) =>
        value.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]").Replace("(", "\\(").Replace(")", "\\)");

    private static string? GetArgument(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);

        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static IReadOnlySet<string> GetRoutes()
    {
        var assembly = typeof(SeoRoutes).Assembly;
        var routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in assembly.GetTypes())
            foreach (var route in type.GetCustomAttributes<RouteAttribute>(inherit: false))
                if (!route.Template.Contains('{'))
                    routes.Add(SeoRoutes.Normalize(route.Template));

        if (routes.Count == 0)
            throw new InvalidOperationException("No routes found in the Demo RCL.");

        return routes;
    }

    private static void Validate(IReadOnlySet<string> routes)
    {
        var problems = new List<string>();

        foreach (var (from, to) in SeoRoutes.LegacyRedirects)
        {
            if (routes.Contains(from))
                problems.Add($"legacy URL '{from}' is still a route; remove its @page");
            if (!routes.Contains(SeoRoutes.Normalize(to)))
                problems.Add($"legacy URL '{from}' redirects to '{to}', which is not a route");
        }

        foreach (var page in SeoRoutes.DocsSiteCanonical.Keys)
            if (!routes.Contains(page))
                problems.Add($"docs-site canonical key '{page}' is not a route");

        foreach (var page in SeoRoutes.IndexableShowcaseRoutes)
            if (!routes.Contains(page))
                problems.Add($"indexable Showcase route '{page}' is not a route");

        if (problems.Count > 0)
            throw new InvalidOperationException(string.Join("; ", problems));
    }

    private static int WriteIfChanged(string path, string content)
    {
        if (File.Exists(path) && File.ReadAllText(path) == content)
            return 0;

        File.WriteAllText(path, content, new UTF8Encoding(false));

        return 1;
    }

    #endregion

    #endregion
}
