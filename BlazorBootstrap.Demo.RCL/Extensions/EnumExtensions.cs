namespace BlazorBootstrap.Demo.RCL;

public static class EnumExtensions
{
    public static string? ToLanguageCssClass(this LanguageCode languageCode) =>
        languageCode switch
        {
            LanguageCode.AspNet => "language-aspnet",
            LanguageCode.CSharp => "language-csharp",
            LanguageCode.Css => "language-css",
            LanguageCode.Html => "language-html",
            LanguageCode.JavaScript => "language-js",
            LanguageCode.Json => "language-json",
            LanguageCode.Jsonp => "language-jsonp",
            LanguageCode.Markdown => "language-md",
            LanguageCode.PowerShell => "language-powershell",
            LanguageCode.Razor => "language-razor",
            LanguageCode.StructuredText => "language-none",
            LanguageCode.Yaml => "language-yaml",
            _ => null
        };

    public static string? ToLanguageCodeString(this LanguageCode languageCode) =>
        languageCode switch
        {
            LanguageCode.AspNet => "ASP.NET",
            LanguageCode.CSharp => "C#",
            LanguageCode.Css => "CSS",
            LanguageCode.Html => "HTML",
            LanguageCode.JavaScript => "JS",
            LanguageCode.Json => "JSON",
            LanguageCode.Jsonp => "JSONP",
            LanguageCode.Markdown => "Markdown",
            LanguageCode.PowerShell => "PowerShell",
            LanguageCode.Razor => "Razor",
            LanguageCode.StructuredText => "Text",
            LanguageCode.Yaml => "yaml",
            _ => null
        };
}
