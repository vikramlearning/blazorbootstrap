namespace BlazorBootstrap.Demo.RCL;

public static class EnumExtensions
{
    public static string? ToLanguageCssClass(this LanguageCode languageCode) =>
        languageCode switch
        {
            LanguageCode.AspNet => "language-aspnet",
            LanguageCode.CSharp => "language-csharp",
            LanguageCode.Css => "language-css",
            LanguageCode.HTML => "language-html",
            LanguageCode.JavaScript => "language-js",
            LanguageCode.JSON => "language-json",
            LanguageCode.JSONP => "language-jsonp",
            LanguageCode.Markdown => "language-md",
            LanguageCode.PowerShell => "language-powershell",
            LanguageCode.Razor => "language-razor",
            LanguageCode.Text => "language-none",
            LanguageCode.YAML => "language-yaml",
            _ => null
        };

    public static string? ToLanguageCodeString(this LanguageCode languageCode) =>
        languageCode switch
        {
            LanguageCode.AspNet => "ASP.NET",
            LanguageCode.CSharp => "C#",
            LanguageCode.Css => "CSS",
            LanguageCode.HTML => "HTML",
            LanguageCode.JavaScript => "JS",
            LanguageCode.JSON => "JSON",
            LanguageCode.JSONP => "JSONP",
            LanguageCode.Markdown => "Markdown",
            LanguageCode.PowerShell => "PowerShell",
            LanguageCode.Razor => "Razor",
            LanguageCode.Text => "Text",
            LanguageCode.YAML => "yaml",
            _ => null
        };
}
