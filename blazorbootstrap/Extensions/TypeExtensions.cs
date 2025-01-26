namespace BlazorBootstrap;

/// <summary>
/// Various extension methods for <see cref="Type" />.
/// </summary>
public static class TypeExtensions
{
    #region Methods

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

        var propertyType = type.GetPropertyType(propertyName);
        if (propertyType is null)
            return string.Empty;

        var propertyTypeName = propertyType?.ToString();
        if (string.IsNullOrWhiteSpace(propertyTypeName))
            return string.Empty;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameInt16, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameInt16;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameInt32, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameInt32;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameInt64, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameInt64;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameChar, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameChar;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameString, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameString;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameSingle, StringComparison.InvariantCulture)) // float
            return StringConstants.PropertyTypeNameSingle;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameDecimal, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameDecimal;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameDouble, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameDouble;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameDateOnly, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameDateOnly;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameDateTime, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameDateTime;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameBoolean, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameBoolean;

        if (propertyType!.IsEnum)
            return StringConstants.PropertyTypeNameEnum;

        if (propertyTypeName.Contains(StringConstants.PropertyTypeNameGuid, StringComparison.InvariantCulture))
            return StringConstants.PropertyTypeNameGuid;

        return string.Empty;
    }

    /// <summary>
    /// Get property type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns>Type?</returns>
    public static Type? GetPropertyType(this Type type, string propertyName)
    {
        if (type is null || string.IsNullOrWhiteSpace(propertyName))
            return null;

        if (!propertyName.Contains("."))
            return type.GetProperty(propertyName)?.PropertyType;
        else // Nested property
        {
            var propertyNames = propertyName.Split('.');
            var _propertyName = propertyNames[0];

            var propertyType = type.GetProperty(_propertyName)?.PropertyType;

            if (propertyType is null)
                return null;

            for (var i = 1; i < propertyNames.Length; i++)
            {
                var _propName = propertyNames[i];
                propertyType = propertyType.GetProperty(_propName)?.PropertyType;

                if (propertyType is null)
                    return null;
            }
            return propertyType;
        }
    }

    #endregion
}
