namespace BlazorBootstrap;

/// <summary>
/// Specifies the direction in which the branches of a <see cref="MindMap{TItem}" /> grow from its root.
/// </summary>
public enum MindMapDirection
{
    /// <summary>
    /// The root sits at the left and branches grow to the right.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("The root sits at the left and branches grow to the right.")]
    Right,

    /// <summary>
    /// The root sits at the right and branches grow to the left.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("The root sits at the right and branches grow to the left.")]
    Left,

    /// <summary>
    /// The root sits in the centre and its branches are split between the right and left sides.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("The root sits in the centre and its branches are split between the right and left sides.")]
    BothSides,

    /// <summary>
    /// The root sits at the top and branches grow down.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("The root sits at the top and branches grow down.")]
    Down,

    /// <summary>
    /// The root sits at the bottom and branches grow up.
    /// </summary>
    [AddedVersion("4.1.0")]
    [Description("The root sits at the bottom and branches grow up.")]
    Up
}
