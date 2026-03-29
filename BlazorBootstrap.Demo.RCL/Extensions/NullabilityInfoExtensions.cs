namespace BlazorBootstrap.Demo.RCL;

/// <summary>
/// Extension methods for <see cref="NullabilityInfo" />.
/// These methods help in getting friendly type names considering nullability and generics.
/// </summary>
public static class NullabilityInfoExtensions
{
    public static string GetFriendlyTypeName(this NullabilityInfo info)
    {
        if (info.Type.IsGenericType && info.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(info.Type);
            return $"{underlyingType?.GetCSharpTypeName()}?";
        }

        var type = info.Type;
        string typeName;

        if (IsTuple(type))
        {
            var genericArgs = info.GenericTypeArguments.Select(GetFriendlyTypeName);
            typeName = $"({string.Join(", ", genericArgs)})";
        }
        else if (info.Type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(EventHandler<>))
            {
                var genericArg = info.GenericTypeArguments.Select(GetFriendlyTypeName);
                typeName = $"EventHandler<{string.Join(", ", genericArg)}>";
            }
            else
            {
                var genericTypeName = info.Type.Name;
                var backtickIndex = genericTypeName.IndexOf('`');
                if (backtickIndex > 0)
                {
                    genericTypeName = genericTypeName.Remove(backtickIndex);
                }
                var genericArgs = info.GenericTypeArguments.Select(GetFriendlyTypeName);
                typeName = $"{genericTypeName}<{string.Join(", ", genericArgs)}>";
            }
        }
        else
        {
            typeName = info.Type.GetCSharpTypeName();
        }

        if (info.ReadState == NullabilityState.Nullable && info.Type.IsValueType == false)
        {
            typeName += "?";
        }

        return typeName;
    }

    private static bool IsTuple(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(ValueTuple<>) ||
               genericTypeDefinition == typeof(ValueTuple<,>) ||
               genericTypeDefinition == typeof(ValueTuple<,,>) ||
               genericTypeDefinition == typeof(ValueTuple<,,,>) ||
               genericTypeDefinition == typeof(ValueTuple<,,,,>) ||
               genericTypeDefinition == typeof(ValueTuple<,,,,,>) ||
               genericTypeDefinition == typeof(ValueTuple<,,,,,,>) ||
               genericTypeDefinition == typeof(ValueTuple<,,,,,,,>);
    }
}
