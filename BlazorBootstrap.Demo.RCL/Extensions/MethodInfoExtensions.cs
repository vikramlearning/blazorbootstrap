namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// Extension methods for <see cref="MethodInfo" />.
/// <para>
/// <see href="https://github.com/BlazorExpress/BlazorExpress.Bulma.Docx/blob/master/BlazorExpress.Bulma.Docx/Extensions/MethodInfoExtensions.cs" />
/// </para>
/// </summary>
public static class MethodInfoExtensions
{
    /// <summary>
    /// Get added version of a method.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <returns>string</returns>
    public static string GetMethodAddedVersion(this MethodInfo methodInfo)
    {
        var addedVersionAttribute = methodInfo.GetCustomAttributes(typeof(AddedVersionAttribute), false).FirstOrDefault() as AddedVersionAttribute;
        return addedVersionAttribute?.Version ?? string.Empty;
    }

    /// <summary>
    /// Get method description.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <returns>string</returns>
    public static string GetMethodDescription(this MethodInfo methodInfo)
    {
        var descriptionAttribute = methodInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
        return descriptionAttribute?.Description ?? string.Empty;
    }

    public static string GetMethodParameters(this MethodInfo methodInfo)
    {
        var parameters = methodInfo.GetParameters();
        if (parameters.Length == 0)
            return string.Empty;

        var parametersWithType = new HashSet<string>();
        foreach (var parameter in parameters)
            parametersWithType.Add($"{parameter.ParameterType.GetCSharpTypeName()} {parameter.Name}");

        return string.Join(",", parametersWithType);
    }

    /// <summary>
    /// Get method return type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <returns>string</returns>
    public static string GetMethodReturnType(this MethodInfo methodInfo)
        => methodInfo.ReturnType.GetCSharpTypeName();

    /// <summary>
    /// Get method return type name.
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns>string</returns>
    public static string GetMethodReturnTypeName(this MethodInfo methodInfo)
    {
        var parameterTypeNameAttribute = methodInfo.GetCustomAttributes(typeof(MethodReturnTypeNameAttribute), false).FirstOrDefault() as MethodReturnTypeNameAttribute;
        return parameterTypeNameAttribute?.TypeName ?? null!;
    }
}
