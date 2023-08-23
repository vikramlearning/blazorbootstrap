namespace BlazorBootstrap;

/// <summary>
/// An interface implemented by Id generators.
/// </summary>
public interface IIdGenerator
{
    #region Properties, Indexers

    /// <summary>
    /// Gets the newly generated and globally unique value.
    /// This value is guaranteed to be unique across all calls to the <see cref="Generate" /> property.
    /// </summary>
    string Generate { get; }

    #endregion
}