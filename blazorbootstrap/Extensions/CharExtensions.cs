namespace BlazorBootstrap;

public static class CharExtensions
{
    public static bool IsAlphanumeric(this char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               (c >= '0' && c <= '9');
    }
}
