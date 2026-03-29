namespace BlazorBootstrap;

/// <summary>
/// Attribute to specify the method return type name.
/// </summary>
public class MethodReturnTypeNameAttribute : Attribute
{
    #region Constructors

    public MethodReturnTypeNameAttribute(string typeName)
    {
        TypeName = typeName;
    }

    #endregion

    #region Properties, Indexers

    public string TypeName { get; }

    #endregion
}
