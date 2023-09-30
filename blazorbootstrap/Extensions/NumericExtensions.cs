namespace BlazorBootstrap.Extensions;

public static class NumericExtensions
{
    #region Methods
    
    public static bool IsNumeric(this string? value)
    {
        if (int.TryParse(value, out int ir))
            return true;

        if (double.TryParse(value, out double dr))
            return true;

        if (long.TryParse(value, out long lr))
            return true;

        if (short.TryParse(value, out short sr))
            return true;

        if (float.TryParse(value, out float fr))
            return true;

        return false;
    }
    
    #endregion
}
