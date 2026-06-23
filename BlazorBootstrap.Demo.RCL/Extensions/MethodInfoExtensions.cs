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
    /// Get the effective added version of a method using an optional docs context.
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="versionContextType"></param>
    /// <returns>string</returns>
    public static string GetMethodAddedVersion(this MethodInfo methodInfo, Type? versionContextType)
    {
        var methodVersion = methodInfo.GetMethodAddedVersion();
        var contextVersion = versionContextType.GetTypeAddedVersion();

        if (string.IsNullOrWhiteSpace(methodVersion))
            return contextVersion;

        if (string.IsNullOrWhiteSpace(contextVersion))
            return methodVersion;

        if (Version.TryParse(methodVersion, out var parsedMethodVersion)
            && Version.TryParse(contextVersion, out var parsedContextVersion))
            return parsedMethodVersion >= parsedContextVersion ? methodVersion : contextVersion;

        return methodVersion;
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

    private static string GetTypeAddedVersion(this Type? type)
    {
        if (type is null)
            return string.Empty;

        var addedVersionAttribute = (AddedVersionAttribute?)Attribute.GetCustomAttribute(type, typeof(AddedVersionAttribute));
        return addedVersionAttribute?.Version ?? string.Empty;
    }
}
