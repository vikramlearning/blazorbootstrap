namespace BlazorBootstrap;

/// <summary>
/// Attribute to specify the version when a property was added.
/// </summary>
public class AddedVersionAttribute : Attribute
{
    #region Constructors

    public AddedVersionAttribute(string version)
    {
        Version = version;
    }

    #endregion

    #region Properties, Indexers

    public string Version { get; }

    #endregion
}
