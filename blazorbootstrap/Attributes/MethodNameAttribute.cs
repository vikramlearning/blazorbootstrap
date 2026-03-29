namespace BlazorBootstrap;

/// <summary>
/// Attribute to specify the method name with parameters.
/// </summary>
public class MethodNameAttribute : Attribute
{
    #region Constructors
    
    public MethodNameAttribute(string methodName)
    {
        MethodName = methodName;
    }

    #endregion

    #region Properties, Indexers

    public string MethodName { get; }

    #endregion
}
