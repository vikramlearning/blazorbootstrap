namespace BlazorBootstrap.Demo.RCL;

public static class EnumExtensions
{
    public static string? ToLanguageCssClass(this LanguageCode languageCode) =>
        languageCode switch
        {
            LanguageCode.AspNet => "language-aspnet",
            LanguageCode.CSharp => "language-csharp",
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
}
