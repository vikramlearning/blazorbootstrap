namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// Extension methods for <see cref="PropertyInfo" />.
/// <para>
/// <see href="https://github.com/BlazorExpress/BlazorExpress.Bulma.Docx/blob/master/BlazorExpress.Bulma.Docx/Extensions/PropertyInfoExtenstions.cs" />
/// </para>
/// </summary>
public static class PropertyInfoExtenstions
{
    private static readonly NullabilityInfoContext _nullabilityInfoContext = new();

    /// <summary>
    /// Get event callback return type.
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns>Returns list of component event callbacks.</returns>
    public static string GetEventCallbackReturnType(this PropertyInfo propertyInfo)
    {
        HashSet<string> arguments = new();
        if (propertyInfo.PropertyType.IsGenericType)
        {
            Type[] genericArguments = propertyInfo.PropertyType.GetGenericArguments();
            if (genericArguments.Length > 0)
            {
                foreach (Type genericArgument in genericArguments)
                    arguments.Add(genericArgument.GetCSharpTypeName());
            }
        }

        return arguments.Count > 0 ? $"EventCallback<{string.Join(",", arguments)}>" : "EventCallback";
    }

    /// <summary>
    /// Get parameter type name.
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns>string</returns>
    public static string GetParameterTypeName(this PropertyInfo propertyInfo)
    {
        var nullabilityInfo = _nullabilityInfoContext.Create(propertyInfo);
        var typeName = nullabilityInfo.GetFriendlyTypeName();
        return typeName;
    }

    /// <summary>
    /// Get added version of a property.
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns>string</returns>
    public static string GetPropertyAddedVersion(this PropertyInfo propertyInfo)
    {
        var addedVersionAttribute = (AddedVersionAttribute?)Attribute.GetCustomAttribute(propertyInfo, typeof(AddedVersionAttribute));
        return addedVersionAttribute?.Version ?? string.Empty;
    }

    /// <summary>
    /// Get default value of a property.
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns>string</returns>
    public static string GetPropertyDefaultValue(this PropertyInfo propertyInfo)
    {
        var defaultValueAttribute = (DefaultValueAttribute?)Attribute.GetCustomAttribute(propertyInfo, typeof(DefaultValueAttribute));
        return defaultValueAttribute?.Value?.ToString() ?? "null";
    }

    /// <summary>
    /// Get property description.
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns>string</returns>
    public static string GetPropertyDescription(this PropertyInfo propertyInfo)
    {
        var descriptionAttribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute));
        return descriptionAttribute?.Description ?? string.Empty;
    }

    /// <summary>
    /// Determines whether the specified property is an EventCallback or EventCallback&lt;T&gt;.
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns>bool</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool IsEventCallbackProperty(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
        {
            throw new ArgumentNullException(nameof(propertyInfo));
        }

        // Check for EventCallback
        if (propertyInfo.PropertyType == typeof(EventCallback))
        {
            return true;
        }

        // Check for EventCallback<T>
        if (propertyInfo.PropertyType.IsGenericType &&
            propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(EventCallback<>))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if the property is required. Otherwise, false.
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns>bool</returns>
    public static bool IsPropertyRequired(this PropertyInfo propertyInfo)
    {
        var editorRequiredAttribute = (EditorRequiredAttribute?)Attribute.GetCustomAttribute(propertyInfo, typeof(EditorRequiredAttribute));
        return editorRequiredAttribute is not null;
    }
}