namespace BlazorBootstrap;

/// <summary>
/// Attribute to specify the parameter type name.
/// </summary>
public class ParameterTypeNameAttribute : Attribute
{
    #region Constructors

    public ParameterTypeNameAttribute(string typeName)
    {
        TypeName = typeName;
    }

    #endregion

    #region Properties, Indexers

    public string TypeName { get; }

    #endregion
}
