namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// Extension methods for <see cref="Type" />.
/// <para>
/// <see href="https://github.com/BlazorExpress/BlazorExpress.Bulma.Docx/blob/master/BlazorExpress.Bulma.Docx/Extensions/TypeExtensions.cs" />
/// </para>
/// </summary>
public static class TypeExtensions
{
    #region Methods

    /// <summary>
    /// Get component parameters.
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Returns list of component parameters</returns>
    private static IEnumerable<PropertyInfo>? GetComponentParameters(this Type type)
    {
        if (type is null)
            return null;

        var properties = type.GetProperties();
        return properties?.Where(p => p.GetCustomAttributes(typeof(ParameterAttribute), false).Any())?.OrderBy(p => p.Name);
    }

    /// <summary>
    /// Get component parameters and excludes event callbacks.
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Returns list of component parameters</returns>
    public static HashSet<PropertyInfo> GetComponentParametersOnly(this Type type)
    {
        var parameters = type.GetComponentParameters();
        if (parameters is null)
            return new HashSet<PropertyInfo>();

        return parameters
            .Where(p => !p.IsEventCallbackProperty())
            .ToHashSet();
    }

    /// <summary>
    /// Get component event callbacks.
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Returns list of component event callbacks.</returns>
    public static HashSet<PropertyInfo> GetComponentEventCallbacks(this Type type)
    {
        HashSet<string> eventCallbacks = new();

        var parameters = type.GetComponentParameters();
        if (parameters is null)
            return new HashSet<PropertyInfo>();

        return parameters
            .Where(p => p.IsEventCallbackProperty())
            .ToHashSet();
    }

    /// <summary>
    /// Get component methods.
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Returns list of component methods.</returns>
    public static HashSet<MethodInfo> GetComponentMethods(this Type type)
    {
        var methods = new HashSet<MethodInfo>();

        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            // Filter out methods inherited from System.Object (if needed)
            if (method.DeclaringType != typeof(object)
                && method.DeclaringType == type // Exclude methods declared in base classes
                && !method.Name.StartsWith("get_") // Exclude get_ methods
                && !method.Name.StartsWith("set_") // Exclude set_ methods
                && !method.GetCustomAttributes(typeof(JSInvokableAttribute), false).Any()) // Exclude methods that are not general public methods
            {
                methods.Add(method);
            }
        }

        return methods.ToHashSet();
    }

    /// <summary>
    /// Get property type name.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns>string</returns>
    public static string GetCSharpTypeName(this Type type)
    {
        if (type is null)
            return string.Empty;

        var typeName = type.Name;

        if (typeName.Contains(StringConstants.PropertyTypeNameInt16, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameInt16CSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameInt32, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameInt32CSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameInt64, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameInt64CSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameChar, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameCharCSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameStringComparison, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameStringComparisonCSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameString, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameStringCSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameSingle, StringComparison.InvariantCulture)) // float
            typeName = StringConstants.PropertyTypeNameSingleCSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameDecimal, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameDecimalCSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameDouble, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameDoubleCSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameDateOnly, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameDateOnlyCSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameDateTime, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameDateTimeCSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameBoolean, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameBooleanCSharpTypeKeyword;

        //else if (propertyType!.IsEnum)
        //    propertyTypeName = StringConstants.PropertyTypeNameEnumCSharpTypeKeyword;

        else if (typeName.Contains(StringConstants.PropertyTypeNameGuid, StringComparison.InvariantCulture))
            typeName = StringConstants.PropertyTypeNameGuidCSharpTypeKeyword;

        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            Type enumerableType = type.GetGenericArguments()[0]; // Get the T in IEnumerable<T>
            typeName = $"IEnumerable<{enumerableType.Name}>";
        }

        return typeName;
    }

    /// <summary>
    /// Get model properties.
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Returns list of model properties</returns>
    public static IEnumerable<PropertyInfo> GetModelProperties(this Type type)
    {
        if (type is null)
            return Enumerable.Empty<PropertyInfo>();

        var properties = type.GetProperties();
        return properties?.OrderBy(p => p.Name) ?? Enumerable.Empty<PropertyInfo>();
    }

    #endregion
}
