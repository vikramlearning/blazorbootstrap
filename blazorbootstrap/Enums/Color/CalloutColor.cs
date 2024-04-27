namespace BlazorBootstrap;

public enum CalloutType
{
    Default = 0,
    Danger,
    Warning,
    Info,
    [Obsolete("This enum value is obsolete. Use `Success` instead.")]
    Tip,
    Success
}
