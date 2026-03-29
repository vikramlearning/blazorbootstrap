namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// Extension methods for <see cref="MethodInfo" />.
/// <para>
/// <see href="https://github.com/BlazorExpress/BlazorExpress.Bulma.Docx/blob/master/BlazorExpress.Bulma.Docx/Extensions/MethodInfoExtensions.cs" />
/// </para>
/// </summary>
public static class MethodInfoExtensions
{
    private static readonly NullabilityInfoContext _nullabilityInfoContext = new();

    /// <summary>
    /// Get added version of a method.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <returns>string</returns>
    public static string GetMethodAddedVersion(this MethodInfo methodInfo)
    {
        var addedVersionAttribute = (AddedVersionAttribute?)Attribute.GetCustomAttribute(methodInfo, typeof(AddedVersionAttribute));
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
        var descriptionAttribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(methodInfo, typeof(DescriptionAttribute));
        return descriptionAttribute?.Description ?? string.Empty;
    }

    /// <summary>
    /// Get method name.
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns>string</returns>
    public static string GetMethodName(this MethodInfo methodInfo)
    {
        var parameters = methodInfo.GetParameters();
        var parameterStrings = parameters.Select(p =>
        {
            var paramNullabilityInfo = _nullabilityInfoContext.Create(p);
            return $"{paramNullabilityInfo.GetFriendlyTypeName()} {p.Name}";
        });

        return $"{methodInfo.Name}({string.Join(", ", parameterStrings)})";
    }

    /// <summary>
    /// Get method return type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <returns>string</returns>
    public static string GetMethodReturnType(this MethodInfo methodInfo)
    {
        var nullabilityInfo = _nullabilityInfoContext.Create(methodInfo.ReturnParameter);
        return nullabilityInfo.GetFriendlyTypeName();
    }
}
