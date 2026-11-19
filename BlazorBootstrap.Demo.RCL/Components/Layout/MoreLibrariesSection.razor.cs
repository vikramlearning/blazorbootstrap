namespace BlazorBootstrap.Demo.RCL;

public partial class MoreLibrariesSection : ComponentBase
{
    #region Fields and Constants

    private const string LogoPath = "_content/BlazorBootstrap.Demo.RCL/images/libraries/";

    private static readonly TeamLibrary[] libraries =
    [
        new("UnlimitedPDF",
            "A pure .NET library to create, merge and split PDFs, and convert HTML to PDF.",
            $"{LogoPath}unlimitedpdf.png",
            "https://unlimitedpdf.dev/",
            "https://github.com/unlimited-pdf/UnlimitedPDF",
            "https://www.nuget.org/packages/UnlimitedPDF"),
        new("BlazorExpress.ChartJS",
            "Chart.js charts for Blazor, configured in C#.",
            $"{LogoPath}blazorexpress-chartjs.svg",
            "https://chartjs.blazorexpress.com/",
            "https://github.com/BlazorExpress/BlazorExpress.ChartJS",
            "https://www.nuget.org/packages/BlazorExpress.ChartJS"),
        new("BlazorExpress.Bulma",
            "Enterprise-class Blazor components built on the Bulma CSS framework.",
            $"{LogoPath}blazorexpress-bulma.svg",
            "https://bulma.blazorexpress.com/",
            "https://github.com/BlazorExpress/BlazorExpress.Bulma",
            "https://www.nuget.org/packages/BlazorExpress.Bulma")
    ];

    #endregion

    #region Nested Types

    private sealed record TeamLibrary(string Name, string Description, string LogoUrl, string WebsiteUrl, string GithubUrl, string NugetUrl);

    #endregion
}
