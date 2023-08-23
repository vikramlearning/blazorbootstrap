namespace BlazorBootstrap;

/// <summary>
/// An interface implemented by Id generators.
/// </summary>
public interface IIdGenerator
{
    #region Properties, Indexers

    /// <summary>
    /// Gets the newly generated and globally unique value.
    /// </summary>
    string Generate { get; }

    #endregion
}