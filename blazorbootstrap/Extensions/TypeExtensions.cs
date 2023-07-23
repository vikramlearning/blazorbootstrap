namespace BlazorBootstrap.Extensions;

/// <summary>
/// Various extension methods for <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Get property type name.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns>string</returns>
    public static string GetPropertyTypeName(this Type type, string propertyName)
    {
        if (type is null || string.IsNullOrWhiteSpace(propertyName))
            return string.Empty;

        var propertyTypeName = type.GetProperty(propertyName)?.PropertyType?.ToString();

        if (string.IsNullOrWhiteSpace(propertyTypeName))
            return string.Empty;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameInt16, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameInt16;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameInt32, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameInt32;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameInt64, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameInt64;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameChar, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameChar;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameString, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameString;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameSingle, StringComparison.InvariantCulture)) // float
            return StringConstants.PropertyTypeNameSingle;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameDecimal, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameDecimal;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameDouble, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameDouble;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameDateOnly, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameDateOnly;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameDateTime, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameDateTime;
        else if (propertyTypeName.Contains(StringConstants.PropertyTypeNameBoolean, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameBoolean;

        return string.Empty;
    }
}
