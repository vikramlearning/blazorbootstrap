namespace BlazorBootstrap;

public enum StringFilterOperator
{
    /// <summary>
    /// Satisfied if the current value equals the specified value.
    /// </summary>
    Equals,

    /// <summary>
    /// Satisfied if the current value contains the specified value.
    /// </summary>
    Contains,

    /// <summary>
    /// Satisfied if the current value starts with the specified value.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Satisfied if the current value ends with the specified value.
    /// </summary>
    EndsWith
}